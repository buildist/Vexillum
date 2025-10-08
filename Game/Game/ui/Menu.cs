using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.view;
using Vexillum.util;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;

namespace Vexillum.ui
{
    public class Menu
    {
        protected AbstractView view;
        public int type;
        private List<MenuItem> items = new List<MenuItem>();
        private int startX = 30;
        private int startY = 50;
        private int btnX;
        private int btnY;
        private int lineHeight = 50;
        private int totalHeight = 0;
        private MenuItem mouseItem;
        private List<ButtonControl> bottomButtons = new List<ButtonControl>();
        private List<ScrollPanel> panels = new List<ScrollPanel>(2);
        public bool visible = false;
        public Menu(AbstractView view)
        {
            this.view = view;
            btnX = view.windowWidth - 90;
            btnY = view.windowHeight - 37;
        }
        public void AddItem(MenuItem item)
        {
            items.Add(item);
            totalHeight += lineHeight;
        }
        public void AddButton(string text, EventHandler pressed)
        {
            AddButton(text, pressed, btnX, btnY);
            btnX -= 90;
        }
        public void AddButton(string text, EventHandler pressed, int x, int y)
        {
            ButtonControl btn = new ButtonControl();
            btn.Text = text;
            btn.Bounds = new UniRectangle(new UniVector(new UniScalar(0, x), new UniScalar(0, y)), new UniVector(80, 24));
            btn.Pressed += pressed;
            bottomButtons.Add(btn);
        }
        public void AddScrollPanel(ScrollPanel p)
        {
            panels.Add(p);
        }
        public void MouseDown(int x, int y)
        {
            if (visible)
            {
                MenuItem item = GetItem(x, y);
                if (item != null)
                    item.mouseClicked();
                else
                {
                    foreach (ScrollPanel p in panels)
                        p.XMouseDown(x, y);
                }
            }
        }
        public void MouseOver(int x, int y)
        {
            mouseItem = GetItem(x, y);
            foreach (ScrollPanel p in panels)
            {
                p.XMouseMove(x, y);
            }
        }
        public void MouseUp(int x, int y)
        {
            foreach (ScrollPanel p in panels)
                p.XMouseUp(x, y);
        }
        public void MouseDrag(int x, int y)
        {
            foreach (ScrollPanel p in panels)
                p.XMouseDrag(x, y);
        }
        private MenuItem GetItem(int x, int y)
        {
            for (int i = 0; i < items.Count; i++)
            {
                MenuItem item = items.ElementAt(i);
                Rectangle drawRectangle;
                if(type == 1)
                    drawRectangle = new Rectangle(0, 225 + i * lineHeight, 391, lineHeight);
                else
                    drawRectangle = new Rectangle(startX, startY + i * lineHeight, 200 - startX, lineHeight);
                if (drawRectangle.Contains(x, y))
                    return item;
            }
            return null;
        }
        public void Show()
        {
            if (!visible)
            {
                foreach (ButtonControl c in bottomButtons)
                {
                    view.GetScreen().Desktop.Children.Add(c);
                }
                visible = true;
            }
        }
        public void Hide()
        {
            if(visible)
            {
                foreach (ButtonControl c in bottomButtons)
                {
                    view.GetScreen().Desktop.Children.Remove(c);
                }
                visible = false;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            switch(type)
            {
                case 0:
                    spriteBatch.Draw(GraphicsUtil.pixel, new Rectangle(0, view.windowHeight - 50, view.windowWidth, 50), Colors.menuBottomBarColor);
                    spriteBatch.Draw(GraphicsUtil.pixelBlack, new Rectangle(0, 0, 200, view.windowHeight - 50), Colors.menuBackgroundColor);
                    int x = startX;
                    int y0 = startY;
                    foreach (MenuItem item in items)
                    {
                        item.Draw(spriteBatch, new Vec2(x, y0), item == mouseItem ? Colors.menuHighlightColor : Color.White);
                        y0 += lineHeight;
                    }
                    break;
                case 1:
                int y = 225;
                foreach (MenuItem item in items)
                {
                    Vec2 rect = TextRenderer.MeasureString(TextRenderer.FancyFont, item.Text);
                    item.Draw(spriteBatch, new Vec2(391-rect.X, y), item == mouseItem ? Colors.menuHighlightColor : Color.White);
                    y += lineHeight;
                }
                break;
                default:
                foreach (ScrollPanel p in panels)
                {
                    p.Draw(spriteBatch);
                }
                break;
            }
        }
    }
    public delegate void MouseClicked();
    public class MenuItem
    {
        public string Text;
        public MouseClicked mouseClicked;
        public void Draw(SpriteBatch spriteBatch, Vec2 pos, Color color)
        {
            TextRenderer.DrawString(spriteBatch, TextRenderer.FancyFont, Text, pos, color, true);
        }
    }
}
