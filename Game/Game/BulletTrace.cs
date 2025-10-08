using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.util;

namespace Vexillum
{
    class BulletTrace
    {
        private Vec2 v1;
        private Vec2 v2;
        private Vec2 offset = Vec2.Zero;
        private Vec2 unit;

        private float angle;
        private float length;

        private int frames = 0;
        public bool deleted = false;
        public BulletTrace(Vec2 point1, Vec2 point2)
        {
            Random r = new Random();

            int totalLengthX = (int) (point2.X - point1.X);
            int totalLengthY = (int) (point2.Y - point1.Y);

            float ratio = (float)(r.NextDouble() * 0.25+0.25);
            float ratio2 = (float) r.NextDouble();

            int lengthX = (int) (totalLengthX * ratio);
            int lengthY = (int) (totalLengthY * ratio);

            int offsetX = (int) ((totalLengthX - lengthX) * ratio2);
            int offsetY = (int) ((totalLengthY - lengthY) * ratio2);

            v1 = new Vec2(point1.X + offsetX, point1.Y +offsetY);
            v2 = v1 + new Vec2(lengthX, lengthY);
            unit = new Vec2(v2.X - v1.X, v1.Y - v2.Y);
            unit.Normalize();
            angle = (float) Math.Atan2(unit.Y, unit.X);
            length = Vec2.Distance(v1, v2);
        }

        public void Draw(GameView view, Level level, SpriteBatch s)
        {
            s.Draw(GraphicsUtil.pixel, (new Vec2(v1.X - view.CamStart.X, view.CamStart.Y - v1.Y)+offset).XNAVec, null, Colors.bulletTraceColor, angle, Vec2.Zero.XNAVec, new Vec2(length, 2).XNAVec, SpriteEffects.None, 0);
            offset += unit*5;
            frames++;
            if (frames == 3)
                deleted = true;
        }
    }
}
