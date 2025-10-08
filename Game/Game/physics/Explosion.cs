using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.util;

namespace Vexillum.physics
{
    class Explosion : ParticleSystem
    {
        private static Texture2D[] images;
        static Explosion()
        {
            images = new Texture2D[] { AssetManager.loadTexture("explode1.png"), AssetManager.loadTexture("explode2.png"), AssetManager.loadTexture("explode3.png") };
        }
        public Explosion(Level l, Vec2 position) : base(position, 0, 1f, Vec2.Zero, 1f, (float) Math.PI, 500)
        {
            maxParticles = 15;
        }
        protected override Texture2D getImage()
        {
            return images[level.random.Next(images.Count())];
        }
        protected virtual float getScale(int time)
        {
            return 1 - (float)time / lifetime;
        }
    }
}
