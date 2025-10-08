using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;
using System.Threading;

namespace Vexillum.ui
{
    class ServerListPanel : ScrollPanel
    {
        private List<Server> servers = new List<Server>();
        private int selectedIndex;
        private Thread updateThread;

        public ServerListPanel(MainMenuView view)
            : base(view.gd, 50, 50, 700, 400)
        {

        }
        public void UpdateList()
        {
            if (updateThread == null)
            {
                updateThread = new Thread(new ThreadStart(this.DoUpdate));
                updateThread.Start();
            }
        }
        protected override void Resize(int x, int y)
        {
        }
        public void DoUpdate()
        {
            servers.Clear();
            string[][] array = Util.HttpGetArray("servers.php");
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
                //serverList.Items.Add(array[i][0]);
            }
        }
        protected override void DrawContent(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {

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
