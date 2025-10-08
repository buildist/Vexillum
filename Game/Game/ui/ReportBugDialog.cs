using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;
using System.Threading;
using System.IO;

namespace Vexillum.ui
{
    class ReportBugDialog : WindowControl
    {
        private LabelControl label;
        private InputControl info;
        private ButtonControl ok;
        private ButtonControl cancel;
        public ReportBugDialog()
        {
            this.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -135), new UniScalar(0.5f, -150f)), new UniVector(370, 180));
            label = new LabelControl("This will upload your log files to\nplayvexillum.com to help fix crashes.\n\nAny additional info?");
            label.Bounds = new UniRectangle(10, 0, 300, 100);
            info = new InputControl();
            info.Bounds = new UniRectangle(10, 115, 350, 24);

            ok = new ButtonControl();
            ok.Text = "Send";
            ok.Bounds = new UniRectangle(190, 145, 80, 24);

            cancel = new ButtonControl();
            cancel.Text = "Cancel";
            cancel.Bounds = new UniRectangle(280, 145, 80, 24);

            Title = "Report Bug";

            cancel.Pressed += Close;
            ok.Pressed += Send;

            Children.Add(label);
            Children.Add(info);
            Children.Add(ok);
            Children.Add(cancel);
        }
        private void Close(object source, EventArgs args)
        {
            Close();
        }
        private void Send(object source, EventArgs args)
        {
            WindowControl status = Vexillum.game.StatusBox("Sending report...");
            Util.WriteDebugLog();
            new Thread(delegate()
            {
                string log = File.ReadAllText(Util.debugFileName);
                string id;

                id = Util.HttpPost("reportBug.php", "version=" + Vexillum.VERSION + "&os=" + Util.EscapeUriString(Environment.OSVersion.ToString()) + "&data=" + Util.EscapeUriString(log) + "&info=" + Util.EscapeUriString(info.Text));
                if (id == "")
                {
                    status.Close();
                    Vexillum.game.MessageBox("Error sending report. Is the website down?");
                    return;
                }
                status.Close();
                Vexillum.game.MessageBox("Report sent! ID: " + id);
                Close();
            }).Start();
        }
    }
}
