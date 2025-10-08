/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Visuals.Flat;
using Nuclex.UserInterface;
using Awesomium.Core;
using Microsoft.Xna.Framework;

namespace Vexillum.ui
{
    public class WebControlRenderer : IFlatControlRenderer<WebControl>
    {
        public void Render(WebControl control, IFlatGuiGraphics graphics)
        {
            control.Update();
            RectangleF controlBounds = control.GetAbsoluteBounds();
            Rectangle xnaRect = new Rectangle((int) controlBounds.X, (int) controlBounds.Y, (int) controlBounds.Width, (int) controlBounds.Height);
            ((FlatGuiGraphics)graphics).spriteBatch.Draw(((XNASurface)control.webView.Surface).texture, xnaRect, Color.White);
        }
    }
}
*/