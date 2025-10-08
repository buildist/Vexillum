using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using Vexillum.Entities.Weapons;
using Vexillum.Game;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.game
{
    class MenuAI
    {
        private Level level;
        private Agent greenPlayer;
        private Agent bluePlayer;
        private Vec2 groundOffset;
        public MenuAI(Level l)
        {
            level = l;
            
            bluePlayer = AddPlayer(new Vec2(740, 30), Game.PlayerClass.Blue);
            greenPlayer = AddPlayer(new Vec2(100, 30), Game.PlayerClass.Green);
            groundOffset = new Vec2(0, -20);
        }
        private Agent AddPlayer(Vec2 position, Game.PlayerClass cl)
        {
            HumanoidEntity e= HumanoidTypes.CreateHumanoid(Game.PlayerClass.Green);
            NetworkPlayer pl = new NetworkPlayer("", e);
            pl.Inventory = new Weapon[] { new RocketLauncher(), new SMG(), new Sword() };
            pl.SelectWeapon(0);
            e.SetReady(true);
            e.Weapon = new RocketLauncher();
            e.Position = new Vec2(30, 100);
            level.AddEntity(e);
            Agent a = new Agent();
            a.player = pl;
            a.entity = e;
            return a;
        }
        public void Step(int gameTime)
        {
            Think(gameTime, greenPlayer, bluePlayer);
            Think(gameTime, bluePlayer, greenPlayer);
        }
        public void Think(int gameTime, Agent playerAgent, Agent enemyAgent)
        {
            HumanoidEntity player = playerAgent.entity;
            HumanoidEntity enemy = enemyAgent.entity;
            Vec2 distance = enemy.Position + groundOffset - player.Position;
            float length = distance.Length();
            playerAgent.player.SelectWeapon(0);
            ReloadableWeapon weapon = (ReloadableWeapon)player.Weapon;
            Vec2 unitDistance = distance;
            unitDistance.Normalize();
            if (Math.Abs(distance.X) < 20)
            {
                player.xVelocity = 0;
            }
            else if (unitDistance.X < 0)
            {
                player.xVelocity = -player.Speed;
            }
            else if (unitDistance.X > 0)
            {
                player.xVelocity = player.Speed;
            }
            float angle = (float) (Math.Atan2(-unitDistance.Y, unitDistance.X));
            player.ArmAngle = angle;
            if (weapon.clipAmmo == 0)
                weapon.Reload();
            //else
                //((ReloadableWeapon)player.Weapon).MouseDownMenu(level, angle, 0, 0, 0);
            player.Weapon.Step(null, weapon.GetPivot(), angle, gameTime);
        }
        public class Agent
        {
            public HumanoidEntity entity;
            public NetworkPlayer player;
        }
    }
}
