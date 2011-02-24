using System;
using Skylabs.ConsoleHelper;
using System.Xml;

namespace Skylabs.oserver
{
	public class MainClass : ConsoleGlove
	{
		public static MainClass main;
		public static void Main (string[] args)
		{
			main = new MainClass();
			main.Start();
			
		}
		
		public XmlDocument Properties{get;set;}
		public ConsoleHand Con;
		public void Start()
		{
			main.Con = new ConsoleHand("oserver: ",this);
            if (!LoadProperties())
                return;
			main.Con.Start();
			
		}
        public Boolean LoadProperties()
        {
            Properties = new XmlDocument();
            try
            {
                Properties.LoadXml("ServerOptions.xml");
            }
            catch (XmlException ex)
            {
                return false;
            }
            return true;

        }
		public void onInput(string str)
		{
			main.Con.writeLine(str,true);
		}
	}
}

