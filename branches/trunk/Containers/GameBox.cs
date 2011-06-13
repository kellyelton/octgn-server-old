using System;
using System.Collections;
using Skylabs.ConsoleHelper;

namespace Skylabs.Containers
{
    public class GameBox
    {
        public static ArrayList Games
        {
            get
            {
                lock (_Games)
                {
                    return _Games;
                }
            }
            set
            {
                lock (_Games)
                {
                    _Games = value;
                }
            }
        }

        private static ArrayList _Games = new ArrayList();

        public HostedGame this[int index]
        {
            get
            {
                return (HostedGame)Games[index];
            }
            set
            {
                Games[index] = value;
            }
        }

        public static int AddGame(HostedGame game)
        {
            int i = Games.Add(game);
            HostedGame h = (HostedGame)Games[i];
            h.ID = i;
            Version V = new Version(h.GameVersion);
            Guid G = new Guid(h.GUID);
            int port = 6000 + i;
            ConsoleEventLog.addEvent(new ConsoleEvent("#Starting server on port " + port.ToString()), true);
            //h.Server = new Octgn.Server.Server(port, false, G, V);
            return i;
        }

        public static String RemoveByUID(int UID)
        {
            int ret = -1;
            String sret = "";
            for (int i = 0; i < Games.Count; i++)
            {
                HostedGame g = (HostedGame)Games[i];
                if (g.UID == UID && g.Available)
                {
                    ret = g.ID;
                    //Games[i].Server.Stop();
                    g.Available = false;
                    //Games.RemoveAt(i);
                    sret += ret.ToString() + ":";
                }
                else
                {
                    int deadCount = 0;
                }
            }
            return sret;
        }
    }
}