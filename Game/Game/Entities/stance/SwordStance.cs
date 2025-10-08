using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.util;

namespace  Vexillum.Entities.stance
{
    class SwordStance : BasicStance
    {
        public bool shouldFire;
        private float angleStep;
        private int angleStepCount = 0;
        private int angleChangeCount = 0;
        private bool angleDirection;

        private static Sounds[] sounds = new Sounds[] { Sounds.SWORD1, Sounds.SWORD2, Sounds.SWORD3 };
        private RandomSoundPlayer soundPlayer;
        public SwordStance(Weapon weapon, HumanoidEntity entity)
            : base(weapon, entity)
        {
            Init(1, 13);
            AddFrame(0, 6, 30, 129, 37, 22, 2, 27, 0);
            AddFrame(0, 7, 68, 129, 36, 22, 2, 26, 0);
            AddFrame(0, 8, 105, 129, 33, 29, 2, 23, 7);
            AddFrame(0, 9, 139, 129, 29, 35, 2, 19, 13);
            AddFrame(0, 10, 170, 129, 22, 39, 2, 12, 17);
            AddFrame(0, 11, 196, 129, 16, 41, 2, 6, 19);
            AddFrame(0, 12, 214, 129, 12, 42, 2, 2, 20);
            AddFrame(0, 5, 28, 176, 35, 22, 2, 25, -0);
            AddFrame(0, 4, 65, 172, 32, 26, 2, 22, -4);
            AddFrame(0, 3, 101, 166, 28, 32, 2, 18, -10);
            AddFrame(0, 2, 133, 163, 21, 34, 2, 11, -13);
            AddFrame(0, 1, 157, 170, 14, 38, 2, 4, -16);
            AddFrame(0, 0, 174, 168, 12, 40, 2, 2, -18);
            if (!Util.IsServer)
                SetCrosshair(new Rectangle(61, 4, 7, 7), new Rectangle(60, 3, 9, 9));
            soundPlayer = new RandomSoundPlayer(sounds);
        }
        public override void Fire()
        {
            shouldFire = true;
            angleStepCount = 0;
            overrideRotation = true;
            angleChangeCount = 0;
            angle = entity.ArmAngle;
            angleDirection = facingDirection;
            angleStep = (angleDirection ? -1 : 1) * (float)Math.PI / 24;

            soundPlayer.PlaySound(entity.Level, entity);
        }

        public override bool Update(float angle)
        {
            if (overrideRotation)
            {
                if (angleStepCount > 4 && angleChangeCount == 0)
                {
                    angleStepCount = 0;
                    angleStep = (angleDirection ? 1 : -1) * (float)Math.PI / 6;
                    angleChangeCount = 1;
                }
                else if (angleStepCount > 3 && angleChangeCount == 1)
                {
                    angleStepCount = 0;
                    angleStep = (angleDirection ? -1 : 1) * (float)Math.PI / 12;
                    angleChangeCount = 2;
                }
                else if (angleStepCount > 3 && angleChangeCount == 2)
                {
                    overrideRotation = false;
                }
                this.angle = Util.NormalizeAngle(this.angle + angleStep);
                angle = this.angle;
                angleStepCount++;
            }
            return base.Update(angle);
        }

        public override Vec2 GetEnd(float angle)
        {
            return entity.Position + GetOffset() + new Vec2((float)Math.Cos(angle), -(float)Math.Sin(angle)) * weapon.HalfSize.X;
        }

        public override Vec2 GetCrosshairPosition(Vec2 drawPosition, float angle)
        {
            return drawPosition - GetOffset() + new Vec2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50 - crosshairSize;
        }

        public override Vec2 GetScreenPivot(view.GameView view)
        {
            return entity.GetScreenPosition() - GetOffset();
        }

        public override Vec2 GetPivot()
        {
            return GetOffset() + entity.Position;
        }
    }
}
