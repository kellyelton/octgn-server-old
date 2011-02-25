using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.oserver
{
    public class User
    {
        public String Username { get; set; }
        public String Email { get; set; }
        public int UID { get; set; }
        public User()
        {
            UID = -1;
            Username = "";
            Email = "";
        }
        public User(int uid, String username, String email)
        {
            Username = username;
            Email = email;
            UID = uid;
        }
    }
}
