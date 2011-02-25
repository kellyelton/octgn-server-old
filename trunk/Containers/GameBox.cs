using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.ConsoleHelper;

namespace Skylabs.Containers
{
    public class GameBox
    {
        public static List<HostedGame> Games
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

        private static List<HostedGame> _Games = new List<HostedGame>();

        public static int AddGame(HostedGame game)
        {
            Games.Add(game);
            int i = Games.IndexOf(game);
            Games[i].ID = i;
            Version V = new Version(Games[i].GameVersion);
            Guid G = new Guid(Games[i].GUID);
            ConsoleWriter.writeLine("#Done making version and GUID", true);
            Games[i].Server = new Octgn.Server.Server(6000 + i, false, G, V);
            return i;
        }
        public static int RemoveByUID(int UID)
        {
            int ret = -1;
            for (int i = 0; i < Games.Count; i++)
            {
                if (Games[i].UID == UID && Games[i].Available)
                {
                    ret = Games[i].ID;
                    Games[i].Server.Stop();
                    Games.RemoveAt(i);
                    return ret;
                }
            }
            return ret;
        }
    }
}
