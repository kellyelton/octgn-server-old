using System;

//using System.Linq;

namespace Skylabs
{
    public class HostedGame
    {
        public int ID { get; set; }

        public int UID { get; set; }

        public String Name { get; set; }

        public Boolean Available { get; set; }

        public String Description { get; set; }

        public HostedGame(int uid, String name, String description)
        {
            UID = uid;
            Name = name;
            Description = description;
            Available = true;
        }
    }
}