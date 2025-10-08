using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.Game;
using Vexillum.util;

namespace  Vexillum.Entities
{
    public static class HumanoidTypes
    {
        private static Dictionary<PlayerClass, HumanoidDef> types = new Dictionary<PlayerClass, HumanoidDef>();
        public class HumanoidDef
        {
            public PlayerClass name;
            public Texture2D texture;
            public Vec2 size;
            public float speed;
            public Rectangle[] walkImages = new Rectangle[4];
            public int[] xOffsetsL;
            public int[] xOffsetsR;
            public int[] yOffsets;
            public Rectangle flying;
            public HumanoidDef(PlayerClass cl, Texture2D texture, Vec2 size, float speed, Rectangle[] walkImages, int[] xOffsetsR, int[] xOffsetsL, int[] yOffsets, Rectangle? flying)
            {
                name = cl;
                this.texture = texture;
                this.size = size;
                this.speed = speed;
                this.walkImages = walkImages;
                this.xOffsetsL = xOffsetsL;
                this.xOffsetsR = xOffsetsR;
                this.yOffsets = yOffsets;
                if(flying != null)
                    this.flying = (Rectangle) flying;
            }
        }
        private static void AddType(PlayerClass name, Texture2D texture, Vec2 size, float speed, Rectangle[] walkImages, int[] xOffsetsR, int[] xOffsetsL, int[] yOffsets, Rectangle? flying)
        {
            types[name] = new HumanoidDef(name, texture, size, speed, walkImages, xOffsetsR, xOffsetsL, yOffsets, flying);
        }
        public static void LoadContent()
        {
            AddType(PlayerClass.Green, AssetManager.loadTexture("soldier_green.png"), new Vec2(8, 40), 2, new Rectangle[] { new Rectangle(3, 1, 17, 18), new Rectangle(21, 2, 11, 16), new Rectangle(34, 1, 8, 18), new Rectangle(44, 1, 14, 18) }, new int[] { 4, 3, 0, 2 }, new int[] { 5, 0, 0, 4 }, new int[] { 0, -1, 0, 0 }, new Rectangle(166, 86, 15, 42));
            AddType(PlayerClass.Blue, AssetManager.loadTexture("soldier_blue.png"), new Vec2(8, 40), 2, new Rectangle[] { new Rectangle(3, 1, 17, 18), new Rectangle(21, 2, 11, 16), new Rectangle(34, 1, 8, 18), new Rectangle(44, 1, 14, 18) }, new int[] { 4, 3, 0, 2 }, new int[] { 5, 0, 0, 4 }, new int[] { 0, -1, 0, 0 }, new Rectangle(166, 86, 15, 42));
            AddType(PlayerClass.Spectator, null, Vec2.Zero, 2, null, null, null, null, Rectangle.Empty);
        }
        public static void LoadContentServer()
        {
            AddType(PlayerClass.Green, null, new Vec2(8, 40), 2, null, null, null, null, null);
            AddType(PlayerClass.Blue, null, new Vec2(8, 40), 2, null, null, null, null, null);
            AddType(PlayerClass.Spectator, null, Vec2.Zero, 2, null, null, null, null, null);
        }

        public static HumanoidEntity CreateHumanoid(PlayerClass type)
        {
            return new HumanoidEntity().SetType(types[type]);
        }
        public static HumanoidDef GetType(PlayerClass cl)
        {
            return types[cl];
        }
    }
}
