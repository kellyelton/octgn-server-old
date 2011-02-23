using System;
namespace oserver
{
	public class ConsoleHand 
	{
		private System.IO.TextReader cin;
		private System.IO.TextReader cout;
		public ConsoleHand ()
		{
			cin = Console.In;
			cout = Console.Out;
		}
		override public void run()
		{
			
		}
	}
}


