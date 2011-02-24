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
		public static String CommandText{get;set;}
        //TODO create a new color for general output, er something.
        public static ConsoleColor InputColor { get; set; }
        public static ConsoleColor CommandTextColor { get; set; }
        public static ThreadState ThreadState
        {
            get
            {
                return thread.ThreadState;
            }
        }

        private static Thread thread;
        private static Boolean endIt;
        private static enConsoleEvent lastEvent;

        public static void Start(String commandText, ConsoleColor commandTextColor,ConsoleColor inputcolor)
		{
            thread = new Thread(run);
            endIt = false;
            lastEvent = enConsoleEvent.Wrote;
            CommandText = commandText;
            InputColor = inputcolor;
            CommandTextColor = commandTextColor;
            Console.ForegroundColor = commandTextColor;
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
                Console.ForegroundColor = CommandTextColor;
                Console.Out.Write(CommandText);
				lastEvent = enConsoleEvent.ComText;
                Console.ForegroundColor = InputColor;
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


