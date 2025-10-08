using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum;

namespace Server
{
    public static class Heartbeat
    {
        public static bool success = false;
        public static void Send(bool isPublic, string name, int key, string map, int players, int maxPlayers, int port)
        {
            try
            {
                name = Uri.EscapeDataString(name);
                string request = "name=" + name;
                request += "&key=" + key;
                request += "&map=" + map;
                request += "&players=" + players;
                request += "&maxplayers=" + maxPlayers;
                request += "&port=" + port;
                request += "&public=" + isPublic;
                string response = Util.HttpGet("ping.php?" + request);
                if (response != "OK")
                    Server.Debug("Error contacting the master server: " + response);
                else
                    success = true;
            }
            catch (Exception ex)
            {
                success = false;
                Server.Debug("Error contacting the master server: "+ex.Message);
            }
        }
    }
}
