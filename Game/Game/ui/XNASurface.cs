/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace Vexillum.ui
{
    class XNASurface : ISurface
    {
        public Texture2D texture;
        private GraphicsDevice graphics;
        public XNASurface(GraphicsDevice graphicsDevice) {
            graphics = graphicsDevice;
        }

        public void Initialize(IWebView view, int width, int height)
        {
            texture = new Texture2D(graphics, width, height, true, SurfaceFormat.Color);
        }

        public unsafe void Paint(IntPtr srcBuffer, int srcRowSpan, AweRect srcRect, AweRect destRect)
        {
            byte[] buffer = new byte[destRect.Width * destRect.Height * 4];
            Marshal.Copy(srcBuffer, buffer, 0, buffer.Length);
            Rectangle xnaRectangle = new Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height);
            Vexillum.game.GraphicsDevice.Textures[0] = null;
            texture.SetData(0, xnaRectangle, buffer, 0, buffer.Length);
        }

        public void Scroll(int dx, int dy, AweRect clipRect)
        {
            int srcX = clipRect.X;
            int srcY = clipRect.Y;
            int dstX = srcX + dx;
            int dstY = srcY + dy;
            int width = clipRect.Width;
            int height = clipRect.Height;
            if (dstX < 0)
            {
                width += dstX;
                srcX -= dstX;
                dstX = 0;

            }
            else if (dstX + width > texture.Width)
            {
                width -= dstX;
                srcX += dstX;
                dstX = texture.Width - width;

            }
            byte[] buffer = new byte[width * height * 4];
            Rectangle srcRectangle = new Rectangle(srcX, srcY, width, height);
            Rectangle dstRectangle = new Rectangle(dstX, dstY, width, height);
            Vexillum.game.GraphicsDevice.Textures[0] = null;
            texture.GetData<byte>(0, srcRectangle, buffer, 0, buffer.Length);
            texture.SetData(0, dstRectangle, buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            texture.Dispose();
        }
    }
}
*/