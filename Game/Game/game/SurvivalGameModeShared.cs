using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.Entities;
using Vexillum.Entities.Weapons;
using Vexillum.view;
using Vexillum.ui;
using Vexillum.util;
using Vexillum.game;
using Microsoft.Xna.Framework.Graphics;

namespace Vexillum.Game
{
    public abstract class SurvivalGameModeShared : IGameMode
    {
        public int maxCaptures = 4;
        public int respawnTime = 5000;

        public int numPlayers = 0;
        public int numGreen = 0;
        public int numBlue = 0;
        public int greenCaptures = 0;
        public int blueCaptures = 0;
        public Player greenFlagCarrier;
        public Player blueFlagCarrier;
        private Random rand = new Random();

        public static Texture2D ctfTexture;
        public static Rectangle? greenFlag = null;
        public static Rectangle? blueFlag = null;

        static SurvivalGameModeShared()
        {
            if (!Util.IsServer)
            {
                greenFlag = new Rectangle(0, 0, 22, 30);
                blueFlag = new Rectangle(26, 0, 22, 30);
            }
        }

        public override PlayerClass GetSpawnClass()
        {
            Util.Debug(numGreen + " " + numBlue);
            if (numGreen == numBlue)
                return rand.NextDouble() < 0.5 ? PlayerClass.Blue : PlayerClass.Green;
            else if (numGreen < numBlue)
                return PlayerClass.Green;
            else
                return PlayerClass.Blue;
        }

        public override void PlayerAdded(Player p)
        {
            numPlayers++;
            UpdatePlayerList();
        }

        public override void PlayerRemoved(Player p)
        {

            numPlayers--;
            UpdatePlayerList();
        }

        public abstract void UpdatePlayerList();

        public override void SetClass(Player p, PlayerClass pClass)
        {
            if(p.CurrentClass == pClass)
                return;
            p.CurrentClass = pClass;
            if (p.Entity != null)
            {
                p.Entity.SetType(HumanoidTypes.GetType(pClass));
            }
        }

        protected bool IsGreenClass(PlayerClass c)
        {
            return c == PlayerClass.Green;
        }

        protected bool IsBlueClass(PlayerClass c)
        {
            return c == PlayerClass.Blue;
        }

        public override Vec2 GetSpawnPosition(PlayerClass playerClass, LivingEntity playerEntity)
        {
            return level.GetSpawnPosition(playerClass, playerEntity.Size);
        }

        public override HumanoidEntity GetPlayerEntity(Player p)
        {
            p.PlayerClass = GetSpawnClass();
            SetClass(p, p.PlayerClass);
            HumanoidEntity e = HumanoidTypes.CreateHumanoid(p.PlayerClass);
            e.Position = GetSpawnPosition(p.PlayerClass, e);
            return e;
        }

        public override Weapon[] GetPlayerInventory(Player p)
        {
            if (p == null || p.CurrentClass != PlayerClass.Spectator)
            {
                Weapon[] inv = new Weapon[WeaponParameters.weaponNames.Length];
                for (int i = 0; i < inv.Length; i++)
                {
                    inv[i] = (Weapon) Activator.CreateInstance(Type.GetType("Vexillum.Entities.Weapons." + WeaponParameters.weaponNames[i]));
                }
                return inv;
            }
            else
                return new Weapon[] { };
        }

        public override string GetDisplayName(Player p)
        {
            switch (p.CurrentClass)
            {
                case PlayerClass.Green:
                    return TextUtil.COLOR_GREEN + p.name;
                case PlayerClass.Blue:
                    return TextUtil.COLOR_BLUE + p.name;
                default:
                    return TextUtil.COLOR_GRAY + p.name;
            }
        }

        public override Color GetNameColor(Player p)
        {
            switch (p.CurrentClass)
            {
                case PlayerClass.Green:
                    return TextRenderer.colors[8];
                case PlayerClass.Blue:
                    return TextRenderer.colors[6];
                default:
                    return TextRenderer.colors[4];
            }
        }

        public PlayerClass GetEnemyClass(PlayerClass cl)
        {
            if (cl == PlayerClass.Green)
                return PlayerClass.Blue;
            else
                return PlayerClass.Green;
        }

        public override bool CanDamage(Player p1, Player p2)
        {
            return p1 == p2 || p1.CurrentClass != p2.CurrentClass;
        }
    }
}
