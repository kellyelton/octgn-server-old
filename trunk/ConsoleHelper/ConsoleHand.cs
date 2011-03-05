using System;
using System.Threading;
namespace Skylabs.ConsoleHelper
{
	public enum ConsoleEvent
	{Read,Wrote,ComText};
		
	public class ConsoleHand
	{
		private Thread thread;
		private System.IO.TextReader cin;
		private System.IO.TextWriter cout;
		private Boolean endIt;
		private ConsoleGlove glove;
		private ConsoleEvent lastEvent;
		public String CommandText{get;set;}
		
		public ConsoleHand (String CommandText, ConsoleGlove glove)
		{
			cin = Console.In;
			cout = Console.Out;
			thread = new Thread(run);
			endIt = false;
			this.glove = glove;
			lastEvent = ConsoleEvent.Wrote;
			this.CommandText = CommandText;
		}
		public void Start()
		{
			thread.Start();
		}
		private void run()
		{
			while(!endIt)
			{
				if(cin.Peek() != 0)
				{
					lastEvent = ConsoleEvent.Read;
					glove.onInput(cin.ReadLine());
					
					writeCT();
				}
				else
				{
					Thread.Sleep(100);	
				}
			}
		}
		public void writeCT()
		{
			if(lastEvent != ConsoleEvent.ComText)
			{
				cout.Write(CommandText);
				lastEvent = ConsoleEvent.ComText;
			}
		}
		public void writeLine(String st, Boolean writeComText)
		{
			cout.WriteLine(st);
			lastEvent = ConsoleEvent.Wrote;			
			if(writeComText)
				writeCT();

		}
		public void end()
		{
			endIt = true;	
		}
	}
}


