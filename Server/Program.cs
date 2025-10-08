using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Vexillum;
using Vexillum.util;
using System.IO;
using System.Threading;
using Vexillum.ui;
using Vexillum.game;

namespace Server
{
    class Program
    {
        private static Server server;
        public static int key;
        private static Vexillum.Util.ServerConfig sc;

        static void Main(string[] args)
        {
            Util.IsServer = true;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs)
            {
                server.Stop();
                Util.CloseLockFile();
                Util.WriteDebugLog();
            };
            Util.OpenLockFile();
            bool error = false;
            try
            {
                using (FileStream stream = File.OpenRead(Util.GetServerFile("settings.txt")))
                {
                    StreamReader sr = new StreamReader(stream);
                    StringBuilder b =  new StringBuilder();
                    string s;
                    int l = 1;
                    while ((s = sr.ReadLine()) != null)
                    {
                        b.Append(s + "\n");
                    }
                    sc = Util.ParseServerConfig(b.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening settings.txt: "+ex);
                Util.Debug(ex.StackTrace);
                error = true;
            }
            if (error)
                return;
            server = new Server(sc);
            key = new Random().Next(int.MaxValue);
            Thread heartbeatThread = new Thread(SendHeartbeat);
            heartbeatThread.Name = "SendHeartbeat";
            heartbeatThread.Start();
        }
        private static void SendHeartbeat()
        {
            while (server.running)
            {
                if (server.serverStarted)
                {
                    Heartbeat.Send(sc.isPublic, sc.name, key, server.level.ShortName, server.players.Count, server.maxPlayers, server.port);
                    Thread.Sleep(45000);
                }
            }
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Util.WriteDebugLog();
        }
        public static void SetVerifyNames(bool v)
        {
            server.verifyNames = v;
        }
    }
}
