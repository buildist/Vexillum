using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Vexillum.view;
using Vexillum.Entities;
using Microsoft.Xna.Framework.Audio;
using Vexillum.physics;
using Vexillum.Game;
using Vexillum.util;
using Vexillum.Entities.Weapons;
using System.Drawing.Imaging;

namespace Vexillum
{
    public abstract class Level
    {
        public string ShortName;
        public readonly Vec2 Size;
        protected int width;
        protected int height;

        public int frame = 0;
        protected int gameTime;

        public Texture2D MainTexture;
        public System.Drawing.Bitmap MainBitmap;
        public System.Drawing.Bitmap BackgroundBitmap;

        public Dictionary<int, Entity> EntityIndex = new Dictionary<int,Entity>();
        protected List<Entity> Entities = new List<Entity>();

        public TerrainArray terrain;
        protected Dictionary<PlayerClass, List<Region>> SpawnPositions = new Dictionary<PlayerClass,List<Region>>();
        protected Dictionary<PlayerClass, Region> flagPositions = new Dictionary<PlayerClass, Region>(2);
        protected List<Region> ladders = new List<Region>(8);

        protected Dictionary<Entity, SoundDef> audioEmitters = new Dictionary<Entity, SoundDef>();
        public float Gravity = 0.3f;

        public float friction = 0.5f;

        private ContactFilter contactFilter = new ContactFilter();

        public Random random = new Random();

        public int visibleEntities;
        public int frameDiff;

        private float cameraShakeAmount = 0;
        public Vec2 cameraShake = Vec2.Zero;
        private float cameraShakeFrame = 0;

        public Level(string shortName, string longName, System.Drawing.Bitmap main, System.Drawing.Bitmap background, System.Drawing.Bitmap collision, List<util.Region> regions)
        {
            ShortName = shortName;

            width = main.Width;
            height = main.Height;
            Size = new Vec2(main.Width, main.Height);

            terrain = new TerrainArray((int)Size.X, (int)Size.Y);

            BitmapData cData = collision.LockBits(new System.Drawing.Rectangle(0, 0, collision.Width, collision.Height),
                 System.Drawing.Imaging.ImageLockMode.ReadWrite, collision.PixelFormat);

            byte[] cBytes = new byte[cData.Stride * cData.Height];
            System.Runtime.InteropServices.Marshal.Copy(cData.Scan0, cBytes, 0
                                   , cBytes.Length);
            collision.UnlockBits(cData);

            uint cColor;
            int ptr = 0;
            int stride = cData.Stride;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ptr = (height-y-1) * stride + x * 4;
                    cColor = 0;
                    byte red = cBytes[ptr+2];
                    byte green = cBytes[ptr + 1];
                    cColor |= (uint)(cBytes[ptr+3]<<24);
                    cColor |= (uint)(red<<16);
                    cColor |= (uint)(green<<8);
                    cColor |= (uint)(cBytes[ptr]<<0);
                    switch (cColor)
                    {
                        case 0xFFFF00FF:
                            terrain.SetTerrainAndCollisionData(x, y, 0, TerrainCollisionType.Empty, 0);
                            break;
                        case 0:
                            terrain.SetTerrainAndCollisionData(x, y, 1, TerrainCollisionType.Solid, 0);
                            break;
                        case 0xFFFFFFFF:
                            terrain.SetTerrainAndCollisionData(x, y, 1, TerrainCollisionType.Solid, 0);
                            break;
                        case 0xFF0000FF:
                            terrain.SetTerrainAndCollisionData(x, y, 0, TerrainCollisionType.Empty, 0);
                            main.SetPixel(x, (int)Size.Y - y - 1, GraphicsUtil.transparent);
                            break;
                        case 0xFFFFFF00:
                            terrain.SetTerrainAndCollisionData(x, y, 0, TerrainCollisionType.Empty, 1);
                            
                            break;
                        case 0xFFFFFF80:
                            terrain.SetTerrainAndCollisionData(x, y, 0, TerrainCollisionType.Empty, 1);
                            main.SetPixel(x, (int)Size.Y - y - 1, GraphicsUtil.transparent);
                            break;
                        default:
                            int cl = red/16;
                            if(cl > 15)
                                cl = 15;
                            else if(cl < 2)
                                cl = 2;
                            terrain.SetTerrainAndCollisionData(x, y, 1, (byte) cl, 0);
                            if (green == 128)
                                terrain.SetTransparent(x, y, true);
                            break;
                    }
                }
            }
            if (!Util.IsServer)
            {
                MainBitmap = (System.Drawing.Bitmap)main.Clone();
                MainBitmap.MakeTransparent(GraphicsUtil.transparent);
                MainTexture = Util.loadTexture(MainBitmap);
                BackgroundBitmap = background;
            }
            main = null;
            collision = null;
        }

        public int GetTime()
        {
            return (int) gameTime;
        }
        public void Explode(int x, int y, int radius, Player player, Weapon weapon)
        {
            Explode(x, y, radius, DateTime.Now.Millisecond, false, player, weapon);
        }
        public void Explode(int x, int y, int radius, bool nonlethal, Player player, Weapon weapon)
        {
            Explode(x, y, radius, DateTime.Now.Millisecond, nonlethal, player, weapon);
        }
        public virtual void Explode(int x, int y, int radius, int randomSeed, Player attacker, Weapon source)
        {
            Explode(x, y, radius, randomSeed, false, attacker, source);
        }
        public virtual void Explode(int x, int y, int radius, int randomSeed, bool nonlethal, Player attacker, Weapon source)
        {
            int damageRadius = 3 * radius / 2;
            Random random = new Random(randomSeed);
            List<Entity> exploded = new List<Entity>();
            Vec2 center = new Vec2(x, y);
            if (radius < 4)
            {
                DrawCircle(center, center, radius, radius, radius, true, exploded, null, true);
                return;
            }
            const int numTraces = 16;
            float rand = (float)(random.NextDouble() * Math.PI);
            for (int i = 0; i < numTraces; i++)
            {
                int traceRadius = 8;
                float angle = ((float)Math.PI * 2) * ((float)i / numTraces) + rand;
                float sx = (float)Math.Cos(angle);
                float sy = (float)Math.Sin(angle);
                bool doDamage = true;
                const int step = 2;
                for (float t = 0; t < damageRadius; t += step)
                {
                    if (t >= radius)
                        doDamage = false;
                    Vec2 pos = center + new Vec2(sx, sy) * t;
                    int collision = terrain.GetCollisionData((int)pos.X, (int)pos.Y);
                    {
                        float fraction;
                        if (collision == TerrainCollisionType.Empty)
                            fraction = 0.5f;
                        else if (collision == TerrainCollisionType.Solid)
                        {
                            fraction = 0;
                            doDamage = false;
                        }
                        else
                            fraction = (float)(collision - 2) / 14;
                        int maxRadius = (int)(radius * fraction*0.8);
                        double r = random.NextDouble();
                        int r2 = random.Next(16);
                        float offset = 0.05f * step;
                        switch (r2)
                        {
                            case 15:
                                sx *= (1+offset);
                                break;
                            case 14:
                                sy *= (1+offset);
                                break;
                            case 13:
                                sx *= (1 - offset);
                                break;
                            case 12:
                                sy *= (1 - offset);
                                break;
                        }
                        if (t > maxRadius && r < ((float)(t-maxRadius)/(radius-maxRadius))*8)
                        {
                            traceRadius -= step;
                            if (traceRadius <= 0)
                                doDamage = false;
                        }
                        DrawCircle(pos, center, traceRadius, 8, damageRadius, nonlethal, exploded, delegate(Entity e, float distance, float ratio)
                        {
                            if (e.player != null && e.player is LocalPlayer)
                            {
                                cameraShakeAmount = ratio;
                                const float k = 64f;
                                cameraShake = new Vec2(((float)random.NextDouble() - 0.5f) * k, ((float)random.NextDouble() - 0.5f) * k);
                                cameraShakeFrame = 0;
                                ((GameView)Vexillum.game.View).AddConditionalTask(delegate()
                                {
                                    //if (cameraShakeFrame == 2)
                                    {
                                        cameraShakeAmount *= 0.75f;
                                        cameraShakeFrame = 0;
                                        cameraShake = new Vec2(((float)random.NextDouble() - 0.5f) * k * cameraShakeAmount, ((float)random.NextDouble() - 0.5f) * k * cameraShakeAmount)/100f;
                                        if (cameraShakeAmount > 0.01f)
                                            return true;
                                        else
                                        {
                                            cameraShake = Vec2.Zero;
                                            return false;
                                        }
                                    }
                                    cameraShakeFrame++;
                                    return true;
                                });
                            }
                        }, doDamage && collision != TerrainCollisionType.Empty);
                    }
                }
            }
        }
        public delegate void EntityDelegate(Entity e, float distance, float ratio);
        protected void DrawCircle(Vec2 center, Vec2 explodeCenter, int radius, int circleDamageRadius, int explodeRadius, bool nonlethal, List<Entity> exploded, EntityDelegate onEntityExplode, bool destroy)
        {
            int dx, dy;
            int outer = radius * radius;
            int damageOuter = circleDamageRadius * circleDamageRadius;
            for (dx = -circleDamageRadius; dx < circleDamageRadius; dx++)
            {
                for (dy = -circleDamageRadius; dy < circleDamageRadius; dy++)
                {
                    if (dx * dx + dy * dy < damageOuter && (true))
                    {
                        if (!nonlethal)
                        {
                            int s = terrain.GetEntity(null, (int)center.X + dx, (int)center.Y+ dy, this);
                            if (s != 0)
                            {
                                Entity e = GetEntityByID(s);
                                if (e != null && !exploded.Contains(e))
                                {
                                    Vec2 v = (e.Position - explodeCenter);
                                    float d = v.Length();
                                    v.Normalize();
                                    float ratio = 1 - Math.Min(1, d / explodeRadius);
                                    e.Velocity += v * ratio * 16f;
                                    e.jumping = true;
                                    exploded.Add(e);
                                    if (onEntityExplode != null)
                                        onEntityExplode(e, d, ratio);
                                }
                            }
                        }
                        if (destroy && dx * dx + dy * dy < outer)
                            Destroy((int)center.X + dx, (int)center.Y + dy);
                    }
                }
            }
        }
        public virtual bool Destroy(int x, int y)
        {
            return terrain.Destroy(x, y);
        }
        public Entity GetEntityByID(int id)
        {
            try
            {
                return EntityIndex[id];
            }
            catch (Exception ex)
            {
                Util.Debug(this, "Tried to get unkown entity: " + id);
                return null;
            }
        }
        public void EnableEntity(Entity e)
        {
            e.enablePhysics = true;
        }
        public void AddEntity(Entity e)
        {
            AddEntity(e, false);
        }
        public virtual void AddEntity(Entity e, bool isPlayer)
        {
            e.Level = this;
            EntityIndex.Add(e.ID, e);
            Entities.Add(e);
            SetEntityPosition(e, e.Position);
            e.isPlayer = isPlayer;
        }
        public virtual void AddProjectile(Projectile e, float angle, Entity owner)
        {
            AddProjectile(e, -1, angle, owner);
        }
        protected void AddProjectile(Projectile e, short id, float angle, Entity owner)
        {
            if(id != -1)
                e.ID = id;
            e.Level = this;
            EntityIndex.Add(e.ID, e);
            SetEntityPosition(e, e.Position);
            Entities.Add(e);
            e.Setup(angle, owner);
        }
        public virtual void RemoveEntity(Entity e)
        {
            terrain.SetEntityState(e, e.positionInLevel, false);
            e.removed = true;
            EntityIndex.Remove(e.ID);
            Entities.Remove(e);
        }
        public void AddHitscan(Vec2 startPos, float angle, Entity ignore)
        {
            Vec2 unitVec = new Vec2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            Vec2 pos = startPos;
            bool done = false;
            bool terrain = false;
            Entity entity = null;
            int x, y, px, py;
            px = (int)pos.X;
            py = (int)pos.Y;
            while (!done)
            {
                x = (int)pos.X;
                y = (int)pos.Y;
                if (this.terrain.GetTerrain(x, y))
                {
                    done = true;
                    terrain = true;
                }
                else
                {
                    foreach (Entity e in Entities)
                    {
                        if (e.TestPoint(x, y) && e != ignore)
                        {
                            done = true;
                            entity = e;
                        }
                    }
                }
                if(!done)
                    pos += unitVec;
            }
            if (!terrain && entity == null)
                return;
            OnHitscanHit(pos, startPos, unitVec);
        }
        protected virtual void OnHitscanHit(Vec2 pos, Vec2 startPos, Vec2 unitVec)
        {
        }
        public Vec2 GetSpawnPosition(PlayerClass cl, Vec2 size)
        {
            if (!SpawnPositions.ContainsKey(cl) || SpawnPositions[cl].Count == 0)
                return Size / 2;
            Region r = SpawnPositions[cl].ElementAt(random.Next(SpawnPositions[cl].Count));
            return r.RandomPosition(random, size, height);
        }
        public void DrawCollisionBoxes(GameView view, SpriteBatch spriteBatch)
        {
            for(int x = (int) view.CamStart.X; x < view.CamStart.X + view.width; x++)
            {
                for(int y = (int) view.CamStart.Y; y > view.CamStart.Y - view.height; y--)
                {
                    if (x >= 0 && x < Size.X && y >= 0 && y < Size.Y && terrain.GetEntity(null, x, y, this) > 0)
                    {
                        spriteBatch.Draw(GraphicsUtil.pixel, new Vec2(x - view.CamStart.X, view.CamStart.Y - y).XNAVec, Color.Yellow);
                    }
                }
            }
        }
        public List<Entity> getEntities()
        {
            return Entities;
        }
        public List<Entity> GetNonProjectiles()
        {
            List<Entity> e = new List<Entity>(Entities.Count);
            foreach (Entity ent in Entities)
            {
                if(!(ent is Projectile))
                    e.Add(ent);
            }
            return e;
        }
        public int GetActionFrame()
        {
            return frame + 2;
        }
        public virtual void Step(long gameTime)
        {
            frame++;
            this.gameTime = (int) gameTime;
            DoPhysics();
        }
        public bool DoPhysicsForEntity(Entity e)
        {
            if (!e.enablePhysics || e.disabled)
                return false;
            e.Step(gameTime);
            if (e.ladder)
            {
                switch (e.ladderDirection)
                {
                    case -1:
                        e.FixedVelocity.Y = -3f;
                        break;
                    case 1:
                        e.FixedVelocity.Y = 3f;
                        break;
                }
            }
            else
            {
                if (e.ladderDirection != 0)
                {
                    e.ladderDirection = 0;
                    e.FixedVelocity.Y = 0;
                }
            }
            Vec2 newPos;

            if (e.FixedVelocity != Vec2.Zero || e.anchored)
                newPos = e.Position + e.FixedVelocity;
            else
                newPos = e.Position + e.Velocity;

            float d = (newPos - e.Position).Length();
            if(d > 2)
                e.ladder = false;
            float nx = e.Position.X, ny = e.Position.Y;
            bool xCollision = false, yCollision = false;
            Entity collideEntity = null;
            Vec2 oldVelocity = e.Velocity;

            e.xCollision = false;
            e.yCollision = false;

            for (float i = 0; i < d; i++)
            {
                float k = (float)(i + 1) / d;
                Vec2 testOffset = new Vec2(e.Velocity.X > 0 ? e.HalfSize.X : -e.HalfSize.X-1, e.Velocity.Y > 0 ? e.HalfSize.Y+1 : -e.HalfSize.Y);

                if (e.Velocity.Y != 0)
                {
                    for (float tx = nx - e.HalfSize.X + 1; tx <= nx + e.HalfSize.X - 1; tx++)
                    {
                        if (e is HumanoidEntity && terrain.GetLadder((int)tx, (int)(ny - e.HalfSize.Y + 5)))
                        {
                            e.ladder = true;
                            if (e.ladderDirection == 0 && testOffset.Y < 0)
                            {
                                yCollision = true;
                                break;
                            }
                        }
                        int c = terrain.GetEntity(e, (int)tx, (int)(ny + testOffset.Y), this);
                        if (c != 0 && contactFilter.shouldCollide(GetEntityByID(c), e))
                        {
                            collideEntity = EntityIndex[c];
                            yCollision = true;
                            break;
                        }
                        else if (terrain.GetTerrain((int)tx, (int)(ny + testOffset.Y)))
                        {
                            yCollision = true;
                            break;
                        }
                    }
                }

                float yPoint = 0;
                if (e.Velocity.X != 0)
                {
                    for (float ty = ny + e.HalfSize.Y - 1; ty >= ny - e.HalfSize.Y + 1; ty--)
                    {
                        int c = terrain.GetEntity(e, (int)(nx + testOffset.X), (int)ty, this);
                        if (c != 0 && contactFilter.shouldCollide(GetEntityByID(c), e))
                        {
                            collideEntity = EntityIndex[c];
                            xCollision = true;
                            yPoint = ty;
                            break;
                        }
                        else if (terrain.GetTerrain((int)(nx + testOffset.X), (int)ty))
                        {
                            xCollision = true;
                            yPoint = ty;
                            break;
                        }
                        if (e is HumanoidEntity && terrain.GetLadder((int)(nx + testOffset.X), (int)ty))
                        {
                            e.ladder = true;
                        }
                    }
                }

                //stepping
                if (yPoint > 0 && xCollision && yPoint - ny + e.HalfSize.Y < 7 && d > 1)
                {
                    ny = yPoint + e.HalfSize.Y;
                    break;
                }

                if (yCollision)
                {
                    e.yCollision = true;
                    if (e.Velocity.X == 0)
                        break;
                }
                else
                    ny = (newPos.Y - e.Position.Y) * k + e.Position.Y;
                if (xCollision)
                {
                    e.xCollision = true;
                    if (e.Velocity.Y == 0)
                        break;
                }
                else
                    nx = (newPos.X - e.Position.X) * k + e.Position.X;
            }

            //set new position
            if (d > 0)
            {
                terrain.SetEntityState(e, e.positionInLevel, false);
                e.Position = new Vec2(nx, ny);
                terrain.SetEntityState(e, e.Position, true);
            }

            //collision response
            if (collideEntity != null)
            {
                Vec2 dv = oldVelocity - collideEntity.Velocity;
                float dm = e.Mass + collideEntity.Mass;
                Vec2 k = dv / dm;
                Vec2 v1 = oldVelocity - k * 2 * collideEntity.Mass;
                Vec2 v2 = collideEntity.Velocity + k * 2 * e.Mass;
                if (xCollision)
                {
                    Collision(collideEntity, e, 0);
                    e.velocity.X = v1.X;
                    collideEntity.velocity.X = v2.X;
                }
                else if (yCollision)
                {
                    Collision(collideEntity, e, 1);
                    e.velocity.Y = v1.Y;
                    collideEntity.velocity.Y = v2.Y;
                }
            }
            else if (d > 1) //collision with terrain
            {
                if (xCollision)
                {
                    Collision(e, null, 0);
                }
                else if (yCollision)
                {
                    Collision(e, null, 1);
                }
            }

            if (yCollision)
            {
                if (e is LivingEntity && e.Velocity.Y < 0)
                {
                    e.jumping = false;
                    e.ladderDirection = 0;
                    e.FixedVelocity.Y = 0;
                }
                e.velocity.Y = 0;
            }
            else if (xCollision)
                e.velocity.X = 0;

            if (!yCollision)
            {
                if(e.Velocity.Y < -1)
                    e.jumping = true;
                e.velocity.Y -= Gravity;
            }
            if(e.velocity.Y == 0)
                e.velocity.X *= friction;

            if (Math.Abs(e.Velocity.X) < Util.EPSILON)
                e.velocity.X = 0;
            if (Math.Abs(e.Velocity.Y) < Util.EPSILON)
                e.velocity.Y = 0;

            if (yCollision)
            {
                int bottomY = (int)(e.Position.Y - e.HalfSize.Y);
                int leftX = (int)(e.Position.X - e.HalfSize.X);
                int rightX = (int)(e.Position.X + e.HalfSize.X);
                int leftY = -1;
                int middleY = -1;
                int rightY = -1;
                for (int y = bottomY; y > bottomY - 100; y--)
                {
                    if(rightY == -1 && terrain.GetTerrain(rightX, y))
                        rightY = y;
                    if(middleY == -1 && terrain.GetTerrain((int) e.Position.X, y))
                        middleY = y;
                    if(leftY == -1 && terrain.GetTerrain(leftX, y))
                        leftY = y;
                    if(leftY != -1 && rightY != -1 && middleY != -1)
                        break;
                }
                if (Math.Abs(leftY - middleY) >= 7 && Math.Abs(rightY - middleY) >= 7)
                {
                    float m = (rightY - leftY) / (rightX - leftX);
                    e.velocity.X += rightY > leftY ? -0.5f : 0.5f;
                }
                /*if (terrain.GetTerrain((int)e.Position.X, bottomY - 6) == false)
                {
                    for (int i = leftX; i < leftX + 5; i++)
                    {
                        if (terrain.GetTerrain(i, bottomY) == false)
                        {
                            if (i == leftX)
                                break;
                            else
                            {
                                e.velocity.X = 1.5f;
                                break;
                            }
                        }
                    }
                    for (int i = rightX; i > rightX - 5; i--)
                    {
                        if (terrain.GetTerrain(i, bottomY) == false)
                        {
                            if (i == rightX)
                                break;
                            else
                            {
                                e.velocity.X = -1.5f;
                                break;
                            }
                        }
                    }
                }*/
            }

            //process controllers
            /*if (e.PhysicsController != null)
            {
                e.PhysicsController.run();
                if (!e.removed)
                {
                    RemoveEntity(e);
                    removed = true;
                }
            }*/
            return e.disabled;
        }
        public void DoPhysics()
        {
            for(int idx = 0; idx < Entities.Count; idx++)
            {
                Entity e = Entities.ElementAt(idx);
                bool removed = DoPhysicsForEntity(e);
                if (removed)
                    idx--;
            }
        }
        protected abstract void Collision(Entity e1, Entity e2, int direction);
        public void SetEntityPosition(Entity e, Vec2 pos)
        {
            terrain.SetEntityState(e, e.positionInLevel, false);
            terrain.SetEntityState(e, pos, true);
        }
        public virtual void PlaySound(SoundEffect e)
        {
        }
        public virtual void PlaySound(Sounds name, int x, int y)
        {
        }
        public virtual void PlaySound(Player ignorePlayer, Sounds name, Entity sourceEntity)
        {
        }
        public byte[] GetTerrainState()
        {
            return terrain.ToBytes();
        }
        public void SetTerrainState(byte[] values)
        {
            terrain.SetBytes(values);
        }
        public virtual TaskQueue GetTaskQueue()
        {
            return ((GameView)Vexillum.game.View).tasks;
        }
        public virtual void OnFlagCollide(Player p, Entity flag)
        {

        }
        public virtual void OnEntityDeath(Entity e)
        {

        }
    }
    public struct SoundDef
    {
        public SoundDef(AudioEmitter e, SoundEffectInstance i)
        {
            emitter = e;
            instance = i;
        }
        public AudioEmitter emitter;
        public SoundEffectInstance instance;
    }
}
