using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Vexillum.util;
using Vexillum.net;
using System.Threading;
using Vexillum.ui;
using System.Diagnostics;
using Vexillum.view;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace Vexillum.mp
{
    public class MPClient : Disconnectable
    {
        public string name = "Player_" + new Random().Next();

        private string ip;
        private int port;
        private TcpClient client;
        private Socket socket;

        private NetworkStream stream;
        private EndianBinaryReader reader;
        private MemoryStream writebuffer;
        private EndianBinaryWriter writer;
        private StreamHelper helper;

        private DataTaskQueue tasks;

        private Thread thread;
        private Thread pingThread;

        private long pingReceiveTime;
        private long pingSendTime;

        private bool connected = true;

        private Stopwatch timer = new Stopwatch();

        private static StatusDialog status;

        private MultiplayerView view;

        public MPClient(MultiplayerView view, string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            this.view = view;
        }
        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(this.Run));
            thread.Name = "ClientThread";
            thread.Start();
            new Thread(new ThreadStart(delegate()
                {
                    while (true)
                    {
                        if (helper != null)
                        {
                            string c = "";
                            for (int i = 0; i < new Random().Next(512); i++)
                            {
                                c += (char)new Random().Next(26)+65;
                            }
                            SendChat(c);
                        }
                        Thread.Sleep(100);
                    }
                })).Start();
        }

        public void Disconnect()
        {
            Disconnect(false);
        }
        public void Disconnect(bool userDisconnect)
        {
            Disconnect(userDisconnect ? null : "Disconnected from server.");
        }

        public void Disconnect(string msg)
        {
            if (!connected)
                return;
            if (status != null)
            {
                status.Close();
                status = null;
            }
            try
            {
                connected = false;
                reader.Close();
                writer.Close();
                socket.Close();
                tasks.Stop();
                pingThread.Interrupt();
                thread.Interrupt();
            }
            catch (Exception ex)
            {
            }
            if (msg != null)
                ShowLoadingScreen(msg);
        }

        private void Cancel(object souce, EventArgs ea)
        {
            Disconnect(true);
        }

        private void ShowLoadingScreen(string msg)
        {
            /*if (status == null)
            {
                Vexillum.game.SetMenuView(false);
                status = Vexillum.game.StatusBox(msg);
            }
            status.SetCancelAction(new EventHandler(Cancel));
            status.SetMessage(msg);*/
            view.AddChat(msg);
        }

        public void Run()
        {
            Util.Debug("Connecting to" + ip + ":" + port);
            ShowLoadingScreen("Connecting...");
            try
            {
                client = new TcpClient(ip, port);
                socket = client.Client;
                stream = new NetworkStream(socket);
                reader = new EndianBinaryReader(EndianBitConverter.Big, stream);
                writebuffer = new MemoryStream();
                writer = new EndianBinaryWriter(EndianBitConverter.Big, writebuffer);
                helper = new StreamHelper(reader, writer);
            }
            catch (Exception ex)
            {
                Disconnect("Could not connect to the server.");
                return;
            }
            tasks = new DataTaskQueue(this, stream, true);
            tasks.Start();
            pingThread = new Thread(new ThreadStart(this.Ping));
            pingThread.Name = "PingThread";
            pingThread.Start();

            ShowLoadingScreen("Logging in...");
            SendLogin(name, "poop");
            while (connected)
            {
                try
                {
                    int cmd = reader.ReadByte();
                    ProcessPacket(cmd);
                }
                catch (ThreadInterruptedException ex)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Disconnect();
                    Util.Debug("Disconnected due to exception: " + ex);
                    Util.Debug(ex.StackTrace);
                    return;
                }
            }
        }
        private void ProcessPacket(int cmd)
        {
            if(((cmd < 30 || cmd > 32) && cmd != 8))
                Util.Debug("Packet: " + cmd);
            switch (cmd)
            {
                case 0:
                    reader.ReadByte();
                    pingReceiveTime = timer.ElapsedMilliseconds;
                    SendPing();
                    break;
                case 2:
                    String message = helper.ReadJString();
                    view.AddChat(message);
                    break;
                default:
                    Disconnect("Invalid command: " + cmd);
                    break;
            }
        }
        private void SendPing()
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    WriteData();
                }
                pingSendTime = timer.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }
        private void SendLogin(string username, string key)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)1);
                    helper.WriteJString(username);
                    helper.WriteJString(key);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        public void SendChat(string msg)
        {
            try
            {
                lock (tasks)
                {
                    writer.Write((byte)2);
                    helper.WriteJString(msg);
                    WriteData();
                }
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        private void WriteData()
        {
            try
            {
                tasks.Send(writebuffer, false);
            }
            catch (IOException ex)
            {
                Util.Debug("Disconnected due to exception: " + ex);
                Util.Debug(ex.StackTrace);
                Disconnect();
            }
        }

        private void Ping()
        {
            while (connected)
            {
                try
                {
                    Thread.Sleep(5000);
                    if (timer.ElapsedMilliseconds - pingReceiveTime > 15000)
                        Disconnect("Lost connection to the server");
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }

        public bool IsConnected()
        {
            return connected;
        }
    }
}
