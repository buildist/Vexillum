using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.Entities;
using Vexillum.util;

namespace Server
{
    public class Frame
    {
        public List<EntityDef> entities;
        public Frame(List<Entity> entities)
        {
            this.entities = new List<EntityDef>(entities.Count);
            foreach (Entity e in entities)
            {
                this.entities.Add(new EntityDef(e));
            }
        }
        public static bool TestPoint(EntityDef e, int x, int y)
        {
            return x > e.tCorner.X && x < e.bCorner.X && y > e.tCorner.Y && y < e.bCorner.Y;
        }
        public struct EntityDef
        {
            public EntityDef(Entity e)
            {
                pos = e.Position;
                size = e.Size;
                tCorner = pos - e.HalfSize;
                bCorner = pos + e.HalfSize;
                id = (short) e.ID;
            }
            public Vec2 tCorner;
            public Vec2 bCorner;
            public Vec2 pos;
            public Vec2 size;
            public short id;
        }
    }
}
