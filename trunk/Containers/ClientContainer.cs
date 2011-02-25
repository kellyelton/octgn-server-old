using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.NetShit;

namespace Skylabs.oserver.Containers
{
    public class ClientContainer
    {
        public static List<Client> Clients 
        {
            get
            {
                lock (_Clients)
                {
                    return _Clients;
                }
            }
            set
            {
                lock (_Clients)
                {
                    _Clients = value;
                }
            }
        }

        private static List<Client> _Clients = new List<Client>();

        public static void AllUserCommand(SocketMessage command)
        {
            int intcount = Clients.Count;
            for(int i=0;i<intcount;i++)
            {
                if(Clients[i].Connected)
                {
                    Clients[i].writeMessage(command);
                }
            }
        }
    }
}
