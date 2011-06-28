using System;
using System.Net;
using System.Net.Sockets;
using Skylabs.oserver;
using Skylabs.oserver.Containers;

namespace Skylabs.Networking
{
    public class Listener
    {
        public TcpListener Sock;

        public Listener(String host, int port)
        {
            IPAddress ip = null;
            host = host.Trim();
            if (host.Equals("*") || host.Equals(""))
                ip = IPAddress.Any;
            else
            {
                IPAddress.Parse(host);
            }
            Sock = new TcpListener(ip, port);
        }

        public void Start()
        {
            Sock.Start(1);
            Sock.BeginAcceptTcpClient(new AsyncCallback(SockConnect), Sock);
        }

        private void SockConnect(IAsyncResult AsyncCall)
        {
            //Accept the connection.
            TcpListener listener = (TcpListener)AsyncCall.AsyncState;
            Client c = new Client();
            c.GetAcceptedSocket(listener.EndAcceptTcpClient(AsyncCall));
            ClientContainer.AddClient(c);
            Start();
        }

        public void Stop()
        {
            Sock.Stop();
        }
    }
}