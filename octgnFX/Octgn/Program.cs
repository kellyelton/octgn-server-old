using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Windows;
using System.Reflection;
using Octgn.Properties;
using Octgn.Play;

namespace Octgn
{
	public static class Program
	{
		public static Game Game;
		public static GameSettings GameSettings = new GameSettings();
		internal static bool IsGameRunning = false;
		// TODO: Refactoring > those paths belong to the Octgn.Data or somewhere else
    internal readonly static string BasePath;
		internal readonly static string GamesPath;
		public static Data.GamesRepository GamesRepository;

		internal static ulong PrivateKey = ((ulong)Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom();

		internal static Server.Server Server;
		internal static Networking.Client Client;
        //TODO Added for lobby
        ////////////////////////////////////////
        internal static Lobby.LobbyClient LClient;
        internal static Lobby.LobbyWindow lwLobbyWindow;
        public static Launcher.LauncherWindow lwMainWindow;
        public static Play.PlayWindow lwPlay;
        ////////////////////////////////////////
		internal static event EventHandler<ServerErrorEventArgs> ServerError;

        //TODO Changed for lobby
        // Old internal static bool IsHost { get { return Server != null; } }
        internal static bool IsHost
        {
            get
            {
                if (Server == null)
                {
                    if (LClient != null)
                    {
                        if (LClient.isHosting)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return true;
            }
        }
		internal static Dispatcher Dispatcher;
		
		internal readonly static TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
		internal static System.Windows.Documents.Inline LastChatTrace;
		
    // TODO: Refactoring > this belongs to the Markers class
		internal readonly static DefaultMarkerModel[] DefaultMarkers = new DefaultMarkerModel[] 
    { 
        new DefaultMarkerModel("white", new Guid(0,0,0,0,0,0,0,0,0,0,1)),
        new DefaultMarkerModel("blue",  new Guid(0,0,0,0,0,0,0,0,0,0,2)),
        new DefaultMarkerModel("black", new Guid(0,0,0,0,0,0,0,0,0,0,3)),
        new DefaultMarkerModel("red",  new Guid(0,0,0,0,0,0,0,0,0,0,4)),
        new DefaultMarkerModel("green", new Guid(0,0,0,0,0,0,0,0,0,0,5)),
        new DefaultMarkerModel("orange", new Guid(0,0,0,0,0,0,0,0,0,0,6)),
        new DefaultMarkerModel("brown",  new Guid(0,0,0,0,0,0,0,0,0,0,7)),
        new DefaultMarkerModel("yellow",  new Guid(0,0,0,0,0,0,0,0,0,0,8))
    };

		static Program()
		{      
			BasePath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + '\\';
			GamesPath = BasePath + @"Games\";
		}

		public static void StopGame()
		{
			Client.Disconnect(); Client = null;
			if (Server != null)
			{ Server.Stop(); Server = null; }
			Game.End(); Game = null;
			Dispatcher = null;
			Database.Close();
			IsGameRunning = false;
		}

		internal static void OnServerError(string serverMessage)
		{
			var args = new ServerErrorEventArgs() { Message = serverMessage };
			if (ServerError != null)
				ServerError(null, args);
			if (args.Handled) return;
			
			MessageBox.Show(Application.Current.MainWindow, 
				"The server has returned an error:\n" + serverMessage, 
				"Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

    // TODO: Refactoring: those helper methods belong somewhere else (near the tracing classes)
    internal static void TracePlayerEvent(Player player, string message, params object[] args)
    {
        Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), message, args);
    }

    internal static void TraceWarning(string message)
    {
        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
    }
      
    internal static void TraceWarning(string message, params object[] args)
    {
        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        
    }
	}
}
