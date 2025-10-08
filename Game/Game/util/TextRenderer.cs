using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
namespace Vexillum.util
{
    public class TextRenderer
    {
        private const int nFonts = 4;
        public const int DefaultFont = 0;
        public const int TitleFont = 1;
        public const int FancyFont = 2;
        public const int TinyFont = 3;
        private static SpriteFont[] fonts = new SpriteFont[nFonts];
        private static int[] lineHeights = new int[nFonts];
        public static Color[] colors = new Color[] { 
            Color.Black,
            Color.White,
            new Color(128, 64, 32),
            new Color(49, 70, 43),
            new Color(120, 120, 120),
            new Color(195, 40, 40),
            new Color(40, 40, 195),
            new Color(146, 40, 195),
            new Color(40, 195, 73),
            new Color(228, 135, 40)
        };
        private static System.Drawing.Brush[] brushes = new System.Drawing.Brush[colors.Length];
        private static Color shadowColor = new Color(0, 0, 0, 160);
        private static readonly Vec2 shadowOffset = new Vec2(1, 1);
        private const char colorChar = TextUtil.colorChar;
        private static char[] charArray = new char[] { colorChar };
        private static readonly Regex colorRegex = new Regex(colorChar+"[0-9]", RegexOptions.CultureInvariant);

        public static void LoadFonts(ContentManager content)
        {
            fonts[DefaultFont] = content.Load<SpriteFont>("DefaultFont");
            fonts[TitleFont] = content.Load<SpriteFont>("TitleFont");
            fonts[FancyFont] = content.Load<SpriteFont>("FancyFont");
            fonts[TinyFont] = content.Load<SpriteFont>("TinyFont");
            for (int i = 0; i < nFonts; i++)
            {
                lineHeights[i] = (int) fonts[i].MeasureString("Test").Y;
            }
            for (int i = 0; i < colors.Length; i++)
            {
                System.Drawing.Color c = System.Drawing.Color.FromArgb(colors[i].R, colors[i].G, colors[i].B);
                brushes[i] = new System.Drawing.SolidBrush(c);
            }
        }
        public static void DrawFormattedString(SpriteBatch spriteBatch, int fontID, string str, Vec2 pos)
        {
            DrawFormattedString(spriteBatch, fontID, str, pos, 255, false, 0, 0);
        }
        public static void DrawFormattedString(SpriteBatch spriteBatch, int fontID, string str, Vec2 pos, bool shadow)
        {
            DrawFormattedString(spriteBatch, fontID, str, pos, 255, true, 0, 0);
        }
        public static string WordWrap(string str, int fontID, int width)
        {
            char[] chars = str.ToCharArray();
            StringBuilder r = new StringBuilder();
            StringBuilder line = new StringBuilder();
            StringBuilder word = new StringBuilder();
            int lineWidth = 0;
            int wordWidth = 0;
            int spaceWidth = (int) MeasureString(fontID, " ").X;
            for (int i = 0; i < chars.Length; i++)
            {
                word.Append(chars[i]);
                if(chars[i] == ' ')
                {
                    wordWidth = (int) MeasureString(fontID, RemoveColors(word.ToString())).X;
                    if(lineWidth + (int) wordWidth > width && lineWidth > 0)
                    {
                        r.Append(line).Append('\n');
                        line.Remove(0, line.Length);
                        lineWidth = 0;
                    }

                    line.Append(word);
                    lineWidth += (int) wordWidth + spaceWidth;
                    word.Remove(0, word.Length);
                }
            }

            if (word.Length > 0)
            {
                wordWidth = (int)MeasureString(fontID, RemoveColors(word.ToString())).X;
                if (wordWidth + lineWidth > width && line.Length > 0)
                {
                    r.Append(line.Append('\n'));
                    line.Remove(0, line.Length);
                }
                line.Append(word);
            }
            if (line.Length > 0)
                r.Append(line.Append('\n'));
            return r.ToString().Substring(0, r.Length-1);
        }
        public static int DrawFormattedString(SpriteBatch spriteBatch, int fontID, string str, Vec2 pos, int alpha, bool shadow, int wrapWidth, int padding)
        {
            int height;
            int startX = (int) pos.X;
            SpriteFont font = fonts[fontID];
            bool doAlpha = alpha < 255;
            int idx = 0;
            char[] a = str.ToCharArray();
            int colorIndex;
            Color color = Color.White;
            int cx = startX;
            int cy = (int) pos.Y;
            int charHeight = lineHeights[fontID];
            height = charHeight + padding;
            int lineCount = str.Count(f => f == '\n');
            cy -= lineCount * (charHeight + padding);
            while (idx < str.Length)
            {
                char c = a[idx];
                switch (c)
                {
                    case colorChar:
                        if (idx + 1 < str.Length)
                        {
                            if (Char.IsNumber(a[idx + 1]))
                            {
                                colorIndex = int.Parse("" + a[idx + 1]);
                                if (colorIndex < colors.Length)
                                {
                                    color = colors[colorIndex];
                                    if (doAlpha)
                                        color = new Color(color.R, color.G, color.B, alpha);
                                }
                            }
                        }
                        idx++;
                        break;
                    case '\n':
                        cx = startX;
                        cy += padding + charHeight;
                        height += padding + charHeight;
                        break;
                    default:
                        if(shadow)
                            spriteBatch.DrawString(font, "" + c, new Vec2(cx+1, cy+1).XNAVec, shadowColor);
                        spriteBatch.DrawString(font, ""+c, new Vec2(cx, cy).XNAVec, color);
                        cx += (int)MeasureString(fontID, ""+c).X;
                        break;
                }
                idx++;
            }
            return height;
        }
        public static int DrawFormattedString(System.Drawing.Graphics g, System.Drawing.Font font, string str, Vec2 pos, int alpha, bool shadow, int wrapWidth, int padding)
        {
            int height;
            int startX = (int)pos.X;
            bool doAlpha = alpha < 255;
            int idx = 0;
            char[] a = str.ToCharArray();
            int colorIndex;
            System.Drawing.Brush color = System.Drawing.Brushes.White;
            int cx = startX;
            int cy = (int)pos.Y;
            int charHeight = lineHeights[DefaultFont];
            height = charHeight + padding;
            int lineCount = str.Count(f => f == '\n');
            cy -= lineCount * (charHeight + padding);
            while (idx < str.Length)
            {
                char c = a[idx];
                switch (c)
                {
                    case colorChar:
                        if (idx + 1 < str.Length)
                        {
                            if (Char.IsNumber(a[idx + 1]))
                            {
                                colorIndex = int.Parse("" + a[idx + 1]);
                                if (colorIndex < colors.Length)
                                {
                                    color = brushes[colorIndex];
                                }
                            }
                        }
                        idx++;
                        break;
                    case '\n':
                        cx = startX;
                        cy += padding + charHeight;
                        height += padding + charHeight;
                        break;
                    default:
                        if (shadow)
                            g.DrawString("" + c, font, color, new System.Drawing.PointF(cx+1, cy+1));
                        g.DrawString("" + c, font, color, new System.Drawing.PointF(cx, cy));
                        cx += (int)MeasureString(TextRenderer.DefaultFont, "" + c).X;
                        break;
                }
                idx++;
            }
            return height;
        }
        public static void DrawString(SpriteBatch spriteBatch, int fontID, string str, Vec2 pos, Color color, bool shadow)
        {
            SpriteFont font = fonts[fontID];
            if (shadow)
                spriteBatch.DrawString(font, str, (pos + shadowOffset).XNAVec, shadowColor);
            spriteBatch.DrawString(font, str, pos.XNAVec, color);
        }
        public static Vec2 MeasureString(int fontID, string str)
        {
            SpriteFont font = fonts[fontID];
            return new Vec2(font.MeasureString(str));
        }
        public static int GetLineHeight(int fontID)
        {
            return lineHeights[fontID];
        }
        public static string RemoveColors(string str)
        {
            return colorRegex.Replace(str, "");
        }
    }
}
