using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


    }
}
