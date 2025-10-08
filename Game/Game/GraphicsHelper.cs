using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Vexillum
{
    public class GraphicsUtil
    {
        public static readonly Texture2D pixel = AssetManager.loadTexture("pixel_white.png");
        public static readonly Texture2D pixelBlack = AssetManager.loadTexture("pixel.png");
        public static System.Drawing.Color transparent = System.Drawing.Color.Transparent;
    }
}
