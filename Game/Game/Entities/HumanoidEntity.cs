using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.view;
using Vexillum.Entities.Weapons;
using Vexillum.Entities.stance;
using Vexillum.Game;
using Vexillum.util;

namespace  Vexillum.Entities
{
    public class HumanoidEntity : LivingEntity
    {
        protected bool ready = false;
        protected bool facingDirection;
        private float health;
        public float Health {
            get
            {
                return health;
            }
            set
            {
                health = Math.Max(Math.Min(value, MaxHealth), 0);
                if (health == 0)
                    Level.OnEntityDeath(this);
            }
        }
        public float MaxHealth { get; set; }
        public Stance stance;
        protected Texture2D sprite;
        protected Rectangle[] walk = new Rectangle[4];
        protected int[] walkYOffsets = new int[4];
        protected int[] walkRightXOffsets = new int[4];
        protected int[] walkLeftXOffsets = new int[4];
        protected int idx = 0;
        protected int stepTime;
        protected bool animate = false;
        public PlayerClass type = PlayerClass.Spectator;
        public Entity hook;
        protected Rectangle flying;
        private Vec2 rotateCenter;
        private bool playSoundNextFrame = false;

        public HumanoidEntity SetType(HumanoidTypes.HumanoidDef def)
        {
            if (Level != null)
                Level.terrain.SetEntityState(this, positionInLevel, false);

            MaxHealth = 100;
            Health = MaxHealth;
            type = def.name;
            sprite = def.texture;
            Size = def.size;
            Speed = def.speed;
            walk = def.walkImages;
            walkLeftXOffsets = def.xOffsetsL;
            walkRightXOffsets = def.xOffsetsR;
            walkYOffsets = def.yOffsets;
            flying = def.flying;
            rotateCenter = new Vec2(flying.Width / 2, flying.Height / 2);
            if (type == PlayerClass.Spectator)
            {
                enablePhysics = false;
                stance = null;
            }
            else
                enablePhysics = true;

            Velocity = Vec2.Zero;

            return this;
        }
        public void SetReady(bool v)
        {
            ready = v;
        }
        public override Weapon Weapon
        {
            get
            {
                return base.Weapon;
            }
            set
            {
                Stance s = value.GetStanceInstance(this);
                stance = s;
                value.SetStance(s);
                base.Weapon = value;
            }
        }
        protected void AddWalkImage(int i, Rectangle rect, int xOffsetR, int xOffsetL, int yOffset)
        {
            walk[i] = rect;
            walkRightXOffsets[i] = xOffsetR;
            walkLeftXOffsets[i] = xOffsetL;
            walkYOffsets[i] = yOffset;
        }
        public override void Draw(GameView view, SpriteBatch spriteBatch)
        {
            if (!ready || type == PlayerClass.Spectator)
                return;
            Vec2 drawPos = GetDrawPosition();
            screenPos = new Vec2((int)(drawPos.X - view.CamStart.X), (int)(view.CamStart.Y - drawPos.Y));
            float dx=0, dy=0, angle=(float)Math.PI/2;
            if (hook != null)
            {
                dx = hook.Position.X - drawPos.X;
                dy = hook.Position.Y - drawPos.Y;
                angle = (float)Math.Atan2(dy, -dx);
            }
            
            if(hook != null)
                spriteBatch.Draw(GraphicsUtil.pixel, new Vec2(hook.Position.X - view.CamStart.X, view.CamStart.Y - hook.Position.Y).XNAVec, null, Color.Gray, angle, Vec2.Zero.XNAVec, new Vec2((float)Math.Sqrt(dx * dx + dy * dy), 1).XNAVec, SpriteEffects.None, 0);
            if((hook != null && hook.anchored && (Math.Abs(dx) >= HalfSize.X * 2 && Math.Abs(dy) >= HalfSize.Y * 2)))
            {
                spriteBatch.Draw(sprite, new Rectangle((int)(screenPos.X), (int)(screenPos.Y), flying.Width, flying.Height), flying, Color.White, angle - (float)Math.PI / 2, rotateCenter.XNAVec, dx < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
            else{
                facingDirection = stance.Update(ArmAngle);

                int stanceWidth = stance.GetWidth();
                int stanceHeight = stance.GetHeight();
                int totalHeight = walk[idx].Height + stanceHeight - stance.GetYOffset();

                int topXOffset = (facingDirection ? -stance.GetRightXOffset() : -stance.GetLeftXOffset());
                int topY = (int)screenPos.Y - totalHeight / 2 - walkYOffsets[idx];
                stance.Draw(spriteBatch, new Rectangle((int)(screenPos.X - HalfSize.X) + topXOffset, topY, stanceWidth, stanceHeight));
                if (stance.GetYOffset() < 0)
                    topY += stance.GetYOffset();
                Rectangle bottomRectangle = walk[idx];
                SpriteEffects effect = facingDirection ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                spriteBatch.Draw(sprite, new Rectangle((int)(screenPos.X - HalfSize.X) + (facingDirection ? -walkRightXOffsets[idx] : -walkLeftXOffsets[idx]), topY + stanceHeight - stance.GetYOffset(), bottomRectangle.Width, bottomRectangle.Height), bottomRectangle, Color.White, 0, Vec2.Zero.XNAVec, effect, 1);
            }

            ((ClientLevel)Level).DrawEntity(this, spriteBatch);
        }
        public void DrawCrosshair(GameView view, SpriteBatch spriteBatch, bool canGrapple)
        {
            if(stance != null)
                spriteBatch.Draw(sprite, stance.GetCrosshairPosition(screenPos, ArmAngle).XNAVec, stance.GetCrosshair(canGrapple), Color.White);
        }
        public override void Step(int time)
        {
            base.Step(time);
            if (type == PlayerClass.Spectator || Util.IsServer)
                return;
            bool backwards = direction != facingDirection;
            if (moving)
                animate = true;
            if (idx == 0)
            {
                if (!moving || jumping)
                    animate = false;
                else if (player is NetworkPlayer)
                {
                    if (playSoundNextFrame)
                    {
                        PlayWalkSound();
                        playSoundNextFrame = false;
                    }
                    else
                        playSoundNextFrame = true;
                }
            }
            if (animate)
            {
                if (time - stepTime > 50)
                {
                    if (backwards)
                    {
                        idx--;
                        if (idx == -1)
                            idx = 3;
                    }
                    else
                    {
                        idx++;
                        if (idx == 4)
                            idx = 0;
                    }
                    stepTime = time;
                }
            }
        }
        public Texture2D GetTexture()
        {
            return sprite;
        }
        public GrapplingHook FireGrapplingHook()
        {
            Vec2 v = new Vec2((float) Math.Cos(ArmAngle), (float) -Math.Sin(ArmAngle));
            GrapplingHook h = new GrapplingHook();
            h.Position = stance.GetEnd(ArmAngle);
            Level.AddProjectile(h, ArmAngle, this);
            hook = h;
            return h;
        }
        public void SetGrapplingHook(GrapplingHook h)
        {
            jumping = true;
            hook = h;
        }
    }
}
