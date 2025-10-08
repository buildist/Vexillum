using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum
{
    public class TerrainParticle
    {
        public Color color;
        public Vec2 position;
        public Vec2 velocity;
        public int startTime;
        public bool removed;
        public Level level;

        public TerrainParticle(Level l, System.Drawing.Color c, int x, int y)
        {
            this.color = new Color(c.R, c.G, c.B);
            this.level = l;
            position = new Vec2(x, y);
            startTime = level.GetTime();
        }
        public int GetX()
        {
            return (int) position.X;
        }
        public int GetY()
        {
            return (int) position.Y;
        }
        public void step()
        {
            if (level.GetTime() - startTime > 2000 && level.random.NextDouble() < 0.1)
            {
                removed = true;
                return;
            }
            Vec2 newPos = position + velocity;
            float d = (newPos - position).Length();
            if (d == 0)
                d = 0;
            int nx = (int) position.X, ny = (int) position.Y;
            int px = (int) position.X, py = (int) position.Y;
            bool collision = false;
            for (int i = 0; i < d; i++ )
            {
                float k = (float)i / d;
                px = nx;
                py = ny;
                nx = (int)(position.X + (newPos.X - position.X) * k);
                ny = (int)(position.Y + (newPos.Y - position.Y) * k);
                if (level.terrain.GetTerrainOrParticle(nx, ny))
                {
                    collision = true;
                    break;
                }
                else
                {
                }
            }

            if(collision)
                position = new Vec2(px, py);
            else
                position = new Vec2(nx, ny);

            if (collision || level.terrain.GetTerrainOrParticle(nx, ny - 1))
                velocity.Y = 0;
            else
                velocity.Y -= level.Gravity + (float) (level.random.NextDouble() * 0.5 - 0.25);

            bool yCollision = level.terrain.GetTerrainOrParticle(px, py - 1);
            if (yCollision)
            {
                if (!level.terrain.GetTerrainOrParticle(px + 1, py - 1))
                    position += new Vec2(1, -1);
                else if (!level.terrain.GetTerrainOrParticle(px + 1, py - 1))
                    position += new Vec2(-1, -1);
                else
                    velocity.X *= 0.75f;
            }

            if (ny < 0 || ny >= level.Size.Y || nx < 0 || nx >= level.Size.X)
                removed = true;
        }
    }
}
