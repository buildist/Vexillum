using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Vexillum.Acceptance.toolsconfig
{
    /// <summary>Quick TCP status probe using the game's own protocol (byte 255 -> bool ready).</summary>
    public static class ServerProbe
    {
        public static bool IsReady(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (TcpClient c = new TcpClient())
                {
                    Task connect = c.ConnectAsync(host, port);
                    if (!connect.Wait(timeout) || !c.Connected)
                        return false;
                    c.ReceiveTimeout = (int)timeout.TotalMilliseconds;
                    NetworkStream s = c.GetStream();
                    s.WriteByte(255);
                    s.Flush();
                    int b = s.ReadByte();
                    return b == 1;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
