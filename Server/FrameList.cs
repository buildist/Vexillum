using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class FrameList : Dictionary<int, Frame>
    {
        private int max;
        private int last = 0;
        public FrameList(int max)
        {
            this.max = max;
        }
        public void AddFrame(int i, Frame f)
        {
            if (this.Count == max)
                Remove(i - max);
            base.Add(i, f);
            last = i;
        }
        public Frame GetFrame(int i)
        {
            if (base.ContainsKey(i))
                return this[i];
            else
                return this[last];
        }
    }
}
