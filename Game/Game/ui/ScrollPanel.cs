using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Vexillum.ui
{
    public abstract class ScrollPanel
    {
        private const int scrollbarWidth = 10;
        public int x;
        public int y;
        public int width;
        public int height;
        public int contentWidth;
        public int contentHeight;
        private bool shouldUpdate = true;

        private int scrollbarClickY;
        private int scrollbarPos;
        private int scrollPos;

        private int maxScrollPos;
        private int maxScrollbarPos;
        private int scrollbarHeight;

        private Rectangle scrollRect;
        private Rectangle contentRect;
        private Rectangle scrollbarRect;
        protected System.Drawing.Rectangle localRect;
        private GraphicsDevice gd;
        private RenderTarget2D renderTarget;
        private Texture2D bitmap;

        protected ScrollPanel(GraphicsDevice gd, int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width+scrollbarWidth;
            this.height = height;
            contentWidth = width;
            contentRect = new Rectangle(x, y, contentWidth, height);
            this.gd = gd;
        }
        private void SetSize(int height)
        {
            contentHeight = height;
            localRect = new System.Drawing.Rectangle(0, 0, contentWidth, contentHeight);
            renderTarget = new RenderTarget2D(gd, contentWidth, height);
        }
        public void Update(int height, SpriteBatch spriteBatch)
        {
            if (this.contentHeight != height)
            {
                SetSize(height);
                UpdateScrollBarBounds();
                Resize(contentWidth, height);
            }
            gd.SetRenderTarget(renderTarget);
            spriteBatch.End();
            spriteBatch.Begin();
            gd.Clear(Color.Black);
            //DrawContent(spriteBatch);
            spriteBatch.End();
            gd.SetRenderTargets(null);
            bitmap = renderTarget;
            Vexillum.game.BeginSpriteBatch();
        }
        protected abstract void DrawContent(SpriteBatch spriteBatch);
        protected abstract void Resize(int x, int y);
        public void Draw(SpriteBatch s)
        {
            if (bitmap != null)
            {
                s.Draw(bitmap, contentRect, scrollRect, Color.White);
                //bitmap.Dispose();
            }
            s.Draw(GraphicsUtil.pixel, scrollbarRect, Color.Red);
        }

        public void XMouseDown(int x, int y)
        {
            if (x > this.x + this.contentWidth && y > this.y && x < this.x + this.width && y < height)
            {
                if (scrollbarClickY == -1)
                {
                    scrollbarClickY = y - this.y - scrollbarPos;
                }
            }
        }
        public void XMouseMove(int x, int y)
        {

        }
        public void XMouseDrag(int x, int y)
        {
            if (scrollbarClickY != -1)
            {
                scrollbarPos += (y - this.y - scrollbarPos)-scrollbarClickY;
                UpdateScrollBar();
            }
        }
        public void XMouseUp(int x, int y)
        {
            scrollbarClickY = -1;
        }
        private void UpdateScrollBarBounds()
        {
            float ratio = ((float)height / contentHeight);
            scrollbarHeight = (int)(ratio * height);
            maxScrollbarPos = height - scrollbarHeight;
            maxScrollPos = contentHeight - height;
            UpdateScrollBar();
        }
        private void UpdateScrollBar()
        {
            scrollbarPos = Math.Max(Math.Min(maxScrollbarPos, scrollbarPos), 0);
            float ratio = ((float)scrollbarPos / maxScrollbarPos);
            scrollPos = (int) (maxScrollPos * ratio);
            scrollRect = new Rectangle(0, scrollPos, contentWidth, height);
            scrollbarRect = new Rectangle(x + contentWidth, y + scrollbarPos, scrollbarWidth, scrollbarHeight);
        }
        public void ScrollToBottom()
        {
            scrollbarPos = maxScrollPos;
            UpdateScrollBar();
        }
    }
}
