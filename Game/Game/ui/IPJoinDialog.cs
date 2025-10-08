using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;
using System.Net;
using Vexillum.util;

namespace Vexillum.ui
{
	class IPJoinDialog : WindowControl
	{
        private InputControl ipBox;
        private InputControl portBox;
        private ButtonControl connectButton;
        private ButtonControl cancelButton;
        public IPJoinDialog()
        {
            ipBox = new InputControl();
            portBox = new InputControl();
            connectButton = new ButtonControl();
            cancelButton = new ButtonControl();

            cancelButton.Pressed += new EventHandler(cancel);
            connectButton.Pressed += new EventHandler(connect);

            ipBox.Text = "127.0.0.1";
            portBox.Text = ""+VexillumConstants.DEFAULT_PORT;
            connectButton.Text = "Connect";
            cancelButton.Text = "Cancel";

            ipBox.Bounds = new UniRectangle(10, 30, 150, 24);
            portBox.Bounds = new UniRectangle(170, 30, 40, 24);
            connectButton.Bounds = new UniRectangle(220, 30, 80, 24);
            cancelButton.Bounds = new UniRectangle(310, 30, 80, 24);

            this.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -200), new UniScalar(0.5f, -65f)), new UniVector(400, 65));
            this.Title = "Connect to Server";

            Children.Add(ipBox);
            Children.Add(portBox);
            Children.Add(connectButton);
            Children.Add(cancelButton);
        }

        private void cancel(object sender, EventArgs arguments)
        {
            Close();
        }
        private void connect(object sender, EventArgs arguments)
        {
            string ip = ipBox.Text;
            int port = 0;
            try
            {
                port = int.Parse(portBox.Text);
            }
            catch (Exception ex)
            {
                port = VexillumConstants.DEFAULT_PORT;
            }
            Vexillum.game.Connect(ip, port);
        }
	}
}
