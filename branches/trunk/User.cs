using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.oserver
{
    public enum UserStatus
    {
        Available, Hosting, Playing, Away
    };
    public class User
    {
        public String Username { get; set; }
        public String Email { get; set; }
        public UserStatus Status { get; set; }
        public int UID { get; set; }
        public User()
        {
            UID = -1;
            Username = "";
            Email = "";
            Status= UserStatus.Available;
        }
        public User(int uid, String username, String email)
        {
            Username = username;
            Email = email;
            UID = uid;
            Status= UserStatus.Available;
        }
        public User(int uid, String username, String email, UserStatus status)
        {
            Username = username;
            Email = email;
            UID = uid;
            Status = status;
        }
    }
}
