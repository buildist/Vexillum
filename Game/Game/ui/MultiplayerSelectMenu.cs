using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;

namespace Vexillum.ui
{
    class MultiplayerSelectMenu : Menu
    {
        public MultiplayerSelectMenu(AbstractView view) : base(view)
        {
            type = 1;
            MenuItem playItem = new MenuItem();
            playItem.Text = "Find Server";
            AddItem(playItem);
            playItem.mouseClicked = ShowServerList;

            MenuItem connect = new MenuItem();
            connect.Text = "Direct Connect";
            AddItem(connect);
            connect.mouseClicked += ShowConnectDialog;

            MenuItem back = new MenuItem();
            back.Text = "Main Menu";
            AddItem(back);
            back.mouseClicked += Back;

            AddButton("Quit", new EventHandler(quit));
        }
        private void ShowServerList()
        {
            Vexillum.game.SetMultiplayerView();
        }
        private void ShowConnectDialog()
        {
            view.OpenWindow(new IPJoinDialog());
        }
        private void Back()
        {
            view.Menu = new MainMenu(view);
        }

        public void quit(object source, EventArgs args)
        {
            Vexillum.game.Exit();
        }
    }
}
