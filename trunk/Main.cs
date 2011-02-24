using System;
using Skylabs.ConsoleHelper;
using System.Xml;
using System.IO;
using System.Threading;

namespace Skylabs.oserver
{
	public class MainClass
	{
        public static XmlDocument Properties { get; set; }

		public static void Main (string[] args)
		{
            Console.ForegroundColor = ConsoleColor.White;
            RegisterHandlers();
            ConsoleHand.Start("O-Lobby: ", ConsoleColor.White, ConsoleColor.DarkGreen);
            if (!LoadProperties())
                return;
            ConsoleHand.writeCT();
            while (ConsoleHand.ThreadState != System.Threading.ThreadState.Stopped && ConsoleHand.ThreadState != System.Threading.ThreadState.Aborted)
            {

                Thread.Sleep(1000);
            }
            UnregisterHandlers();
		}
        public static void RegisterHandlers()
        {
            ConsoleEventLog.eAddEvent += new ConsoleEventLog.EventEventDelegate(eLog_eAddEvent);
            ConsoleHand.eConsoleInput += new ConsoleHand.ConsoleInputDelegate(ConsoleHand_eConsoleInput);
        }
        public static void UnregisterHandlers()
        {
            ConsoleEventLog.eAddEvent -= eLog_eAddEvent;
            ConsoleHand.eConsoleInput -= ConsoleHand_eConsoleInput;
        }
        private static void ConsoleHand_eConsoleInput(ConsoleMessage input)
        {
            switch (input.Header)
            {
                case "quit":
                    new ConsoleEvent("Quitting...").writeEvent(true);
                    ConsoleHand.end();
                    break;
                case "bake":
                    String ret = "Baking args brah...\n";
                    foreach (ConsoleArgument arg in input.Args)
                    {
                        ret += arg.Argument;
                        if (arg.Value != "")
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

