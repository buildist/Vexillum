using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;

namespace Vexillum.ui
{
    class PauseMenu : Menu
    {
        public PauseMenu(AbstractView view) : base(view)
        {
            type = 0;
            MenuItem playItem = new MenuItem();
            playItem.Text = "Disconnect";
            AddItem(playItem);
            playItem.mouseClicked = delegate() { Vexillum.game.Disconnect(); };

            MenuItem optionsItem = new MenuItem();
            optionsItem.Text = "Options";
            AddItem(optionsItem);
            optionsItem.mouseClicked = delegate() { view.OpenWindow(new OptionsDialog()); };
            AddButton("Exit", new EventHandler(quit));
        }
        private void quit(object source, EventArgs args)
        {
            Vexillum.game.Disconnect();
            Vexillum.game.Exit();
        }
    }
}
