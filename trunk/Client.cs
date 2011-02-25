using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.NetShit;
using Skylabs.ConsoleHelper;
using Skylabs.oserver.Containers;
using System.Net.Sockets;
using Skylabs.Containers;

namespace Skylabs.oserver
{
    public class Client : ShitSock
    {
        public User User { get { return _User;} set { _User = value;} }
        public Boolean LoggedIn { get; set; }
        public int ID { get; set; }

        private User _User = new User();
        private Boolean isRc = false;

        override public void handleError(Exception e,String error)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError(error, e), true);
        }

        override public void handleInput(SocketMessage input)
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
                            SocketMessage sm = new SocketMessage("LOGSUCCESS");
                            sm.Arguments.Add(this.User.Username);
                            writeMessage(sm);
                            ClientContainer.UserEvent(UserEventType.LogIn, this);
                            sm = new SocketMessage("CHATINFO");
                            sm.Arguments.Add(MainClass.getDailyMessage());
                            this.writeMessage(sm);
                            LoggedIn = true;
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
                            sb.Append(listOnlineUsers[i]);
                            sb.Append(':');
                            sb.Append(listOnlineUsers[i].Username);
                            sm.Arguments.Add(sb.ToString());
                        }
                        writeMessage(sm);
                        //Main.writeEvent("Sending user list: " + sb);
                        break;
                    case "LOBCHAT":
                        ClientContainer.LobbyChat(User.Username, input.Arguments[0].Trim());
                        break;
                    case "HOST":
                        String[] ips = input.Arguments[0].Split(new char[1] { '?' });
                        HostedGame h = new HostedGame(User.UID, input.Arguments[5], input.Arguments[3], input.Arguments[2], input.Arguments[4], "");
                        //h.setStrHost(this.sock.getRemoteSocketAddress().toString());
                        int gID = GameBox.AddGame(h);
                        SocketMessage stemp = input;
                        stemp.Arguments.Add(this.User.Username);
                        stemp.Arguments.Add(gID.ToString());
                        ClientContainer.AllUserCommand(stemp);
                        break;
                    case "UNHOST":
                        int ret = GameBox.RemoveByUID(User.UID);
                        if (ret != -1)
                        {
                            SocketMessage stemp2 = input;
                            stemp2.Arguments.Add(ret.ToString());
                            ClientContainer.AllUserCommand(stemp2);
                        }
                        break;
                    case "GAMELIST":
                        int mGames = GameBox.Games.Count;
                        for (int i = 0; i < mGames; i++)
                        {
                            SocketMessage stemp3 = new SocketMessage("GAMELIST");

                            //Game list
                            //GID,IP,Port,GameName,GUID,GameVersion,Username,Name
                            //GID
                            stemp3.Arguments.Add(GameBox.Games[i].ID.ToString());
                            //IP
                            stemp3.Arguments.Add(MainClass.getProperty("BindInt"));
                            //Port
                            int port = (GameBox.Games[i].ID + 6000);
                            stemp3.Arguments.Add(port.ToString());
                            //GameName
                            stemp3.Arguments.Add(GameBox.Games[i].GameName);
                            //GUID
                            stemp3.Arguments.Add(GameBox.Games[i].GUID);
                            //GameVersion
                            stemp3.Arguments.Add(GameBox.Games[i].GameVersion);

                            int uid = GameBox.Games[i].UID;
                            Client cl = ClientContainer.getClientFromUID(uid);
                            //Username
                            stemp3.Arguments.Add(cl.User.Username);
                            //Name
                            stemp3.Arguments.Add(GameBox.Games[i].Name);
                            //Main.writeEvent("Sending GAMELIST: " + stemp3.getMessage());
                            this.writeMessage(stemp3);

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
                //TODO Add RC Input Handling.
                /*
                Main.writeEvent("#RC message: " + input.getMessage());
                if(head.equals("1"))
                {
                    socketMessage sm = new socketMessage("CHATINFO");
                    sm.Arguments.add(args.get(0));
                    Main.setAllUserCommand(sm);
                }
                else if(head.equals("2"))
                {
                    Main.killServer("RC Requested.");
                }
                else if(head.equals("3"))
                {
                    try
                    {
                        int time = Integer.parseInt(args.get(0));
                        Main.timekillServer(time,null);
                    }
                    catch(Exception e)
                    {
        			
                    }
                }
                else if(head.equals("4"))
                {
                    try
                    {
                        int time = Integer.parseInt(args.get(0));
                        Main.timekillServer(time,args.get(1));
                    }
                    catch(Exception e)
                    {
        			
                    }
                }
                else if(head.equals("5"))
                {
                    synchronized(Main.HostedGames)
                    {
                        for(int i=0;i<Main.HostedGames.size();i++)
                        {
                            socketMessage stemp = new socketMessage("UNHOST");
                            stemp.Arguments.add(Integer.toString(Main.HostedGames.get(i).getIntUGameNum()));
                            Main.setAllUserCommand(stemp);
                        }
                        Main.HostedGames.clear();
                    }
                }
                else if(head.equals("6"))
                {
                    synchronized(Main.HostedGames)
                    {
                        for(int i=0;i<Main.HostedGames.size();i++)
                        {
                            if(Main.HostedGames.get(i).intUGameNum == Integer.parseInt(args.get(0)))
                            {
                                socketMessage stemp = new socketMessage("UNHOST");
                                stemp.Arguments.add(Integer.toString(Main.HostedGames.get(i).getIntUGameNum()));
                                Main.setAllUserCommand(stemp);
                                Main.HostedGames.removeByUID(Main.HostedGames.get(i).getiUID());
                                break;
                            }
                        }
                        //Main.HostedGames.clear();
                    }
                }
                 */
            }
        }
        override public void handleConnect(String host, int port)
        {
            ConsoleEventLog.addEvent(new ConsoleEvent("Client " + host + " connected."), true);
        }

        override public void handleDisconnect(String reason, String host, int port)
        {
            ConsoleEventLog.addEvent(new ConsoleEvent("Client " + host + " disconnected because " + reason), true);
        }
    }
}
