using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.util;

namespace  Vexillum.Entities.stance
{
    abstract class BasicStance : Stance
    {
        private Rectangle currentRect;
        private int currentYOffset;
        private int currentRightOffset;
        private int currentLeftOffset;
        protected bool facingDirection;
        protected bool overrideRotation = false;
        protected float angle;
        protected BasicStance(Weapon weapon, HumanoidEntity entity)
            : base(weapon, entity)
        {

        }
        public override bool Update(float angle)
        {
            facingDirection = angle < Math.PI / 2 && angle > -Math.PI / 2;

            int a = Util.RoundToMultiple(Util.Deg(angle) + 90, 15);
            a /= 15;
            if (a >= 12)
                a = 12 - (a - 12);
            else if (a <= 0)
            {
                a = 12 - (a + 12);
            }

            currentRect = frames[0, a];
            currentRightOffset = rightXOffsets[0, a];
            currentLeftOffset = leftXOffsets[0, a];
            currentYOffset = yOffsets[0, a];

            return facingDirection;
        }

        public override int GetLeftXOffset()
        {
            return currentLeftOffset;
        }
        public override int GetRightXOffset()
        {
            return currentRightOffset;
        }
        public override int GetYOffset()
        {
            return currentYOffset;
        }
        public override int GetHeight()
        {
            return currentRect.Height;
        }

        public override int GetWidth()
        {
            return currentRect.Width;
        }

        public Vec2 GetOffset()
        {
            return new Vec2(0, 8);
        }


        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Rectangle drawRectangle)
        {
            SpriteEffects effect = facingDirection ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawRectangle, currentRect, Color.White, 0, Vec2.Zero.XNAVec, effect, 1);
        }
    }
}
