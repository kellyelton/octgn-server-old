using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
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
        public static String RootPath = "";
        private static TimeSpan timebetweenads = new TimeSpan(0, 5, 0);

        public static void WriteAd(TimeSpan lastAd)
        {
            return;
            if (lastAd >= timebetweenads)
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
                        String xaml = "";// HtmlToXamlConverter.ConvertHtmlToXaml(fullhtml, true);
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
            }
        }

        public static void Main(string[] args)
        {
            //Console.ForegroundColor = ConsoleColor.White;
            RegisterHandlers();
            ConsoleReader.Start();
            ConsoleWriter.CommandText = "O-Lobby: ";
            RootPath = System.IO.Directory.GetCurrentDirectory();
            if (!LoadProperties())
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
            catch (Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Problem with settings 'BindInt' or 'BindPort'." + host + ":" + sport, e), true);
                Stop();
                return;
            }
            //IrcBot.Start();
            while (endIt == false)
            {
                if (fKillTime == 0)
                    break;
                if (fKillTime > 0)
                    fKillTime -= 1;
                Thread.Sleep(1000);
            }
            //IrcBot.Stop();
            new ConsoleEvent("Quitting...").writeEvent(true);
            //ClientContainer.AllUserCommand(new NetShit.EndMessage());
            foreach (Client c in ClientContainer.Clients)
            {
                c.Close("Server shutdown.", true);
            }
            foreach (HostedGame h in GameBox.Games)
            {
                //h.Server.Stop();
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
                s = System.IO.File.ReadAllText(Path.Combine(RootPath, "currevision.txt"));
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
                s = System.IO.File.ReadAllText(Path.Combine(RootPath, getProperty("DailyMessage")));
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
#if(DEBUG)
                f = File.Open(Path.Combine(RootPath, "ServerOptions-Debug.xml"), FileMode.Open);
#else
                f = File.Open(Path.Combine(RootPath , "ServerOptions.xml"), FileMode.Open);
#endif
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