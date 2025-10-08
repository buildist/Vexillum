using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.Entities
{
    public class PixelEntity : BasicEntity
    {
        private static Texture2D crate;
        static PixelEntity()
        {
            crate = AssetManager.loadTexture("pixel.png");
        }
        public PixelEntity()
        {
            Texture = crate;
            Size = new Vec2(1, 1);
            enablePhysics = false;
        }
    }
}
