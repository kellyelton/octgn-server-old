using System;
using System.Threading;
namespace Skylabs.ConsoleHelper
{
	public enum enConsoleEvent
	{Read,Wrote,ComText};
		
	public class ConsoleHand
	{
		private Thread thread;
		private System.IO.TextReader cin;
		private System.IO.TextWriter cout;
		private Boolean endIt;
		private ConsoleGlove glove;
		private enConsoleEvent lastEvent;
		public String CommandText{get;set;}
		
		public ConsoleHand (String CommandText, ConsoleGlove glove)
		{
			cin = Console.In;
			cout = Console.Out;
			thread = new Thread(run);
			endIt = false;
			this.glove = glove;
			lastEvent = enConsoleEvent.Wrote;
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
					lastEvent = enConsoleEvent.Read;
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
			if(lastEvent != enConsoleEvent.ComText)
			{
				cout.Write(CommandText);
				lastEvent = enConsoleEvent.ComText;
			}
		}
		public void writeLine(String st, Boolean writeComText)
		{
			cout.WriteLine(st);
			lastEvent = enConsoleEvent.Wrote;			
			if(writeComText)
				writeCT();

		}
		public void writeEvent(ConsoleEvent cone)
		{
			ConsoleColor cc = Console.ForegroundColor;
			Console.ForegroundColor = cone.Color;
			writeLine(cone.Header + cone.Message,true);
			Console.ForegroundColor = cc;
			ConsoleEventLog.addEvent(cone);
		}
		public void end()
		{
			endIt = true;	
		}
	}
}


