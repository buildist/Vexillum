using System;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Vexillum.net;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace Vexillum.util
{
    public class DataTaskQueue
    {
        public int testLagTime = 0;
        private Disconnectable parent;
        private NetworkStream dataStream;
        private EndianBinaryWriter writer;
        private bool sendLength = false;
        Queue tasks = new Queue();
        Thread thread;
        bool running = true;
        bool disconnected = false;
        public DataTaskQueue(Disconnectable p, NetworkStream s, bool bigEndian)
        {
            sendLength = true;
            parent = p;
            dataStream = s;
            writer = new EndianBinaryWriter(EndianBitConverter.Big, dataStream);
        }
        public DataTaskQueue(Disconnectable p, NetworkStream s)
        {
            parent = p;
            dataStream = s;
            writer = new EndianBinaryWriter(EndianBitConverter.Little, dataStream);
        }
        public void Send(MemoryStream buffer, bool disconnect)
        {
            if (disconnected)
                return;
            disconnected = disconnect;
            /*Thread t = new Thread(delegate()
            {
                Thread.Sleep(testLagTime);*/
                try
                {
                    lock (this)
                    {
                        byte[] bytes = new byte[buffer.Position];
                        buffer.Seek(0, SeekOrigin.Begin);
                        for (int i = 0; i < bytes.Length; i++)
                            bytes[i] = (byte)buffer.ReadByte();

                        tasks.Enqueue(new DataArray(bytes, bytes.Length));

                        buffer.Seek(0, SeekOrigin.Begin);
                        buffer.SetLength(0);
                        Monitor.Pulse(this);
                    }
                }
                catch (Exception ex)
                {
                    ex = ex;
                }
            /*});
            t.Start();*/
        }
        public void Start()
        {
            thread = new Thread(new ThreadStart(Process));
            thread.Name = "DataSender";
            thread.Start();
        }
        public void Stop()
        {
            running = false;
            thread.Interrupt();
        }
        private void Process()
        {
            while (running && parent.IsConnected())
            {
                try
                {
                    lock (this)
                    {
                        while (tasks.Count > 0)
                        {
                            DataArray data = (DataArray)tasks.Dequeue();
                            if(sendLength)
                                writer.Write((short)data.length);
                            writer.Write(data.array, 0, data.length);
                            writer.Flush();
                        }
                        if (disconnected)
                        {
                            break;
                        }
                        else
                            Monitor.Wait(this);
                    }
                }
                catch (ThreadInterruptedException ex)
                {
                    running = false;
                    return;
                }
                catch (Exception ex)
                {
                    Util.Debug("Disconnected due to exception: " + ex);
                    Util.Debug(ex.StackTrace);
                    parent.Disconnect();
                    return;
                }
            }
            parent.Disconnect();
        }

        private struct DataArray
        {
            public DataArray(byte[] a, int l)
            {
                array = a;
                length = l;
            }
            public byte[] array;
            public int length;
        }
    }
}
