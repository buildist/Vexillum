using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.ui
{
    class AmmoIndicator
    {
        private Texture2D texture;
        private Rectangle[] rects = new Rectangle[24];
        private Rectangle backgroundRectangle;
        private Rectangle foregroundRectangle;
        private Rectangle drawRectangle;
        private Vec2 position;
        private Vec2 middle;

        public AmmoIndicator(Texture2D texture, int x, int y)
        {
            this.texture = texture;
            backgroundRectangle = new Rectangle(4, 186, 29, 29);
            foregroundRectangle = new Rectangle(33, 186, 29, 29);
            drawRectangle = new Rectangle(x, y, 29, 29);
            position = new Vec2(x, y);
            middle = position + new Vec2(14, 14);

            int px = 62;
            int py = 186;
            for (int i = 0; i < 24; i++)
            {
                rects[i] = new Rectangle(px, py, 29, 29);
                px += 29;
                if (i == 11)
                {
                    px = 62;
                    py += 29;
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch, int totalAmmo, int maxAmmo, int currentClipAmmo, int maxClipAmmo, Color color)
        {
            spriteBatch.Draw(texture, drawRectangle, backgroundRectangle, Color.White);
            if (currentClipAmmo > 0)
            {
                int idx = (int)Math.Round(((float)currentClipAmmo / maxClipAmmo) * 360 / 15) - 1;
                spriteBatch.Draw(texture, drawRectangle, rects[idx >= 0 ? idx : 0], color);
            }
            spriteBatch.Draw(texture, drawRectangle, foregroundRectangle, Color.White);
            string ammoString = "" + totalAmmo;
            Vec2 textPos = Util.Round(middle - TextRenderer.MeasureString(TextRenderer.DefaultFont, ammoString) / 2);
            TextRenderer.DrawString(spriteBatch, TextRenderer.DefaultFont, ammoString, textPos, Color.White, false);
        }
    }
}
