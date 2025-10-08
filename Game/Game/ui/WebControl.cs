/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls;
using Awesomium.Core;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.Input;
using Microsoft.Xna.Framework.Input;

namespace Vexillum.ui
{
    public class WebControl : Control
    {
        public WebView webView;

        public WebControl()
        {
            
        }
        public static void Init()
        {
            WebConfig config = new WebConfig();
            config.LogLevel = LogLevel.Verbose;
            WebCore.Initialize(config);

            String path = Util.GetGameFile("ui");
        }
        public void Update()
        {
            WebCore.Update();
        }
        public void UpdateSize()
        {
            webView = WebCore.CreateWebView((int)this.Bounds.Size.X.Offset, (int)this.Bounds.Size.Y.Offset);
            webView.Source = new Uri("http://google.com");
            webView.Surface = new XNASurface(Vexillum.game.GraphicsDevice);
        }
        protected override void OnMouseWheel(float ticks)
        {
            webView.InjectMouseWheel((int)ticks, 0);
        }
        protected override void OnMouseMoved(float x, float y)
        {
            webView.InjectMouseMove((int)x, (int)y);
        }
        protected override void OnMousePressed(Nuclex.Input.MouseButtons button)
        {
            MouseButton aButton = MouseButton.Left;
            switch(button) {
                case MouseButtons.Left:
                    aButton = MouseButton.Left;
                break;
                case MouseButtons.Middle:
                    aButton = MouseButton.Middle;
                break;
                case MouseButtons.Right:
                    aButton = MouseButton.Right;
                break;
            }
            webView.InjectMouseDown(aButton);
        }
        protected override void OnMouseReleased(MouseButtons button)
        {
            MouseButton aButton = MouseButton.Left;
            switch (button)
            {
                case MouseButtons.Left:
                    aButton = MouseButton.Left;
                    break;
                case MouseButtons.Middle:
                    aButton = MouseButton.Middle;
                    break;
                case MouseButtons.Right:
                    aButton = MouseButton.Right;
                    break;
            }
            webView.InjectMouseUp(aButton);
        }
        protected bool OnKeyPressed(Keys keyCode)
        {
            if (!HasFocus)
            {
                return false;
            }
            unsafe {
                uint m = 0x0100;
                int l = 0;
                int w = (int) keyCode;
                WebKeyboardEvent keyEvent = new WebKeyboardEvent(m, new IntPtr(&w), new IntPtr(&l), Modifiers.ControlKey);
                webView.InjectKeyboardEvent(keyEvent);
            }
            return true;
        }
        protected bool OnKeyReleased(Keys keyCode)
        {
            if (!HasFocus)
            {
                return false;
            }
            unsafe
            {
                uint m = 0x0101;
                int l = 0;
                int w = (int)keyCode;
                WebKeyboardEvent keyEvent = new WebKeyboardEvent(m, new IntPtr(&w), new IntPtr(&l), Modifiers.ControlKey);
                webView.InjectKeyboardEvent(keyEvent);
            }
            return true;
        }
        public bool HasFocus
        {
            get
            {
                return
                  (Vexillum.game.View != null) && Vexillum.game.View.GetScreen() != null &&
                  ReferenceEquals(Vexillum.game.View.GetScreen().FocusedControl, this);
            }
        }
    }
}
*/