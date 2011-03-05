using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Octgn.Play;

namespace Octgn.Lobby
{
	public partial class StartGame : Page
	{
		public StartGame()
		{
      Program.GameSettings.UseTwoSidedTable = false;

			InitializeComponent();

            if (!Program.LClient.isHosting)
            {
                descriptionLabel.Text = "The following players have joined the game.\nPlease wait until the game starts, or click 'Cancel' to leave this game.";
                startBtn.Visibility = Visibility.Collapsed;
                options.IsEnabled = playersList.IsEnabled = false;
            }
            else
            {
                descriptionLabel.Text = "The following players have joined your game.\nClick 'Start' when everyone has joined. No one will be able to join once the game has started.";

            }

      Loaded += delegate
      {
        Program.Dispatcher = this.Dispatcher;
        Program.ServerError += HandshakeError;
        if (Program.IsHost)
          Program.GameSettings.PropertyChanged += SettingsChanged;
        // Fix: defer the call to Program.Game.Begin(), so that the trace has 
        // time to connect to the ChatControl (done inside ChatControl.Loaded).
        // Otherwise, messages notifying a disconnection may be lost
        Dispatcher.BeginInvoke(new Action(Program.Game.Begin));
      };      
      Unloaded += delegate
      {
        Program.GameSettings.PropertyChanged -= SettingsChanged;
        Program.ServerError -= HandshakeError;
      };
		}

		private void SettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable);
		}

		internal void Start()
		{
			// Reset the InvertedTable flags if they were set and they are not used
			if (!Program.GameSettings.UseTwoSidedTable)
				foreach (var player in Play.Player.AllExceptGlobal)
					player.InvertedTable = false;

      // At start the global items belong to the player with the lowest id
      if (Player.GlobalPlayer != null)
      {
        var host = Player.AllExceptGlobal.OrderBy(p => p.Id).First();
        foreach (var group in Player.GlobalPlayer.Groups)
          group.Controller = host;
      }
            
			Program.lwMainWindow = (Octgn.Launcher.LauncherWindow)Application.Current.MainWindow;
			Program.lwPlay = new Octgn.Play.PlayWindow();
            Application.Current.MainWindow = Program.lwPlay;

            Program.lwPlay.Closed += new EventHandler(lwPlay_Closed);

            if (Program.LClient.isHosting)
            {
                Program.LClient.unHost_Game();
            }

            NavigationService.GoBack();
            Program.lwMainWindow.Hide();
            Program.lwPlay.Show();
		}

        void lwPlay_Closed(object sender, EventArgs e)
        {
             Program.lwMainWindow.OkToClose = true;
            Program.lwMainWindow.Close();
            Program.lwMainWindow = (Octgn.Launcher.LauncherWindow)Application.Current.MainWindow;
            Program.lwMainWindow.Closing += new System.ComponentModel.CancelEventHandler(Program.lwLobbyWindow.navWnd_Closing);
            Program.LClient.ChangePlayerStatus(PlayerStatus.Available);
            Program.LClient.isHosting = false;
            Program.LClient.isJoining = false;
            Program.lwPlay.Closed -= lwPlay_Closed;
           
        }


		private void StartClicked(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			Program.Client.Rpc.Start();
            Start();
		}

		private void CancelClicked(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			Back();
		}

		private void Back()
		{
            Program.LClient.isJoining = false;
            if (Program.LClient.isHosting)
            {
                //NavigationService.RemoveBackEntry();
                Program.LClient.isHosting = false;
                Program.LClient.unHost_Game();
               
            }
            Program.LClient.ChangePlayerStatus(PlayerStatus.Available);
			Program.StopGame();
			NavigationService.GoBack();
		}

		private void HandshakeError(object sender, ServerErrorEventArgs e)
		{
			MessageBox.Show("The server returned an error:\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			Back();
		}		
	}
}