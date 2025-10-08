using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Vexillum.util
{
    public class Region
    {
        public string name;
        public int x1;
        int x2;
        public int y1;
        int y2;
        int width;
        int height;
        public Region(string name, int x1, int y1, int x2, int y2)
        {
            this.name = name;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            width = x2 - x1;
            height = y2 - y1;
        }
        public Vec2 RandomPosition(Random random, Vec2 size, int levelHeight)
        {
            int w = width - (int)size.X;
            int h = height - (int)size.Y;
            return new Vec2(x1 + random.Next(w) + (int)(size.X / 2), levelHeight - (y1 + random.Next(h) + (int)(size.Y / 2)) + 16);
        }
    }
}
