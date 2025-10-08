using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.net;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.util;
using Microsoft.Xna.Framework;

namespace Vexillum.view
{
    static class DebugOverlay
    {
        private const int x = 50;
        private static int y;
        public static void Draw(SpriteBatch s, GameView v, Level l, Client c)
        {
            y = 100;
            //l.DrawCollisionBoxes(v, s);
            DrawLine(s, "Frame: " + l.frame + "("+l.frameDiff+")");
            DrawLine(s, "Entity: "+l.visibleEntities+" / "+l.getEntities().Count);
            DrawLine(s, "Position: " + v.CamTarget);
        }
        private static void DrawLine(SpriteBatch s, string text)
        {
            TextRenderer.DrawString(s, TextRenderer.TinyFont, text, new Vec2(x, y), Color.White, true);
            y += 16;
        }
    }
}
