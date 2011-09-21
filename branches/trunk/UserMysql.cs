using System;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Skylabs.ConsoleHelper;

namespace Skylabs.oserver
{
    public class UserMysql
    {
        public static Boolean login(Client c, String username, String password)
        {
            StringBuilder conString = new StringBuilder("SERVER=");
            conString.Append(MainClass.getProperty("DBHost"));
            conString.Append(";");
            conString.Append("DATABASE=");
            conString.Append(MainClass.getProperty("DB"));
            conString.Append(";");
            conString.Append("UID=");
            conString.Append(MainClass.getProperty("DBUser"));
            conString.Append(";");
            conString.Append("PASSWORD=");
            conString.Append(MainClass.getProperty("DBPass"));
            conString.Append(";");

            MySqlConnection connection = new MySqlConnection(conString.ToString());
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader = null;

            command.CommandText = "SELECT * FROM users WHERE email='" + username + "' AND pass='" + password + "';";
            try
            {
                connection.Open();
                Reader = command.ExecuteReader();
                while(Reader.Read() == true)
                {
                    if(!Reader.IsDBNull(0))
                    {
                        for(int i = 0; i < Reader.FieldCount; i++)
                        {
                            switch(Reader.GetName(i))
                            {
                                case "uid":
                                    c.User.UID = Reader.GetInt32(i);
                                    break;
                                case "username":
                                    c.User.Username = Reader.GetString(i);
                                    break;
                            }
                        }
                        c.User.Email = username;
                        Reader.Close();
                        connection.Close();
                        return true;
                    }
                }
                Reader.Close();
                connection.Close();
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error1." + e.Message, e), true);
                //TODO Some exception here if we lose connectivity, fix it, don't have the global catch all.
                try
                {
                    Reader.Close();
                    connection.Close();
                }
                catch(Exception ex)
                {
                    ConsoleEventLog.addEvent(new ConsoleEventError(ex.Message, ex), true);
                }
            }
            return false;
            //Failure
        }

        public static String register(String nick, String email1, String pass1)
        {
            StringBuilder conString = new StringBuilder("SERVER=");
            conString.Append(MainClass.getProperty("DBHost"));
            conString.Append(";");
            conString.Append("DATABASE=");
            conString.Append(MainClass.getProperty("DB"));
            conString.Append(";");
            conString.Append("UID=");
            conString.Append(MainClass.getProperty("DBUser"));
            conString.Append(";");
            conString.Append("PASSWORD=");
            conString.Append(MainClass.getProperty("DBPass"));
            conString.Append(";");
            MySqlConnection connection = new MySqlConnection(conString.ToString());
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader = null;

            #region VerifyInput

            nick = nick.Trim();
            email1 = email1.Trim().ToLower();
            pass1 = pass1.Trim();
            if(String.IsNullOrEmpty(nick))
            {
                return "REGERR1";
            }
            if(String.IsNullOrEmpty(email1))
            {
                return "REGERR1";
            }
            else
            {
                Regex r = new Regex("^[\\w\\-]+(\\.[\\w\\-]+)*@([A-Za-z0-9-]+\\.)+[A-Za-z]{2,4}$");
                if(!r.IsMatch(email1))
                {
                    return "REGERR1";
                }
            }

            if(pass1.Length < 6)
            {
                return "REGERR2";
            }

            #endregion VerifyInput

            try
            {
                connection.Open();
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error2.", e), true);
                connection.Close();
                return "REGERR0";
            }

            //See if e-mail address is already registered
            command.CommandText = "select * from users WHERE email='" + email1 + "';";
            try
            {
                Reader = command.ExecuteReader();
                if(Reader.Read() != false)
                {
                    if(!Reader.IsDBNull(0))
                    {
                        Reader.Close();
                        connection.Close();
                        return "REGERR4";
                    }
                }
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
                Reader.Close();
                connection.Close();
                return "REGERR0";
            }
            Reader.Close();
            //Sees if username is already regestered
            command.CommandText = "select * from users WHERE username='" + nick + "';";
            try
            {
                Reader = command.ExecuteReader();
                if(Reader.Read() != false)
                {
                    if(!Reader.IsDBNull(0))
                    {
                        Reader.Close();
                        connection.Close();
                        return "REGERR3";
                    }
                }
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
                Reader.Close();
                connection.Close();
                return "REGERR0";
            }
            Reader.Close();

            //Hash password
            //Encoding enc = Encoding.ASCII;
            //byte[] data = enc.GetBytes(pass1);
            //pass1 = BitConverter.ToString((new SHA1CryptoServiceProvider()).ComputeHash(data));
            //pass1 = pass1.Replace("-", "");
            //pass1 = pass1.ToLower();

            command.CommandText =
                    "INSERT INTO users (email,pass,username)"
                    + " VALUES"
                    + "('"
                    + email1
                    + "','"
                    + pass1
                    + "','"
                    + nick
                    + "')";

            try
            {
                int uid = command.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
                return "REGERR0";
            }
            return "REGSUCCESS";
        }
    }
}