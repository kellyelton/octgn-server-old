using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.NetShit;

namespace Skylabs.oserver.Containers
{
    public enum UserEventType
    {
        LogIn, LogOut
    };
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

        private static List<Client> _Clients = new List<Client>(1000);
        private static int intCurrentClientID = 0;

        public static void AddClient(Client c)
        {
            Clients.Add(c);
            int i = Clients.IndexOf(c);
            Clients[i].ID = i;
        }
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
        public static void UserEvent(UserEventType ue, Client c)
        {
            SocketMessage sm;
            switch (ue)
            {
                case Containers.UserEventType.LogIn:
                    sm = new SocketMessage("USERONLINE");
                    sm.Arguments.Add(c.User.Email + ":" + c.User.Username);
                    AllUserCommand(sm);
                break;
                case Containers.UserEventType.LogOut:
                    sm = new SocketMessage("USEROFFLINE");
                    sm.Arguments.Add(c.User.Email);
                    AllUserCommand(sm);
                break;
            }
        }
    }
}
