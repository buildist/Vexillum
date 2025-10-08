using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Input;
using Vexillum.Entities;
using MiscUtil.IO;
using Vexillum.util;

namespace Vexillum.net
{
    public class StreamHelper
    {
        private EndianBinaryReader reader;
        private EndianBinaryWriter writer;
        private static Type keyType = typeof(Keys);
        private static Type actionType = typeof(KeyAction);
        private static Array keyValues = Enum.GetValues(keyType);
        private static Array actionValues = Enum.GetValues(actionType);
        private static List<string> entityTypes = new List<string>(new string[] { "Vexillum.Entities.BlueFlagEntity", "Vexillum.Entities.CrateEntity", "Vexillum.Entities.GrapplingHook", "Vexillum.Entities.GreenFlagEntity", "Vexillum.Entities.HumanoidEntity", "Vexillum.Entities.Rocket", "Vexillum.Entities.Weapons.RocketLauncher", "Vexillum.Entities.NullEntity", "Vexillum.Entities.Weapons.ClusterBombLauncher", "Vexillum.Entities.Weapons.SMG", "Vexillum.Entities.Weapons.Sword", "Vexillum.Entities.PixelEntity", "Vexillum.Entities.ClusterBomb", "Vexillum.Entities.Bomblet" });
        private int wrapCount = 0;
        public StreamHelper(EndianBinaryReader r, EndianBinaryWriter w)
        {
            reader = r;
            writer = w;
        }
        public float ReadAngle()
        {
            return ((float) reader.ReadSByte() / 127) * (float) Math.PI;
        }
        public void WriteAngle(float radians)
        {
            writer.Write((sbyte) ((radians / Math.PI) * 127));
        }
        public Vec2 ReadVec2()
        {
            float x = reader.ReadInt16();
            float y = reader.ReadInt16();
            return new Vec2((int) x, (int) y);
        }
        public void WriteVec2(Vec2 v)
        {
            writer.Write((short) v.X);
            writer.Write((short) v.Y);
        }
        /*public void WriteBitArray(params bool[] array)
        {
            BitArray arr = new BitArray(array);

            int num_bytes = arr.Length / 8;

            if (arr.Length % 8 != 0)
            {
                num_bytes += 1;
            }

            byte[] bytes = new byte[num_bytes];
            writer.Write((byte)bytes.Length);
            arr.CopyTo(bytes, 0);
            writer.Write(bytes);
        }
        public bool[] ReadBitArray()
        {
            int length = reader.ReadByte();
            byte[] bytes = reader.ReadBytes(length);
            BitArray arr = new BitArray(bytes);
            bool[] r = new bool[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                r[i] = arr[i];
            return r;
        }*/
        public BitArray ReadMovementData()
        {
            BitArray arr = new BitArray(reader.ReadBytes(1));
            return arr;
        }
        public void WriteMovementData(params bool[] data)
        {
            BitArray arr = new BitArray(data);
            byte[] b = new byte[1];
            arr.CopyTo(b, 0);
            writer.Write(b);
        }
        public void WriteEnum(object obj)
        {
            int index = Array.IndexOf(Enum.GetValues(obj.GetType()), obj);
            writer.Write((byte)index);
        }
        public object ReadEnum(Type t)
        {
            return Enum.GetValues(t).GetValue(reader.ReadByte());
        }
        public void WriteEntityType(object e)
        {
            writer.Write((byte)entityTypes.IndexOf(e.GetType().FullName));
        }
        public Type ReadEntityType()
        {
            return Type.GetType(entityTypes.ElementAt(reader.ReadByte()));
        }
        public Color ReadColor()
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            return new Color(r, g, b);
        }
        public void WriteColor(Color c)
        {
            writer.Write((byte)c.R);
            writer.Write((byte)c.G);
            writer.Write((byte)c.B);
        }
        public int ReadFrameByte(int cFrame)
        {
            if (cFrame != -1)
            {
                int delta = reader.ReadSByte();
                return cFrame + delta;
            }
            else
                return reader.ReadInt32();
        }
        public void WriteFrameByte(int lastFrame, int cFrame)
        {
            if (lastFrame == -1)
                writer.Write(cFrame);
            else
                writer.Write((sbyte)(cFrame - lastFrame));
        }
        public KeyAction ReadAction()
        {
            short actionID = reader.ReadInt16();
            return (KeyAction)actionValues.GetValue(actionID);
        }
        public void WriteAction(KeyAction k)
        {
            writer.Write((short)Array.IndexOf(actionValues, k));
        }
        public string ReadJString()
        {
            int len = reader.ReadInt16();
            byte[] bytes = new byte[len];
            reader.Read(bytes, 0, len);
            return Encoding.UTF8.GetString(bytes);
        }
        public void WriteJString(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            writer.Write((short)bytes.Length);
            writer.Write(bytes);
        }
    }
}
