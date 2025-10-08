using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;
using Vexillum.util;
using Nuclex.UserInterface;
using Steamworks;

namespace Vexillum.ui
{
    class MainMenu : Menu
    {
        public MainMenu(AbstractView view) : base(view)
        {
            type = 1;
            MenuItem playItem = new MenuItem();
            playItem.Text = "Start Playing";
            AddItem(playItem);
            playItem.mouseClicked = playClicked;

            MenuItem optionsItem = new MenuItem();
            optionsItem.Text = "Options";
            AddItem(optionsItem);
            optionsItem.mouseClicked += optionsClicked;
            AddButton("Exit", new EventHandler(quit));
            AddButton("Report Bug", new EventHandler(ReportBug));

        }
        private void playClicked()
        {
            //view.Menu = new MultiplayerSelectMenu(view);
            view.OpenWindow(new ServerDialog(view));
            //Vexillum.game.Connect("128.61.105.216", Constants.DEFAULT_PORT);
        }
        private void optionsClicked()
        {
            view.OpenWindow(new OptionsDialog());
        }
        public void ReportBug(object source, EventArgs args)
        {
            view.OpenWindow(new ReportBugDialog());
        }
        public void quit(object source, EventArgs args)
        {
            Vexillum.game.Exit();
        }
    }
}
