using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Vexillum.util
{
    public class BitArray2D
    {
        private int width;
        private int height;
        private BitArray array;
        public BitArray2D(int width, int height)
        {
            this.width = width;
            this.height = height;
            array = new System.Collections.BitArray(width * height);
        }
        public bool this[int col, int row]
        {
            get { return Get(col, row); }
            set { Set(col, row, value); }
        }
        public void Set(int col, int row, bool b)
        {
            array[(row * width) + col] = b;
        }
        public bool Get(int col, int row)
        {
            return array[(row * width) + col];
        }
        public void SetAll(bool value)
        {
            array.SetAll(value);
        }
        public byte[] ToBytes()
        {
            int num_bytes = array.Length / 8;

            if (array.Length % 8 != 0)
            {
                num_bytes += 1;
            }

            byte[] bytes = new byte[num_bytes];
            array.CopyTo(bytes, 0);
            return bytes;
        }
        public void Set(byte[] bytes)
        {
            array = new BitArray(bytes);
        }
    }
}
