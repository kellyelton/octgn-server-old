using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Skylabs.ConsoleHelper;
using Skylabs.Containers;
using Skylabs.NetShit;
using Skylabs.oserver.Containers;

namespace Skylabs.oserver
{
    public class Client : ShitSock
    {
        public User User { get { return _User; } set { _User = value; } }

        public Boolean LoggedIn { get; set; }

        public int ID { get; set; }

        public Boolean NotifiedLoggedOff { get; set; }

        private User _User = new User();
        private Boolean isRc = false;

        override public void handleError(ShitSock me, Exception e, String error)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError(error, e), true);
        }

        override public void handleInput(ShitSock me, SocketMessage input)
        {
            if (!LoggedIn)
            {
                switch (input.Header)
                {
                    case "LOG":
                        Boolean worked = UserMysql.login(this, input.Arguments[0], input.Arguments[1]);
                        if (worked)
                        {
                            for (int i = 0; i < ClientContainer.Clients.Count; i++)
                            {
                                if (ClientContainer.Clients[i].User.UID == User.UID)
                                {
                                    if (ClientContainer.Clients[i].ID != this.ID)
                                    {
                                        if (ClientContainer.Clients[i].Connected)
                                        {
                                            ClientContainer.UserEvent(UserEventType.LogOut, ClientContainer.Clients[i]);
                                            ClientContainer.Clients[i].Close("Another user logged in.", false);
                                        }
                                    }
                                }
                            }
                            SocketMessage sm;
                            if (input.Arguments.Count < 3)
                            {
                                sm = new SocketMessage("LOGSUCCESS");
                                sm.Arguments.Add(this.User.Username);
                                writeMessage(sm);
                                ClientContainer.UserEvent(UserEventType.LogIn, this);
                                Thread th = new Thread(new ThreadStart(delegate()
                                {
                                    Thread.Sleep(1000);
                                    sm = new SocketMessage("CHATINFO");
                                    sm.Arguments.Add("YOU DO NOT HAVE THE LATEST LOBBY VERSION. PLEASE VISIT http://www.skylabsonline.com/blog/project/octgn-w-lobby/ TO GET IT!");
                                    this.writeMessage(sm);
                                    sm = new SocketMessage("CHATINFO");
                                    sm.Arguments.Add("User " + this.User.Username + " could not log in because he/she needs an update.");
                                    ClientContainer.AllUserCommand(sm);
                                    Thread.Sleep(1000);
                                    this.Close("Incompatable version.", true);
                                }
                                ));
                                th.Start();
                            }
                            else
                            {
                                if (!input.Arguments[2].Equals(MainClass.getCurRevision().Trim()))
                                {
                                    sm = new SocketMessage("LOGSUCCESS");
                                    sm.Arguments.Add(this.User.Username);
                                    writeMessage(sm);
                                    ClientContainer.UserEvent(UserEventType.LogIn, this);
                                    Thread th = new Thread(new ThreadStart(delegate()
                                        {
                                            Thread.Sleep(1000);
                                            sm = new SocketMessage("CHATINFO");
                                            sm.Arguments.Add("YOU DO NOT HAVE THE LATEST LOBBY VERSION. PLEASE VISIT http://www.skylabsonline.com/blog/project/octgn-w-lobby/ TO GET IT!");
                                            this.writeMessage(sm);
                                            sm = new SocketMessage("CHATINFO");
                                            sm.Arguments.Add("User " + this.User.Username + " could not log in because he/she needs an update.");
                                            ClientContainer.AllUserCommand(sm);
                                            Thread.Sleep(1000);
                                            this.Close("Incompatable version.", true);
                                        }
                                    ));
                                    th.Start();
                                }
                                else
                                {
                                    sm = new SocketMessage("LOGSUCCESS");
                                    sm.Arguments.Add(this.User.Username);
                                    writeMessage(sm);
                                    ClientContainer.UserEvent(UserEventType.LogIn, this);
                                    sm = new SocketMessage("CHATINFO");
                                    sm.Arguments.Add(MainClass.getDailyMessage());
                                    this.writeMessage(sm);
                                    LoggedIn = true;
                                }
                            }
                        }
                        else
                        {
                            SocketMessage sm = new SocketMessage("LOGERROR");
                            writeMessage(sm);
                            LoggedIn = false;
                        }
                        break;
                    case "REG":
                        String nick = input.Arguments[0];
                        String email = input.Arguments[1];
                        String pass = input.Arguments[2];
                        String regret = UserMysql.register(nick, email, pass);
                        writeMessage(new SocketMessage(regret));
                        break;
                    case "RC":
                        this.isRc = true;
                        User = new User(987654321, "rc", "rc");
                        break;
                }
            }
            else if (LoggedIn)
            {
                switch (input.Header)
                {
                    case "GETONLINELIST":

                        StringBuilder sb = new StringBuilder();
                        SocketMessage sm = new SocketMessage("ONLINELIST");
                        List<User> listOnlineUsers = ClientContainer.getOnlineUserList();
                        int intOnlineUsers = listOnlineUsers.Count;
                        for (int i = 0; i < intOnlineUsers; i++)
                        {
                            sb = new StringBuilder();
                            sb.Append(listOnlineUsers[i].Email);
                            sb.Append(':');
                            sb.Append(listOnlineUsers[i].Username);
                            sb.Append(':');
                            sb.Append(listOnlineUsers[i].Status.ToString());
                            sm.Arguments.Add(sb.ToString());
                        }
                        /*
                        foreach (String user in IrcBot.Users)
                        {
                            sb = new StringBuilder();
                            sb.Append(user + "@irc.irc");
                            sb.Append(':');
                            sb.Append("<irc>" + user);
                            sb.Append(':');
                            sb.Append(UserStatus.Available.ToString());
                            sm.Arguments.Add(sb.ToString());
                        }
                         */
                        writeMessage(sm);
                        break;
                    case "LOBCHAT":
                        ClientContainer.LobbyChat(User.Username, input.Arguments[0].Trim());
                        break;
                    case "HOST":
                        HostedGame h = new HostedGame(User.UID, input.Arguments[0], input.Arguments[1]);
                        int gID = GameBox.AddGame(h);
                        SocketMessage stemp = input;
                        stemp.Arguments.Add(this.User.Username);
                        stemp.Arguments.Add(gID.ToString());

                        ClientContainer.AllUserCommand(stemp);
                        break;
                    case "UNHOST":
                        string ret = GameBox.RemoveByUID(User.UID);
                        String[] rets = ret.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (String b in rets)
                        {
                            if (!ret.Equals("-1"))
                            {
                                SocketMessage stemp2 = input;
                                stemp2.Arguments.Add(b);
                                ClientContainer.AllUserCommand(stemp2);
                            }
                        }
                        break;
                    case "STATUS":
                        User.Status = (UserStatus)Enum.Parse(typeof(UserStatus), input.Arguments[0], true);
                        input.Arguments.Add(User.Username);
                        ClientContainer.AllUserCommand(input);
                        break;
                    case "GAMELIST":
                        int mGames = GameBox.Games.Count;
                        for (int i = 0; i < mGames; i++)
                        {
                            HostedGame hg = (HostedGame)GameBox.Games[i];
                            if (hg.Available)
                            {
                                SocketMessage stemp3 = new SocketMessage("GAMELIST");
                                //GameID
                                stemp3.Arguments.Add(hg.ID.ToString());
                                //UserEmail
                                stemp3.Arguments.Add(ClientContainer.getClientFromUID(hg.UID).User.Email);
                                //GameName
                                stemp3.Arguments.Add(hg.Name);
                                //Description
                                stemp3.Arguments.Add(hg.Description);
                                this.writeMessage(stemp3);
                            }
                        }
                        break;
                    default:
                        //TODO add ConsoleEvent to handle unknown input
                        //Main.writeEvent("Input from `" + super.strHost + "' :" + input.getMessage());
                        //Main.writeIn();
                        break;
                }
            }
            if (isRc)
            {
                ConsoleEventLog.addEvent(new ConsoleEvent("#RC: ", input.getMessage()), true);
                switch (input.Header)
                {
                    case "1":
                        SocketMessage sm = new SocketMessage("CHATINFO");
                        sm.Arguments.Add(input.Arguments[0]);
                        ClientContainer.AllUserCommand(sm);
                        break;
                    case "2":
                        MainClass.KillServer();
                        break;
                    case "3":
                        try
                        {
                            int time = int.Parse(input.Arguments[0]);
                            MainClass.TimeKillServer(time, null);
                        }
                        catch (Exception e)
                        {
                        }
                        break;
                    case "4":
                        try
                        {
                            int time = int.Parse(input.Arguments[0]);
                            MainClass.TimeKillServer(time, input.Arguments[1]);
                        }
                        catch (Exception e)
                        {
                        }
                        break;
                }
            }
        }

        override public void handleConnect(ShitSock me, String host, int port)
        {
            ConsoleEventLog.addEvent(new ConsoleEvent("Client " + host + " connected."), true);
        }

        override public void handleDisconnect(ShitSock me, String reason, String host, int port)
        {
            if (!NotifiedLoggedOff)
            {
                ClientContainer.UserEvent(UserEventType.LogOut, this);
                this.NotifiedLoggedOff = true;
                this.LoggedIn = false;
                string ret = GameBox.RemoveByUID(User.UID);
                String[] rets = ret.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String b in rets)
                {
                    if (!ret.Equals("-1"))
                    {
                        SocketMessage stemp2 = new SocketMessage("UNHOST");
                        stemp2.Arguments.Add(b);
                        ClientContainer.AllUserCommand(stemp2);
                    }
                }
            }
            ConsoleEventLog.addEvent(new ConsoleEvent("Client " + host + " disconnected because " + reason), true);
        }
    }
}