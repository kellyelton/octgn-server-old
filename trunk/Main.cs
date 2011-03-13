using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using HTMLConverter;
using Skylabs.ConsoleHelper;
using Skylabs.Containers;
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
#if(DEBUG)
        public static String RootPath = "";
#else
        public static String RootPath = "/var/oserver/";
#endif

        public static void Main(string[] args)
        {
            //Console.ForegroundColor = ConsoleColor.White;
            RegisterHandlers();
            ConsoleWriter.CommandText = "O-Lobby: ";
            //ConsoleWriter.CommandTextColor = ConsoleColor.White;
            //ConsoleWriter.OutputColor = ConsoleColor.Gray;
            //ConsoleReader.InputColor = ConsoleColor.Yellow;
            ConsoleReader.Start();
            if (!LoadProperties())
                return;
            ConsoleWriter.writeCT();

            String host = Properties.GetElementsByTagName("BindInt").Item(0).InnerText;
            String sport = Properties.GetElementsByTagName("BindPort").Item(0).InnerText;
            int port = int.Parse(sport);

            try
            {
                Server = new Listener(host, port);
                Server.Start();
            }
            catch (Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Problem with settings 'BindInt' or 'BindPort'." + host + ":" + sport, e), true);
                Stop();
                return;
            }
            IrcBot.Start();
            TimeSpan timebetweenads = new TimeSpan(0, 0, 15);
            TimeSpan curTimeSpan = new TimeSpan(0);
            DateTime dtLastSent = DateTime.Now;
            while (endIt == false)
            {
                curTimeSpan = new TimeSpan(DateTime.Now.Ticks - dtLastSent.Ticks);
                if (curTimeSpan >= timebetweenads)
                {
                    WebClient wc = new WebClient();
                    try
                    {
                        Uri u = new Uri("http://www.qksz.net/1e-jror");

                        String ad = @wc.DownloadString(u);
                        ad = ad.Replace("\\\\\\", "\\");
                        Regex r = new Regex("^document\\.write\\(\"(.+)+\"\\);$");
                        Match m = r.Match(ad);
                        if (m.Groups.Count >= 2)
                        {
                            String fullhtml = m.Groups[1].Value;
                            //ConsoleWriter.writeLine(fullhtml, true);
                            String xaml = HtmlToXamlConverter.ConvertHtmlToXaml(fullhtml, true);
                            if (!xaml.Trim().Equals(""))
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
                    catch (Exception e)
                    {
                    }
                    dtLastSent = DateTime.Now;
                }
                if (fKillTime == 0)
                    break;
                if (fKillTime > 0)
                    fKillTime -= 1;

                Thread.Sleep(1000);
            }
            IrcBot.Stop();
            new ConsoleEvent("Quitting...").writeEvent(true);
            //ClientContainer.AllUserCommand(new NetShit.EndMessage());
            foreach (Client c in ClientContainer.Clients)
            {
                c.Close("Server shutdown.", true);
            }
            foreach (HostedGame h in GameBox.Games)
            {
                h.Server.Stop();
            }
            Server.Stop();

            UnregisterHandlers();
            ConsoleReader.Stop();
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
            if (message != null && !String.IsNullOrEmpty(message))
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
            switch (input.Header)
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
                    IrcBot.ChatAsUser(input.Args[0].Argument, "");
                    break;
                default:
                    new ConsoleEventError("Invalid command '" + input.RawData + "'.", new Exception("Invalid console command.")).writeEvent(true);
                    break;
            }
        }

        private static void eLog_eAddEvent(ConsoleEvent e)
        {
            ConsoleEventLog.SerializeEvents(RootPath + "elog.xml");
        }

        public static String getProperty(String ID)
        {
            String ret = "";
            try
            {
                ret = Properties.GetElementsByTagName(ID).Item(0).InnerText;
            }
            catch (Exception e)
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
                s = System.IO.File.ReadAllText(RootPath + "currevision.txt");
            }
            catch (Exception e)
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
                s = System.IO.File.ReadAllText(RootPath + getProperty("DailyMessage"));
            }
            catch (Exception e)
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
            try
            {
                f = File.Open(RootPath + "ServerOptions.xml", FileMode.Open);
                Properties.Load(f);
                new ConsoleEvent("#Event: ", "Settings file loaded.").writeEvent(true);
            }
            catch (Exception ex)
            {
                if (f != null)
                    f.Close();
                new ConsoleEventError("Could not load the settings file.", ex).writeEvent(true);
                ret = false;
            }
            return ret;
        }
    }
}