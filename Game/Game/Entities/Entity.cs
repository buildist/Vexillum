using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.physics;
using Vexillum.Game;
using Vexillum.util;

namespace  Vexillum.Entities
{
    public delegate void AddHandler();
    public delegate void RemoveHandler();
    public delegate void ClientAddHandler(ClientLevel l);
    public delegate void ClientRemoveHandler(ClientLevel l);
    public abstract class Entity
    {
        public static short CurrentID = 1;
        private static bool CheckIDs = false;
        private Vec2 position = Vec2.Zero;
        public PhysicsController PhysicsController { get; set; }
        public int addedFrame = 0;
        public bool isPlayer;
        public bool clientSide = false;
        public Player player;
        public Vec2 Force = Vec2.Zero;
        public static short NextID(Level l)
        {
            CurrentID++;

            if (CheckIDs)
            {
                while (CurrentID == -1 || l.EntityIndex.ContainsKey(CurrentID))
                {
                    CurrentID++;
                }
            }
            if (CurrentID == -2)
                CheckIDs = true;
            return CurrentID;
        }
        public static void ResetID()
        {
            CurrentID = 1;
            CheckIDs = false;
        }
        public Vec2 FeetPosition;
        public Vec2 Position
        {
            get
            {
                return position;
            }
            set
            {
                if (Level != null)
                    Level.SetEntityPosition(this, value);
                position = value;
                FeetPosition = position + new Vec2(0, -HalfSize.Y + 1);
            }
        }
        public void OnClientAdd()
        {
            }
        public Vec2 GetDrawPosition()
        {
            if (smoothedPosition == Vec2.Zero)
                return position;
            else
                return smoothedPosition * 0.75f + position * 0.25f;
        }
        public Vec2 positionInLevel;
        private Vec2 targetPosition;
        private Vec2 originalPosition;
        private Vec2 smoothedPosition = Vec2.Zero;
        private Vec2 targetDiff;
        private int targetSetTime;
        private bool first = true;
        public void SetTargetPosition(Vec2 pos)
        {
            Position = pos;
            originalPosition = smoothedPosition;
            targetPosition = pos;

            if (first)
            {
                smoothedPosition = pos;
                targetDiff = Vec2.Zero;
                first = false;
            }
            else
                    {
                targetDiff = pos - originalPosition;
                targetSetTime = level.GetTime();
            }

        }

        public Vec2 FixedVelocity = Vec2.Zero;
        public bool anchored = false;
        public Vec2 velocity = Vec2.Zero;
        public Vec2 Velocity
        {
            get
            {
                if (FixedVelocity != Vec2.Zero || anchored)
                    return FixedVelocity;
                return velocity;
            }
            set
            {
                velocity = value;
                if (velocity.Y > 0)
                    size=size;
            }
        }
        private Vec2 size = Vec2.Zero;
        public Vec2 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                HalfSize = size / 2;
                tCorner = position - HalfSize;
                bCorner = position + HalfSize;
                Volume = (int) (size.X * size.Y);
                Mass = (float) Math.Sqrt(Volume * Density);
            }
        }
        public float Rotation { get; set; }
        public Vec2 HalfSize { get; set; }
        public Vec2 tCorner;
        public Vec2 bCorner;
        public int Volume;
        public int Density = 1;
        public float Mass;
        private short id = -1;
        public short ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public float Speed;
        public bool jumping = false;
        private Level level;
        protected Vec2 screenPos;
        public Vec2 GetScreenPosition()
        {
            return screenPos;
        }

        public Level Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
                if (ID == -1 && level != null)
                {
                    ID = NextID(level);
                }
            }
        }

        public bool enablePhysics = true;
        public bool disabled = false;
        public bool removed = false;

        public bool velocityChanged = false;
        public bool ladder = false;
        public int ladderDirection = 0;

        protected void SetRotationEnabled()
        {
            float max = Math.Max(Size.X, Size.Y);
            Size = new Vec2(max, max);
        }

        public virtual void OnCollide(Entity e, int direction)
        {
        }
        public virtual void OnClientCollide(Entity e, int direction)
        {
        }

        public bool TestPoint(int x, int y)
        {
            return x > tCorner.X && x < bCorner.X && y > tCorner.Y && y < bCorner.Y;
        }

        public virtual void Step(int gameTime)
        {
            if(targetSetTime != 0)
            {
                float f = (gameTime - targetSetTime) / 100f;
                if (f > 1)
                {
                    f = 1;
                }
                smoothedPosition = originalPosition + targetDiff * f;
            }
        }

        public abstract void Draw(GameView view, SpriteBatch spriteBatch);
        public AddHandler Added = delegate() { };
        public RemoveHandler Removed = delegate() { };
        public ClientAddHandler AddedClient = delegate(ClientLevel l) { };
        public ClientRemoveHandler RemovedClient = delegate(ClientLevel l) { };

        public bool xCollision = false;
        public bool yCollision = false;
    }
}
