using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.view;
using Vexillum.util;

namespace Vexillum.physics
{
    public class Particle
    {
        private Texture2D image;
        public Color color = Color.White;
        public Vec2 center;
        public Vec2 position;
        public Vec2 velocity;
        public int origin;
        public float scale = 1;
        public float angle = 0;
        private Level level;
        public Particle(Level level, Vec2 p, Vec2 v)
        {
            position = p;
            velocity = v;
            origin = level.GetTime();
            this.level = level;
            angle = (float) (level.random.NextDouble() * Math.PI * 2);
        }
        public void setImage(Texture2D i)
        {
            image = i;
            center = new Vec2(i.Width, i.Height) / 2;
        }
        public void draw(ParticleSystem system, GameView view, GraphicsDevice g, SpriteBatch spriteBatch)
        {
            Vec2 pos = new Vec2((int)(position.X - view.CamStart.X), (int)(view.CamStart.Y - position.Y));
            spriteBatch.Draw(image, pos.XNAVec, null, color, angle, center.XNAVec, scale, SpriteEffects.None, 0);
        }
    }
}
