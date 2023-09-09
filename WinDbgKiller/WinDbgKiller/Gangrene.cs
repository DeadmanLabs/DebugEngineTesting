using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Diagnostics.Runtime.Interop;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace WinDbgKiller
{
    class Attacker
    {
        public bool running { get; private set; }
        private Socket endpoint { get; set; }
        private List<object> actions { get; set; }
        private Thread background { get; set; }
        private Thread starter { get; set; }
        private (IPAddress, uint) client { get; set; }
        public Attacker(IPAddress ip, uint port)
        {
            running = false;
            actions = new List<object>();
            client = (ip, port);
        }

        public void start()
        {
            endpoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endpoint.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5);
            starter = new Thread(() =>
            {
                _run();
            });
            starter.Start();
        }

        public void send(byte[] packet)
        {
            if (running)
            {
                endpoint.Send(packet);
                actions.Add(packet);
            }
        }

        public void stop()
        {
            running = false;
            background.Join();
            endpoint.Close();
            if (MessageBox.Show("Attack Completed! Would you like to save a log?", "Save attack log?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.RestoreDirectory = true;
                    sfd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sfd.Filter = "Log File (*.log)|*.log";
                    sfd.AddExtension = true;
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter output = new StreamWriter(sfd.FileName))
                        {
                            foreach (object loggedEvent in actions)
                            {
                                output.WriteLine("Balls!"); //Fix this later
                            }
                        }
                    }
                }
            }
        }

        private void _run()
        {
            for (int i = 0; i < 5; i++)
            {
                if (endpoint.Connected)
                {
                    break;
                }
                try
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(client.Item1, (int)client.Item2);
                    endpoint.Connect(clientEndPoint);
                }
                catch (TimeoutException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            endpoint.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1);
            running = true;
            background = new Thread(() =>
            {
                _receivePort();
            });
            background.Start();
            return;
        }

        private void _receivePort()
        {
            while (running)
            {
                try
                {
                    byte[] data = new byte[4096];
                    int size = endpoint.Receive(data);
                    if (size > 0)
                    {
                        actions.Add(data); //Fix later
                    }
                }
                catch(TimeoutException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    actions.Add(ex); //Fix later
                }
            }
        }
    }
}
