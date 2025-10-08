using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.util;

namespace Vexillum.ui
{
    class ProgressBar
    {
        private int x;
        private int y;
        private Vec2 pos;
        private int font;

        private Texture2D texture;
        private Rectangle bgTexture;
        private Rectangle barTexture;

        private Rectangle bg;
        private Rectangle bar;

        private bool showNumber;

        private int width;
        private int totalWidth;
        private string valueString;
        private Vector2 stringSize;

        private float value;
        private int valueDiff;
        private int maxValue;
        private int targetValue;
        public ProgressBar(int x, int y, int font, Texture2D texture, Rectangle bar, Rectangle bg, int width, int maxValue, bool showNumber)
        {
            this.x = x;
            this.y = y;
            pos = new Vec2(x, y);
            this.font = font;

            this.texture = texture;
            this.barTexture = bar;
            this.bgTexture = bg;
            this.totalWidth = width;
            this.maxValue = maxValue;
            this.showNumber = showNumber;

            this.bg = new Rectangle(x, y, width, 16);
            SetValue(0);
        }
        public void SetValue(int v)
        {
            //value = v;
            Update();
            targetValue = v;
            valueDiff = (int) Math.Abs(value - v);
            valueString = "" + v;
            stringSize = TextRenderer.MeasureString(font, valueString).XNAVec;
        }
        public void SetMaxValue(int v)
        {
            maxValue = v;
            Update();
        }

        private void Update()
        {
            width = (int)((float)value / maxValue * totalWidth);

            if (stringSize.X + 4 > width)
                width = (int)stringSize.X + 4;
            bar = new Rectangle(x, y, width, 16);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (value < targetValue)
            {
                if (value + valueDiff > targetValue)
                    value = targetValue;
                else
                    value += valueDiff / 8f;
                Update();
            }
            else if (value > targetValue)
            {
                if (value - valueDiff > targetValue)
                    value = targetValue;
                else
                    value -= valueDiff / 8f;
                Update();
            }
            spriteBatch.Draw(texture, bg, bgTexture, Color.White);
            spriteBatch.Draw(texture, bar, barTexture, Color.White);

            TextRenderer.DrawString(spriteBatch, font, valueString, new Vec2(x + width - (int) stringSize.X - 2, y - 2), Color.White, false);
        }
    }
}
