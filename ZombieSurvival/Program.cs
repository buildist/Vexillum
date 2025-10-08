using System;
using System.Windows.Forms;
using Vexillum.ui;
using System.Threading;
using Vexillum.steam;

namespace Vexillum
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Util.OpenLockFile();

            if (!SteamManager.Initialize())
            {
                System.Windows.Forms.MessageBox.Show("Error communicating with Steam. Make sure the Steam client is running and restart the game.");
                return;
            }


            using (Vexillum game = new Vexillum())
            {
                game.Run();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Util.Debug("Unexpected exception: " + e.ExceptionObject.ToString());
            if (e.ExceptionObject is Exception)
                Util.Debug(((Exception)e.ExceptionObject).StackTrace);
            Util.WriteDebugLog();
            MessageBox.Show("Sorry, an unexpected error occured! Please visit playvexillum.com/forum to submit a bug report.");
        }
    }
#endif
}

