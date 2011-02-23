using System;
using Skylabs.ConsoleHelper;

namespace Skylabs
{
	class oserver : ConsoleGlove
	{
		public static oserver main;
		public ConsoleHand Con;
		public static void Main (string[] args)
		{
			main = new oserver();
			main.Start();
			
		}
		public void Start()
		{
			main.Con = new ConsoleHand("oserver: ",this);			
			main.Con.Start();
			
		}
		
		public void onInput(string str)
		{
			main.Con.writeLine(str,true);
		}
	}
}

