using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Vexillum.ui
{
    class UISettings
    {
        public static SpriteFont DefaultFont;
        public static void LoadContent(ContentManager content)
        {
            DefaultFont = content.Load<SpriteFont>("GameFont");
        }
    }
}
