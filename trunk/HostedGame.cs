using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs
{
    public class HostedGame
    {
        public int ID { get; set; }
        public int UID { get; set; }
        public String Name { get; set; }
        public String GUID { get; set; }
        public String GameName { get; set; }
        public String GameVersion { get; set; }
        public String Password { get; set; }
        public Boolean Available { get; set; }
        public HostedGame(int uid, String name, String guid, String gamename, String gameversion, String password)
        {
            UID = uid;
            Name = name;
            GUID = guid;
            GameName = gamename;
            GameVersion = gameversion;
            Password = password;
            Available = true;
        }
    }
}
