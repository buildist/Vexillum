using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;

namespace Vexillum.ui
{
    class ServerListMenu : Menu
    {
        public ServerListMenu(AbstractView view)
            : base(view)
        {
            type = 2;
            AddButton("Main Menu", Back);
        }

        public void Back(object source, EventArgs args)
        {
            Vexillum.game.MPDisconnect();
            Vexillum.game.SetMenuView(true);
        }
    }

}
