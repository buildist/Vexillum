using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.physics;
using Vexillum.util;
using Vexillum.view;

namespace  Vexillum.Entities
{
    public class GrapplingHook : Projectile
    {
        private static Texture2D hook;
        private Entity owner;
        private Entity hitEntity;
        private Vec2 hitOffset;
        static GrapplingHook() {
            hook = AssetManager.loadTexture("hook.png");
        }
        public GrapplingHook()
        {
            //clientSide = true;
            Texture = hook;
            Size = new Vec2(6, 6);
            AddedClient = delegate(ClientLevel l)
            {
                l.PlaySound(null, Sounds.ROCKET, this);
            };
            RemovedClient = delegate(ClientLevel l)
            {
                ((HumanoidEntity)owner).SetGrapplingHook(null);
            };
            Removed = delegate()
            {
                ((HumanoidEntity)owner).SetGrapplingHook(null);
            };
        }
        public override void Setup(float angle, Entity owner)
        {
            this.owner = owner;
            Rotation = angle;
            Vec2 v = new Vec2((float)Math.Cos(angle), (float)-Math.Sin(angle)) * 24;
            FixedVelocity = v;
        }
        public override void OnCollide(Entity e, int direction)
        {
            FixedVelocity = Vec2.Zero;
            Velocity = Vec2.Zero;
            anchored = true;
            enablePhysics = false;
            /*if (e != null)
            {
                hitEntity = e;
                hitOffset = Position - e.Position;
            }*/
            ((HumanoidEntity)owner).xVelocity = 0;
            ((HumanoidEntity)owner).moving = false;
            Level.GetTaskQueue().AddConditionalTask(delegate()
            {
                if (Level == null)
                    return false;
                if (hitEntity != null)
                    Position = hitEntity.Position - hitOffset;
                Vec2 diff = Position - owner.Position;
                float l2 = diff.LengthSquared();
                if (((hitEntity == null || hitEntity.removed) && l2 < owner.Size.Y*owner.Size.Y) || (hitEntity != null && hitEntity.removed))
                {
                    ((HumanoidEntity)owner).SetGrapplingHook(null);
                    owner.Velocity *= 0.25f;
                    if(owner.Velocity.Y > 1f)
                        ((HumanoidEntity)owner).Jump();
                    Level.RemoveEntity(this);
                    return false;
                }
                diff.Normalize();
                if ((hitEntity == null || l2 > 50 * 50) && (owner.Velocity.Length() < 20 || (owner.Velocity.X * diff.X < 0 && owner.Velocity.Y * diff.Y < 0)))
                    owner.Force = diff * 2;
                else
                    owner.Force = Vec2.Zero;
                return true;
            });
        }
        public override void OnClientCollide(Entity e, int direction)
        {
            if (owner == null)
                return;
            FixedVelocity = Vec2.Zero;
            Velocity = Vec2.Zero;
            anchored = true;
            enablePhysics = false;
            /*if (e != null)
            {
                hitEntity = e;
                hitOffset = Position - e.Position;
            }*/
            ((HumanoidEntity)owner).xVelocity = 0;
            ((HumanoidEntity)owner).moving = false;
            ((GameView)Vexillum.game.View).AddConditionalTask(delegate() {
                if (Level == null)
                    return false;
                if (hitEntity != null)
                    Position = hitEntity.Position - hitOffset;
                Vec2 diff = Position - owner.Position;
                float l2 = diff.LengthSquared();
                if (((hitEntity == null || hitEntity.removed) && l2 < owner.Size.Y * owner.Size.Y) || (hitEntity != null && hitEntity.removed))
                {
                    ((HumanoidEntity)owner).SetGrapplingHook(null);
                    owner.Velocity *= 0.25f;
                    if (owner.Velocity.Y > 1f)
                        ((HumanoidEntity)owner).Jump();
                    return false;
                }
                diff.Normalize();
                if ((hitEntity == null || l2 > 50 * 50) && (owner.Velocity.Length() < 20 || (owner.Velocity.X * diff.X < 0 && owner.Velocity.Y * diff.Y < 0)))
                    owner.Force = diff * 2;
                else
                    owner.Force = Vec2.Zero;
                return true;
            });
        }
        public override Entity GetOwner()
        {
            return owner;
        }
        public override void Draw(GameView view, SpriteBatch spriteBatch)
        {
            if(!anchored && hitEntity == null)
                base.Draw(view, spriteBatch);
        }
    }
}
