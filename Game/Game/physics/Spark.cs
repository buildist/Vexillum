using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.util;

namespace Vexillum.physics
{
    class Sparks : ParticleSystem
    {
        private static Texture2D[] images;
        private static Vec2 down = new Vec2(0f, -0.04f);
        static Sparks()
        {
            images = new Texture2D[] { AssetManager.loadTexture("spark1.png"), AssetManager.loadTexture("spark2.png"), AssetManager.loadTexture("spark3.png"), AssetManager.loadTexture("spark4.png") };
        }
        public Sparks(Level l, Vec2 position) : base(position, (float) Math.PI/2, 1.5f, down, 1.5f, (float) Math.PI/2, 1000)
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
