using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Vexillum.Entities;
using Vexillum.view;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Vexillum.util;
using Vexillum.Game;
using Vexillum.physics;
using Vexillum.Entities.Weapons;
using Vexillum.Game;

namespace Vexillum
{
    public class ClientLevel: Level
    {
        protected List<TerrainParticle> terrainParticles = new List<TerrainParticle>(1024);
        private List<BulletTrace> traces = new List<BulletTrace>(32);
        protected List<ParticleSystem> particleSystems = new List<ParticleSystem>(64);
        public LocalPlayer player;
        private Texture2D sky;
        private Texture2D left;
        private Texture2D right;
        private Texture2D bottom;

        public ClientLevel(string shortName, string longName, System.Drawing.Bitmap main, System.Drawing.Bitmap background, System.Drawing.Bitmap sky, System.Drawing.Bitmap left, System.Drawing.Bitmap right, System.Drawing.Bitmap bottom, System.Drawing.Bitmap collision, List<util.Region> regions)
            : base(shortName, longName, main, background, collision, regions)
        {
            this.sky = Util.loadTexture(sky);
            this.left = Util.loadTexture(left);
            this.right = Util.loadTexture(right);
            this.bottom = Util.loadTexture(bottom);
        }

        public override bool Destroy(int x, int y)
        {
            if (base.Destroy(x, y))
            {
                
                Color xnaColor;
                if (terrain.GetTransparent(x, y))
                {
                    xnaColor = Color.Transparent;
                }
                else
                {
                    System.Drawing.Color color = BackgroundBitmap.GetPixel(x, (int)Size.Y - y - 1);
                    xnaColor = new Color(color.R, color.G, color.B);
                }
                MainTexture.SetData<Color>(0, new Rectangle(x, (int)Size.Y - y-1, 1, 1), new Color[] { xnaColor }, 0, 1);
                if (random.NextDouble() < 0.1)
                {
                    TerrainParticle p = new TerrainParticle(this, MainBitmap.GetPixel(x, (int)Size.Y - y-1), x, y);
                    AddTerrainParticle(p);
                }
                return true;
            }
            return false;
        }
        public override void AddEntity(Entities.Entity e, bool isPlayer)
        {
            base.AddEntity(e, isPlayer);
            e.AddedClient(this);
        }
        public override void AddProjectile(Projectile e, float angle, Entity owner)
        {
            base.AddProjectile(e, angle, owner);
            e.AddedClient(this);
        }
        public void AddTerrainParticle(TerrainParticle p)
        {
            terrainParticles.Add(p);
        }
        public override void Explode(int x, int y, int radius, int randomSeed, bool nonlethal, Player attacker, Weapon source)
        {
            base.Explode(x, y, radius, randomSeed, nonlethal, attacker, source);
            if (!nonlethal)
            {
                PlaySound(Sounds.EXPLOSION, x, y);
                AddParticleSystem(new Explosion(this, new Vec2(x, y)));
                AddParticleSystem(new Sparks(this, new Vec2(x, y)));
            }
        }
        protected override void OnHitscanHit(Vec2 pos, Vec2 startPos, Vec2 unitVec)
        {
            if(terrain.GetTerrain((int) pos.X, (int) pos.Y))
                AddTerrainParticles(6, pos, unitVec, 1);
            traces.Add(new BulletTrace(startPos, pos));
        }
        public void AddTerrainParticles(int n, Vec2 pos, Vec2 unitVec, float force)
        {
            bool xd = unitVec.X > 0;
            bool yd = unitVec.Y > 0;
            for (int i = 0; i < n; i++)
            {
                int rxv = random.Next(-1, 2);
                int ryv = random.Next(-1, 2);
                int rxp = random.Next(-1, 2);
                int ryp = random.Next(-1, 2);
                int tx = (int)(pos.X - unitVec.X * 2);
                int ty = (int)(pos.Y - unitVec.Y * 2);
                if (!(ty + ryp < 0 || tx + rxp < 0 || tx + rxp >= Size.X || ty + ryp >= Size.Y) && !(pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y))
                {
                    TerrainParticle p = new TerrainParticle(this, MainBitmap.GetPixel((int)(pos.X), (int)(Size.Y - (int)pos.Y - 1)), tx + rxp, ty + ryp);
                    p.velocity = new Vec2(rxv, ryv) + new Vec2(xd ? -force : force, yd ? -force : force);
                    AddTerrainParticle(p);
                }
            }
        }
        public override void RemoveEntity(Entity e)
        {
            base.RemoveEntity(e);
            e.RemovedClient(this);
            e.Level = null;
        }
        public void AddParticleSystem(physics.ParticleSystem s)
        {
            s.SetLevel(this);
            particleSystems.Add(s);
        }
        private void RemoveParticleSystem(physics.ParticleSystem s)
        {
            particleSystems.Remove(s);
        }
        public void Draw(GameView view, GraphicsDevice g, SpriteBatch spriteBatch, Rectangle worldRectangle, int ix, int iy, int width, int height)
        {
            if (sky != null)
            {
                DrawLayer(spriteBatch, ((int)(Size.X - view.CamPosition.X)/4 % view.width), sky);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
    DepthStencilState.Default, RasterizerState.CullNone);
                //DrawLayer(spriteBatch, ((int)(Size.X -view.CamPosition.X)%view.width)/4, sky.clouds1);
                //DrawLayer(spriteBatch, 100, sky.clouds2);
                spriteBatch.End();
                Vexillum.game.BeginSpriteBatchScaled();
            }
            int x = 0, y = 0, lx=-1, rx=-1, lix=0, lw=0, rix=0, rw=0, by=-1, bh=0, bix=0;
            int oldWidth = width;
            if (ix < 0)
            {
                x = -ix;
                ix = 0;
                lx = 0;
                lix = bix = left.Width - x;
                lw = left.Width - lix;
            }
            else
                bix = ix+450;
            if (iy >= Size.Y)
            {
                y = iy - (int)Size.Y - 1;
                iy = (int)Size.Y;
            }
            if (ix + width > this.Size.X)
            {
                width = (int)this.Size.X - ix;
                rx = width;
                rix = 0;
                rw = oldWidth - width;
            }
            if (iy - height < 0)
            {
                bh = height - iy;
                by = iy;
                height = iy;
                //bh = height - by;
            }
            iy = (int)Size.Y - iy - 1;
            spriteBatch.Draw(MainTexture, new Rectangle(x, y, width, height), new Rectangle(ix, iy, width, height), Color.White, 0, Vec2.Zero.XNAVec, SpriteEffects.None, 1);
            if(lx != -1)
                spriteBatch.Draw(left, new Rectangle(lx, y, lw, height), new Rectangle(lix, iy, lw, height), Color.White, 0, Vec2.Zero.XNAVec, SpriteEffects.None, 1);
            if (rx != -1)
                spriteBatch.Draw(right, new Rectangle(rx, y, rw, height), new Rectangle(0, iy, rw, height), Color.White, 0, Vec2.Zero.XNAVec, SpriteEffects.None, 1);
            if(by != -1)
                spriteBatch.Draw(bottom, new Rectangle(0, by, oldWidth, bh), new Rectangle(bix, 0, oldWidth, bh), Color.White, 0, Vec2.Zero.XNAVec, SpriteEffects.None, 1);
            visibleEntities = 0;
            foreach (Entity e in Entities)
            {
                if (e.Position.X > worldRectangle.X - e.HalfSize.X && e.Position.Y > worldRectangle.Y - e.HalfSize.Y && e.Position.X < worldRectangle.X + worldRectangle.Width + e.HalfSize.X && e.Position.Y < worldRectangle.Y + worldRectangle.Height + e.HalfSize.Y)
                {
                    e.Draw(view, spriteBatch);
                    visibleEntities++;
                }
                //TextRenderer.DrawString(spriteBatch, TextRenderer.TinyFont, e.ID + "", e.GetScreenPosition(), Color.White, false);
            }

            foreach (TerrainParticle p in terrainParticles)
            {
                if (worldRectangle.Contains(p.GetX(), p.GetY()))
                    spriteBatch.Draw(GraphicsUtil.pixel, new Rectangle((int)(p.GetX() - view.CamStart.X), (int)(view.CamStart.Y - p.GetY()), 2, 2), p.color);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            for (int i = 0; i < traces.Count; i++)
            {
                traces[i].Draw(view, this, spriteBatch);
                if (traces[i].deleted)
                {
                    traces.RemoveAt(i);
                    i--;
                }
            }
            foreach (ParticleSystem s in particleSystems)
            {
                foreach (Particle p in s.particles)
                {
                    p.draw(s, view, g, spriteBatch);
                }
            }
            spriteBatch.End();
            Vexillum.game.BeginSpriteBatch();
            //DrawCollisionBoxes(view, spriteBatch);
        }
        private void DrawLayer(SpriteBatch spriteBatch, int xOffset, Texture2D texture)
        {
            spriteBatch.Draw(texture, new Vec2(xOffset, 0).XNAVec, Color.White);
            spriteBatch.Draw(texture, new Vec2(xOffset - texture.Width, 0).XNAVec, Color.White);
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
            for(int i = 0; i < audioEmitters.Count; i++)
            {
                Entity ent = audioEmitters.Keys.ElementAt(i);
                if (ent.removed)
                {
                    audioEmitters.Remove(ent);
                    i--;
                }
                else
                {
                    SoundDef def = audioEmitters.Values.ElementAt(i);
                    def.emitter.Position = new Vector3(ent.Position.X, ent.Position.Y, 0);
                    def.emitter.Velocity = new Vector3(ent.Velocity.X, ent.Velocity.Y, 0);
                    def.instance.Apply3D(player.Listener, def.emitter);
                }
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
        protected override void Collision(Entity e1, Entity e2, int direction)
        {
            e1.OnClientCollide(e2, direction);
            if (e2 != null)
                e2.OnClientCollide(e1, direction);
        }
        public override void PlaySound(SoundEffect e)
        {
            e.Play();
        }
        public override void PlaySound(Sounds name, int x, int y)
        {
            SoundEffect e = AssetManager.GetSound(name);
            AudioEmitter emitter = new AudioEmitter();
            Vector3 pos = new Vector3(x, y, 0);
            emitter.Position = pos;
            emitter.Up = new Vector3(0, 1, 0);
            emitter.Forward = new Vector3(0, 0, 1);

            SoundEffectInstance instance = e.CreateInstance();
            instance.Apply3D(player.Listener, emitter);
            instance.Play();
        }
        public override void PlaySound(Player ignorePlayer, Sounds name, Entity sourceEntity)
        {
            SoundEffect e = AssetManager.GetSound(name);
            AudioEmitter emitter = new AudioEmitter();
            Vector3 pos = new Vector3(sourceEntity.Position.X, sourceEntity.Position.Y, 0);
            emitter.Position = pos;
            emitter.Up = new Vector3(0, 1, 0);
            emitter.Forward = new Vector3(0, 0, 1);

            SoundEffectInstance instance = e.CreateInstance();
            if (player != null)
            {
                instance.Apply3D(player.Listener, emitter);
                instance.Play();
            }

            audioEmitters[sourceEntity] = new SoundDef(emitter, instance);
        }
        public void ApplyTerrainState()
        {
            for (int x = 0; x < (int)Size.X; x++)
            {
                for (int y = 0; y < (int)Size.Y; y++)
                {
                    if (!terrain.GetTerrain(x, y) && terrain.GetCollisionData(x, y) != TerrainCollisionType.Empty)
                    {
                        Color xnaColor;
                        if (terrain.GetTransparent(x, y))
                        {
                            xnaColor = Color.Transparent;
                        }
                        else
                        {
                            System.Drawing.Color color = BackgroundBitmap.GetPixel(x, (int)Size.Y - y - 1);
                            xnaColor = new Color(color.R, color.G, color.B);
                        }
                        MainTexture.SetData<Color>(0, new Rectangle(x, (int)Size.Y - y - 1, 1, 1), new Color[] { xnaColor }, 0, 1);
                    }
                }
            }
        }
        public void SetLocalPlayer(LocalPlayer p)
        {
            player = p;
        }
        public virtual IGameMode GetGameMode()
        {
            return player.GetGameMode();
        }
        public virtual void DrawEntity(HumanoidEntity e, SpriteBatch s)
        {
            player.GetGameMode().DrawEntity(e, s);
            if (!(e.player is LocalPlayer))
            {
                Vec2 pos = e.GetScreenPosition() + new Vec2(-TextRenderer.MeasureString(TextRenderer.DefaultFont, e.player.name).X / 2, -e.HalfSize.Y - 24);
                TextRenderer.DrawString(s, TextRenderer.DefaultFont, e.player.name, pos, GetGameMode().GetNameColor(e.player), true);
            }
        }
    }
}
