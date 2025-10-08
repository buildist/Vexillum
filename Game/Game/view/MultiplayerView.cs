using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Nuclex.Input;
using Vexillum.ui;
using Vexillum.util;
using Vexillum.mp;

namespace Vexillum.view
{
    public class MultiplayerView : AbstractView
    {
        private const string chatServer = "127.0.0.1";
        private const int chatPort = 34224;

        private MPClient client;

        private const int MAX_CHATS = 50;
        private static Texture2D bg = AssetManager.loadTexture("ui/multiplayer.png");
        public List<string> chatMessages = new List<String>(MAX_CHATS);

        private ChatPanel chatPanel;
        private bool repaintChat = false;

        private string chatBarText = "";
        public string chatBarDisplayText = "";
        
        public const int chatWidth = 522;
        public readonly int chatHeight = TextRenderer.GetLineHeight(TextRenderer.DefaultFont) + 2;

        public GraphicsDevice gd;

        public MultiplayerView(GraphicsDevice g, int width, int height)
            : base(width, height)
        {
            gd = g;
            menu = new MultiplayerMenu(this);
            menu.AddScrollPanel(chatPanel = new ChatPanel(this));
            menu.Show();
            
            client = Vexillum.game.MPConnect(this, chatServer, chatPort);
        }

        public void AddChat(string c)
        {
            lock (chatMessages)
            {
                if (chatMessages.Count == MAX_CHATS)
                    chatMessages.RemoveAt(chatMessages.Count - 1);
                chatMessages.Insert(0, TextRenderer.WordWrap(c, TextRenderer.DefaultFont, chatWidth));
                repaintChat = true;
            }
        }

        public override void DrawStuff(Microsoft.Xna.Framework.Graphics.GraphicsDevice g, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(bg, Vec2.Zero.XNAVec, Color.White);
            if (repaintChat)
            {
                repaintChat = false;
                UpdateChatPanel(spriteBatch);
            }
            menu.Draw(spriteBatch);
            
        }

        private void UpdateChatPanel(SpriteBatch spriteBatch)
        {
            chatPanel.Update(Math.Max(562-4, chatMessages.Count * chatHeight + (chatBarText == "" ? 0 : chatHeight)), spriteBatch);
        }

        public override void KeyPressed(Keys key, bool isNew)
        {
            switch (key)
            {
                case Keys.Escape:
                    chatBarText = "";
                    UpdateChatBarText();
                    break;
                case Keys.Back:
                    if (chatBarText.Length <= 1)
                        chatBarText = "";
                    else
                        chatBarText = chatBarText.Substring(0, chatBarText.Length - 1);
                    UpdateChatBarText();
                    break;
                case Keys.Enter:
                    if (chatBarText.Length > 0)
                    {
                        client.SendChat(chatBarText);
                        AddChat(client.name + "> " + chatBarText);
                        chatBarText = "";
                        UpdateChatBarText();
                    }
                    break;
            }
        }
        public override void CharacterEntered(char character)
        {
            if (!Char.IsControl(character))
            {
                chatBarText += character;
                UpdateChatBarText();
            }
        }

        private void UpdateChatBarText()
        {
            if (chatBarText == "")
                chatBarDisplayText = "";
            else
                chatBarDisplayText = TextRenderer.WordWrap(TextUtil.COLOR_BLUE+"> "+TextUtil.COLOR_WHITE+chatBarText, TextRenderer.DefaultFont, chatWidth);
            repaintChat = true;
        }

        public override void KeyReleased(Keys key)
        {

        }

        public override void MouseDown(MouseButtons b)
        {
            if (b.HasFlag(MouseButtons.Left))
                menu.MouseDown(mouseX, mouseY);
        }
        public override void MouseUp(MouseButtons b)
        {
            if (b.HasFlag(MouseButtons.Left))
                menu.MouseUp(mouseX, mouseY);
        }
        public override void MouseDrag(MouseButtons b, float x, float y)
        {
            if (b.HasFlag(MouseButtons.Left))
                menu.MouseDrag((int)x, (int)y);
        }
        public override void MouseWheelMoved(float ticks)
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
        }
    }
}
