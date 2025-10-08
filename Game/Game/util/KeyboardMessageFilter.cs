using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Vexillum.util
{
    public class KeyboardMessageFilter : IMessageFilter
    {
        [DllImport("user32.dll")]
        static extern bool TranslateMessage(ref Message lpMsg);

        const int WM_CHAR = 0x0102;
        const int WM_KEYDOWN = 0x0100;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                TranslateMessage(ref m);
            }
            else if (m.Msg == WM_CHAR)
            {
                if (KeyPressed != null)
                    KeyPressed.Invoke(this,
                        new KeyboardMessageEventArgs()
                        {
                            Character = Convert.ToChar((int)m.WParam)
                        });
            }
            return false;
        }

        public event EventHandler KeyPressed;
    }

    public class KeyboardMessageEventArgs : EventArgs
    {
        public char Character;
    }
}
