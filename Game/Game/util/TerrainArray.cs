using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using Microsoft.Xna.Framework;

namespace Vexillum.util
{
    public class TerrainArray
    {
        private int[,] terrain;
        private int width;
        private int height;
        public TerrainArray(int sx, int sy)
        {
            this.width = sx;
            this.height = sy;
            terrain = new int[sx, sy];
        }
        private bool CheckOutOfBounds(int x, int y)
        {
            return (x < 0 || y < 0 || x >= width || y >= height);
        }
        private short GetShort(int x, int y)
        {
            int num = terrain[x, y];
            int numMask = 0xFFFF << (1 << 4); 
            num = num & numMask;
            num = num >> (1 << 4);
            if (num < 0)
                num = num & ~numMask;
            return (short) num;
        }
        private void SetShort(int x, int y, int data)
        {
            int num = terrain[x, y];
            int shifted = data << (1 << 4);
            int numMask = 0xFFFF << (1 << 4);
            terrain[x, y] = (num & ~numMask) | shifted;
        }
        private void SetShort0(int x, int y, int data)
        {
            int num = terrain[x, y];
            int numMask = 0xFFFF;
            terrain[x, y] = (num & ~numMask) | data;
        }
        private byte GetNibble(int x, int y, int index)
        {
            int num = terrain[x, y];
            int numMask = 0xF << (index << 2);
            num = num & numMask;
            num = num >> (index << 2);
            if (num < 0)
                num = num & ~numMask;
            return (byte) num;
        }
        private void SetNibble(int x, int y, int idx, int data)
        {
            int num = terrain[x, y];
            int shifted = data << (idx << 2);
            int numMask = 0xF << (idx << 2);
            terrain[x, y] = (num & ~numMask) | shifted;
        }
        private void SetByte(int x, int y, int idx, bool value)
        {
            int mask = 1 << idx;
            if (value)
                terrain[x, y] = terrain[x, y] | mask;
            else
                terrain[x, y] = terrain[x, y] & ~mask;
        }
        private bool GetByte(int x, int y, int idx)
        {
            int mask = 1 << idx;
            return (terrain[x,y] & mask) != 0;
        }
        public bool GetTerrain(int x, int y)
        {
            if (CheckOutOfBounds(x, y))
                return true;
            return GetByte(x, y, 0);
        }
        public void SetTerrain(int x, int y, bool value)
        {
            if (CheckOutOfBounds(x, y))
                return;
            SetByte(x, y, 0, value);
        }
        public bool GetParticle(int x, int y)
        {
            if (CheckOutOfBounds(x, y))
                return true;
            return GetByte(x, y, 1);
        }
        public void SetParticle(int x, int y, bool value)
        {
            if (CheckOutOfBounds(x, y))
                return;
            SetByte(x, y, 1, value);
        }
        public bool GetTerrainOrParticle(int x, int y)
        {
            if (CheckOutOfBounds(x, y))
                return true;
            return GetByte(x, y, 1) || GetByte(x, y, 0);
        }
        public short GetEntity(Entity e, int x, int y, Level l)
        {
            if (CheckOutOfBounds(x, y))
                return 0;
            short id = GetShort(x, y);
            if (e == null || e.ID != id && l.EntityIndex.ContainsKey(id))
                return id;
            else
                return 0;
        }
        public byte GetCollisionData(int x, int y)
        {
            if (CheckOutOfBounds(x, y))
                return 0;
            return GetNibble(x, y, 1);
        }
        public void SetCollisionData(int x, int y, byte v)
        {
            SetNibble(x, y, 1, v);
        }
        public void SetTransparent(int x, int y, bool v)
        {
            SetByte(x, y, 2, v);
        }
        public bool GetTransparent(int x, int y)
        {
            return GetByte(x, y, 2);
        }
        public bool GetLadder(int x, int y)
        {
            if (CheckOutOfBounds(x, y))
                return false;
            return GetByte(x, y, 3);
        }
        public void SetTerrainAndCollisionData(int x, int y, byte t, byte c, byte l)
        {
            SetShort0(x, y, t | (c << 4) | (l << 3));
        }
        public void SetEntityState(Entity e, Vec2 pos, bool v)
        {
            if (v && e.removed)
                return;
            if (v)
                e.positionInLevel = Util.Round(pos);
            //if (!EntityIndex.ContainsKey(e.ID))
            //    e = e;
            int posX = (int)pos.X;
            int posY = (int)pos.Y;
            int gx, gy;
            int n = v ? e.ID : 0;
            for (int x = (int)-e.HalfSize.X + 1; x < e.HalfSize.X - 1; x++)
            {
                gx = posX + x;
                if (gx < 0 || gx >= width)
                    continue;

                gy = (int)(posY + e.HalfSize.Y);
                if (gy >= 0 && gy < height)
                    SetShort(gx, gy, n);

                gy = (int)(posY - e.HalfSize.Y + 1);
                if (gy >= 0 && gy < height)
                    SetShort(gx, gy, n);
            }

            for (int y = (int)-e.HalfSize.Y + 2; y < e.HalfSize.Y; y++)
            {
                gy = posY + y;
                if (gy < 0 || gy >= height)
                    continue;

                gx = (int)(posX - e.HalfSize.X);
                if (gx >= 0 && gx < width)
                    SetShort(gx, gy, n);

                gx = (int)(posX + e.HalfSize.X - 1);
                if (gx >= 0 && gx < width)
                    SetShort(gx, gy, n);
            }
        }
        public bool Destroy(int x, int y)
        {
            if (CheckOutOfBounds(x, y) || GetCollisionData(x, y) == TerrainCollisionType.Solid || GetTerrain(x, y) == false)
                return false;
            SetTerrain(x, y, false);
            return true;
        }
        public byte[] ToBytes()
        {
            byte[] r = new byte[(width*height)/8];
            int index = 0;
            int bitIndex = 0;
            byte[] mask = new byte[]{1, 2, 4, 8, 16, 32, 64, 128};
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if(GetTerrain(x, y))
                        r[index] = (byte) (r[index] | mask[bitIndex]);
                    bitIndex++;
                    if(bitIndex == 8)
                    {
                        bitIndex = 0;
                        index++;
                    }
                }
            }
            return r;
        }
        public void SetBytes(byte[] b)
        {
            int index = 0;
            int bitIndex = 0;
            byte[] mask = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((b[index] & mask[bitIndex]) != 0)
                        SetTerrain(x, y, true);
                    else
                        SetTerrain(x, y, false);
                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        bitIndex = 0;
                        index++;
                    }
                }
            }
        }
    }
}
