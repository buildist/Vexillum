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
    class SMGStance : BasicStance
    {
        public SMGStance(Weapon weapon, HumanoidEntity entity)
            : base(weapon, entity)
        {
            Init(1, 13);
            AddFrame(0, 6, 7, 77, 20, 22, 2, 10, 0);
            AddFrame(0, 7, 29, 77, 20, 22, 2, 10, 0);
            AddFrame(0, 8, 52, 77, 18, 22, 2, 8, 0);
            AddFrame(0, 9, 74, 77, 15, 22, 2, 5, 0);
            AddFrame(0, 10, 95, 77, 14, 24, 2, 4, 2);
            AddFrame(0, 11, 119, 77, 12, 27, 2, 2, 5);
            AddFrame(0, 12, 142, 77, 12, 27, 2, 2, 5);
            AddFrame(0, 5, 29, 104, 20, 22, 2, 10, 0);
            AddFrame(0, 4, 53, 104, 18, 22, 2, 8, 0);
            AddFrame(0, 3, 74, 104, 16, 22, 2, 6, 0);
            AddFrame(0, 2, 96, 104, 14, 22, 2, 4, 0);
            AddFrame(0, 1, 120, 103, 13, 23, 2, 3, -1);
            AddFrame(0, 0, 143, 103, 13, 23, 2, 3, -1);
            if (!Util.IsServer)
                SetCrosshair(new Rectangle(61, 4, 7, 7), new Rectangle(60, 3, 9, 9));
        }
        public override void Fire()
        {

        }

        public override Vec2 GetEnd(float angle)
        {
            return entity.Position + GetOffset(); //new Vec2((float)Math.Cos(angle), -(float)Math.Sin(angle)) * weapon.HalfSize.X;
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
