using System;
using Skylabs.ConsoleHelper;
using System.Xml;
using System.IO;
using System.Threading;
using Skylabs.Networking;

namespace Skylabs.oserver
{
	public class MainClass
	{
        public static XmlDocument Properties { get; set; }
        public static Listener Server { get; set; }
        private static bool endIt = false;

		public static void Main (string[] args)
		{
            Console.ForegroundColor = ConsoleColor.White;
            RegisterHandlers();
            ConsoleWriter.CommandText = "O-Lobby: ";
            ConsoleWriter.CommandTextColor = ConsoleColor.White;
            ConsoleWriter.OutputColor = ConsoleColor.Gray;
            ConsoleReader.InputColor = ConsoleColor.Yellow;
            ConsoleReader.Start();
            if (!LoadProperties())
                return;
            ConsoleWriter.writeCT();

            String host = Properties.GetElementsByTagName("BindInt").Item(0).InnerText;
            String sport = Properties.GetElementsByTagName("BindPort").Item(0).InnerText;
            int port =int.Parse(sport) ;

            try
            {
                Server = new Listener(host, port);
            }
            catch (Exception e)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError("Problem with settings 'BindInt' or 'BindPort'.", e), true);
                Stop();
            }

            while (endIt == false)
            {
                Thread.Sleep(1000);
            }
            new ConsoleEvent("Quitting...").writeEvent(true);
            UnregisterHandlers();
            ConsoleReader.Stop();
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
                case "bake":
                    String ret = "Baking args brah...\n";
                    foreach (ConsoleArgument arg in input.Args)
                    {
                        ret += arg.Argument;
                        if (!String.IsNullOrEmpty(arg.Value))
                            ret += " = " + arg.Value;
                        ret += "\n";
                    }
                    new ConsoleEvent(ret).writeEvent(true);
                    break;
                default:
                    new ConsoleEventError("Invalid command '" + input.RawData + "'.", new Exception("Invalid console command.")).writeEvent(true);
                    break;
            }
        }
        private static void eLog_eAddEvent(ConsoleEvent e)
        {
            ConsoleEventLog.SerializeEvents("d:\\elog.xml");
        }

        public static Boolean LoadProperties()
		{
			Properties = new XmlDocument();
            Boolean ret = true;
            FileStream f = null;
            try
            {
                f = File.Open("ServerOptions.xml", FileMode.Open);
                Properties.Load(f);
                new ConsoleEvent("#Event: ", "Settings file loaded.", ConsoleColor.Gray).writeEvent(true);
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

