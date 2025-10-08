using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.steam;

namespace Server
{
    class ServerSteamAPI
    {
        private Server server;
        private Callback<ValidateAuthTicketResponse_t> validateAuthTicketResponse;

        public ServerSteamAPI(Server server)
        {
            this.server = server;
        }

        public void Initialize()
        {
            SteamManager.CreateCallback<ValidateAuthTicketResponse_t>(ValidateAuthTicketResponse);
        }

        private void ValidateAuthTicketResponse(ValidateAuthTicketResponse_t cb)
        {
            if (cb.m_eAuthSessionResponse != EAuthSessionResponse.k_EAuthSessionResponseOK)
            {
                server.KickPlayer(cb.m_SteamID.m_SteamID, "Steam authentication failed");
            }
        }
    }
}
