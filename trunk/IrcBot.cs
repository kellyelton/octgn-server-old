﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Skylabs.ConsoleHelper;
using Skylabs.NetShit;
using Skylabs.oserver.Containers;

namespace Skylabs
{
    public static class IrcBot
    {
        private static string SERVER = "irc.ircstorm.net";
        private static int PORT = 6667;
#if(!DEBUG)
        private static string USER = "USER OctgnwLobby OctgnwLobby irc.ircstorm.net :OctgnwLobby";
        private static string NICK = "OctgnwLobby";
#else
        private static string USER = "USER OctgnwLobby_DEBUG OctgnwLobby_DEBUG irc.ircstorm.net :OctgnwLobby_DEBUG";
        private static string NICK = "OctgnwLobby_DEBUG";
#endif
        private static string CHANNEL = "#octgn";
        private static Boolean run;
        private static Thread thread;
        private static StreamWriter writer;
        private static StreamReader reader;
        private static TcpClient irc;
        private static NetworkStream stream;

        public static String[] Users
        {
            get
            {
                return _users;
            }
        }

        private static String[] _users;

        public static void Start()
        {
            run = true;
            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        private static void Run()
        {
            while (run)
            {
                irc = null;
                _users = new String[0];
                try
                {
                    irc = new TcpClient(SERVER, PORT);
                    irc.Client.Blocking = true;
                }
                catch (Exception e)
                {
                    ConsoleEventLog.addEvent(new ConsoleEventError("Irc Connect Error: ", e), true);
                    continue;
                }
                string inputLine = "";
                irc.Client.ReceiveTimeout = 500;
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                writer.WriteLine(USER);
                writer.Flush();
                writer.WriteLine("NICK " + NICK);
                writer.Flush();

                while (irc.Connected)
                {
                    try
                    {
                        inputLine = reader.ReadLine();
                        while (inputLine != null)
                        {
                            HandleLine(inputLine);
                            inputLine = reader.ReadLine();
                        }
                        Thread.Sleep(1000);
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode != SocketError.TimedOut)
                        {
                            ConsoleEventLog.addEvent(new ConsoleEventError("Irc SocketException", se), true);
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    catch (IOException e)
                    {
                        SocketException se = e.InnerException as SocketException;
                        if (se == null)
                            ConsoleEventLog.addEvent(new ConsoleEventError("Irc IOException", e), true);
                        else
                        {
                            if (se.SocketErrorCode != SocketError.WouldBlock)
                                ConsoleEventLog.addEvent(new ConsoleEventError("Irc IOException:SocketException", e), true);
                        }
                        Thread.Sleep(1000);
                        break;
                    }
                    catch (Exception e)
                    {
                        ConsoleEventLog.addEvent(new ConsoleEventError("Irc Exception", e), true);
                        break;
                    }
                    // Close all streams
                }

                writer.Close();
                reader.Close();
                irc.Close();
            }
        }

        public static void Stop()
        {
            run = false;
        }

        private static void HandleLine(String line)
        {
            if (line == null)
                return;
            Regex rpm = new Regex("\\:([^!]+){1}![^ ]+ PRIVMSG #octgn \\:(.+)", RegexOptions.IgnoreCase);
            Regex rquit = new Regex("\\:([^!]+){1}![^ ]+ QUIT", RegexOptions.IgnoreCase);
            Regex rjoin = new Regex("\\:([^!]+){1}![^ ]+ JOIN", RegexOptions.IgnoreCase);
            Regex rpmess = new Regex("\\:([^!]+){1}![^ ]+ PRIVMSG ([^ #]+){1} \\:(.+)", RegexOptions.IgnoreCase);
            Regex rpolist = new Regex(":.+353.+:(.+)+", RegexOptions.IgnoreCase);
            //353 OctgnwLobby = #OCTGN :
            //line = line.Replace(":helios.ircstorm.net", "");
            line = line.Trim();
            try
            {
                if (rpm.IsMatch(line))
                {
                    Match m = rpm.Match(line);
                    String user = m.Groups[1].Value;
                    String mess = m.Groups[2].Value;
                    //SocketMessage sm = new SocketMessage("CHAT");
                    ClientContainer.LobbyChat("<irc>" + user, mess);
                    //ClientContainer.AllUserCommand(
                }
                else if (rpmess.IsMatch(line))
                {
                    //TODO PM's go here, so put OP commands here
                    //1 is who it's from, 2 is who it goes to, and 3 is the message.
                    Match m = rpmess.Match(line);
                    if (m.Groups[1].Value.Equals("StatServ"))
                    {
                        if (m.Groups[3].Value.Contains("VERSION"))
                        {
                            WriteLine("JOIN " + CHANNEL + " x");
                        }
                    }
                    else
                    {
                        PMUser(m.Groups[1].Value, "Replying is not implemented yet.");
                    }
                }
                else if (rquit.IsMatch(line))
                {
                    Match m = rquit.Match(line);
                    String user = m.Groups[1].Value;
                    if (user != null)
                    {
                        if (UserOnline(user))
                        {
                            String[] temp = new String[_users.Length - 1];
                            int i = 0;
                            foreach (String u in _users)
                            {
                                if (!u.Equals(user))
                                {
                                    temp[i] = u;
                                    i++;
                                }
                            }
                            _users = temp;
                            SocketMessage sm = new SocketMessage("USEROFFLINE");
                            sm.Arguments.Add(user + "@irc.irc");
                            ClientContainer.AllUserCommand(sm);
                        }
                    }
                }
                else if (rjoin.IsMatch(line))
                {
                    // Make sure that user isn't online first
                    Match m = rjoin.Match(line);
                    String user = m.Groups[1].Value;
                    Array.Resize<String>(ref _users, _users.Length + 1);
                    _users[_users.Length - 1] = user;

                    SocketMessage sm = new SocketMessage("USERONLINE");
                    sm.Arguments.Add(user + "@irc.irc" + ":<irc>" + user);
                    ClientContainer.AllUserCommand(sm);

                    //SocketMessage sm = new SocketMessage("CHATINFO");
                    //sm.Arguments.Add("IRC User " + user + " online.");
                    //ClientContainer.AllUserCommand(sm);
                }
                else if (rpolist.IsMatch(line))
                {
                    Match m = rpolist.Match(line);
                    String strusers = m.Groups[1].Value;

                    _users = strusers.Split(new char[1] { ' ' });
                    //ConsoleWriter.writeLine("#IRC-IN: " + line, true);
                }
                else if (line.Substring(0, 4).Equals("PING"))
                {
                    WriteLine("PONG :" + line.Substring(6).Trim());
                }
                else
                {
                    ConsoleWriter.writeLine("#IRC-IN: " + line, true);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                ConsoleWriter.writeLine("#IRC-IN: " + line, true);
            }
        }

        public static Boolean UserOnline(String user)
        {
            if (user != null)
            {
                foreach (String u in _users)
                {
                    if (u.Equals(user))
                        return true;
                }
                return false;
            }
            return false;
        }

        public static void PMUser(string username, string chat)
        {
            WriteLine("PRIVMSG " + username + " :" + chat);
        }

        public static void PMUser(String username, String fromuser, String chat)
        {
            WriteLine("PRIVMSG " + username + " :" + fromuser + " says: " + chat);
        }

        public static void ChatAsUser(String username, String chat)
        {
            String user = username;
            while (UserOnline(user))
            {
                user += "_";
            }
            //WriteLine("NICK " + user);
            WriteLine("PRIVMSG #OCTGN :" + username + " says: " + chat);
        }

        public static void GeneralChat(String chat)
        {
            // WriteLine("NICK OctgnwLobby");
            WriteLine("PRIVMSG #OCTGN :" + chat);
        }

        public static void WriteLine(String line)
        {
            try
            {
                writer.WriteLine(line);
                writer.Flush();
            }
            catch (Exception e)
            {
            }
        }
    }
}