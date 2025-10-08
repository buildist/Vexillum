using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Vexillum.view;
using Microsoft.Xna.Framework;
using Vexillum.util;
using Microsoft.Xna.Framework.Graphics;

namespace Vexillum.ui
{
    class ChatPanel : ScrollPanel
    {
        private MultiplayerView view;
        private Font font = new Font("Arial", 8, FontStyle.Bold);
        private int chatY;
        private Vec2 pos = new Vec2(0, 0);
        public ChatPanel(MultiplayerView view) : base(view.gd, 9, 9, MultiplayerView.chatWidth, 557)
        {
            this.view = view;
        }
        protected override void Resize(int x, int y)
        {
            chatY = y - view.chatHeight;
            ScrollToBottom();
        }
        protected override void DrawContent(SpriteBatch spriteBatch)
        {
            pos.Y = chatY;
            if (view.chatBarDisplayText.Length != 0)
            {
                int height = TextRenderer.DrawFormattedString(spriteBatch, TextRenderer.DefaultFont, view.chatBarDisplayText, pos, 255, false, MultiplayerView.chatWidth, 2);
                pos.Y -= height;
            }
            lock (view.chatMessages)
            {
                foreach (string c in view.chatMessages)
                {
                    int height = TextRenderer.DrawFormattedString(spriteBatch, TextRenderer.DefaultFont, c, pos, 255, false, MultiplayerView.chatWidth, 2);
                    pos.Y -= height;
                }
            }
        }
    }
}
