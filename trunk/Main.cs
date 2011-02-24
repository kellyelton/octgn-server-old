using System;
using Skylabs.ConsoleHelper;
using System.Xml;
using System.IO;

namespace Skylabs.oserver
{
	public class MainClass : ConsoleGlove
	{
		public static MainClass main;
		public static void Main (string[] args)
		{
			main = new MainClass();
			main.Start();
            ConsoleEventLog.SerializeEvents("d:\\elog.xml",main.Con.eLog);
		}
		
		public XmlDocument Properties{get;set;}
		public ConsoleHand Con;
		public void Start()
		{
			Con = new ConsoleHand("oserver: ",this);
			if (!LoadProperties())
				return;
			main.Con.Start();

			
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
                Con.writeEvent(new ConsoleEvent("#Event: ", "Settings file loaded.", ConsoleColor.Gray));
            }
            catch (Exception ex)
            {
                if (f != null)
                    f.Close();
                Con.writeEvent(new ConsoleEventError("Could not load the settings file.", ex));
                ret = false;
            }
			return ret;

		}
		public void onInput(string str)
		{
			Con.writeLine(str,true);
		}
	}
}

