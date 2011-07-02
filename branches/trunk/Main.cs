using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Skylabs.ConsoleHelper;
using Skylabs.Lobby;
using Skylabs.Lobby.Containers;
using Skylabs.NetShit;
using Skylabs.Networking;
using Skylabs.oserver.Containers;

namespace Skylabs.oserver
{
    public class MainClass
    {
        public static XmlDocument Properties { get; set; }

        public static Listener Server { get; set; }

        private static bool endIt = false;
        private static int fKillTime = -1;
        public static String RootPath = "";
        private static TimeSpan timebetweenads = new TimeSpan(0, 5, 0);

        public static void WriteAd(TimeSpan lastAd)
        {
            return;
            if(lastAd >= timebetweenads)
            {
                WebClient wc = new WebClient();
                try
                {
                    Uri u = new Uri("http://www.qksz.net/1e-jror");

                    String ad = @wc.DownloadString(u);
                    ad = ad.Replace("\\\\\\", "\\");
                    Regex r = new Regex("^document\\.write\\(\"(.+)+\"\\);$");
                    Match m = r.Match(ad);
                    if(m.Groups.Count >= 2)
                    {
                        String fullhtml = m.Groups[1].Value;
                        //ConsoleWriter.writeLine(fullhtml, true);
                        String xaml = "";// HtmlToXamlConverter.ConvertHtmlToXaml(fullhtml, true);
                        if(!xaml.Trim().Equals(""))
                        {
                            SocketMessage sm = new SocketMessage("XAMLCHAT");
                            sm.Arguments.Add("SUPPORT");
                            sm.Arguments.Add(xaml);
                            ClientContainer.AllUserCommand(sm);
                        }
                        //ConsoleWriter.writeLine(xaml, true);
                    }
                    //ConsoleWriter.writeLine(evalstring, true);
                }
                catch(Exception e)
                {
                }
            }
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //Console.ForegroundColor = ConsoleColor.White;
            RegisterHandlers();
            ConsoleReader.Start();
            ConsoleWriter.CommandText = "O-Lobby: ";
            RootPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
            //RootPath = System.IO.Directory.GetCurrentDirectory();
            if(!LoadProperties())
                return;
            ConsoleWriter.writeCT();
            String host = getProperty("BindInt");
            String sport = getProperty("BindPort");
            int port = int.Parse(sport);
            try
            {
                Server = new Listener(host, port);
                Server.Start();
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Problem with settings 'BindInt' or 'BindPort'." + host + ":" + sport, e), true);
                Stop();
                return;
            }
            //IrcBot.Start();
            while(endIt == false)
            {
                if(fKillTime == 0)
                    break;
                if(fKillTime > 0)
                    fKillTime -= 1;
                if(fKillTime > -1)
                    Thread.Sleep(1000);
                else
                {
                    //ClientContainer.AllUserCommand(new PingMessage());
                    Thread.Sleep(30000);
                }
            }
            //IrcBot.Stop();
            new ConsoleEvent("Quitting...").writeEvent(true);
            //ClientContainer.AllUserCommand(new NetShit.EndMessage());
            foreach(Client c in ClientContainer.Clients)
            {
                c.Close();
            }
            foreach(HostedGame h in GameBox.Games)
            {
                //h.Server.Stop();
            }
            Server.Stop();

            UnregisterHandlers();
            ConsoleReader.Stop();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleCatchAll((Exception)e.ExceptionObject);
        }

        private static void HandleCatchAll(Exception e)
        {
#if(!DEBUG)
            ConsoleColor f = Console.ForegroundColor;
            ConsoleColor b = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("###################################");
            Console.WriteLine("###ERROR###ERROR###ERROR###ERROR###");
            Console.WriteLine("###    (Oh my god! A WhAt?!)    ###");
            Console.WriteLine("###################################");
            Console.Write("Message: ");
            Console.Write(e.Message);
            Console.WriteLine();
            Console.WriteLine("##############STACK################");
            Console.WriteLine();
            Console.Write(e.StackTrace);
            Console.WriteLine();
            Console.WriteLine("#############ENDSTACK##############");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("So uh, yeah. That's about it. I feel \na little embarrassed about the way \nthat I acted there.");
            Console.WriteLine("I don't think errors are that \nterrifying, especially this little bugger.");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Well, anyways, have a nice day!");

            Console.ForegroundColor = f;
#else
            System.Diagnostics.Debugger.Break();
#endif
        }

        public static void KillServer()
        {
            ConsoleEventLog.addEvent(new ConsoleEvent("Killing server..."), true);
            endIt = true;
        }

        public static void TimeKillServer(int time, String message)
        {
            fKillTime = time;
            SocketMessage sm = new SocketMessage("CHATINFO");
            String mess = "Server shutting down in " + fKillTime.ToString() + " seconds. ";
            if(message != null && !String.IsNullOrEmpty(message))
                mess += "\n Reason: " + message;
            sm.Arguments.Add(mess);
            ClientContainer.AllUserCommand(sm);
        }

        public static void Stop()
        {
            endIt = true;
        }

        public static void RegisterHandlers()
        {
            ConsoleEventLog.eAddEvent += new ConsoleEventLog.EventEventDelegate(eLog_eAddEvent);
            ConsoleReader.eConsoleInput += new ConsoleReader.ConsoleInputDelegate(ConsoleReader_eConsoleInput);
        }

        public static void UnregisterHandlers()
        {
            ConsoleEventLog.eAddEvent -= eLog_eAddEvent;
            ConsoleReader.eConsoleInput -= ConsoleReader_eConsoleInput;
        }

        private static void ConsoleReader_eConsoleInput(ConsoleMessage input)
        {
            switch(input.Header)
            {
                case "quit":
                    Stop();
                    break;
                case "server":
                    String ret = "Host: " + getProperty("BindInt") + ":" + getProperty("BindPort") + "\n";
                    new ConsoleEvent(ret).writeEvent(true);
                    break;
                case "hosting":
                    ret = "IsBound: " + Server.Sock.Server.IsBound.ToString();
                    ret += "\n";
                    ret += Server.Sock.Server.LocalEndPoint.ToString();
                    new ConsoleEvent(ret).writeEvent(true);
                    break;
                case "ircchangenick":
                    //IrcBot.ChatAsUser(input.Args[0].Argument, "");
                    break;
                default:
                    new ConsoleEventError("Invalid command '" + input.RawData + "'.", new Exception("Invalid console command.")).writeEvent(true);
                    break;
            }
        }

        private static void eLog_eAddEvent(ConsoleEvent e)
        {
            ConsoleEventLog.SerializeEvents(Path.Combine(RootPath, "elog.xml"));
        }

        public static String getProperty(String ID)
        {
            String ret = "";
            try
            {
                ret = Properties.GetElementsByTagName(ID).Item(0).InnerText;
            }
            catch(Exception e)
            {
                ret = "";
            }
            return ret;
        }

        public static String getCurRevision()
        {
            String s = "";
            try
            {
                s = System.IO.File.ReadAllText(Path.Combine(RootPath, "currevision.txt"));
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Problem opening revision file.", e), true);
            }
            return s;
        }

        public static String getDailyMessage()
        {
            String s = "";
            try
            {
                s = System.IO.File.ReadAllText(Path.Combine(RootPath, getProperty("DailyMessage")));
            }
            catch(Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Problem opening daily message.", e), true);
            }
            return s;
        }

        public static Boolean LoadProperties()
        {
            Properties = new XmlDocument();
            Boolean ret = true;
            FileStream f = null;
            String path = "";
            try
            {
#if(DEBUG)
                path = Path.Combine(RootPath, "ServerOptions-Debug.xml");
#else
                path = Path.Combine(RootPath, "ServerOptions.xml");
#endif
                f = File.Open(path, FileMode.Open);
                Properties.Load(f);
                new ConsoleEvent("#Event: ", "Settings file loaded.").writeEvent(true);
            }
            catch(Exception ex)
            {
                if(f != null)
                    f.Close();
                new ConsoleEventError("Could not load the settings file at " + path, ex).writeEvent(true);
                ret = false;
            }
            return ret;
        }
    }
}