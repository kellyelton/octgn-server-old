using System;
using Skylabs.ConsoleHelper;
using System.Xml;
using System.IO;

namespace Skylabs.oserver
{
	public class MainClass
	{
		public static MainClass main;
		public static void Main (string[] args)
		{
			main = new MainClass();
			main.Start();
            Console.ForegroundColor = ConsoleColor.White;
		}
		
		public XmlDocument Properties{get;set;}

        public void RegisterHandlers()
        {
            ConsoleEventLog.eAddEvent += new ConsoleEventLog.EventEventDelegate(eLog_eAddEvent);
            ConsoleHand.eConsoleInput += new ConsoleHand.ConsoleInputDelegate(ConsoleHand_eConsoleInput);
        }
        public void UnregisterHandlers()
        {
            ConsoleEventLog.eAddEvent -= eLog_eAddEvent;
            ConsoleHand.eConsoleInput -= ConsoleHand_eConsoleInput;
        }
        void ConsoleHand_eConsoleInput(ConsoleMessage input)
        {
            switch (input.Header)
            {
                case "quit":
                    new ConsoleEvent("Quitting...").writeEvent(true);
                    ConsoleHand.end();
                    break;
                default:
                    new ConsoleEventError("Invalid command '" + input.RawData + "'.", new Exception("Invalid console command.")).writeEvent(true);
                    break;
            }
        }
        void eLog_eAddEvent(ConsoleEvent e)
        {
            ConsoleEventLog.SerializeEvents("d:\\elog.xml");
        }
		public void Start()
		{
            RegisterHandlers();
            ConsoleHand.Start("O-Lobby: ");
			if (!LoadProperties())
				return;
            ConsoleHand.writeCT();
		}

		public Boolean LoadProperties()
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

