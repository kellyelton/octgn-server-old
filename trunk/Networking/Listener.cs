using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Skylabs.oserver;
using Skylabs.oserver.Containers;
using Skylabs.ConsoleHelper;

namespace Skylabs.Networking
{
    public class Listener
    {
        private TcpListener Sock;
        private Thread thread;
        private Boolean endIt = false;

        public Listener(String host, int port)
        {
            IPAddress ip = null;
            host = host.Trim();
            if(host.Equals("*") || host.Equals(""))
                ip = IPAddress.Loopback;
            else
            {
                IPAddress.Parse(host);
            } 
            Sock = new TcpListener(ip, port);
            thread = new Thread(new ThreadStart(run));
            thread.Name = "ServerThread";
            
        }
        public void Start()
        {
            thread.Start();
        }
        public void Stop()
        {
            endIt = true;
        }
        private void run()
        {
            try
            {
                Sock.Start();
            }
            catch (SocketException e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Socket error.", e), true);
                MainClass.Stop();
            }
            Sock.Server.ReceiveTimeout = 500;
            Sock.Server.SendTimeout = 500;
            Sock.Server.Blocking = false;
            Sock.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
            while (!endIt)
            {
                Client c = new Client();
                try
                {
                    c.sock = Sock.AcceptSocket();
                    ClientContainer.Clients.Add(c);
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.WouldBlock)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }
    }
}
