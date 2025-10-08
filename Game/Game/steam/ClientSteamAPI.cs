using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;

namespace Vexillum.steam
{
    class ClientSteamAPI
    {
        private Vexillum game;
        private Callback<GameOverlayActivated_t> gameOverlayActivated;

        public ClientSteamAPI(Vexillum game)
        {
            this.game = game;
        }

        public void Initialize()
        {
            gameOverlayActivated = SteamManager.CreateCallback<GameOverlayActivated_t>(GameOverlayActivated);
        }

        private void GameOverlayActivated(GameOverlayActivated_t cb)
        {
            if (game.View is GameView)
            {
                if (cb.m_bActive == 1)
                    ((GameView)Vexillum.game.View).Pause();
                else
                    ((GameView)Vexillum.game.View).Unpause();
            }
        }

    }
}
