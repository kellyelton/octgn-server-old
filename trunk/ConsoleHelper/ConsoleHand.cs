using System;
using System.Threading;
namespace Skylabs.ConsoleHelper
{
	public enum enConsoleEvent
	{Read,Wrote,ComText};
		
	public class ConsoleHand
	{
        public delegate void ConsoleInputDelegate(ConsoleMessage input);
        public static event ConsoleInputDelegate eConsoleInput;
		private static Thread thread;
		private static Boolean endIt;
		private static enConsoleEvent lastEvent;
		public static String CommandText{get;set;}

        public static void Start(String commandText)
		{
            thread = new Thread(run);
            endIt = false;
            lastEvent = enConsoleEvent.Wrote;
            CommandText = commandText;
			thread.Start();
		}
        private static void handleInput(ConsoleMessage cm)
        {
            if(eConsoleInput != null)
            {
                if(eConsoleInput.GetInvocationList().Length > 0)
                    eConsoleInput.Invoke(cm);
            }
        }
        private static void run()
		{
			while(!endIt)
			{
				if(Console.In.Peek() != 0)
				{
					lastEvent = enConsoleEvent.Read;
                    handleInput(new ConsoleMessage(Console.In.ReadLine()));
					writeCT();
				}
				else
				{
					Thread.Sleep(100);	
				}
			}
		}
        public static void writeCT()
		{
			if(lastEvent != enConsoleEvent.ComText)
			{
                Console.Out.Write(CommandText);
				lastEvent = enConsoleEvent.ComText;
			}
		}
        public static void writeLine(String st, Boolean writeComText)
		{
            Console.Out.WriteLine(st);
			lastEvent = enConsoleEvent.Wrote;			
			if(writeComText)
				writeCT();

		}
        public static void end()
		{
			endIt = true;	
		}
	}
}


