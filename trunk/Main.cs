using System;
using Skylabs.ConsoleHelper;

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
			main.Con.Start();
			LoadProperties();
			
		}
		public void LoadProperties()
		{
			Properties = new XmlDocument();
			Properties.LoadXml("ServerOptions.xml");
		}
		public void onInput(string str)
		{
			main.Con.writeLine(str,true);
		}
	}
}

