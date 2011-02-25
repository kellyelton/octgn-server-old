using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.NetShit;

namespace Skylabs.oserver.Containers
{
    public enum UserEventType
    {
        LogIn, LogOut
    };
    public class ClientContainer
    {
        public static List<Client> Clients 
        {
            get
            {
                lock (_Clients)
                {
                    return _Clients;
                }
            }
            set
            {
                lock (_Clients)
                {
                    _Clients = value;
                }
            }
        }

        private static List<Client> _Clients = new List<Client>(1000);
        private static int intCurrentClientID = 0;

        public static void AddClient(Client c)
        {
            Clients.Add(c);
            int i = Clients.IndexOf(c);
            Clients[i].ID = i;
        }
        public static void AllUserCommand(SocketMessage command)
        {
            int intcount = Clients.Count;
            for(int i=0;i<intcount;i++)
            {
                if(Clients[i].Connected)
                {
                    Clients[i].writeMessage(command);
                }
            }
        }
        public static void UserEvent(UserEventType ue, Client c)
        {
            SocketMessage sm;
            switch (ue)
            {
                case Containers.UserEventType.LogIn:
                    sm = new SocketMessage("USERONLINE");
                    sm.Arguments.Add(c.User.Email + ":" + c.User.Username);
                    AllUserCommand(sm);
                break;
                case Containers.UserEventType.LogOut:
                    sm = new SocketMessage("USEROFFLINE");
                    sm.Arguments.Add(c.User.Email);
                    AllUserCommand(sm);
                break;
            }
        }

        public static Client getClientFromUID(int uid)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].User.UID == uid)
                {
                    if (Clients[i].LoggedIn)
                        return Clients[i];
                }
            }
            return new Client();
        }
        public static void LobbyChat(String user, String chat)
        {
    	    if(chat.Substring(0,1).Equals("/"))
    	    {
    		    int sp = chat.IndexOf(' ');
    		    String command = "";
    		    if(sp == -1)
    			    command = chat.Substring(1);
    		    else
    			    command =chat.Substring(1,sp-1);
    		    chat = chat.Substring(sp+1);
    		    if(command.Equals("w"))
    		    {
    			    String to;
    			    sp = chat.IndexOf(' ');
        		    if(sp != -1)
        		    {
                        to = chat.Substring(0,sp);
        			    chat = chat.Substring(sp+1);
    	        	    Client c = getClientFromUserName(to);
    	        	    Client from = getClientFromUserName(user);
    	        	    if(!c.User.Email.Equals("") && c.Connected)
    	        	    {
    	        		    SocketMessage sm = new SocketMessage("LOBW");
    	        		    sm.Arguments.Add(user + ":" + to);
    	        		    sm.Arguments.Add(chat);
    	        		    c.writeMessage(sm);
    	        		    from.writeMessage(sm);
    	        	    }
    	        	    else
    	        	    {
    	        		    SocketMessage sm = new SocketMessage("CHATERROR");
    	        		    sm.Arguments.Add("User '" + to + "' not online.");
    	        		    from.writeMessage(sm);
    	        	    }
        		    }
        		
    		    }
    		    else if(command.Equals("?"))
    		    {
                    //TODO Implement /? help function
    			    //Client from = Clients.getClientFromUserName(user);
                    /*
    			    String ch = "";
    			    ch = ch.concat("CHATINFO" + (char)3 + "Lobby chat help\n");
    			    ch = ch.concat("CHATINFO" + (char)3 + "---------------\n");
    			    ch = ch.concat("CHATINFO" + (char)3 + "\\?\n");
    			    ch = ch.concat("CHATINFO" + (char)3 + "--This menu.\n");
    			    ch = ch.concat("CHATINFO" + (char)3 + "\\w user message\n");
    			    ch = ch.concat("CHATINFO" + (char)3 + "--sends the 'user' 'message'\n");
    			    ch = ch.concat("CHATINFO" + (char)3 + "---------------\n");
    			    //from.writeLine(ch);
                     */
    		    }
    		    else
    		    {
    			    SocketMessage sm =new SocketMessage("LOBCHAT");
    			    sm.Arguments.Add(user);
    			    sm.Arguments.Add(chat);
    			    AllUserCommand(sm);
    		    }
    	    }
    	    else
    	    {
			    SocketMessage sm =new SocketMessage("LOBCHAT");
			    sm.Arguments.Add(user);
			    sm.Arguments.Add(chat);
			    AllUserCommand(sm);
    	    }
        }
        public static Client getClientFromUserName(String u)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].User.Username.Equals(u))
                {
                    if (Clients[i].LoggedIn)
                        return Clients[i];
                }
            }
            return new Client();
        }
        public static List<User> getOnlineUserList()
        {
            List<User> ret = new List<User>();
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].LoggedIn)
                    ret.Add(Clients[i].User);
            }
            return ret;
        }
    }
}
