using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace  Vexillum.Entities
{
    public class CrateEntity : BasicEntity
    {
        private static Texture2D crate;
        static CrateEntity()
        {
            crate = AssetManager.loadTexture("crate.png");
        }
        public CrateEntity()
        {
            Texture = crate;
            Size = new Vec2(16, 16);
            enablePhysics = false;
        }
    }
}
