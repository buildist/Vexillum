using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Nuclex.Input;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using Vexillum.ui;

namespace Vexillum.view
{
    public abstract class AbstractView
    {
        protected Screen screen;
        protected Dictionary<Type, WindowControl> openWindows = new Dictionary<Type, WindowControl>(4);
        public Screen GetScreen()
        {
            return screen;
        }
        public int width;
        public int height;
        public int windowWidth;
        public int windowHeight;
        protected int mouseX;
        protected int mouseY;
        protected int windowMouseX;
        protected int windowMouseY;
        public AbstractView(int width, int height)
        {
            this.width = width;
            this.height = height;
            windowWidth = Vexillum.WindowWidth;
            windowHeight = Vexillum.WindowHeight;
            screen = new Screen();
            screen.Width = windowWidth;
            screen.Height = windowHeight;
            screen.Desktop.Bounds = new UniRectangle(
                            new UniScalar(0f, 0.0f), new UniScalar(0f, 0.0f), // x and y = 10%
                            new UniScalar(1f, 0.0f), new UniScalar(1f, 0.0f) // width and height = 80%
                          );
        }
        public void OpenWindow(WindowControl c)
        {
            Type t = c.GetType();
            if (openWindows.ContainsKey(t))
            {
                openWindows[t].Close();
            }
            openWindows[t] = c;
            screen.Desktop.Children.Add(c);
            c.BringToFront();
            if (c.Bounds.Left.ToOffset(windowWidth) + c.Bounds.Size.X.Offset + 10 > windowWidth)
                c.Bounds.Left = windowWidth - c.Bounds.Size.X.Offset - 10;
            if (c.Bounds.Top.ToOffset(windowHeight) + c.Bounds.Size.Y.Offset + 10 > windowHeight)
                c.Bounds.Top = windowHeight - c.Bounds.Size.Y.Offset - 10;

        }
        public abstract void KeyPressed(Keys key, bool isNew);
        public abstract void CharacterEntered(char character);
        public abstract void KeyReleased(Keys key);
        public virtual void MouseMove(float x, float y)
        {
            mouseX = (int)(x/Vexillum.Scale);
            mouseY = (int)(y/Vexillum.Scale);
            windowMouseX = (int)x;
            windowMouseY = (int)y;
            if(menu != null)
                menu.MouseOver(windowMouseX, windowMouseY);
        }
        public abstract void DrawStuff(GraphicsDevice g, SpriteBatch spriteBatch);
        protected void DrawMenu(SpriteBatch spriteBatch)
        {
            if (menu != null && menu.visible)
            {
                menu.Draw(spriteBatch);
            }
        }
        public abstract void MouseClick(int x, int y);
        public abstract void MouseDown(MouseButtons button);
        public abstract void MouseDrag(MouseButtons b, float x, float y);
        public abstract void MouseUp(MouseButtons button);
        public abstract void MouseWheelMoved(float ticks);
        public abstract void Step(GameTime gameTime);
        protected Menu menu;
        public Menu Menu
        {
            get
            {
                return menu;
            }
            set
            {
                if (menu != null)
                    menu.Hide();
                menu = value;
                if (menu != null)
                    menu.Show();
            }
        }
        public void SetMenuVisible(bool v)
        {
            if (!v)
            {
                for(int i = 0; i < openWindows.Count; i++)
                {
                    WindowControl c = openWindows.Values.ElementAt(i);
                    if (!(c is StatusDialog))
                        c.Close();
                }
            }
            if (menu != null)
            {
                if (v)
                    menu.Show();
                else
                    menu.Hide();
            }
        }
    }
}
