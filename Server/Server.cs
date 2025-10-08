#define SERVER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Vexillum;
using Vexillum.Entities;
using Microsoft.Xna.Framework;
using Vexillum.Game;
using Vexillum.util;
using System.Diagnostics;
using Vexillum.Entities.Weapons;
using Vexillum.game;
using Vexillum.steam;

namespace Server
{
    public class Server
    {
        public bool ready = false;
        public bool running = true;
        public bool serverStarted = false;

        private ServerSteamAPI steam;

        public ServerLevel level;
        public byte[] levelBytes;
        public SurvivalGameMode gameMode;
        private byte[] levelMd5;

        public int port;
        private TcpListener listener;
        private Thread serverThread;
        private Thread stepThread;
        private Thread entityThread;
        private Thread pingThread;

        public int maxPlayers;
        public int maxBots;
        public HashSet<ServerPlayer> players;
        private Dictionary<ServerPlayer, Dictionary<Entity, Vec2>> localPositions;
        private Dictionary<ServerPlayer, Dictionary<Entity, Vec2>> localVelocities;
        private Dictionary<ServerPlayer, Dictionary<Entity, float>> localAngles;

        public TaskQueue tasks;
        private FrameTaskQueue frameTasks;

        private Stopwatch timer = new Stopwatch();
        public bool verifyNames;
        private Server server;

        private AIController ai;
        private List<string> botNames = new List<string>();

        public Server(Util.ServerConfig sc)
        {
            SteamManager.Initialize();
            steam = new ServerSteamAPI(this);
            steam.Initialize();

            botNames.Add("Bot #2");
            botNames.Add("Brains");
            botNames.Add("Bot #1");
            botNames.Add("Unknown");
            botNames.Add("Vexillum Player");
            botNames.Add("GLaDOS");
            botNames.Add("Gabe");
            botNames.Add("Keybored");
            botNames.Add("Bolo Santosi");
            botNames.Add("Babies in Africa");
            botNames.Add("aimbot.exe");
            botNames.Add("Cry Some Moar");
            botNames.Add("Rick Astley");
            botNames.Add("Nyan Cat");
            botNames.Add("Borg");
            botNames.Add("Notch");
            botNames.Add("Artificial Stupidity");
            botNames.Add("xXxCODNoScope420SniperxXx");
            botNames.Add("Human");
            botNames.Add("There's a spy around here!");
            Shuffle(botNames);

            this.port = sc.port;
            verifyNames = sc.verifyNames;
            HumanoidTypes.LoadContentServer();
            this.maxPlayers = sc.maxPlayers;
            this.maxBots = sc.maxBots;

            WeaponParameters.weaponNames = sc.weapons;


            players = new HashSet<ServerPlayer>();
            localPositions = new Dictionary<ServerPlayer, Dictionary<Entity, Vec2>>();
            localVelocities = new Dictionary<ServerPlayer, Dictionary<Entity, Vec2>>();
            localAngles = new Dictionary<ServerPlayer, Dictionary<Entity, float>>();
            listener = new TcpListener(IPAddress.Any, port);

            PlayerList.Load("ops");
            PlayerList.Load("banned");

            Debug("Loading level...");
            LevelLoader.LoadLevelList(sc.maps);
            setLevel(LevelLoader.GetRandomLevel());
            ai = new AIController(this);

            gameMode.maxCaptures = sc.maxCaptures;
            gameMode.respawnTime = sc.respawnTime;

            stepThread = new Thread(new ThreadStart(Step));
            tasks = new TaskQueue();

            serverThread = new Thread(new ThreadStart(RunServer));
            serverThread.Name = "Client Acceptor";
            serverThread.Start();

            stepThread.Name = "Server Main";
            stepThread.Start();

            entityThread = new Thread(new ThreadStart(UpdateEntityPositions));
            entityThread.Name = "Entity Updater";
            entityThread.Start();
            
            pingThread = new Thread(new ThreadStart(SendPings));
            pingThread.Name = "Ping Sender";
            pingThread.Start();
        }
        static readonly Random rng = new Random();
        public static void Shuffle(System.Collections.IList list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                object value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public bool IsFull()
        {
            return players.Count > maxPlayers;
        }
        public void SetRandomLevel()
        {
            setLevel(LevelLoader.GetRandomLevel());
        }
        public void AddBots()
        {
            ClearBots();
            for (int i = 0; i < maxBots && i < maxPlayers; i++)
            {
                AddBot();
            }
        }
        private void setLevel(string name)
        {
            if (LevelLoader.LevelExists(name))
            {
                ready = false;
                foreach (ServerPlayer p in players)
                {
                    p.SendLevelChanging();
                }
                if (serverThread != null)
                    AddBots();
                bool first = level == null;
                level = null;
                Entity.ResetID();
                Vexillum.LevelLoader.LevelData d = LevelLoader.LoadData(name);
                levelBytes = d.bytes;
                level = new ServerLevel(this, name, d.longName, d.bitmaps["main"], d.bitmaps["background"], d.bitmaps["collision"], d.regions);
                levelMd5 = LevelLoader.LevelMd5(level.ShortName);
                gameMode = new SurvivalGameMode();
                gameMode.SetServer(this);
                gameMode.SetLevel(level);
                frameTasks = new FrameTaskQueue(tasks, level);
                timer.Stop();
                timer.Start();
                if (!first)
                    level.PlaceFlags();
                ready = true;
            }
            else
                Debug("Error: Level "+name+" does not exist.");
        }
        public void AddConditionalTask(ConditionalTaskDelegate t)
        {
            tasks.AddConditionalTask(t);
        }
        public void AddTask(TaskDelegate t)
        {
            if (Thread.CurrentThread == stepThread)
            {
                t();
            }
            else
                tasks.AddTask(t);
        }
        public void AddTask(TaskDelegate t, int frame, Entity e)
        {
            frameTasks.Add(t, frame, e);
        }
        public void WaitForFrame(int frame)
        {
            frameTasks.WaitForFrame(frame);
        }
        public HumanoidEntity AddPlayer(ServerPlayer player)
        {
            UpdateBots();
            player.isOp = PlayerList.Check("ops", player.name);
            localPositions[player] = new Dictionary<Entity, Vec2>();
            localAngles[player] = new Dictionary<Entity, float>();
            localVelocities[player] = new Dictionary<Entity, Vec2>();
            HumanoidEntity e = gameMode.GetPlayerEntity(player);
            gameMode.UpdatePlayerList();
            //e.enablePhysics = false;
            e.Weapon = player.Inventory[0];
            player.Entity = e;
            level.AddEntity(e, true);
            return e;
        }
        public void RemovePlayer(ServerPlayer player)
        {
            localPositions.Remove(player);
            localAngles.Remove(player);
            localVelocities.Remove(player);
            if (player.Entity != null && player.ready)
            {
                level.RemoveEntity(player.Entity);
            }
            players.Remove(player);
            if (!(player is AIPlayer))
            {
                UpdateBots();
            }
            else
            {
                ai.RemovePlayer((AIPlayer)player);
            }
            gameMode.PlayerRemoved(player);
            System.GC.Collect();
        }
        public void SendAddPlayer(Player t)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p.playersLoaded && p != t)
                        p.SendAddPlayer(t);
                }
            });
        }
        public void SendPlayerHealth(Player t)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p.playersLoaded)
                        p.SendPlayerHealth(t, t.Entity.Health);
                }
            });

        }
        public void SendAddEntity(int frame, int id, Entity e)
        {
            AddTask(delegate()
            {
                if(e.clientSide && e is Projectile)
                    ((ServerPlayer)((Projectile)e).GetOwner().player).SendEntityCreate(frame, id, e);
                else
                    foreach (ServerPlayer p in players)
                    {
                        if (p.playersLoaded)
                            p.SendEntityCreate(frame, id, e);
                    }
            });

        }
        public void SendProjectile(int frame, int id, Projectile e, float angle, Entity owner)
        {
            AddTask(delegate()
            {
                if (e.clientSide && e is Projectile)
                    ((ServerPlayer)((Projectile)e).GetOwner().player).SendEntityCreate(frame, id, e);
                else
                    foreach (ServerPlayer p in players)
                    {
                        if (p.playersLoaded)
                            p.SendProjectile(frame, id, e, angle, owner);
                    }
            });
        }
        public void SendRemoveEntity(int frame, Entity e)
        {
            AddTask(delegate()
            {
                if (e.clientSide && e is Projectile)
                    ((ServerPlayer)((Projectile)e).GetOwner().player).SendEntityRemove(frame, e);
                else
                foreach (ServerPlayer p in players)
                {
                    if (p.playersLoaded)
                        p.SendEntityRemove(frame, e);
                }
            });
        }
        public void SendHitscan(HumanoidEntity source)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p.playersLoaded && p != source.player)
                        p.SendHitscan(source);
                }
            });

        }
        public void SendClassChange(Player player, PlayerClass c)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p.playersLoaded)
                        p.SendClassChange(player, c);
                }
            });

        }
        public void SendWeaponSelect(ServerPlayer player, ServerPlayer ignorePlayer, int idx)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p != ignorePlayer && p.playersLoaded)
                        p.SendWeaponSelect(player, idx);
                }
            });
        }
        public void SendWeaponFire(ServerPlayer player, ServerPlayer ignorePlayer, int idx)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p != ignorePlayer && p.playersLoaded)
                        p.SendWeaponFire(player, idx);
                }
            });
        }
        public void SendExplode(int frame, int x, int y, int radius, int randomSeed, bool nonlethal)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p.levelLoaded)
                        p.SendExplode(frame, x, y, radius, randomSeed, nonlethal);
                }
            });
        }
        public void SendSound(Player ignorePlayer, int frame, int id, Entity sourceEntity)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if (p != ignorePlayer && p.playersLoaded)
                        p.SendSound(frame, id, sourceEntity);
                }
            });
        }
    
        public void Chat(ServerPlayer p, string msg)
        {
            Debug(p.name + ": " + msg);
            if (msg.StartsWith("/"))
            {
                string[] parts = msg.Split(' ');
                string cmd = parts[0].Replace("/", "");
                int n = parts.Length - 1;
                switch (cmd)
                {
                    case "spec":
                        gameMode.SetSpectator(p);
                        break;
                    case "newgame":
                        if (p.isOp)
                        {
                            if(n == 1)
                                AddTask(delegate()
                                {
                                    setLevel(parts[1]);
                                });
                            else
                                AddTask(delegate()
                                {
                                    setLevel(LevelLoader.GetRandomLevel());
                                });                                
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "You need to be op to do that!");
                        break;
                    case "ban":
                        if (n == 1 && p.isOp)
                        {
                            PlayerList.Add("banned", parts[1]);
                            foreach (Player other in players)
                            {
                                if (p.name == parts[1])
                                {
                                    p.SendDisconnect("You've been banned!");
                                }
                            }
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "You need to be op to do that!");
                        break;
                    case "unban":
                        if (n == 1 && p.isOp)
                        {
                            PlayerList.Remove("banned", parts[1]);
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "You need to be op to do that!");
                        break;
                    case "kick":
                        if (n == 1 && p.isOp)
                            foreach (Player other in players)
                            {
                                if (p.name == parts[1])
                                {
                                    p.SendDisconnect("You were kicked");
                                }
                            }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "You need to be op to do that!");
                        break;
                    case "op":
                        if (n == 1 && p.isOp)
                        {
                            PlayerList.Add("ops", parts[1]);
                            foreach (Player other in players)
                            {
                                if (other.name == parts[1])
                                {
                                    ((ServerPlayer)other).SendMessage(TextUtil.COLOR_ORANGE + "You're now an op!");
                                    ((ServerPlayer)other).isOp = true;
                                }
                            }
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "You need to be op to do that!");
                        break;
                    case "deop":
                        if (n == 1 && p.isOp)
                        {
                            PlayerList.Remove("ops", parts[1]);
                            foreach (Player other in players)
                            {
                                if (other.name == parts[1])
                                {
                                    ((ServerPlayer)other).SendMessage(TextUtil.COLOR_ORANGE + "You're no longer an op!");
                                    ((ServerPlayer)other).isOp = false;
                                }
                            }
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "You need to be op to do that!");
                        break;
                    case "green":
                        if (p.PlayerClass == PlayerClass.Green)
                            return;
                        else if (p.PlayerClass == PlayerClass.Blue && gameMode.numGreen < gameMode.numBlue)
                        {
                            p.PlayerClass = PlayerClass.Green;
                            if (p.CurrentClass != PlayerClass.Spectator)
                                gameMode.ResetPlayer(p);
                            SendChat(gameMode.GetDisplayName(p) + TextUtil.COLOR_WHITE + " joined the green team.");
                            gameMode.UpdatePlayerList();
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "Green team is full.");
                        break;
                    case "blue":
                        if (p.PlayerClass == PlayerClass.Blue)
                            return;
                        else if (p.PlayerClass == PlayerClass.Green && gameMode.numBlue < gameMode.numGreen)
                        {
                            p.PlayerClass = PlayerClass.Blue;
                            if (p.CurrentClass != PlayerClass.Spectator)
                                gameMode.ResetPlayer(p);
                            SendChat(gameMode.GetDisplayName(p) + TextUtil.COLOR_WHITE + " joined the blue team.");
                            gameMode.UpdatePlayerList();
                        }
                        else
                            p.SendMessage(TextUtil.COLOR_ORANGE + "Green team is full.");
                        break;
                    default:
                        p.SendMessage(TextUtil.COLOR_ORANGE + "Unknown command: /" + cmd);
                        break;
                }
            }
            else
            {
                msg = gameMode.GetDisplayName(p) + TextUtil.COLOR_WHITE + "> " + msg;
                SendChat(msg);
            }
        }
        public void SendChat(string msg)
        {
            AddTask(delegate()
            {

                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendMessage(msg);
                }
            });
        }
        public void SendMessage(int messageID, string[] args, int time)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendMessage(messageID, args, time);
                }
            });
        }
        public void SendGrapplingHook(int frame, Player p, Entity hook)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendGrapplingHook(frame, p, hook);
                }
            });
        }
        public void SendScore(Player p)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendScore(p, p.Score);
                }
            });
        }
        public void SendScores(ServerPlayer t)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer p in players)
                {
                    if(p.ready)
                        t.SendScore(p, p.Score);
                }
            });
        }
        private void RunServer()
        {
            listener.Start();
            Debug("Ready for connections");
            serverStarted = true;
            while (running)
            {
                TcpClient client = listener.AcceptTcpClient();
                AddTask(delegate()
                {
                    players.Add(new ServerPlayer(this, client));
                });
            }
        }
        public void Stop()
        {
            running = false;
            try
            {
                listener.Stop();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void UpdateEntityPositions()
        {
            while (running)
            {
                Thread.Sleep(50);
                AddTask(delegate()
                {
                    foreach (ServerPlayer p in players)
                    {
                        try
                        {
                            if (p.playersLoaded)
                            {
                                p.SendEntityVelocities(GetUpdatedVelocities(p));
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Debug("Disconnected "+p.name+" due to exception: " + ex);
                            Util.Debug(ex.StackTrace);
                            p.Disconnect();
                        }
                    }
                });
                Thread.Sleep(50);
                AddTask(delegate()
                {
                    foreach (ServerPlayer p in players)
                    {
                        try
                        {
                            if (p.playersLoaded)
                            {
                                List<Entity> updatedP = GetUpdatedPositions(p);
                                List<Entity> updatedV = GetUpdatedVelocities(p);
                                p.SendEntityVelocities(updatedV);
                                p.SendEntityPositions(updatedP);
                                List<Entity> updatedAngle = GetUpdatedAngles(p);
                                p.SendEntityAngles(updatedAngle);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Debug("Disconnected " + p.name + " due to exception: " + ex);
                            Util.Debug(ex.StackTrace);
                            p.Disconnect();
                        }
                    }
                });
            }
        }
        private List<Entity> GetUpdatedPositions(ServerPlayer p)
        {
            List<Entity> allEntities = level.getEntities();
            List<Entity> result = new List<Entity>();
            lock (allEntities)
            {
                foreach (Entity e in allEntities)
                {
                    if (e.clientSide && e is Projectile && ((Projectile)e).GetOwner() == p.Entity)
                        continue;
                    if ((e != p.Entity && !(e is Projectile) && ((!localPositions[p].ContainsKey(e) || localPositions[p][e] != e.Position) || (e is LivingEntity && ((LivingEntity)e).movementChanged))))
                    {
                        if (e is LivingEntity)
                        {
                            ((LivingEntity)e).movementChanged = false;
                        }
                        localPositions[p][e] = e.Position;
                        result.Add(e);
                    }
                }
            }
            return result;
        }
        private List<Entity> GetUpdatedVelocities(ServerPlayer p)
        {
            List<Entity> allEntities = level.getEntities();
            List<Entity> result = new List<Entity>();
            lock (allEntities)
            {
                foreach (Entity e in allEntities)
                {
                    if (e.clientSide && e is Projectile && ((Projectile)e).GetOwner() == p.Entity)
                        continue;
                    if ((e.velocityChanged && e == p.Entity) || (!(e is Projectile) && e != p.Entity && (!localVelocities[p].ContainsKey(e) || !CompareVelocity(localVelocities[p][e], e.Velocity))))
                    {
                        if (e == p.Entity)
                            e.velocityChanged = false;
                        localVelocities[p][e] = e.Velocity;
                        result.Add(e);
                    }
                }
            }
            return result;
        }
        private bool CompareVelocity(Vec2 v1, Vec2 v2)
        {
            if (v1 == v2)
                return true;
            else if ((int)v1.X == (int)v2.X && (int)v2.Y == (int)v1.Y)
                return true;
            else
                return false;
        }
        private List<Entity> GetUpdatedAngles(ServerPlayer p)
        {
            List<Entity> allEntities = level.getEntities();
            List<Entity> result = new List<Entity>();
            lock (allEntities)
            {
                foreach (Entity e in allEntities)
                {
                    if (e != p.Entity && e is LivingEntity && e.isPlayer && (!localAngles[p].ContainsKey(e) || localAngles[p][e] != ((LivingEntity)e).ArmAngle))
                    {
                        localAngles[p][e] = ((LivingEntity)e).ArmAngle;
                        result.Add(e);
                    }
                }
            }
            return result;
        }
        private void SendPings()
        {
            while (running)
            {
                foreach (ServerPlayer p in players)
                {
                    try
                    {
                        if (p.CheckPingTime() && p.playersLoaded)
                        {
                            p.SendPing();
                            p.SendPingTimes();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Debug("Disconnected " + p.name + " due to exception: " + ex);
                        Util.Debug(ex.StackTrace);
                        p.Disconnect();
                    }
                }
                Thread.Sleep(5000);
            }
        }
        public void SetFrame(int f)
        {
            level.frame = f;
            frameTasks.setFrame(f);
        }
        public void DoHitscan(ServerPlayer p, Vec2 startPos, float angle, Weapon weapon, int clientFrame)
        {
            if(weapon.showHitscan)
                SendHitscan(p.Entity);
            AddTask(delegate()
            {
                Frame f = level.GetFrame(clientFrame);
                if (f == null)
                {            
                    p.SendDisconnect("Too much lag");
                    return;
                }
                Vec2 unitVec = new Vec2((float)Math.Cos(angle), -(float)Math.Sin(angle));
                Vec2 pos = startPos;
                bool done = false;
                int entityID = -1;
                int x, y, px, py;
                px = (int)pos.X;
                py = (int)pos.Y;
                while (!done)
                {
                    float distance = new Vec2(pos.X - startPos.X, pos.Y - startPos.Y).LengthSquared();
                    x = (int)pos.X;
                    y = (int)pos.Y;
                    if (weapon.maxHitscanLengthSquared != -1 && distance > weapon.maxHitscanLengthSquared)
                    {
                        done = true;
                    }
                    if (level.terrain.GetTerrain(x, y))
                    {
                        done = true;
                        entityID = 0;
                    }
                    else
                    {
                        foreach (Frame.EntityDef e in f.entities)
                        {
                            if (Frame.TestPoint(e, x, y) && p.Entity != null && e.id != p.Entity.ID)
                            {
                                done = true;
                                entityID = e.id;
                            }
                        }
                    }
                    pos += unitVec;
                    px = x;
                    py = y;
                }
                if (entityID == -1)
                    return;
                else if (entityID == 0)
                {
                    int c = level.terrain.GetCollisionData((int)pos.X, (int)pos.Y);
                    if (weapon.showHitscan && c >= 12)
                        level.Explode((int)pos.X, (int)pos.Y, 2, true, p, weapon);
                }
                else
                {
                    Entity e = level.GetEntityByID(entityID);
                    if (e != null)
                    {
                        e.Velocity = unitVec * weapon.knockback;
                        e.velocityChanged = true;
                        if (e is HumanoidEntity && ((HumanoidEntity)e).player != null && gameMode.CanDamage(p, ((HumanoidEntity)e).player))
                        {
                            gameMode.PlayerHit(p, ((HumanoidEntity)e).player, weapon, pos);
                        }
                    }
                }
                /*CrateEntity c = new CrateEntity();
                c.Position = pos;
                level.AddEntity(c);*/
                
            });
        }
        public Boolean CheckLevelHash(byte[] hash)
        {
            for (int i = 0; i < 16; i++)
            {
                if (hash[i] != levelMd5[i])
                    return false;
            }
            return true;
        }
        private void Step()
        {
            int startTime, elapsed;
            level.PlaceFlags();
            while (running)
            {
                startTime = (int) timer.Elapsed.TotalMilliseconds;
                if (level != null)
                {
                    frameTasks.Process();
                    tasks.Process(startTime);
                    level.Step(timer.ElapsedMilliseconds);
                    ai.Step(timer.ElapsedMilliseconds);
                    foreach (ServerPlayer p in players)
                    {
                        if (p.playersLoaded && p.Entity != null && p.Entity.Weapon != null)
                        {
                            p.Entity.Weapon.Step(null, p.Entity.Weapon.GetPivot(), p.ArmAngle, (int)timer.ElapsedMilliseconds);
                            if (p.Entity.Weapon.ammoUpdated)
                            {
                                p.SendAmmo(p.WeaponIndex, ((ReloadableWeapon)p.Entity.Weapon).totalAmmo, ((ReloadableWeapon)p.Entity.Weapon).clipAmmo);
                                p.Entity.Weapon.ammoUpdated = false;
                            }
                        }
                    }
                    gameMode.Step((int) timer.ElapsedMilliseconds);
                }
                elapsed = ((int)timer.Elapsed.TotalMilliseconds - startTime);
                elapsed = Math.Min(elapsed, VexillumConstants.TIME_PER_FRAME);
                Thread.Sleep(VexillumConstants.TIME_PER_FRAME - elapsed);
                SteamManager.Update();
            }
        }
        public void SendGameModeByte(int param, byte value)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendGameModeByte(param, value);
                }
            });
        }
        public void SendGameModeString(int param, string value)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendGameModeString(param, value);
                }
            });
        }
        public void SendGameModeShort(int param, short value)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.ready)
                        t.SendGameModeShort(param, value);
                }
            });
        }
        public void KickPlayer(ulong steamId, string message)
        {
            AddTask(delegate()
            {
                foreach (ServerPlayer t in players)
                {
                    if (t.steamId == steamId)
                    {
                        t.SendDisconnect(message);
                    }
                }

            });
        }
        public static void Debug(string message)
        {
            Util.Debug(message);
        }
        public static void Debug(string msg, Exception ex)
        {
            Debug(msg + ": " +ex.GetType());
            Debug(ex.StackTrace);
        }
        public static void Debug(Exception ex)
        {
            Debug(ex.GetType().ToString());
            Debug(ex.StackTrace);
        }
        public void AddBot()
        {
            string botName = botNames.ElementAt(0);
            botNames.RemoveAt(0);
            AIPlayer pl = new AIPlayer(this, botName);
            pl.Login();
            lock (players)
            {
                players.Add(pl);
            }
            ai.AddPlayer(pl);
        }
        public void RemoveBot()
        {
            PlayerClass cl;
            if (gameMode.numBlue > gameMode.numGreen)
                cl = PlayerClass.Blue;
            else
                cl = PlayerClass.Green;
            lock (players)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    ServerPlayer p = players.ElementAt(i);
                    if (p is AIPlayer && (p.PlayerClass == cl || p.PlayerClass == PlayerClass.Spectator))
                    {
                        RemovePlayer(p);
                        botNames.Add(p.name);
                        break;
                    }
                }
            }
        }
        private void UpdateBots()
        {
            int numRealPlayers = players.Count - ai.NumBots;
            int maxAllowedBots = numRealPlayers == 0 ? 0 : maxBots - numRealPlayers;
            if (ai.NumBots > maxAllowedBots)
                while (ai.NumBots > maxAllowedBots)
                    RemoveBot();
            else if(ai.NumBots < maxAllowedBots)
                while (ai.NumBots < maxAllowedBots)
                    AddBot();
        }
        public void ClearBots()
        {
            lock (players)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    ServerPlayer p = players.ElementAt(i);
                    if (p is AIPlayer)
                    {
                        i--;
                        RemovePlayer(p);
                        botNames.Add(p.name);
                    }
                }
            }
        }
        public void ResetStrategy()
        {
            ai.ResetStrategy();
        }
    }
}
