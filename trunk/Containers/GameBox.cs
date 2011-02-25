using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    Games.RemoveAt(i);
                    return ret;
                }
            }
            return ret;
        }
    }
}
