using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Octgn.Networking;
using Octgn.Properties;
using System.Net.Sockets;
using System.Threading;

namespace Octgn.Launcher
{
  public sealed partial class ServerConnect : Page
  {
    private bool isStarting = false;

    public ServerConnect()
    {
      InitializeComponent();
    //To make sure the page loads before we try and connect, otherwise, shit breaks.
      this.Loaded += new RoutedEventHandler(ServerConnect_Loaded);

    }

    void ServerConnect_Loaded(object sender, RoutedEventArgs e)
    {
             //e.Handled = true;
        this.Loaded -= ServerConnect_Loaded;

    }

    void LClient_eError(string error)
    {
        System.Threading.Thread thread = new System.Threading.Thread
        (
            new System.Threading.ThreadStart
            (
                delegate()
                {
                    this.Dispatcher.Invoke
                    (
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action
                        (
                            delegate()
                            {
                                Program.LClient.eError -=LClient_eError;
                                NavigationService.GoBack();
                                MessageBox.Show("There was a problem connecting to the server. Please try agian later.");
                            }
                        )
                    );
                }
            )
        );
        thread.Start();
    }

    private delegate void NoArgsDelegate();


    private void ConnectedCallback(String Con)
    {
      isStarting = false;
      if (Con.Equals("CON"))
      {
          Program.LClient.eConnection -= ConnectedCallback;
          Program.LClient.eError -= LClient_eError;
          Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgsDelegate(LaunchGame));
      }
        
    }

    private void LaunchGame()
    {
        NavigationService.Navigate(new Login()); 
    }
    private void goBack()
    {
        Program.LClient.eConnection -= ConnectedCallback;
        Program.LClient.eError -= LClient_eError;
        MessageBox.Show("Server is unavailable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.GoBack();
    }
    private void CancelClicked(object sender, RoutedEventArgs e)
    {
        Program.LClient.eConnection -= ConnectedCallback;
        Program.LClient.eError -= LClient_eError;
        NavigationService.GoBack();
    }

    private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (isStarting) return;	// prevents doube-click and such
        Program.LClient = new LobbyClient(Settings.Default.ServerHost, Settings.Default.ServerPort);
        Program.LClient.eConnection += ConnectedCallback;
        Program.LClient.eError += new LobbyClient.ErrorDelegate(LClient_eError);
        Program.LClient.Start();
        isStarting = true;   
    }
  }
}