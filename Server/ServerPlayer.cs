using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Vexillum.util;
using Vexillum.net;
using Vexillum;
using SevenZip.Compression.LZMA;
using Vexillum.Entities;
using Vexillum.Game;
using Vexillum.Entities.Weapons;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Input;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.Security.Cryptography;
using Steamworks;

namespace Server
{
    public class ServerPlayer : Player, Disconnectable
    {
        protected Server server;

        private TcpClient client;
        private NetworkStream stream;
        private EndianBinaryReader reader;
        private MemoryStream writebuffer;
        private EndianBinaryWriter writer;

        private StreamHelper helper;

        private DataTaskQueue tasks;
        private Thread thread;
        
        protected string ip;
        public bool ready = false;
        public bool levelLoaded = false;
        public bool playersLoaded = false;
        protected bool connected = true;
        private bool pingReceived = true;

        private long pingReceiveTime;
        private long pingSendTime;
        private int pingTime;
        private long velocitySetTime;
        private int lagFrames = 0;
        private int sendFrame = -1;

        private Stopwatch timer = new Stopwatch();

        private Vec2 lastPosition;

        private int grapplingHookTime;
        public bool isOp;

        public ServerPlayer(Server server, TcpClient client) : base("unknown", null)
        {
            this.server = server;
            this.client = client;
            if (client == null)
            {
                ip = "local";
            }
            else
            {
                ip = client.Client.RemoteEndPoint.ToString();
                stream = client.GetStream();

                reader = new EndianBinaryReader(EndianBitConverter.Little, stream);
                writebuffer = new MemoryStream();
                writer = new EndianBinaryWriter(EndianBitConverter.Little, writebuffer);
                helper = new StreamHelper(reader, writer);

                thread = new Thread(new ThreadStart(HandleClient));
                thread.Name = "Client_" + ip;
                tasks = new DataTaskQueue(this, stream);
                tasks.Start();
                timer.Start();
                thread.Start();
            }
        }
        public int GetClientFrame()
        {
            return server.level.frame - lagFrames;
        }
        public void ResetReadyState()
        {
            lastPosition = Vec2.Zero;
            playersLoaded = false;
            ready = false;
        }
        public virtual void Disconnect()
        {
            try
            {
                if (connected)
                {
                    connected = false;
                    if(name != null)
                        Server.Debug(name + " disconnected");
                    else
                        Server.Debug(ip + " disconnected");
                    levelLoaded = false;
                    server.AddTask(delegate()
                    {
                        server.RemovePlayer(this);
                    });
                    reader.Close();
                    writer.Close();
                    tasks.Stop();
                    thread.Interrupt();;
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void HandleClient()
        {
            Server.Debug("Connection from " + ip);
            while (connected)
            {
                try
                {
                    int cmd = reader.ReadByte();
                    ProcessPacket(cmd);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }
        private void HandleException(Exception ex)
        {
            if (!connected)
                return;
            Disconnect();
            if(!(ex is IOException))
                Server.Debug("Error handling "+ip, ex); 
        }
        public void FireGrapplingHook()
        {
            entity.FireGrapplingHook();
            server.SendGrapplingHook(server.level.GetActionFrame(), this, entity.hook);
            grapplingHookTime = server.level.GetTime();
        }
        public void ClearGrapplingHook()
        {
            entity.Level.RemoveEntity(entity.hook);
            entity.hook = null;
        }
        private void ProcessPacket(int cmd)
        {
            //Server.Debug("Packet: " + cmd);
            switch (cmd)
            {
                case 0:
                    int dt = 0;
                    pingReceived = true;
                    pingReceiveTime = timer.ElapsedMilliseconds;
                    dt = (int)(pingReceiveTime - pingSendTime);
                    pingTime = dt;
                    pingString = "" + dt;
                    lagFrames = dt / VexillumConstants.TIME_PER_FRAME;
                break;
                case 1:
                    steamId = reader.ReadUInt64();
                    name = reader.ReadString();
                    int tokenLength = reader.ReadInt32();
                    byte[] token = reader.ReadBytes(tokenLength);
                    EBeginAuthSessionResult authResult = SteamUser.BeginAuthSession(token, tokenLength, new CSteamID(steamId));

                    if (authResult != EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
                    {
                        //SendDisconnect("Steam authentication failed.");
                        //return;
                    }

                    if (server.IsFull())
                    {
                        SendDisconnect("This server is full.");
                        return;
                    }
                    else if(PlayerList.Check("banned", name))
                    {
                        SendDisconnect("You're banned!");
                        return;
                    }
                    else if (name.Length == 0 || name.Length > 128)
                    {
                        SendDisconnect("Invalid name");
                        return;
                    }

                    foreach (Player p in server.players)
                    {
                        if (p.name == name && p != this)
                        {
                            SendDisconnect("This name is in use.");
                            return;
                        }
                    }
                    Server.Debug(ip + " logged in as " + name);
                    SendLoginResponse();
                break;
                case 2:
                    bool ready = reader.ReadBoolean();
                    byte[] md5 = reader.ReadBytes(16);
                    if (ready && server.CheckLevelHash(md5))
                    {
                        //if ()
                        {
                            int addedFrame = 0;
                            server.AddTask(delegate()
                            {
                                SendTerrainState(server.level);
                                Inventory = server.gameMode.GetPlayerInventory(this);
                                server.AddPlayer(this);
                                addedFrame = entity.addedFrame;
                                lock (this)
                                {
                                    Monitor.Pulse(this);
                                }
                            });
                            lock (this)
                            {
                                Monitor.Wait(this);
                            }
                            server.WaitForFrame(addedFrame);
                            server.AddTask(delegate()
                            {
                                SendEntities();
                                SendAddPlayer(this);
                                SendPlayers();
                                SendFinish();
                                server.SendAddPlayer(this);
                                this.ready = true;
                                server.gameMode.PlayerAdded(this);
                                SendPingTimes();
                                server.SendScores(this);
                            });
                        }
                        /*else
                        {
                            SendDisconnect("You have a different version of the map (" + server.level.ShortName + ").");
                        }*/
                    }
                    else
                    {
                        SendLevel();
                    }
                break;
                case 10:
                    int selected = reader.ReadByte();
                    SelectWeapon(selected);
                    break;
                case 11:
                    int btn = reader.ReadByte();
                    int down = reader.ReadByte();
                    float mouseAngle = reader.ReadSingle();
                    if (CurrentClass != PlayerClass.Spectator)
                    {
                        server.AddTask(delegate()
                        {
                            ArmAngle = mouseAngle;
                            switch (btn)
                            {
                                case 255:
                                    UpdateCanGrapple();
                                    if (entity.hook != null)
                                    {
                                        ClearGrapplingHook();
                                    }
                                    if (server.level.GetTime() - grapplingHookTime > 250 && canGrapple)
                                    {
                                        FireGrapplingHook();
                                    }
                                    break;
                                case 254:
                                    if (entity.hook != null)
                                    {
                                        ClearGrapplingHook();
                                        entity.Velocity *= 0.5f;
                                    }
                                    break;
                                default:
                                    if (entity.hook == null && CurrentClass != PlayerClass.Spectator)
                                    {
                                        if (down == 1)
                                        {
                                            if (entity.Weapon.MouseDownServer(server.level, mouseAngle, btn, 0, 0))
                                                server.SendWeaponFire(this, this, WeaponIndex);
                                            if (entity.Weapon is ReloadableWeapon)
                                                SendAmmo(WeaponIndex, ((ReloadableWeapon)entity.Weapon).totalAmmo, ((ReloadableWeapon)entity.Weapon).clipAmmo);
                                        }
                                        else
                                            entity.Weapon.MouseUpServer(server.level, mouseAngle, 0, 0);
                                    }
                                break;
                            }
                        });
                    }
                    break;
                case 12:
                    KeyAction k = helper.ReadAction();
                    if(k != null && CurrentClass != PlayerClass.Spectator)
                        server.AddTask(delegate()
                        {
                            entity.Weapon.KeyDownServer(server.level, k);
                            if (entity.Weapon is ReloadableWeapon)
                                SendAmmo(WeaponIndex, ((ReloadableWeapon)entity.Weapon).totalAmmo, ((ReloadableWeapon)entity.Weapon).clipAmmo);
                        });
                    break;
                case 13:
                    int weaponIndex = reader.ReadByte();
                    SelectWeapon(weaponIndex);
                    server.SendWeaponSelect(this, this, weaponIndex);
                    break;
                case 14:
                    server.AddTask(delegate()
                    {
                        entity.Position = lastPosition;
                        SetVelocity(lastPosition, entity.Position);
                    });
                    break;
                case 15:
                    float playerAngle = helper.ReadAngle();
                    BitArray movement = helper.ReadMovementData();
                    server.AddTask(delegate()
                    {
                        entity.Position = lastPosition;
                        ArmAngle = playerAngle;
                        SetVelocity(lastPosition, entity.Position);
                        SetMovement(entity, movement);
                    });
                    break;
                case 16:
                    Vec2 delta = new Vec2(reader.ReadSByte(), reader.ReadSByte());
                    server.AddTask(delegate()
                    {
                        Vec2 newPosition = lastPosition + delta;
                        SetVelocity(lastPosition, newPosition);
                        lastPosition = entity.Position = newPosition;
                    });
                    break;
                case 17:
                    Vec2 delta2 = new Vec2(reader.ReadSByte(), reader.ReadSByte());
                    float playerAngle2 = helper.ReadAngle();
                    BitArray movement2 = helper.ReadMovementData();
                    server.AddTask(delegate()
                    {
                        Vec2 newPosition = lastPosition + delta2;
                        SetVelocity(lastPosition, newPosition);
                        lastPosition = entity.Position = newPosition;
                        ArmAngle = playerAngle2;
                        SetMovement(entity, movement2);
                    });
                    break;
                case 18:
                    Vec2 playerPos = Util.Round(helper.ReadVec2());
                    server.AddTask(delegate()
                    {
                        SetVelocity(lastPosition, playerPos);
                        lastPosition = entity.Position = playerPos;
                    });
                    break;
                case 19:
                    Vec2 playerPos2 = Util.Round(helper.ReadVec2());
                    float playerAngle3 = helper.ReadAngle();
                    BitArray movement3 = helper.ReadMovementData();
                    server.AddTask(delegate()
                    {
                        SetVelocity(lastPosition, playerPos2);
                        lastPosition = entity.Position = playerPos2;
                        ArmAngle = playerAngle3;
                        SetMovement(entity, movement3);
                    });
                    break;
                case 20:
                    float hitscanAngle = helper.ReadAngle();
                    Vec2 hitscanPos = helper.ReadVec2();
                    if(IsAlive())
                        server.DoHitscan(this, hitscanPos, hitscanAngle, entity.Weapon, GetClientFrame());
                    break;
                case 60:
                    string msg = reader.ReadString();
                    server.Chat(this, msg);
                    break;
                case 255:
                    SendServerStatus();
                    break;
            }
        }
        private void SetVelocity(Vec2 oldPosition, Vec2 newPosition)
        {
            if (!entity.velocityChanged)
            {
                Vec2 diff = newPosition - oldPosition;
                float elapsedTime = (server.level.GetTime() - velocitySetTime);
                if (elapsedTime != 0)
                {
                    Vec2 vFrame = (diff / elapsedTime) * VexillumConstants.TIME_PER_FRAME;
                    velocitySetTime = server.level.GetTime();
                    entity.Velocity = vFrame;
                }
            }
        }
        private void SetMovement(LivingEntity entity, BitArray movement)
        {
            if (movement[0] != entity.moving || movement[2] != entity.direction || movement[3] != entity.jumping)
            {
                entity.moving = movement[0];
                entity.direction = movement[1];
                entity.jumping = movement[2];
                entity.movementChanged = true;
            }
        }
        private void WriteData(bool disconnect)
        {
            tasks.Send(writebuffer, disconnect);
        }
        private void WriteData()
        {
            WriteData(false);
        }
        public virtual void SendServerStatus()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write(server.ready);
                    WriteData(true);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendPing()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)0);
                    pingSendTime = timer.ElapsedMilliseconds;
                    WriteData();
                    pingReceived = false;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendPingTimes()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)130);
                    int readyCount = 0;
                    foreach(ServerPlayer p in server.players)
                    {
                        if (p.ready)
                        {
                            readyCount++;
                        }
                    }
                    writer.Write((byte)readyCount);
                    foreach (ServerPlayer p in server.players)
                    {
                        if (p.ready)
                        {
                            writer.Write((short)p.GetID());
                            writer.Write((short)p.pingTime);
                        }
                    }
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendLoginResponse()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)2);
                    writer.Write((byte)VexillumConstants.PROTOCOL_VERSION);
                    //writer.Write(server.gameMode.GetType().Name);
                    writer.Write(server.level.ShortName);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendLevelChanging()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)253);
                    WriteData(true);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendDisconnect(string message)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)254);
                    writer.Write(message);
                    WriteData(true);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            Server.Debug("Disconnecting " + name+": "+message);
        }
        private void SendTerrainState(Level level)
        {
            try
            {
                byte[] bytes = SevenZipHelper.Compress(level.GetTerrainState());
                levelLoaded = true;
                lock (tasks)
                {
                    writer.Write((byte)3);
                    writer.Write((int)bytes.Length);
                    writer.Write(bytes);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            Server.Debug("Sent terrain state to " + name);
        }
        private void SendEntities()
        {
            List<Entity> entities = server.level.GetNonProjectiles();
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)4);
                    writer.Write((short)entities.Count);
                    foreach (Entity e in entities)
                    {
                        writer.Write((short)e.ID);
                        helper.WriteEntityType(e);
                        helper.WriteVec2(e.Position);
                        if (e is HumanoidEntity)
                            helper.WriteEnum(((HumanoidEntity)e).type);
                    }
                    WriteData();
                    playersLoaded = true;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendAddPlayer(Player p)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)5);
                    writer.Write((short)p.GetID());
                    writer.Write((ulong)p.steamId);
                    writer.Write((string)p.name);
                    writer.Write(p.Entity.Health);
                    helper.WriteEnum(p.CurrentClass);
                    Weapon[] weapons = p.Inventory;
                    writer.Write((byte)weapons.Length);
                    for (int i = 0; i < weapons.Length; i++)
                    {
                        helper.WriteEntityType(weapons[i]);
                    }
                    writer.Write((byte)p.WeaponIndex);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public void SendFinish()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)9);
                    WriteData();
                    server.SendMessage(Messages.PLAYER_JOIN, new string[] { server.gameMode.GetDisplayName(this) }, 2000);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendAmmo(int weaponIndex, int clips, int clipAmmo)
        {
            if (clipAmmo != -1)
            {
                try
                {
                    lock (tasks)
                    {
                        writer.Write((byte)11);
                        writer.Write((byte)weaponIndex);
                        writer.Write((byte)clips);
                        writer.Write((byte)clipAmmo);
                        WriteData();
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }
        public virtual void SendWeaponSelect(ServerPlayer p, int idx)
        {
            try
            {
                lock (tasks)
                {
                    if (p.Entity != null)
                    {
                        writer.Write((byte)13);
                        writer.Write((short)p.Entity.ID);
                        writer.Write((byte)idx);
                        WriteData();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendWeaponFire(ServerPlayer p, int idx)
        {
            try
            {
                lock (tasks)
                {
                    if (p.Entity != null)
                    {
                        writer.Write((byte)14);
                        writer.Write((short)p.Entity.ID);
                        writer.Write((byte)idx);
                        WriteData();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        private void SendPlayers()
        {
            HashSet<ServerPlayer> players = server.players;
            foreach (ServerPlayer p in players)
            {
                if (p.ready && p != this)
                {
                    SendAddPlayer(p);
                }
            }
        }
        public virtual void SendEntityCreate(int frame, int id, Entity e)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)40);
                    writer.Write(frame);
                    writer.Write((short)id);
                    helper.WriteEntityType(e);
                    helper.WriteVec2(e.Position);
                    writer.Write(e.isPlayer);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendEntityRemove(int frame, Entity e)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)41);
                    writer.Write(frame);
                    writer.Write((short)e.ID);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendProjectile(int frame, int id, Projectile e, float angle, Entity owner)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)42);
                    writer.Write(frame);
                    writer.Write((short)id);
                    helper.WriteEntityType(e);
                    helper.WriteVec2(e.Position);
                    writer.Write(angle);
                    writer.Write((short)owner.ID);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendEntityPositions(List<Entity> entities)
        {
            if (entities.Count == 0)
            {
                SendFrame();
                return;
            }
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)30);
                    helper.WriteFrameByte(sendFrame, server.level.frame);
                    sendFrame = server.level.frame;
                    writer.Write((short)entities.Count);
                    foreach (Entity e in entities)
                    {
                        writer.Write((short)e.ID);
                        helper.WriteVec2(e.Position);
                        if (e is LivingEntity)
                        {
                            LivingEntity l = (LivingEntity)e;
                            helper.WriteMovementData(l.moving, l.direction, l.jumping);
                        }
                        else
                            writer.Write((byte)0);
                    }
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendEntityVelocities(List<Entity> entities)
        {
            if (entities.Count == 0)
                return;
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)31);
                    writer.Write((short)entities.Count);
                    foreach (Entity e in entities)
                    {
                        writer.Write((short)e.ID);
                        helper.WriteVec2(e.Velocity);
                        if (e is LivingEntity)
                        {
                            LivingEntity l = (LivingEntity)e;
                            helper.WriteMovementData(l.moving, l.direction, l.jumping);
                        }
                        else
                            writer.Write((byte)0);
                    }
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendEntityAngles(List<Entity> entities)
        {
            if (entities.Count == 0)
                return;
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)32);
                    writer.Write((short)entities.Count);
                    foreach (Entity e in entities)
                    {
                        LivingEntity l = (LivingEntity)e;
                        writer.Write((short)e.ID);
                        helper.WriteAngle(l.ArmAngle);
                    }
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendFrame()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)8);
                    helper.WriteFrameByte(sendFrame, server.level.frame);
                    sendFrame = server.level.frame;
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendExplode(int frame, int x, int y, int radius, int randomSeed, bool nonlethal)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)15);
                    writer.Write((int)frame);
                    helper.WriteVec2(new Vec2(x, y));
                    writer.Write((short)radius);
                    writer.Write(randomSeed);
                    writer.Write(nonlethal);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void Teleport(Vec2 position)
        {
            entity.Position = position;
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)18);
                    helper.WriteVec2(position);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendHitscan(HumanoidEntity source)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)20);
                    writer.Write((short)source.ID);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendClassChange(Player p, PlayerClass c)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)21);
                    if(p != this)
                        writer.Write((short)p.GetID());
                    else
                        writer.Write((short)-1);
                    helper.WriteEnum(c);
                    Weapon[] weapons = p.Inventory;
                    writer.Write((byte)weapons.Length);
                    for (int i = 0; i < weapons.Length; i++)
                    {
                        helper.WriteEntityType(weapons[i]);
                    }
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendGrapplingHook(int frame, Player p, Entity hook)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)22);
                    writer.Write(frame);
                    writer.Write((short)p.Entity.ID);
                    writer.Write(hook == null ? (short) -1 : (short)hook.ID);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendSound(int frame, int id, Entity sourceEntity)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)98);
                    writer.Write(frame);
                    writer.Write((byte)id);
                    writer.Write((short) sourceEntity.ID);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendMessage(string msg)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)60);
                    writer.Write(msg);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendMessage(int messageID, string[] args, int time)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)61);
                    writer.Write((byte)messageID);
                    if (args == null)
                        writer.Write((byte)0);
                    else
                    {
                        writer.Write((byte)args.Length);
                        for (int i = 0; i < args.Length; i++)
                        {
                            writer.Write(args[i]);
                        }
                    }
                    writer.Write((short)time);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendPlayerHealth(Player p, float health)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)110);
                    writer.Write((short)p.Entity.ID);
                    writer.Write(p.Entity.Health);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendGameModeByte(int param, byte value)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)120);
                    writer.Write((byte)param);
                    writer.Write((byte)value);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendGameModeShort(int param, short value)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)121);
                    writer.Write((byte)param);
                    writer.Write((short)value);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendGameModeString(int param, string value)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)122);
                    writer.Write((byte)param);
                    writer.Write(value);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual void SendScore(Player p, int score)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)131);
                    writer.Write((short)p.GetID());
                    writer.Write((short)p.Score);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public void SendLevel()
        {
            int len = server.levelBytes.Length;
            const int maxChunkSize = 1020;
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)220);
                    writer.Write(len);
                    WriteData();
                    int offset = 0;
                    byte[] output = new byte[len];
                    while (offset < len)
                    {
                        int chunkLength = maxChunkSize;
                        if (offset + chunkLength > len)
                            chunkLength = len - offset;
                        writer.Write((byte)221);
                        writer.Write((short)chunkLength);
                        writer.Write(server.levelBytes, offset, chunkLength);
                        WriteData();
                        offset += maxChunkSize;
                    }
                    writer.Write((byte)222);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public virtual bool CheckPingTime()
        {
            if (!pingReceived)
            {
                Disconnect();
                return false;
            }
            return true;
        }
        public void SetLagTime(int ms)
        {
            tasks.testLagTime = ms;
        }
        public bool IsConnected()
        {
            return connected;
        }
    }
}
