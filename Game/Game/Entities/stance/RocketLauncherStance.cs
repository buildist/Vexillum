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
    class RocketLauncherStance : BasicStance
    {
        public RocketLauncherStance(Weapon weapon, HumanoidEntity entity)
            : base(weapon, entity)
        {
            Init(1, 13);
            AddFrame(0, 6, 5, 21, 22, 22, 2, 12, 0);
            AddFrame(0, 7, 30, 21, 23, 22, 2, 13, 0);
            AddFrame(0, 8, 55, 21, 22, 22, 2, 12, 0);
            AddFrame(0, 9, 79, 21, 21, 22, 2, 11, 0);
            AddFrame(0, 10, 101, 21, 19, 24, 2, 9, 2);
            AddFrame(0, 11, 121, 21, 17, 25, 2, 7, 3);
            AddFrame(0, 12, 140, 21, 16, 26, 2, 6, 4);
            AddFrame(0, 5, 30, 52, 23, 22, 2, 13, 0);
            AddFrame(0, 4, 54, 52, 22, 22, 2, 12, 0);
            AddFrame(0, 3, 78, 52, 21, 22, 2, 11, 0);
            AddFrame(0, 2, 101, 52, 18, 23, 2, 8, 1);
            AddFrame(0, 1, 121, 52, 18, 23, 2, 8, 1);
            AddFrame(0, 0, 141, 52, 15, 23, 2, 5, 1);
            if (!Util.IsServer)
                SetCrosshair(new Rectangle(61, 4, 7, 7), new Rectangle(60, 3, 9, 9));
        }
        public override void Fire()
        {

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
