using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Nuclex.Input;
using Microsoft.Xna.Framework.Input;
using Vexillum.ui;
using Vexillum.util;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface.Controls;
using Vexillum.game;

namespace Vexillum.view
{
    public class MainMenuView : AbstractView
    {
        private static Texture2D controls;
        private static Vec2 controlsPos;
        private static Vec2 usernamePos;
        private static Rectangle usernameRect;
        private static Texture2D title = AssetManager.loadTexture("title2.jpg");
        private static Texture2D titleOverlay = AssetManager.loadTexture("title_ui.png");
        private static int camX;
        private static Random rand = new Random();
        public GraphicsDevice gd;
        static MainMenuView()
        {
            controls = AssetManager.loadTexture("ui/controls.png");
            controlsPos = new Vec2(20, Vexillum.WindowHeight - controls.Height - 20);
        }
        public MainMenuView(GraphicsDevice g, int width, int height, ClientLevel level)
            : base(width, height)
        {
            gd = g;
            Menu = new MainMenu(this);
            Menu.Show();
            camX = new Random().Next(title.Width);
            Vec2 usernameSize = TextRenderer.MeasureString(TextRenderer.FancyFont, Identity.username);
            usernamePos = new Vec2(Vexillum.WindowWidth - usernameSize.X - 20, 20);
            usernameRect = new Rectangle((int)(usernamePos.X - 5), (int)(usernamePos.Y - 5), (int)(usernameSize.X + 10), (int)(usernameSize.Y + 10));
        }
        public override void KeyPressed(Keys key, bool isNew)
        {

        }
        public override void CharacterEntered(char character)
        {

        }

        public override void KeyReleased(Keys key)
        {

        }

        public override void DrawStuff(Microsoft.Xna.Framework.Graphics.GraphicsDevice g, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Rectangle pos = new Rectangle(0, 0, windowWidth, windowHeight);
            Rectangle source = new Rectangle(camX, 0, windowWidth, windowHeight);
            bool x2 = false;
            if (source.Width + source.X > title.Width)
            {
                source.Width = pos.Width = title.Width - source.X;
                x2 = true;
            }
            if (camX > title.Width)
                camX = 0;
            camX += 1;
            spriteBatch.Draw(title, pos, source, Color.White);
            if(x2)
                spriteBatch.Draw(title, new Rectangle(pos.Width, 0, windowWidth - pos.Width, windowHeight), new Rectangle(0, 0, windowWidth - pos.Width, windowHeight), Color.White);
            if (!menu.visible)
                spriteBatch.Draw(controls, controlsPos.XNAVec, Color.White);
            else
                spriteBatch.Draw(titleOverlay, Vec2.Zero.XNAVec, Color.White);
            DrawMenu(spriteBatch);
            spriteBatch.Draw(GraphicsUtil.pixelBlack, usernameRect, Colors.menuBackgroundColor);
            TextRenderer.DrawString(spriteBatch, TextRenderer.FancyFont, Identity.username, usernamePos, Color.White, true);
        }

        public override void MouseDown(MouseButtons button)
        {
            menu.MouseDown(windowMouseX, windowMouseY);
        }
        public override void MouseUp(MouseButtons button)
        {

        }
        public override void MouseWheelMoved(float ticks)
        {
        }
        public override void MouseDrag(MouseButtons b, float x, float y)
        {

        }


        public override void MouseClick(int x, int y)
        {

        }

        public override void MouseMove(float x, float y)
        {
            base.MouseMove(x, y);

        }

        public override void Step(GameTime gameTime)
        {
            //base.Step(gameTime);
            //ai.Step((int)gameTime.TotalGameTime.TotalMilliseconds);
        }
    }
}
