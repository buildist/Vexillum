using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Vexillum.physics;
using Vexillum.util;
using Vexillum.Entities.Weapons;

namespace Vexillum.Entities
{
    class ClusterBomb : Projectile
    {
        public ParticleSystem particleSystem;
        private static Texture2D rocket;
        private const float force = 2;
        private static Vec2[] velocities = new Vec2[] { new Vec2(-force, 0), new Vec2(force, 0), new Vec2(0, force), new Vec2(0, force) };
        private Entity owner;
        private Weapon weapon;
        static ClusterBomb()
        {
            rocket = AssetManager.loadTexture("missile_cluster.png");
        }
        public ClusterBomb()
        {
            Texture = rocket;
            Size = new Vec2(14, 5);
            particleSystem = new Fire(this);
            AddedClient = delegate(ClientLevel l)
            {
                l.AddParticleSystem(particleSystem);
                l.PlaySound(null, Sounds.ROCKET, this);
                ((LocalPlayer)((ClientLevel)Level).player).UpdateCanGrapple();
            };
            RemovedClient = delegate(ClientLevel l)
            {
                particleSystem.done = true;
                Vec2 unitVelocity = Velocity;
                unitVelocity.Normalize();
                ((LocalPlayer)((ClientLevel)Level).player).UpdateCanGrapple();
            };
        }
        public override void Setup(float angle, Entity owner)
        {
            this.owner = owner;
            Rotation = angle;
            Vec2 v = new Vec2((float)Math.Cos(angle), (float)-Math.Sin(angle)) * 8;
            this.weapon = ((HumanoidEntity)owner).Weapon;
            FixedVelocity = v;
        }
        public override void OnCollide(Entity e, int direction)
        {
            Vec2 unitVelocity = Velocity;
            unitVelocity.Normalize();
            Level.Explode((int)(Position.X + unitVelocity.X * 3), (int)(Position.Y + unitVelocity.Y * 3), 0, true, owner.player, weapon);
            particleSystem.done = true;
            Level.RemoveEntity(this);
            for (int i = 0; i < 4; i++)
            {
                Bomblet b = new Bomblet();
                b.Position = Position;
                b.Velocity = velocities[i];
                b.ownerPlayer = owner.player;
                Level.AddEntity(b);
            }
        }
        public override void OnClientCollide(Entity e, int direction)
        {
            enablePhysics = false;
            particleSystem.done = true;
        }
        public override Entity GetOwner()
        {
            return owner;
        }
    }
}
