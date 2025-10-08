using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.util;

namespace Vexillum.ui
{
    class EComponent
    {
        private Dictionary<int, List<EComponent>> children = new Dictionary<int, List<EComponent>>();
        protected int width;
        protected int height;
        public int X;
        public int Y;
        protected EComponent parent;
        protected int layer = -1;
        protected bool isPrimary = false;
        public bool MouseOver;
        protected bool visible = true;
        public string tooltip = null;
        public int TooltipWidth;
        public int TooltipHeight;
        protected static Color tooltipColor = new Color(123, 128, 154);
        protected static Color tooltipBorderColor = new Color(28, 36, 78);

        public EComponent()
        {
            for (int i = 0; i < 8; i++)
                children[i] = new List<EComponent>(16);
        }

        public void SetTooltip(string tt)
        {
            tooltip = tt;
            if (tt != null)
            {
                Vector2 size = UISettings.DefaultFont.MeasureString(tt);
                TooltipWidth = (int) size.X;
                TooltipHeight = (int) size.Y;
            }
        }

        public void AddChild(int layer, EComponent c)
        {
            //c.setLayer(layer);
            //c.setParent(this);
            children[layer].Add(c);
        }

        public void RemoveChild(EComponent c)
        {
            children[c.layer].Remove(c);
        }
        public void SetParent(EComponent c)
        {
            parent = c;
        }

        public List<EComponent> getChildren()
        {
            List<EComponent> result = new List<EComponent>();
            for (int i = 0; i < 8; i++)
            {
                result.AddRange(children[i]);
            }
            return result;
        }
        public bool hasChild(int layer, EComponent c)
        {
            return children[layer].Contains(c);
        }

        public EComponent[] getChildren(int layer)
        {
            return null;
        }
    }
}
