using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Game;
using Vexillum.util;
using Vexillum.Entities.Weapons;

namespace Server
{
    public class AIController
    {
        const int stepDelay = 500;
        const int maxStuckFrames = 5;
        const int maxEnemyDistance = 300;
        const int maxEnemyDistance2 = maxEnemyDistance * maxEnemyDistance;
        const int playerPathUpdateTime = 1000;
        private long lastStepTime;
        private Server server;
        private SurvivalGameMode gameMode;
        private List<AIPlayer> bots = new List<AIPlayer>(12);
        private Dictionary<PlayerClass, AIPlayer[]> offensivePlayers = new Dictionary<PlayerClass,AIPlayer[]>(2);
        private Dictionary<PlayerClass, AIPlayer[]> defensivePlayers = new Dictionary<PlayerClass, AIPlayer[]>(2);

        public int rocketIndex = -1;
        public int smgIndex = -1;
        public int swordIndex = -1;

        public AIController(Server server)
        {
            this.server = server;
            gameMode = (SurvivalGameMode)server.gameMode;
            Weapon[] inv = gameMode.GetPlayerInventory(null);
            for (int i = 0; i < inv.Length; i++)
            {
                if (inv[i] is RocketLauncher)
                    rocketIndex = i;
                else if (inv[i] is SMG)
                    smgIndex = i;
                else if (inv[i] is Sword)
                    swordIndex = i;
            }
        }
        public int NumBots
        {
            get
            {
                return bots.Count;
            }
        }
        public void AddPlayer(AIPlayer p)
        {
            lock (bots)
            {
                bots.Add(p);
            }
        }
        public void RemovePlayer(AIPlayer p)
        {
            lock (bots)
            {
                bots.Remove(p);
            }
        }
        public void Step(long gameTime)
        {
            //Server.Debug("AIController.Step(" + gameTime + ")");
            if (gameTime - lastStepTime > stepDelay)
            {
                Think(PlayerClass.Green);
                Think(PlayerClass.Blue);
                lastStepTime = gameTime;
            }
            lock (bots)
            {
                foreach (AIPlayer p in bots)
                {
                    if (p.Entity == null)
                        continue;
                    if (p.targetType == TargetType.None)
                    {
                        p.Entity.xVelocity = 0;
                    }
                    else
                    {
                        //move to target
                        if (p.targetPlayer != null)
                        {
                            if (p.targetPlayer.IsAlive())
                            {

                                if ((p.targetPlayer.Entity.Position - p.targetPos).LengthSquared() > 20*20)
                                {
                                    p.targetPos = p.targetPlayer.Entity.Position;
                                    p.lastPlayerPathUpdateTime = gameTime;
                                    p.UpdateTargetPath();
                                }
                            }
                            else
                                p.ClearTarget();
                        }
                        if (p.targetPath != null)
                        {
                            if (!MoveTowards(p, p.nextPathNode))
                            {
                                p.targetPath.index++;
                                p.lastNodeUpdateTime = gameTime;
                                if (p.targetPath.index >= p.targetPath.points.Count)
                                {
                                    p.Entity.xVelocity = 0;
                                    p.ClearTarget();
                                }
                                else
                                {
                                    p.nextPathNode = new Vec2(p.targetPath.points[p.targetPath.index].x, p.targetPath.points[p.targetPath.index].y);
                                }
                            }
                            //jump if stuck
                            if (p.Entity.Position == p.lastPosition)
                            {
                                p.stuckFrames++;
                                if (p.stuckFrames > 5)
                                    p.Entity.Jump();
                            }
                            else
                                p.stuckFrames = 0;
                            p.lastPosition = p.Entity.Position;
                        }
                        else if (p.pathTargetPos != Vec2.Zero)
                        {
                            MoveTowards(p, p.pathTargetPos);
                        }
                        //jump if stuck
                        if (p.Entity.Position == p.lastPosition && p.Entity.xVelocity != 0)
                        {
                            p.stuckFrames++;
                            if (p.stuckFrames > 5)
                            {
                                if (p.Entity.ladder && p.Entity.ladderDirection == 0)
                                {
                                    if (p.Entity.Velocity.Y <= 0 && p.Entity.yCollision)
                                    {
                                        p.Entity.ladderDirection = 1;
                                    }
                                    /*else if (p.Entity.Velocity.Y > 0 && p.Entity.yCollision)
                                    {
                                        p.Entity.ladderDirection = -1;
                                    }*/
                                }
                                else
                                {
                                    p.Entity.xVelocity *= -1;
                                    p.Entity.Jump();
                                }
                            }
                        }
                        else
                            p.stuckFrames = 0;
                        p.lastPosition = p.Entity.Position;

                        if (gameTime - p.lastNodeUpdateTime > 2000)
                            p.UpdateTargetPath();

                        //look for enemies
                        foreach (ServerPlayer t in server.players)
                        {
                            if (t.Entity == null)
                                continue;
                            if (p.CurrentClass != t.CurrentClass && t.CurrentClass != PlayerClass.Spectator)
                            {
                                Vec2 distance = (t.Entity.Position - p.Entity.Position);
                                float length = distance.Length();
                                if (length < maxEnemyDistance)
                                {
                                    Vec2 unit = distance;
                                    unit /= length;
                                    bool canFire = true;
                                    for (float d = 0; d < length; d += 2)
                                    {
                                        Vec2 pos = p.Entity.Weapon.GetPivot() + distance * (d / length);
                                        if (server.level.terrain.GetTerrain((int)pos.X, (int)pos.Y))
                                        {
                                            canFire = false;
                                            break;
                                        }
                                    }
                                    if (canFire)
                                    {
                                        if (length < 32 && swordIndex != -1)
                                            p.SelectWeapon(swordIndex);
                                        else if (t.Entity.Velocity.LengthSquared() > 4 * 4 && smgIndex != -1)
                                            p.SelectWeapon(smgIndex);
                                        else if (rocketIndex != -1)
                                            p.SelectWeapon(rocketIndex);
                                        else
                                            p.SelectWeapon(0);
                                        p.Entity.ArmAngle = (float)Math.Atan2(-unit.Y, unit.X);
                                        p.FireWeapon();
                                        if (((ReloadableWeapon)p.Entity.Weapon).clipAmmo <= 1)
                                            ((ReloadableWeapon)p.Entity.Weapon).Reload();
                                        p.firingAt = t;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private bool MoveTowards(AIPlayer p, Vec2 pos)
        {
            Vec2 diff = (pos) - p.Entity.FeetPosition;
            //if(p.targetPath != null)
            //    Server.Debug(diff + " " + p.targetPath.index);
            float xd = Math.Abs(diff.X);
            float yd = Math.Abs(diff.Y);

            float thresholdY = 5;
            float thresholdX = 5;
            bool moved = false;
            bool useLadder = false;
            if (p.firingAt == null)
            {
                p.ArmAngle = (float) Math.Atan2(-diff.Y, diff.X);
            }
            if ((xd > yd || !p.Entity.ladder) && !p.Entity.xCollision)
            {
                if (diff.X > thresholdX)
                {
                    p.Entity.xVelocity = p.Entity.Speed;
                    p.Entity.direction = true;
                    p.Entity.moving = true;
                    p.Entity.movementChanged = true;
                    moved = true;
                }
                else if (diff.X < -thresholdX)
                {
                    p.Entity.xVelocity = -p.Entity.Speed;
                    p.Entity.direction = false;
                    p.Entity.moving = true;
                    p.Entity.movementChanged = true;
                    moved = true;
                }
            }
            else
            {
                if (diff.Y > thresholdY && p.Entity.ladder)
                {
                    p.Entity.ladderDirection = 1;
                    moved = true;
                    useLadder = true;
                    p.Entity.moving = false;
                    p.Entity.movementChanged = false;
                }
                else if (diff.Y < -thresholdY && p.Entity.ladder)
                {
                    p.Entity.ladderDirection = -1;
                    moved = true;
                    useLadder = true;
                    p.Entity.moving = false;
                    p.Entity.movementChanged = false;
                }
            }

            if (!p.Entity.ladder)
            {
                p.Entity.ladderDirection = 0;
                p.Entity.FixedVelocity.Y = 0;
                p.Entity.jumping = false;
            }


            return !(Math.Abs(diff.X) <= thresholdX && Math.Abs(diff.Y) <= thresholdY);
        }
        private void Think(PlayerClass cl)
        {
            //Server.Debug("AIController.Think(" + cl + ")");
            Player ourFlagCarrier = cl == PlayerClass.Green ? gameMode.greenFlagCarrier : gameMode.blueFlagCarrier;
            Player enemyFlagCarrier = cl == PlayerClass.Blue ? gameMode.greenFlagCarrier : gameMode.blueFlagCarrier;
            bool enemyFlagTaken = ourFlagCarrier != null;
            bool ourFlagTaken = enemyFlagCarrier != null;
            //ServerPlayer enemyFlagCarrier =

            lock (bots)
            {
                foreach (AIPlayer p in bots)
                {
                    //Server.Debug("Current player: "+p.name);
                    if (p.Entity == null || p.CurrentClass != cl)
                        continue;
                    if (p == ourFlagCarrier)
                    {
                        if(p.targetType == TargetType.None || p.targetPath == null)
                            p.TargetOurFlag();
                    }
                    else
                    {
                        //Server.Debug("Target type is " + p.targetType + ", strategy " + p.strategy);
                        switch (p.targetType)
                        {
                            case TargetType.OurFlag:
                                if (!ourFlagTaken || ourFlagCarrier != p)
                                    p.ClearTarget();
                                break;
                            case TargetType.EnemyFlag:
                                if (enemyFlagTaken)
                                {
                                    switch (p.strategy)
                                    {
                                        case AIStrategy.Offensive:
                                            p.TargetRandomEnemy();
                                            break;
                                        case AIStrategy.Defensive:
                                            p.TargetPlayer(ourFlagCarrier);
                                            break;
                                    }
                                }
                                else if (p.firingAt != null && p.firingAt.IsAlive())
                                {
                                    p.TargetPlayer(p.firingAt);
                                    p.firingAt = null;
                                }
                                break;
                            case TargetType.Player:
                                if (p.targetPlayer == null || !p.targetPlayer.IsAlive())
                                {
                                    p.ClearTarget();
                                }
                                else if (p.targetPlayer == p.firingAt && (p.Position - p.targetPlayer.Position).LengthSquared() > maxEnemyDistance2)
                                {
                                    p.ClearTarget();
                                }
                                break;
                            case TargetType.None:
                                switch (p.strategy)
                                {
                                    case AIStrategy.Offensive:
                                        if (!enemyFlagTaken)
                                            p.TargetEnemyFlag();
                                        else
                                        {
                                            if (server.level.random.NextDouble() < 0.5)
                                                p.TargetRandomEnemy();
                                            else
                                                p.TargetPlayer(ourFlagCarrier);
                                        }
                                        break;
                                    case AIStrategy.Defensive:
                                        if (ourFlagTaken)
                                            p.TargetPlayer(enemyFlagCarrier);
                                        else
                                            p.TargetRandomEnemy();
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
        }
        public void ResetStrategy()
        {
            offensivePlayers.Clear();
            defensivePlayers.Clear();
            ResetStrategy(PlayerClass.Green);
            ResetStrategy(PlayerClass.Blue);
        }
        private void ResetStrategy(PlayerClass cl)
        {
            int numOffensive, numDefensive;
            AIStrategy strategy;
            int botCount = 0;
            if (cl == PlayerClass.Green)
            {
                strategy = GetStrategy(gameMode.numGreen, gameMode.greenCaptures, gameMode.blueCaptures, gameMode.greenFlagCarrier != null, gameMode.blueFlagCarrier != null);
            }
            else
            {
                strategy = GetStrategy(gameMode.numBlue, gameMode.blueCaptures, gameMode.greenCaptures, gameMode.blueFlagCarrier != null, gameMode.greenFlagCarrier != null);
            }
            //Server.Debug("Main strategy: " + strategy);
            for (int k = 0; k < bots.Count; k++)
                if (bots[k].CurrentClass == cl)
                    botCount++;
            if (strategy == AIStrategy.Offensive)
            {
                numOffensive = botCount;
                numDefensive = 0;
            }
            else if (strategy == AIStrategy.Defensive)
            {
                numOffensive = 0;
                numDefensive = botCount;
            }
            else
            {
                if (server.level.random.NextDouble() < 0.5)
                {
                    numOffensive = botCount / 2;
                    numDefensive = botCount - numOffensive;
                }
                else
                {
                    numDefensive = botCount / 2;
                    numOffensive = botCount - numDefensive;
                }
            }

            AIPlayer[] offensive = new AIPlayer[numOffensive];
            AIPlayer[] defensive = new AIPlayer[numDefensive];

            int i = 0;
            int j = 0;
            for (int idx = 0; idx < bots.Count; idx++)
            {
                AIPlayer bot = bots[idx];
                if (bot.PlayerClass != cl)
                    continue;
                else
                {
                    if (numOffensive > 0)
                    {
                        offensive[i] = bot;
                        bot.strategy = AIStrategy.Offensive;
                        i++;
                        numOffensive--;
                    }
                    else if (numDefensive > 0)
                    {
                        defensive[j] = bot;
                        bot.strategy = AIStrategy.Defensive;
                        j++;
                        numDefensive--;
                    }
                }
            }

            offensivePlayers.Add(cl, offensive);
            defensivePlayers.Add(cl, defensive);
        }
        private AIStrategy GetStrategy(int playerCount, int myScore, int enemyScore, bool myFlagTaken, bool theirFlagTaken)
        {
            if (playerCount == 1)
                return AIStrategy.Offensive;
            else if (myFlagTaken && theirFlagTaken)
                return AIStrategy.Balanced;
            else if (myFlagTaken)
                return AIStrategy.Defensive;
            else
            {
                if (myScore < enemyScore)
                {
                    if (enemyScore == gameMode.maxCaptures - 1)
                        return AIStrategy.Defensive;
                    else
                        return AIStrategy.Balanced;
                }
                else if (myScore > enemyScore)
                {
                    if (enemyScore == gameMode.maxCaptures - 1)
                        return AIStrategy.Offensive;
                    else
                        return AIStrategy.Balanced;
                }
                else
                    return AIStrategy.Balanced;
            }
        }
        public enum AIStrategy { Defensive, Offensive, Balanced };
        public enum TargetType { Player, EnemyFlag, OurFlag, None};
    }
}
