using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.Entities;
using Vexillum.util;

namespace Vexillum.physics
{
    class Fire : ParticleSystem
    {
        private static Texture2D[] images;
        private Entity entity;
        static Fire()
        {
            images = new Texture2D[] { AssetManager.loadTexture("fire1.png")};
        }
        public Fire(Entity entity) : base(Vec2.Zero, 0, 1f, Vec2.Zero, 0.5f, (float) Math.PI, 300)
        {
            this.entity = entity;
        }
        protected override Texture2D getImage()
        {
            return images[level.random.Next(images.Count())];
        }
        protected override Vec2 getPosition()
        {
            Vec2 v = new Vec2((float)Math.Cos(entity.Rotation), (float)-Math.Sin(entity.Rotation)) * (entity.HalfSize.X - 10);
            return entity.Position + v;
        }
    }
}
