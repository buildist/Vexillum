using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.view;
using Vexillum.util;

namespace Vexillum.physics
{
    public class ParticleSystem
    {
        public bool delete = false;
        public bool done = false;

        protected Level level;

        public List<Particle> particles = new List<Particle>();

        public Vec2 position;
        public float rotation;
        protected float rate;
        protected Vec2 gravity;
        protected float force;
        protected float randomness;
        protected int lifetime;
        protected Texture2D texture;
        protected int maxParticles = 0;
        private int totalParticles;

        public ParticleSystem(Vec2 position, float rotation, float rate, Vec2 gravity, float force, float randomness, int lifetime)
        {
            this.position = position;
            this.rotation = rotation;
            this.rate = rate;
            this.gravity = gravity;
            this.force = force;
            this.randomness = randomness;
            this.lifetime = lifetime;
        }

        public void SetLevel(Level l)
        {
            level = l;
        }

        public void step()
        {
            if (level.random.NextDouble() < rate && !done && (totalParticles < maxParticles || maxParticles == 0))
            {
                float randForce = (float)level.random.NextDouble() * 2 - 1;
                float angle = rotation + randForce * randomness;
                Vec2 v = new Vec2((float) Math.Cos(angle), (float) Math.Sin(angle)) * force;
                Particle p = new Particle(level, getPosition(), v);
                p.setImage(getImage());
                particles.Add(p);
                totalParticles++;
            }
            if ((done || totalParticles >= maxParticles) && particles.Count == 0)
                delete = true;
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                int time = level.GetTime() - p.origin;
                p.scale = getScale(time);
                Vec2 newPosition = p.position + p.velocity;
                if (level.terrain.GetTerrain((int)newPosition.X, (int)newPosition.Y))
                    p.velocity *= -0.75f;
                else {
                    p.position += p.velocity;
                    p.velocity += gravity;
                }
                if (time > lifetime)
                {
                    particles.Remove(p);
                    i--;

                }
            }
        }

        protected virtual float getScale(int time)
        {
            return 1 - (float)time / lifetime;
        }

        protected virtual Vec2 getPosition()
        {
            return position;
        }

        protected virtual Texture2D getImage()
        {
            return texture;
        }
    }
}
