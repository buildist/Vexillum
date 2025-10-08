using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;
using System.Threading;
using Vexillum.view;
using System.Diagnostics;
using System.IO;
using Vexillum.game;

namespace Vexillum.ui
{
    class ServerDialog : WindowControl
    {
        private ListControl serverList;
        private ButtonControl reloadButton;
        private ButtonControl connectButton;
        private ButtonControl ipButton;
        private ButtonControl hostButton;
        private ButtonControl cancelButton;
        private AbstractView view;
        private Thread updateThread;
        private List<Server> servers = new List<Server>();
        private int selectedIndex;
        public ServerDialog(AbstractView view)
        {
            this.view = view;
            Title = "Server List";

            serverList = new ListControl();
            serverList.Bounds = new UniRectangle(new UniVector(10, 34), new UniVector(new UniScalar(1f, -20f), new UniScalar(1f, -80f)));
            serverList.SelectionMode = Nuclex.UserInterface.Controls.Desktop.ListSelectionMode.Single;

            reloadButton = new ButtonControl();
            reloadButton.Text = "Reload";
            reloadButton.Bounds = new UniRectangle(new UniVector(new UniScalar(1f, -120), 40), new UniVector(80, 24));
            reloadButton.Pressed += Update;


            connectButton = new ButtonControl();
            connectButton.Text = "Join Server";
            connectButton.Bounds = new UniRectangle(new UniVector(10, new UniScalar(1f, -37)), new UniVector(80, 24));
            connectButton.Pressed += Connect;

            ipButton = new ButtonControl();
            ipButton.Text = "Direct IP Join...";
            ipButton.Bounds = new UniRectangle(new UniVector(100, new UniScalar(1f, -37)), new UniVector(90, 24));
            ipButton.Pressed += ShowJoinDialog;

            hostButton = new ButtonControl();
            hostButton.Text = "Host Server...";
            hostButton.Bounds = new UniRectangle(new UniVector(200, new UniScalar(1f, -37)), new UniVector(90, 24));
            hostButton.Pressed += ShowHostDialog; 
 
            cancelButton = new ButtonControl();
            cancelButton.Text = "Close";
            cancelButton.Bounds = new UniRectangle(new UniVector(new UniScalar(1f, -90), new UniScalar(1f, -37)), new UniVector(80, 24));
            cancelButton.Pressed += Close;

            Children.Add(reloadButton);
            Children.Add(connectButton);
            Children.Add(serverList);
            Children.Add(cancelButton);
            Children.Add(ipButton);
            Children.Add(hostButton);

            Bounds = new UniRectangle(new UniVector(210, 10), new UniVector(new UniScalar(1f, -220f), new UniScalar(1f, -70f)));
            Update(null, null);
        }
        private void Update(object source, EventArgs e)
        {
            updateThread = new Thread(new ThreadStart(this.Run));
            updateThread.Start();
        }
        public void Run()
        {
            servers.Clear();
            serverList.Items.Clear();
            serverList.Items.Add("Loading...");
            string[][] array = Util.HttpGetArray("servers.php");
            serverList.Items.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                Server server = new Server();
                server.name = array[i][0];
                server.ip = array[i][1];
                try
                {
                    server.port = int.Parse(array[i][2]);
                    server.players = int.Parse(array[i][3]);
                    server.maxPlayers = int.Parse(array[i][4]);
                }
                catch (Exception ex)
                {
                    continue;
                }
                server.map = array[i][5];
                servers.Add(server);
                serverList.Items.Add(array[i][0]+"        Map: "+server.map);
            }
            if (array.Length == 1)
            {
                serverList.SelectedItems.Add(0);
            }
        }
        private void Close(object sender, EventArgs arguments)
        {
            Close();
        }
        private void Connect(object sender, EventArgs args)
        {
            int idx = serverList.SelectedItems[0];
            Server server = servers[idx];
            Vexillum.game.Connect(server.ip, server.port);
        }
        private void ShowJoinDialog(object sender, EventArgs arguments)
        {
            view.OpenWindow(new IPJoinDialog());
        }
        private void ShowHostDialog(object sender, EventArgs arguments)
        {

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Util.GetGameFile("VexillumServerStart.exe");
            Process.Start(info);
            if(!File.Exists(Util.GetGameFile("ops.txt")))
                using (FileStream listFile = File.Create(Util.GetServerFile("ops.txt")))
                {
                    StreamWriter sw = new StreamWriter(listFile);
                    sw.WriteLine(Identity.username);
                    sw.Flush();
                }
        }
        class Server
        {
            public string name;
            public string ip;
            public int port;
            public int players;
            public int maxPlayers;
            public string map;
        }
    }
}
