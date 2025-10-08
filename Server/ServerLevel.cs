using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum;
using Vexillum.Entities;
using Vexillum.util;
using Vexillum.Game;
using Microsoft.Xna.Framework;
using Vexillum.Entities.Weapons;

namespace Server
{
    public class ServerLevel : Level
    {
        private LocalPlayer player;
        public BasicEntity greenFlag;
        public BasicEntity blueFlag;
        private Server server;
        private FrameList frames;
        public LevelNodeGraph graph;
        public ServerLevel(Server server, string shortName, string longName, System.Drawing.Bitmap main, System.Drawing.Bitmap background, System.Drawing.Bitmap collision, List<Region> regions) : base(shortName, longName, main, background, collision, regions)
        {
            foreach (Region r in regions)
            {
                if (r.name.StartsWith("spawn_"))
                {
                    PlayerClass cl = (PlayerClass)Enum.Parse(typeof(PlayerClass), r.name.Split('_')[1], true);
                    if (!SpawnPositions.ContainsKey(cl))
                        SpawnPositions[cl] = new List<Region>();
                    SpawnPositions[cl].Add(r);
                }
                else if (r.name.StartsWith("flag_"))
                {
                    PlayerClass cl = (PlayerClass)Enum.Parse(typeof(PlayerClass), r.name.Split('_')[1], true);
                    flagPositions[cl] = r;
                }
                else if (r.name.StartsWith("ladder_"))
                {
                    //ladders.Add(r);
                }
            }
            this.server = server;
            frames = new FrameList(VexillumConstants.MAX_PING / VexillumConstants.TIME_PER_FRAME);
            graph = new LevelNodeGraph(this);
        }
        public void PlaceFlags()
        {
            blueFlag = PlaceFlag(PlayerClass.Blue);
            greenFlag = PlaceFlag(PlayerClass.Green);
            /*LevelNodeGraph.Path p = graph.FindPath((int)greenFlag.Position.X, (int)greenFlag.Position.Y, (int)blueFlag.Position.X, (int)blueFlag.Position.Y);
            foreach (LevelNodeGraph.Node n in p.points)
            {
                //Util.Debug(n.x + "," + n.y + " ");
                CrateEntity e = new CrateEntity();
                e.Position = new Vec2(n.x, n.y);
                AddEntity(e);
            }*/
        }
        public Frame CreateFrame()
        {
            return new Frame(Entities);
        }
        public Frame GetFrame(int i)
        {
            try
            {
                return frames.GetFrame(i);
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public override void Step(long gameTime)
        {
            base.Step(gameTime);
            frames.AddFrame(frame, CreateFrame());
        }
        public override void Explode(int x, int y, int radius, int randomSeed, bool nonlethal, Player attacker, Weapon weapon)
        {
            int frame = server.level.GetActionFrame();
            server.AddTask(delegate() {
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
                            int maxRadius = (int)(radius * fraction * 0.8);
                            double r = random.NextDouble();
                            int r2 = random.Next(16);
                            float offset = 0.05f * step;
                            switch (r2)
                            {
                                case 15:
                                    sx *= (1 + offset);
                                    break;
                                case 14:
                                    sy *= (1 + offset);
                                    break;
                                case 13:
                                    sx *= (1 - offset);
                                    break;
                                case 12:
                                    sy *= (1 - offset);
                                    break;
                            }
                            if (t > maxRadius && r < ((float)(t - maxRadius) / (radius - maxRadius)) * 8)
                            {
                                traceRadius -= step;
                                if (traceRadius <= 0)
                                    doDamage = false;
                            }
                            DrawCircle(pos, center, traceRadius, 8, damageRadius, nonlethal, exploded, delegate(Entity e, float distance, float ratio)
                            {
                                if (e is HumanoidEntity && server.gameMode.CanDamage(((HumanoidEntity)e).player, attacker))
                                {
                                    if (distance < damageRadius)
                                    {
                                        float amount = 1f - ratio;
                                        amount = Math.Max(Math.Min(amount, 1), 0) * 0.5f;
                                        float h = ((HumanoidEntity)e).MaxHealth * amount;
                                        ((HumanoidEntity)e).Health = ((HumanoidEntity)e).Health - h;
                                        server.gameMode.PlayerHealthChanged(((HumanoidEntity)e).player, weapon);
                                    }
                                }
                            }, doDamage && collision != TerrainCollisionType.Empty);
                        }
                    }
                }
            }, frame, null);
            server.SendExplode(frame, x, y, radius, randomSeed, nonlethal);
        }
        public override void AddEntity(Entity e, bool isPlayer)
        {
            if (server.level == null) //for debugging only
            {
                int frame2 = 0;
                short id2 = Entity.NextID(this);
                e.ID = id2;
                e.addedFrame = frame2;
                base.AddEntity(e, isPlayer);
                return;
            }
            int frame = server.level.GetActionFrame();
            short id = Entity.NextID(this);
            e.ID = id;
            e.addedFrame = frame;
            e.disabled = true;
            base.AddEntity(e, isPlayer);
            server.AddTask(delegate()
            {
                e.disabled = false;
            }, frame, null);
            server.SendAddEntity(frame, id, e);
        }
        public override void AddProjectile(Projectile p, float angle, Entity owner)
        {
            int frame = server.level.GetActionFrame();
            short id = Entity.NextID(this);
            p.addedFrame = frame;
            p.disabled = true;
            base.AddProjectile(p, id, angle, owner);
            p.Added();
            server.AddTask(delegate()
            {
                p.disabled = false;
            }, frame, null);
            server.SendProjectile(frame, id, p, angle, owner);
        }
        public override void RemoveEntity(Entity e)
        {
            int frame = server.level.GetActionFrame();
            e.disabled = true;
            server.AddTask(delegate()
            {
                base.RemoveEntity(e);
                e.Removed();
                e.Level = null;
            }, frame, null);
            server.SendRemoveEntity(frame, e);
        }
        protected override void Collision(Entity e1, Entity e2, int direction)
        {
            e1.OnCollide(e2, direction);
            if (e2 != null)
                e2.OnCollide(e1, direction);
        }
        public override void PlaySound(Player ignorePlayer, Sounds name, Entity source)
        {
            server.SendSound(ignorePlayer, frame, AssetManager.GetIndex(name), source);
        }
        public override void PlaySound(Sounds name, int x, int y)
        {

        }
        public override TaskQueue GetTaskQueue()
        {
            return server.tasks;
        }
        public void TakeFlag(PlayerClass c)
        {
            RemoveFlag(c);
        }
        public void RemoveFlag(PlayerClass c)
        {
            BasicEntity flag = c == PlayerClass.Green ? greenFlag : blueFlag;
            if (flag != null)
                RemoveEntity(flag);
        }
        public BasicEntity PlaceFlag(PlayerClass c)
        {
            return PlaceFlag(c, Vec2.Zero);
        }
        public BasicEntity PlaceFlag(PlayerClass c, Vec2 position)
        {
            BasicEntity flag;
            if (c == PlayerClass.Green)
            {
                flag = new GreenFlagEntity();
                greenFlag = flag;
            }
            else
            {
                flag = new BlueFlagEntity();
                blueFlag = flag;

            }
            if (position == Vec2.Zero)
            {
                Region r = flagPositions[c];
                flag.Position = new Vec2(r.x1, height - r.y1 - 29/2);
            }
            else
                flag.Position = position;
            AddEntity(flag);
            return flag;
        }
        public override void OnFlagCollide(Player p, Entity flag)
        {
            server.gameMode.OnFlagCollide(p, flag);
        }
        public void SetLocalPlayer(LocalPlayer p)
        {
            player = p;
        }
    }
}
