using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vexillum.util
{
    public struct Vec2
    {
        public Microsoft.Xna.Framework.Vector2 XNAVec;
        public static Vec2 Zero = new Vec2(0, 0);
        public float X
        {
            get
            {
                return XNAVec.X;
            }
            set
            {
                XNAVec.X = value;
            }
        }
        public float Y
        {
            get
            {
                return XNAVec.Y;
            }
            set
            {
                XNAVec.Y = value;
            }
        }
        public Vec2(float x, float y)
        {
            XNAVec = new Microsoft.Xna.Framework.Vector2(x, y);
        }
        public Vec2(Microsoft.Xna.Framework.Vector2 v)
        {
            XNAVec = v;
        }
        public float Length()
        {
            return XNAVec.Length();
        }
        public float LengthSquared()
        {
            return XNAVec.LengthSquared();
        }
        public static float Distance(Vec2 u, Vec2 v)
        {
            return (u - v).Length();
        }
        public void Normalize()
        {
            XNAVec.Normalize();
        }
        public bool Equals(Vec2 v)
        {
            return XNAVec.Equals(v);
        }
        public static bool operator ==(Vec2 x, Vec2 y)
        {
            return x.XNAVec.Equals(y.XNAVec);
        }
        public static bool operator !=(Vec2 x, Vec2 y)
        {
            return !x.XNAVec.Equals(y.XNAVec);
        }
        public static Vec2 operator *(Vec2 x, Vec2 y)
        {
            return new Vec2(x.XNAVec * y.XNAVec);
        }
        public static Vec2 operator /(Vec2 x, Vec2 y)
        {
            return new Vec2(x.XNAVec / y.XNAVec);
        }
        public static Vec2 operator +(Vec2 x, Vec2 y)
        {
            return new Vec2(x.XNAVec + y.XNAVec);
        }
        public static Vec2 operator -(Vec2 x, Vec2 y)
        {
            return new Vec2(x.XNAVec - y.XNAVec);
        }
        public static Vec2 operator *(Vec2 x, float y)
        {
            return new Vec2(x.XNAVec * y);
        }
        public static Vec2 operator /(Vec2 x, float y)
        {
            return new Vec2(x.XNAVec / y);
        }
        public override string ToString()
        {
            return X + ", " + Y;
        }
    }
}
