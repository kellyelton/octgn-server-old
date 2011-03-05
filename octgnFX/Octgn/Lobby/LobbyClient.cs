using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows;
using System.Security.Cryptography;
using System.Collections;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Octgn.Properties;
using Skylabs.NetShit;

namespace Octgn.Lobby
{
    public enum PlayerStatus
    {
        Hosting, Playing, Away, Available
    };
    public class LobbyClient : ShitSock
    {
        public delegate void ConnectionDelegate(String ConEvent);
        public delegate void GameHostDelegate(HostedGame game, Boolean unHosting, Boolean isGameListItem);
        public delegate void ErrorDelegate(String error);
        public delegate void UserDelegate(User user, bool Connected);
        //TODO Blah
        public delegate void UserStatusChangedDelegate(User user, PlayerStatus status);
        public delegate void LobbyChatDelegate(LobbyChatTypes type, String user, String chat);
        private delegate void NoArgsDelegate();
        private bool loggedIn;
        private string strLastWhisperFrom;
        private DateTime lastMessageSent;
        public event UserDelegate eUserEvent;
        public event ConnectionDelegate eLogEvent;
        public event LobbyChatDelegate eLobbyChat;
        public event GameHostDelegate eGameHost;
        public event ErrorDelegate eError;
        public event UserStatusChangedDelegate eUserStatusChanged;
        public List<User> OnlineUsers;
        public String strUserName;
        public Boolean isHosting { get; set; }
        public Boolean isJoining { get; set; }
        public String LastGameInfo { get; set; }
        public PlayerStatus Status { get; set; }
        //public HostedGameBox HostedGames = new HostedGameBox();
        public ObservableCollection<HostedGame> HostedGames;
        public enum LobbyChatTypes { Global, Whisper, System, Error };
        public LobbyClient(String host, Int32 port)
        {
            strHost = host;
            intPort = port;
        }
        private void regEvents()
        {
            //Program.lwMainWindow.Closed += new EventHandler(lwMainWindow_Closed);
            Program.lwLobbyWindow.Closed += new EventHandler(lwLobbyWindow_Closed);
        }
        private void unregEvents()
        {
            if (Program.lwLobbyWindow != null)
                Program.lwLobbyWindow.Closed -= lwLobbyWindow_Closed;
        }

        void lwLobbyWindow_Closed(object sender, EventArgs e)
        {
            this.Close("Lobby Closed.", true);

        }
        //
        // Summary:
        //     Connects to the server and starts the run() loop.
        //
        public void Start()
        {
            OnlineUsers = new List<User>(0);
            regEvents();
            isHosting = false;
            LastGameInfo = "";
            strUserName = "";
            strLastWhisperFrom = "";
            loggedIn = false;
            lastMessageSent = DateTime.Now;
            HostedGames = new ObservableCollection<HostedGame>();
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
        public String Chat(String text)
        {
            SocketMessage sm = new SocketMessage("LOBCHAT");
            text = text.Trim();
            if (!isHosting && !isJoining && Status != PlayerStatus.Available)
            {
                if (Status != PlayerStatus.Hosting)
                {
                    ChangePlayerStatus(PlayerStatus.Available);
                }
                lastMessageSent = DateTime.Now;
            }
                
            if (text.Length == 0)
                return text;
            try
            {
                String[] words = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (words[0].ToLower())
                {
                    case "/reconnect":
                        if (!Connected)
                            Start();
                        return "";
                    case "/r":
                        if (strLastWhisperFrom.Equals(""))
                            return "";
                        else
                            text = "/w " + strLastWhisperFrom + " " + text.Substring(3);
                        break;
                    case "/login":
                        if (Connected && !loggedIn)
                        {
                            if (words.Length == 3)
                            {
                                Login(words[1], words[2]);
                            }
                        }
                        return "";
                    case "/away":
                        if (!isHosting && !isJoining && Status != PlayerStatus.Away)
                        {
                            if (Status != PlayerStatus.Hosting)
                            {
                                ChangePlayerStatus(PlayerStatus.Away);
                            }
                        }
                        return "";
                }
            }
            catch (ArgumentOutOfRangeException ae)
            {

            }
            catch (Exception e)
            {
                return text;
            }
            sm.Arguments.Add(text);
            writeMessage(sm);
            return text;
        }

        public void Host_Game(Definitions.GameDef def)
        {
            SocketMessage sm = new SocketMessage("HOST");
            sm.Arguments.Add(def.Name);
            sm.Arguments.Add(def.Id.ToString());
            sm.Arguments.Add(def.Version.ToString());
            sm.Arguments.Add(LastGameInfo);
            this.isHosting = true;
            writeMessage(sm);

        }
        public void unHost_Game()
        {
            SocketMessage sm = new SocketMessage("UNHOST");
            this.isHosting = false;
            writeMessage(sm);
        }

        public void ChangePlayerStatus(PlayerStatus status)
        {
            this.Status = status;
            SocketMessage sm = new SocketMessage("STATUS");
            sm.Arguments.Add(status.ToString());
            writeMessage(sm);
        }

        override
        public void handleError(Exception ex, String error)
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
            String[] up;
            List<String> args = input.Arguments;
            String sPing = new String(new char[1] { (char)1 });
            switch (head)
            {
                case "LOGSUCCESS":
                    loggedIn = true;
                    try
                    {
                        strUserName = args[0];
                        Settings.Default.NickName = Program.LClient.strUserName;
                        Settings.Default.Save();
                    }
                    catch (Exception e)
                    {

                    }
                    eLogEvent.Invoke("LSUCC");
                    break;
                case "LOGERROR":
                    MessageBox.Show("E-Mail or password is incorrect.", "Error");
                    eLogEvent.Invoke("LERR");
                    break;
                case "REGERR0":
                    MessageBox.Show("Lobby server error. Please try again later.");
                    eLogEvent.Invoke("RERR");
                    break;
                case "REGERR1":
                    MessageBox.Show("Please enter a valid e-mail address.");
                    eLogEvent.Invoke("RERR");
                    break;
                case "REGERR2":
                    MessageBox.Show("Password must be at least 6 characters long.");
                    eLogEvent.Invoke("RERR");
                    break;
                case "REGERR3":
                    MessageBox.Show("Nickname already exists.");
                    eLogEvent.Invoke("RERR");
                    break;
                case "REGERR4":
                    MessageBox.Show("E-Mail already registered.");
                    eLogEvent.Invoke("RERR");
                    break;
                case "REGSUCCESS":
                    MessageBox.Show("Account successfully registered.");
                    eLogEvent.Invoke("RSUCC");
                    break;
                default:
                    if (head.Equals(sPing))
                    {
                        if (Program.lwLobbyWindow == null)
                            this.Close("Program failed.", true);
                        else
                        {
                            if (!Program.lwLobbyWindow.IsActive)
                                this.Close("Program failed.", true);
                        }
                    }
                    if (loggedIn)
                    {
                        if (Status != PlayerStatus.Away)
                        {
                            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - lastMessageSent.Ticks);
                            if (ts.TotalMinutes >= 5)
                            {
                                if (!isHosting && !isJoining)
                                {
                                    Status = PlayerStatus.Away;
                                }
                            }
                        }
                        switch (head)
                        {
                            case "USERONLINE":
                                //User[] temp = new User[args.Count];
                                try
                                {
                                    a = args[0];
                                    up = a.Split(new char[1] { ':' });
                                    Boolean userAlreadyOnList = false;
                                    for (int i = 0; i < OnlineUsers.Count; i++)
                                    {
                                        if (OnlineUsers[i].Username.Equals(up[1]))
                                        {
                                            userAlreadyOnList = true;
                                            break;
                                        }
                                    }
                                    if (!userAlreadyOnList)
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
                                            User temp = OnlineUsers[i];
                                            OnlineUsers.RemoveAt(i);
                                            eUserEvent.Invoke(temp, false);

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
                                        else if(up.Length == 3)
                                            OnlineUsers.Add(new User(up[0], up[1], (PlayerStatus)Enum.Parse(typeof(PlayerStatus), up[2], true)));
                                    }
                                    eUserEvent.Invoke(new User("", ""), true);
                                    SocketMessage sm = new SocketMessage("GAMELIST");
                                    writeMessage(sm);
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
                                    if (s[0] != strUserName)
                                        strLastWhisperFrom = s[0];
                                    eLobbyChat.Invoke(LobbyChatTypes.Whisper, (String)args[0], (String)args[1]);
                                }
                                catch (Exception e) { };
                                break;
                            case "CHATERROR":
                                try
                                {
                                    eLobbyChat.Invoke(LobbyChatTypes.Error, "ERROR", (String)args[0]);
                                }
                                catch (Exception e) { };
                                break;
                            case "CHATINFO":
                                try
                                {
                                    eLobbyChat.Invoke(LobbyChatTypes.System, "SYSTEM", (String)args[0]);
                                }
                                catch (Exception e) { };
                                break;
                            case "STATUS":
                                String stat = args[0];
                                String usn = args[1];
                                if (usn.Equals(strUserName))
                                {
                                    Status = (PlayerStatus)Enum.Parse(typeof(PlayerStatus), stat, true);
                                    eUserStatusChanged.Invoke(new User("", strUserName), Status);
                                }
                                foreach (User u in OnlineUsers)
                                {
                                    if (u.Username.Equals(usn))
                                    {
                                        u.Status = (PlayerStatus)Enum.Parse(typeof(PlayerStatus), stat, true);
                                        eUserStatusChanged.Invoke(u, u.Status);
                                        break;
                                    }
                                }
                                

                                break;
                            case "HOST":
                                try
                                {
                                    String t = (string)args[6];
                                    String[] ips = t.Split(new char[1] { '?' });
                                    HostedGame hg = new HostedGame((string)args[3], (string)args[1], (string)args[0], (string)args[2], "", ips, int.Parse((string)args[7]));
                                    hg.strHostName = (string)args[4];
                                    hg.intUGameNum = int.Parse((string)args[5]);
                                    eGameHost.Invoke(hg, false, false);
                                }
                                catch (Exception e) { };
                                break;
                            case "UNHOST":
                                try
                                {
                                    HostedGame hg = new HostedGame();
                                    hg.intUGameNum = int.Parse((String)args[0]);
                                    eGameHost.Invoke(hg, true, false);
                                }
                                catch (Exception e) { };
                                break;
                            case "GAMELIST":
                                try
                                {
                                    String t = (string)args[1];
                                    String[] ips = t.Split(new char[1] { '?' });
                                    HostedGame hg = new HostedGame((string)args[7], (string)args[4], (string)args[3], (string)args[5], "", ips, int.Parse((string)args[2]));
                                    hg.strHostName = (string)args[6];
                                    hg.intUGameNum = int.Parse((string)args[0]);
                                    eGameHost.Invoke(hg, false, true);
                                }
                                catch (Exception e) { };
                                break;
                            default:
#if (DEBUG)
                                MessageBox.Show(input.getMessage(), "Input from sock");
#endif
                                break;
                        }
                    }
                    else
                    {
#if (DEBUG)
                        MessageBox.Show(input.getMessage(), "Input from sock");
#endif
                    }
                    break;
            }

        }
        override
        public void handleConnect(String host, Int32 port)
        {
            try
            {
                eLogEvent.Invoke("CON");
            }
            catch (Exception e) { };
        }
        override
        public void handleDisconnect(String reason, String host, int port)
        {
            try
            {
                loggedIn = false;
                eLogEvent.Invoke("DC");

            }
            catch (Exception e) { }
            unregEvents();
        }
        public class User
        {
            public String Email;
            public String Username;
            public PlayerStatus Status { get; set; }
            public User()
            {
                Email = "";
                Username = "";
                Status = PlayerStatus.Available;
            }
            public User(String email, String username)
            {
                Email = email;
                Username = username;
                Status = PlayerStatus.Available;
            }
            public User(String email, String username, PlayerStatus status)
            {
                Email = email;
                Username = username;
                Status = status;
            }
        }

    }
}
