using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Game;
using System.Threading;
using Vexillum.util;
using Vexillum.Entities.Weapons;

namespace Server
{
    public class AIPlayer : ServerPlayer
    {
        public AIPlayer(Server server, string name) : base(null, null)
        {
            this.server = server;
            this.name = name;
            isBot = true;
        }
        public Vec2 targetPos;
        public LevelNodeGraph.Path targetPath = null;
        public Vec2 nextPathNode;
        public Vec2 nextPathNodeFar;
        public Vec2 pathTargetPos;
        public Vec2 lastPosition;
        public int stuckFrames = 0;
        public long lastPlayerPathUpdateTime;
        public long lastNodeUpdateTime = 0;
        public long followPlayerTime = 0;
        public Player targetPlayer;
        public Player previousTargetPlayer = null;
        public Player firingAt;
        public AIController.TargetType targetType = AIController.TargetType.None;
        public AIController.AIStrategy strategy = AIController.AIStrategy.Balanced;
        public void TargetEnemyFlag()
        {
            //Server.Debug("Targetting enemy flag");
            if (CurrentClass == PlayerClass.Green)
                targetPos = ((ServerLevel)server.level).blueFlag.Position;
            else
                targetPos = ((ServerLevel)server.level).greenFlag.Position;
            UpdateTargetPath();
            targetType = AIController.TargetType.EnemyFlag;
        }
        public void TargetOurFlag()
        {
            //Server.Debug("Targetting our flag");
            if (CurrentClass == PlayerClass.Blue)
                targetPos = ((ServerLevel)server.level).blueFlag.Position;
            else
                targetPos = ((ServerLevel)server.level).greenFlag.Position;
            UpdateTargetPath();
            targetType = AIController.TargetType.OurFlag;
        }
        public void TargetRandomEnemy()
        {
            PlayerClass enemyClass = CurrentClass == PlayerClass.Green ? PlayerClass.Blue : PlayerClass.Green;
            targetPlayer = null;
            int count = 0;
            //Server.Debug("Targetting random enemy...");
            while (targetPlayer == null)
            {
                int r = entity.Level.random.Next(server.players.Count);
                targetPlayer = server.players.ElementAt(r);
                if (targetPlayer.CurrentClass != enemyClass || targetPlayer.Entity == null)
                    targetPlayer = null;
                count++;
                if (count > 20)
                {
                    ClearTarget();
                    //Server.Debug("None found, clearing target");
                    return;
                }
            }
            targetPos = targetPlayer.Entity.Position;
            UpdateTargetPath();
            targetType = AIController.TargetType.Player;
        }
        public void TargetPlayer(Player p)
        {
            if (p == null)
                p= p;
            //Server.Debug("Targetting player: "+p);
            targetPlayer = p;
            targetPos = targetPlayer.Entity.Position;
            UpdateTargetPath();
            targetType = AIController.TargetType.Player;
        }
        public void TargetEnemyFlagCarrier()
        {
            if (CurrentClass == PlayerClass.Green)
            {
                targetPlayer = ((SurvivalGameMode)server.gameMode).blueFlagCarrier;
            }
            else
            {
                targetPlayer = ((SurvivalGameMode)server.gameMode).greenFlagCarrier;
            }
            targetPos = targetPlayer.Position;
            UpdateTargetPath();
            targetType = AIController.TargetType.Player;
        }
        public void UpdateTargetPath()
        {
            lastNodeUpdateTime = server.level.GetTime();
            lastPlayerPathUpdateTime = server.level.GetTime();
            //Server.Debug("Finding path from "+entity.Position+" to "+targetPos);
            DateTime t = new DateTime();
            ((ServerLevel)server.level).graph.FindPath((int)entity.Position.X, (int)entity.Position.Y, (int)targetPos.X, (int)targetPos.Y, this);
        }
        public void OnTargetPathChanged()
        {
            /*if (targetPath == null)
                Server.Debug("None found");
            else
                Server.Debug(targetPath.points.Count + " points");*/
            if (targetPath == null || targetPath.points.Count == 0)
            {
                targetPath = null;
                pathTargetPos = targetPos;
                nextPathNode = Vec2.Zero;
            }
            else
            {
                pathTargetPos = Vec2.Zero;
                nextPathNode = new Vec2(targetPath.points[0].x, targetPath.points[0].y);
            }
        }
        public void ClearTarget()
        {
            //Server.Debug("Clearing target");
            if (targetPlayer == firingAt)
                firingAt = null;
            targetPlayer = null;
            targetType = AIController.TargetType.None;
            targetPos = Vec2.Zero;
            targetPath = null;
            nextPathNode = Vec2.Zero;
        }
        public void Login()
        {
            Server.Debug(ip + " logged in as " + name);
            new Thread(delegate()
            {
                int addedFrame = 0;
                server.AddTask(delegate()
                {
                    Inventory = server.gameMode.GetPlayerInventory(this);
                    server.AddPlayer(this);
                    addedFrame = entity.addedFrame;
                    lock (this)
                    {
                        Monitor.Pulse(this);
                    }
                });
                lock (this)
                {
                    Monitor.Wait(this);
                }
                server.WaitForFrame(addedFrame);
                server.AddTask(delegate()
                {
                    levelLoaded = true;
                    playersLoaded = true;
                    SendAddPlayer(this);
                    server.SendAddPlayer(this);
                    this.ready = true;
                    server.gameMode.PlayerAdded(this);
                    server.SendScores(this);
                });
            }).Start();
        }
        public override void Disconnect()
        {
            try
            {
                if (connected)
                {
                    connected = false;
                    if (name != null)
                        Server.Debug(name + " disconnected");
                    else
                        Server.Debug(ip + " disconnected");
                    levelLoaded = false;
                    server.AddTask(delegate()
                    {
                        server.RemovePlayer(this);
                    });
                }
            }
            catch (Exception ex)
            {

            }
        }
        public override void SendServerStatus()
        {
        }
        public override void SendDisconnect(string message)
        {
        }
        public override void SendLevelChanging()
        {
        }
        public override void SendLoginResponse()
        {
        }
        public override void SendPing()
        {
        }
        public override void SendPingTimes()
        {
        }
        public override void SendAddPlayer(Player p)
        {
        }
        public override void SendAmmo(int weaponIndex, int clips, int clipAmmo)
        {
        }
        public override void SendEntityCreate(int frame, int id, Vexillum.Entities.Entity e)
        {
        }
        public override void SendWeaponFire(ServerPlayer p, int idx)
        {
        }
        public override void SendWeaponSelect(ServerPlayer p, int idx)
        {
        }
        public override void SendEntityAngles(List<Vexillum.Entities.Entity> entities)
        {
        }
        public override void SendEntityPositions(List<Vexillum.Entities.Entity> entities)
        {
        }
        public override void SendEntityRemove(int frame, Vexillum.Entities.Entity e)
        {
        }
        public override void SendEntityVelocities(List<Vexillum.Entities.Entity> entities)
        {
        }
        public override void SendProjectile(int frame, int id, Vexillum.Entities.Projectile e, float angle, Vexillum.Entities.Entity owner)
        {
        }
        public override void Teleport(Vexillum.util.Vec2 position)
        {
            entity.Position = position;
        }
        public override void SendClassChange(Player p, PlayerClass c)
        {
        }
        public override void SendExplode(int frame, int x, int y, int radius, int randomSeed, bool nonlethal)
        {
        }
        public override void SendFrame()
        {
        }
        public override void SendHitscan(Vexillum.Entities.HumanoidEntity source)
        {
        }
        public override void SendGameModeByte(int param, byte value)
        {
        }
        public override void SendGameModeShort(int param, short value)
        {
        }
        public override void SendGameModeString(int param, string value)
        {
        }
        public override void SendGrapplingHook(int frame, Player p, Vexillum.Entities.Entity hook)
        {
        }
        public override void SendMessage(int messageID, string[] args, int time)
        {
        }
        public override void SendMessage(string msg)
        {
        }
        public override void SendPlayerHealth(Player p, float health)
        {
        }
        public override void SendScore(Player p, int score)
        {
        }
        public override void SendSound(int frame, int id, Vexillum.Entities.Entity sourceEntity)
        {
        }
        public override bool CheckPingTime()
        {
            return true;
        }
        public void FireWeapon()
        {
            if (entity.Weapon.MouseDownServer(server.level, entity.ArmAngle, 0, 0, 0))
            {
                server.SendWeaponFire(this, this, WeaponIndex);
                if (entity.Weapon is Sword || entity.Weapon is SMG)
                {
                    server.DoHitscan(this, entity.Weapon.GetPivot(), entity.ArmAngle, entity.Weapon, server.level.frame - 1);
                }
            }
        }
        public override void SelectWeapon(int index)
        {
            if (WeaponIndex != index)
            {
                base.SelectWeapon(index);
                server.SendWeaponSelect(this, this, index);
            }
        }
    }
}
