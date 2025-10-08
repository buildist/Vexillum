using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.Entities;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.util;

namespace Vexillum.Game
{
    public abstract class IGameMode
    {
        public virtual void Command(int c, object value)
        {
            Util.Debug("Warning: GameMode.Command may only be called by the server");
        }
        public abstract void PlayerHealthChanged(Player p, Weapon w);
        public abstract void PlayerHit(Player attacker, Player defender, Weapon weapon, Vec2 hitPos);
        public virtual void ProcessCommand(int c, object value) { }
        public abstract PlayerClass GetSpawnClass();
        public abstract void SetClass(Player p, PlayerClass c);
        public abstract void PlayerAdded(Player p);
        public abstract HumanoidEntity GetPlayerEntity(Player p);
        public abstract Weapon[] GetPlayerInventory(Player p);
        public abstract void PlayerRemoved(Player p);
        public abstract Color GetNameColor(Player p);
        public abstract string GetDisplayName(Player p);
        public abstract Vec2 GetSpawnPosition(PlayerClass playerClass, LivingEntity playerEntity);
        public virtual void DrawUI(GameView view, SpriteBatch spriteBatch, LocalPlayer player) { }
        public virtual void OnFlagCollide(Player p, Entity e) { }
        public virtual void DrawEntity(HumanoidEntity e, SpriteBatch s) { }
        protected Level level;
        public bool gameOver = false;
        public abstract bool CanDamage(Player p1, Player p2);

        public void SetLevel(Level l)
        {
            level = l;
        }
    }
}
