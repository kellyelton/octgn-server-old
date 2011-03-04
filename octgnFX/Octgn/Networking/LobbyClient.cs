using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Skylabs.Networking;
using System.Windows;
using System.Security.Cryptography;
using System.Collections;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Octgn.Networking
{
    public class LobbyClient : SocketClient
    {
        public delegate void ConnectionDelegate(String ConEvent);
        public delegate void ErrorDelegate(String error);
        public delegate void UserDelegate(User user, bool Connected);
        public delegate void LobbyChatDelegate(LobbyChatTypes type,String user, String chat);
        private delegate void NoArgsDelegate();
        private bool loggedIn = false;
        private string strLastWhisperFrom = "";
        public event UserDelegate eUserEvent;
        public event ConnectionDelegate eConnection;
        public event LobbyChatDelegate eLobbyChat;
        public event ErrorDelegate eError;
        public List<User> OnlineUsers;
        public String strUserName = "";
        public enum LobbyChatTypes { Global,Whisper,System, Error};
        public LobbyClient(String host, Int32 port)
        {
            strHost = host;
            intPort = port;
            OnlineUsers = new List<User>(0);
        }
        //
        // Summary:
        //     Connects to the server and starts the run() loop.
        //
        public void Start()
        {
            this.Connect(strHost, intPort);
        }
        public void Login(String email, String password)
        {
            Encoding enc = Encoding.ASCII;
            byte[] buffer = new byte[0];
            String pass = "";
            try
            {
                buffer = enc.GetBytes(password);
            }
            catch (Exception e)
            {
                
            }
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();

            try
            {
                pass = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            }
            catch (Exception e)
            {

            }
            cryptoTransformSHA1.Dispose();
            pass = pass.ToLower();
            SocketMessage sm = new SocketMessage("LOG");
            sm.Arguments.Add(email);
            sm.Arguments.Add(pass);
            this.writeMessage(sm);  
        }
        public void Register(String email, String username, String password)
        {
            SocketMessage sm = new SocketMessage("REG");
            sm.Arguments.Add(username);
            sm.Arguments.Add(email);
            sm.Arguments.Add(password);
            this.writeMessage(sm);
        }
        public void Chat(String text)
        {
            SocketMessage sm = new SocketMessage("LOBCHAT");
            text = text.Trim();
            try
            {
                if (text.ToLower().Substring(0, 3).Equals("/r "))
                    text = "/w " + strLastWhisperFrom + " " + text.Substring(3);
            }
            catch (Exception e)
            {
                
            }
            sm.Arguments.Add(text);
            writeMessage(sm);
        }
        override
        public void handleError(String error) 
        {
            try
            {

                eError.Invoke(error);
            }
            catch (Exception e) { }
        }

        public override void handleInput(SocketMessage input) 
        {
            String head = input.Header;
            String a = "";
            String[] up ;
            List<String> args = input.Arguments;
            switch (head)
            {
                case "LOGSUCCESS":
                    loggedIn = true;
                    try
                    {
                        strUserName = args[0];
                    }
                    catch (Exception e)
                    {
                        
                    }
                    eConnection.Invoke("LIN");
                break;
                case "LOGERROR":
                    MessageBox.Show("E-Mail or password is incorrect.", "Error");
                break;
                case "REGERR0":
                    MessageBox.Show("Lobby server error. Please try again later.");
                break;
                case "REGERR1":
                    MessageBox.Show("Please enter a valid e-mail address.");
                break;
                case "REGERR2":
                    MessageBox.Show("Password must be at least 6 characters long.");
                break;
                case "REGERR3":
                    MessageBox.Show("Nickname already exists.");
                break;
                case "REGERR4":
                    MessageBox.Show("E-Mail already registered.");
                break;
                case "REGSUCCESS":
                MessageBox.Show("Account successfully registered.");
                break;
                default:
                if (loggedIn)
                {
                    switch (head)
                    {
                        case "USERONLINE":
                            //User[] temp = new User[args.Count];
                            try
                            {
                                a = args[0];
                                up = a.Split(new char[1] { ':' });
                                OnlineUsers.Add(new User(up[0], up[1]));
                                eUserEvent.Invoke(new User(up[0], up[1]), true);
                            }
                            catch (Exception e) 
                            {
                                break;
                            }
                            break;
                        case "USEROFFLINE":
                            try
                            {
                                a = args[0];
                                up = a.Split(new char[1] { ':' });
                                for (int i = 0; i < OnlineUsers.Count; i++)
                                {
                                    if (OnlineUsers[i].Email.Equals(up[0]))
                                    {
                                        eUserEvent.Invoke(OnlineUsers[i], false);
                                        OnlineUsers.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            catch (Exception e)
                            {

                                break;
                            }
                            break;
                        case "ONLINELIST":
                            try
                            {
                                OnlineUsers = new List<User>(0);
                                for (int i = 0; i < args.Count; i++)
                                {
                                    a = (String)args[i];
                                    up = a.Split(new char[1] { ':' });
                                    if (up.Length == 2)
                                        OnlineUsers.Add(new User(up[0], up[1]));
                                }
                                eUserEvent.Invoke(new User("", ""), true);
                            }
                            catch (Exception e)
                            { }
                            break;
                        case "LOBCHAT":
                            try
                            {
                                eLobbyChat.Invoke(LobbyChatTypes.Global, (String)args[0], (String)args[1]);
                            }
                            catch (Exception e)
                            { }
                            break;
                        case "LOBW":
                            try
                            {
                                String[] s = args[0].Split(new char[1] { ':' });
                                strLastWhisperFrom = s[0];
                                eLobbyChat.Invoke(LobbyChatTypes.Whisper, (String)args[0], (String)args[1]);
                            }
                            catch(Exception e){};
                            break;
                        case "CHATERROR":
                            try{
                            eLobbyChat.Invoke(LobbyChatTypes.Error, "ERROR", (String)args[0]);
                                                            }
                            catch(Exception e){};
                            break;
                        case "CHATINFO":
                            try
                            {
                                eLobbyChat.Invoke(LobbyChatTypes.System, "SYSTEM", (String)args[0]);
                            }
                            catch(Exception e){};
                        break;
                        default:
                            #if (DEBUG)
                                MessageBox.Show(input.getMessage() , "Input from sock");
                            #endif
                            break;
                    }
                }
                break;
            }
                
        }
        override
        public void handleConnect(String host, Int32 port) 
        {
            try
            {
            eConnection.Invoke("CON");
            }
            catch (Exception e) { };
        }
        override
        public void handleDisconnect(String reason, String host, int port) 
        {
            try
            {
                eConnection.Invoke("DC");
            }
            catch (Exception e) { }
        }
        public class User
        {
            public String Email;
            public String Username;
            public User()
            {
                Email = "";
                Username = "";
            }
            public User(String email, String username)
            {
                Email = email;
                Username = username;
            }
        }
    }
}
