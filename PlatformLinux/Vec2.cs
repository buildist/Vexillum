using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Vexillum.util
{
    public struct Vec2
    {
        public Microsoft.Xna.Framework.Vector2 XNAVec;
        public static Vec2 Zero = new Vec2(0, 0);
        private float x;
        private float y;
        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
            XNAVec = new Vector2();
        }
        public Vec2(Vector2 v2)
        {
            x = 0;
            y = 0;
            XNAVec = new Vector2();
        }
        public float Length()
        {
            return (float) Math.Sqrt(LengthSquared());
        }
        public float LengthSquared()
        {
            return X * X + Y * Y;
        }
        public static float Distance(Vec2 u, Vec2 v)
        {
            return (u - v).Length();
        }
        public void Normalize()
        {
            float l = Length();
            X /= l;
            Y /= l;
        }
        public bool Equals(Vec2 v)
        {
            return v.X == X && v.Y == Y;
        }
        public static bool operator ==(Vec2 x, Vec2 y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(Vec2 x, Vec2 y)
        {
            return !x.Equals(y);
        }
        public static Vec2 operator *(Vec2 x, Vec2 y)
        {
            return new Vec2(x.X*y.X, x.Y*y.Y);
        }
        public static Vec2 operator /(Vec2 x, Vec2 y)
        {
            return new Vec2(x.X / y.X, x.Y / y.Y);
        }
        public static Vec2 operator +(Vec2 x, Vec2 y)
        {
            return new Vec2(x.X + y.X, x.Y + y.Y);
        }
        public static Vec2 operator -(Vec2 x, Vec2 y)
        {
            return new Vec2(x.X - y.X, x.Y - y.Y);
        }
        public static Vec2 operator *(Vec2 x, float y)
        {
            return new Vec2(x.X * y, x.Y*y);
        }
        public static Vec2 operator /(Vec2 x, float y)
        {
            return new Vec2(x.X / y, x.Y / y);
        }
    }
}