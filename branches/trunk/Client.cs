using System;
using Skylabs.ConsoleHelper;
using Skylabs.Lobby;
using Skylabs.Lobby.Containers;
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

        private void iLogin(SocketMessage input)
        {
            Boolean worked = UserMysql.login(this, input.Arguments[0], input.Arguments[1]);
            if(worked)
            {
                //See if we are already logged on width a different client.
                //If we are, dominate him and destroy his connection.
                for(int i = 0; i < ClientContainer.Clients.Count; i++)
                {
                    if(ClientContainer.Clients[i].User.UID == User.UID)
                    {
                        if(ClientContainer.Clients[i].ID != this.ID)
                        {
                            if(ClientContainer.Clients[i].Connected)
                            {
                                //Domination///////////////////////////////////////////////////////////////
                                ClientContainer.UserEvent(UserEventType.LogOut, ClientContainer.Clients[i]);
                                //////////////////////////////////////////
                                //     _________________.---.______     //
                                //    (_(______________(_o o_(____()    //
                                //                 .___.'. .'.___.      //
                                //                 \ o    Y    o /      //
                                //  DISTRUCTION     \ \__   __/ /       //
                                //                   '.__'-'__.'        //
                                //    BOOOOOOM!          ```            //
                                //////////////////////////////////////////
                                ClientContainer.Clients[i].Close();
                            }
                        }
                    }
                }
                writeMessage(SocketMessages.LoginSuccess(this.User));
                ClientContainer.UserEvent(UserEventType.LogIn, this);
                //this.writeMessage(SockMessages.ChatInfo(MainClass.getDailyMessage()));
                LoggedIn = true;
            }
            else
            {
                writeMessage(SocketMessages.LoginError());
                LoggedIn = false;
            }
        }

        override protected void handleError(object me, Exception e, String error)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError(error, e), true);
        }

        override protected void handleInput(object me, SocketMessage input)
        {
            if(!LoggedIn)
            {
                switch(input.Header)
                {
                    case "LOG":
                        iLogin(input);
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
            else if(LoggedIn)
            {
                switch(input.Header)
                {
                    case "GETONLINELIST":
                        writeMessage(SocketMessages.OnlineUserList(ClientContainer.getOnlineUserList()));
                        break;
                    case "LOBCHAT":
                        ClientContainer.LobbyChat(User.Username, input.Arguments[0].Trim());
                        break;
                    case "HOST":
                        //HACK Should serialize game data and send it here.
                        HostedGame h = HostedGame.DeSerialize(Convert.FromBase64String(input.Arguments[0]));
                        h.UID = User.UID;
                        int gID = GameBox.AddGame(h);
                        h.ID = gID;
                        SocketMessage stemp = new SocketMessage("HOST");
                        stemp.Arguments.Add(Convert.ToBase64String(HostedGame.Serialize(h)));
                        ClientContainer.AllUserCommand(stemp);
                        break;
                    case "UNHOST":
                        int g = GameBox.GetGame(User);
                        if(g != -1)
                        {
#if(DEBUG)
                            ConsoleWriter.writeLine("#UNHOST", true);
#endif
                            SocketMessage stemp2 = new SocketMessage("UNHOST");
                            stemp2.Arguments.Add(Convert.ToBase64String(HostedGame.Serialize(GameBox.Games[g])));
                            ClientContainer.AllUserCommand(stemp2);
                            GameBox.RemoveByUID(User.UID);
                        }
                        break;
                    case "STATUS":
                        User.Status = (UserStatus)Enum.Parse(typeof(UserStatus), input.Arguments[0], true);
                        input.Arguments.Add(User.Username);
                        ClientContainer.AllUserCommand(input);
                        break;
                    case "GAMELIST":
                        int mGames = GameBox.Games.Count;
                        for(int i = 0; i < mGames; i++)
                        {
                            HostedGame hg = (HostedGame)GameBox.Games[i];
                            if(hg.Available)
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
                    case "JOINGAME":
                        HostedGame hgg = HostedGame.DeSerialize(Convert.FromBase64String(input.Arguments[0]));
                        ConsoleWriter.writeLine("#JOINGAME(" + User.Username + ")", true);
                        int gn = GameBox.GetGame(hgg.ID);
                        if(gn > -1)
                        {
                            Client c = ClientContainer.getClientFromUID(GameBox.Games[gn].UID);
                            if(c.User.UID != -1)
                            {
                                c.writeMessage(input);
                            }
                        }
                        break;
                    case "GAMEFORWARD":
                        //Basically just forward the whole package to the Host, found by the game ID
                        //which resides at Argument 0
                        //Get the int
                        try
                        {
                            int gid = int.Parse(input.Arguments[0]);
                            HostedGame hg = GameBox.Games[GameBox.GetGame(gid)];
                            if(hg.Available)
                            {
                                Client tclient = ClientContainer.getClientFromUID(hg.UID);
                                if(tclient != null)
                                {
                                    input.Arguments.Insert(1, Convert.ToBase64String(User.Serialize(User)));
                                    tclient.writeMessage(input);
                                }
#if DEBUG
                                else
                                    System.Diagnostics.Debugger.Break();
#endif
                            }
                        }
                        catch(FormatException fe)
                        {
#if DEBUG
                            System.Diagnostics.Debugger.Break();
#endif
                        }
                        break;
                    default:
                        //TODO add ConsoleEvent to handle unknown input
                        //Main.writeEvent("Input from `" + super.strHost + "' :" + input.getMessage());
                        //Main.writeIn();
                        break;
                }
            }
            if(isRc)
            {
                ConsoleEventLog.addEvent(new ConsoleEvent("#RC: ", input.getMessage()), true);
                switch(input.Header)
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
                        catch(Exception e)
                        {
                        }
                        break;
                    case "4":
                        try
                        {
                            int time = int.Parse(input.Arguments[0]);
                            MainClass.TimeKillServer(time, input.Arguments[1]);
                        }
                        catch(Exception e)
                        {
                        }
                        break;
                }
            }
        }

        override protected void handleConnectionEvent(object Sender, ConnectionEvent e)
        {
            if(e.Event == ConnectionEvent.eConnectionEvent.eceConnect)
            {
                ConsoleEventLog.addEvent(new ConsoleEvent("Client " + e.Host + " connected."), true);
            }
            else
            {
                if(!NotifiedLoggedOff)
                {
                    ClientContainer.UserEvent(UserEventType.LogOut, this);
                    this.NotifiedLoggedOff = true;
                    this.LoggedIn = false;
                    string ret = GameBox.RemoveByUID(User.UID);
                    String[] rets = ret.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(String b in rets)
                    {
                        if(!ret.Equals("-1"))
                        {
                            SocketMessage stemp2 = new SocketMessage("UNHOST");
                            stemp2.Arguments.Add(b);
                            ClientContainer.AllUserCommand(stemp2);
                        }
                    }
                }
                ConsoleEventLog.addEvent(new ConsoleEvent("Client " + e.Host + " disconnected ."), true);
            }
        }
    }
}