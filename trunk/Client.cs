﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.NetShit;
using Skylabs.ConsoleHelper;
using Skylabs.oserver.Containers;
using System.Net.Sockets;

namespace Skylabs.oserver
{
    public class Client : ShitSock
    {
        public User User { get { return _User;} set { _User = value;} }
        public Boolean LoggedIn { get; set; }

        private User _User = new User();
        private Boolean isRc = false;

        override public void handleError(Exception e,String error)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError(error, e), true);
        }

        override public void handleInput(SocketMessage input)
        {
            switch (input.Header)
            {
                case "LOG":
                    Boolean worked = UserMysql.login(this, input.Arguments[0], input.Arguments[1]);
                    if(worked)
                    {
                	    for(int i=0;i<ClientContainer.Clients.Count;i++)
                	    {
                            if (ClientContainer.Clients[i].User.Username.Equals(User.Username))
                		    {
                			    if(ClientContainer.Clients[i].User.UID != User.UID)
                			    {
                				    if( ClientContainer.Clients[i].Connected )
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
                        //TODO Impliment daily message again.
                        //sm = new SocketMessage("CHATINFO");
                        //sm.Arguments.Add(Main.strDailyMessage);
                        //this.writeMessage(sm);
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
                    String regret = UserMysql.register(nick,email,pass);
                    writeMessage(new SocketMessage(regret));
                break;
                case "RC":
                    this.isRc = true;
                    User = new User(987654321,"rc","rc");
                break;
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