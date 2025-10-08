using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.util
{
    public class Colors
    {
        public static Color menuBackgroundColor = new Color(1f, 1f, 1f, 0.5f);
        public static Color menuShadowColor = new Color(0f, 0f, 0f, 0.25f);
        public static Color menuHighlightColor = new Color(255, 230, 150);
        public static Color menuLineColor = new Color(192, 192, 192, 16);
        public static Color menuBottomBarColor = new Color(44, 44, 44, 255);

        public static Color bulletTraceColor = new Color(210, 190, 25, (int)(0.65f * 255));
        public static Color ammoIndicatorColor = new Color(225, 145, 0);
    }
}
