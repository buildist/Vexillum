using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Game;
using Vexillum.Entities;
using Microsoft.Xna.Framework.Graphics;
using Vexillum.physics;
using Microsoft.Xna.Framework.Audio;
using Vexillum.util;

namespace Vexillum
{
    class MenuLevel : ClientLevel
    {
        public MenuLevel(string shortName, string longName, System.Drawing.Bitmap main, System.Drawing.Bitmap background, System.Drawing.Bitmap sky, System.Drawing.Bitmap left, System.Drawing.Bitmap right, System.Drawing.Bitmap bottom, System.Drawing.Bitmap collision, List<util.Region> regions) : base(shortName, longName, main, background, sky, left, right, bottom, collision, regions)
        {
        }
        private IGameMode gameMode;
        public override IGameMode GetGameMode()
        {
            return null;
        }
        public override void DrawEntity(HumanoidEntity e, SpriteBatch s)
        {
        }
        protected override void Collision(Entity e1, Entity e2, int direction)
        {
            e1.OnCollide(e2, direction);
            if (e2 != null)
                e2.OnCollide(e1, direction);
        }
        public override void PlaySound(SoundEffect e)
        {

        }
        public override void PlaySound(Sounds name, int x, int y)
        {

        }
        public override void PlaySound(Player ignorePlayer, Sounds name, Entity sourceEntity)
        {

        }
        public override void Step(long gameTime)
        {
            frame++;
            this.gameTime = (int)gameTime;
            DoPhysics();
            for (int i = 0; i < terrainParticles.Count; i++)
            {
                TerrainParticle p = terrainParticles[i];
                terrain.SetParticle(p.GetX(), p.GetY(), false);
                p.step();
                if (p.removed)
                {
                    terrainParticles.RemoveAt(i);
                    i--;
                }
                else
                    terrain.SetParticle(p.GetX(), p.GetY(), true);
            }
            for (int i = 0; i < particleSystems.Count; i++)
            {
                ParticleSystem s = particleSystems.ElementAt(i);
                s.step();
                if (s.delete)
                {
                    particleSystems.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
