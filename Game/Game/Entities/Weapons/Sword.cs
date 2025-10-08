using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.Entities.stance;
using Vexillum.util;
using Vexillum.net;

namespace Vexillum.Entities.Weapons
{
    public class Sword : ReloadableWeapon
    {
        private static Texture2D sword;

        static Sword()
        {
            sword = AssetManager.loadTexture("sword.png");
        }
        public Sword()
            : base(-1, -1, 400, 0)
        {
            Size = new Vec2(21, 6);
            hasClientEffect = true;
            showHitscan = false;
            maxHitscanLengthSquared = 32*32;
            knockback = 8;
        }
        public override Texture2D GetImage()
        {
            return sword;
        }
        public override void FireServer(Level l, float angle)
        {
            Vec2 pos = GetEnd(entity.ArmAngle);
            l.Explode((int)pos.X, (int)pos.Y, 22, true, entity.player, this);
        }

        public override void FireMenu(Level l, float angle)
        {
            //Vec2 pos = GetEnd(entity.ArmAngle);
            //l.Explode((int)pos.X, (int)pos.Y, 22, true);
            //stance.Fire();
        }
        

        public override void FireClient(Client cl, Level l, float angle)
        {
            stance.Fire();
            cl.SendHitscan();
        }
        public override float GetDamage(float distance)
        {
            return 25f;
        }
        public override stance.Stance GetStanceInstance(HumanoidEntity e)
        {
            return new SwordStance(this, e);
        }
    }
}
