using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.view;
using Vexillum.util;

namespace  Vexillum.Entities.stance
{
    public abstract class Stance
    {
        protected Weapon weapon;
        protected HumanoidEntity entity;
        protected Texture2D texture;
        protected int lastStepTime;
        protected int animation;
        protected int currentFrame;

        protected Rectangle[,] frames;
        protected int[,] rightXOffsets;
        protected int[,] leftXOffsets;
        protected int[,] yOffsets;
        protected int[] frameCount;

        private Rectangle crosshair;
        private Rectangle grappleCrosshair;
        protected Vec2 crosshairSize;
        protected Vec2 grappleCrosshairSize;
        public Stance(Weapon weapon, HumanoidEntity entity)
        {
            this.weapon = weapon;
            this.entity = entity;
            texture = entity.GetTexture();
        }
        protected void Init(int numAnimations, int maxFrames)
        {
            frames = new Rectangle[numAnimations, maxFrames];
            rightXOffsets = new int[numAnimations, maxFrames];
            leftXOffsets = new int[numAnimations, maxFrames];
            yOffsets = new int[numAnimations, maxFrames];
            frameCount = new int[numAnimations];
        }
        protected void AddFrame(int animation, int frame, int r1, int r2, int r3, int r4, int xOffsetR, int xOffsetL, int yOffset)
        {
            if(!Util.IsServer)
                frames[animation, frame] = new Rectangle(r1, r2, r3, r4);
            rightXOffsets[animation, frame] = xOffsetR;
            leftXOffsets[animation, frame] = xOffsetL;
            yOffsets[animation, frame] = yOffset;
            frameCount[animation]++;
        }
        protected void PlayAnimation(int animID)
        {
            animation = animID;
            currentFrame = 0;
        }
        public void Step(int time)
        {
            if (animation >= 0 && time - lastStepTime > 50)
            {
                currentFrame++;
                if (currentFrame >= frameCount[animation])
                {
                    animation = -1;
                }
            }
        }
        protected void SetCrosshair(Rectangle r1, Rectangle r2)
        {
            crosshair = r1;
            crosshairSize = new Vec2(crosshair.Width, crosshair.Height)/2;
            grappleCrosshair = r2;
            grappleCrosshairSize = new Vec2(crosshair.Width, crosshair.Height) / 2;
        }
        public Rectangle GetCrosshair(bool canGrapple)
        {
            return canGrapple ? grappleCrosshair : crosshair;
        }
        public abstract int GetRightXOffset();
        public abstract int GetLeftXOffset();
        public abstract int GetYOffset();
        public abstract int GetHeight();
        public abstract int GetWidth();
        public abstract Vec2 GetPivot();
        public abstract Vec2 GetEnd(float angle);
        public abstract Vec2 GetCrosshairPosition(Vec2 drawPosition, float angle);
        public abstract Vec2 GetScreenPivot(GameView view);
        public abstract void Fire();
        public abstract void Draw(SpriteBatch spriteBatch, Rectangle drawRectangle);
        public abstract bool Update(float armAngle);
    }
}
