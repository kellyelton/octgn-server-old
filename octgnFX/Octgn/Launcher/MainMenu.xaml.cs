using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Reflection;

namespace Octgn.Launcher
{
	public sealed partial class MainMenu : Page
	{
        private DeckBuilder.DeckBuilderWindow lwDeck;
        private Boolean canOpenDeckEditor = true;
		public MainMenu()
		{
			InitializeComponent();
			versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
            tbRelease.Text = "Release 1.0.1.8";

		}

		private void StartGame(object sender, RoutedEventArgs e)
		{ NavigationService.Navigate(new Serve()); }

		private void JoinGame(object sender, RoutedEventArgs e)
		{ NavigationService.Navigate(new Join()); }

		private void ManageGames(object sender, RoutedEventArgs e)
		{ if(canOpenDeckEditor)NavigationService.Navigate(new GameManager()); }

		private void EditDecks(object sender, RoutedEventArgs e)
		{
            if (canOpenDeckEditor)
            {
                if (Program.GamesRepository.Games.Count == 0)
                {
                    MessageBox.Show("You have no game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                canOpenDeckEditor = false;
                var launcherWnd = Application.Current.MainWindow;
                lwDeck = new DeckBuilder.DeckBuilderWindow();
                lwDeck.Show();
                lwDeck.Closing += new System.ComponentModel.CancelEventHandler(lwDeck_Closing);

                //Application.Current.MainWindow = deckWnd;
                //launcherWnd.Close();
            }
		}

        void lwDeck_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            canOpenDeckEditor = true;
        }

		private void QuitClicked(object sender, RoutedEventArgs e)
        {
            if (Program.lwLobbyWindow == null)
            {
                Application.Current.MainWindow.Close();
                Application.Current.Shutdown(0);
            }
            else
            {
                if (Program.lwLobbyWindow.IsVisible == false)
                {
                    Program.lwLobbyWindow = new Octgn.Lobby.LobbyWindow();
                    Program.lwLobbyWindow.Show();
                    Program.lwLobbyWindow.Navigate(new Octgn.Lobby.ServerConnect());
                }
                else
                    MessageBox.Show("You must exit the lobby system first.");
            }
        }

		#region Hint texts

		private void EnterBtn(object sender, RoutedEventArgs e)
		{
			Button btn = (Button)sender;
			hintText.Text = (string)btn.Tag;
		}

		private void LeaveBtn(object sender, RoutedEventArgs e)
		{ hintText.Text = ""; }

		#endregion

        private void LobbyClicked(object sender, RoutedEventArgs e)
        {
            if (Program.lwLobbyWindow == null)
            {
                Program.lwLobbyWindow = new Octgn.Lobby.LobbyWindow();
                Program.lwLobbyWindow.Show();
                Program.lwLobbyWindow.Navigate(new Octgn.Lobby.ServerConnect());
            }
            else if (Program.lwLobbyWindow.IsVisible == false)
            {
                Program.lwLobbyWindow = new Octgn.Lobby.LobbyWindow();
                Program.lwLobbyWindow.Show();
                Program.lwLobbyWindow.Navigate(new Octgn.Lobby.ServerConnect());
            }
        }
	}
}