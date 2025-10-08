using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Vexillum;

namespace ServerStart
{
    public partial class HostServerForm : Form
    {
        const string defaultConfig = "#Name of your server, will be shown in the server list.\nname Vexillum Server\n" +
                            "#The port number that the server should run on (must be accessible from the Internet.)\nport 24224\n" +
                            "#Maximum number of players\nmaxplayers 12\n"+
                            "#Default weapons\nweapons RocketLauncher SMG Sword\n"+
                            "#Captures needed to win\nmaxcaptures 4\n"+
                            "#Time to respawn in milliseconds\nrespawntime 5000\n"+
                            "#Should the server verify usernames with playvexillum.com? Recommended if it will be open to the Internet\nverifynames true\n"+
                            "#Should the server be displayed in the server list?\npublic true\n"+
                            "#List of maps for the server\nmaps bases complex\n"+
                            "#Maximum number of bots used to fill the server widh players\nmaxbots 6";
        public HostServerForm()
        {
            InitializeComponent();
            cancelButton.Click +=new EventHandler(Cancel);
            startButton.Click += new EventHandler(StartServer);
            if(!File.Exists(Util.GetServerFile("settings.txt")))
            {
                FileStream s = File.Open(Util.GetServerFile("settings.txt"), FileMode.Create);
                byte[] b = Encoding.ASCII.GetBytes(defaultConfig);
                s.Write(b, 0, b.Length);
                s.Close();
            }
            using (FileStream input = File.Open(Util.GetServerFile("settings.txt"), FileMode.Open, FileAccess.Read))
            {
                StreamReader r = new StreamReader(input);    
                StringBuilder b = new StringBuilder();
                string s;
                while ((s = r.ReadLine()) != null)
                {
                    b.Append(s + "\r\n");
                }
                textBox.Text = b.ToString();
            }
        }
        private void Cancel(object source, EventArgs e)
        {
            Dispose();
        }
        private void StartServer(object source, EventArgs e)
        {
            FileStream s = File.Open(Util.GetServerFile("settings.txt"), FileMode.Truncate);
            byte[] b = Encoding.ASCII.GetBytes(textBox.Text);
            s.Write(b, 0, b.Length);
            s.Close();
            try
            {
                Util.ParseServerConfig(textBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not parse server configuration: " + ex.Message);
                return;
            }
            lock (this)
            {
                Monitor.Pulse(this);
            }
            Dispose();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Util.GetGameFile("VexillumServer.exe");
            info.WorkingDirectory = Util.GetGameFile("");
            Process.Start(info);

        }

        private void HostServerForm_Load(object sender, EventArgs e)
        {

        }
    }
}
