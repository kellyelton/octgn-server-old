using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Skylabs.ConsoleHelper;

namespace Skylabs.oserver
{
public class UserMysql {
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
		MySqlDataReader Reader;

		command.CommandText = "SELECT * FROM users WHERE email='" + username + "' AND pass='" + password + "';";
        try
        {
            connection.Open();
            Reader = command.ExecuteReader();
            while(Reader.Read() == true)
            {
                if (!Reader.IsDBNull(0))
                {
                        for (int i = 0; i < Reader.FieldCount; i++)
                        {
                            switch (Reader.GetName(i))
                            {
                                case "uid":
                                    c.User.UID = Reader.GetInt32(i);
                                break;
                                case "email":
                                    c.User.Email = Reader.GetString(i);
                                break;
                            }
                        }
                        c.User.Username = username;
                    connection.Close();
                    return true;
                }
            }
            connection.Close();
        }
        catch (Exception e)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
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
        MySqlDataReader Reader;

        #region VerifyInput
        nick = nick.Trim();
        email1 = email1.Trim().ToLower();
        pass1 = pass1.Trim();
        if(String.IsNullOrEmpty(nick))
        {
            return "REGERR1";
        }
        if (String.IsNullOrEmpty(email1))
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
        catch (Exception e)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
            connection.Close();
            return "REGERR0";
        }

        //See if e-mail address is already registered
        command.CommandText = "select `*` from `users` WHERE `email`='" + email1 + "';";
        try
        {
            Reader = command.ExecuteReader();
            if (Reader.Read() != false)
            {
                if (!Reader.IsDBNull(0))
                {
                    connection.Close();
                    return "REGERR4";
                }
            }
            connection.Close();
        }
        catch (Exception e)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
            connection.Close();
            return "REGERR0";
        }

        //Sees if username is already regestered
        command.CommandText = "select `*` from `users` WHERE `username`='" + nick + "';";
        try
        {
            Reader = command.ExecuteReader();
            if (Reader.Read() != false)
            {
                if (!Reader.IsDBNull(0))
                {
                    connection.Close();
                    return "REGERR3";
                }
            }
            connection.Close();
        }
        catch (Exception e)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
            connection.Close();
            return "REGERR0";
        }

        //Hash password
        byte[] data = System.Text.Encoding.ASCII.GetBytes(pass1);
        byte[] result; 

        SHA1 sha = new SHA1CryptoServiceProvider(); 
        result = sha.ComputeHash(data);
        pass1 = Encoding.ASCII.GetString(result);

        command.CommandText = 
                "INSERT INTO `users` (email,pass,username)"
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
        catch (Exception e)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError("Mysql error.", e), true);
            return "REGERR0";
        }
        return "REGSUCCESS";



    }
}
}
