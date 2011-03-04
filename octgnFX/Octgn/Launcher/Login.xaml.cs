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
  public sealed partial class Login : Page
  {

    public Login()
    {
      InitializeComponent();
      tbLogEmail.Text = Settings.Default.Email;
      Program.LClient.eConnection += new LobbyClient.ConnectionDelegate(LClient_eConnection);
    }

    private delegate void NoArgsDelegate();

    private void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        funLogin();
    }

    void LClient_eConnection(string ConEvent)
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
                                    switch (ConEvent)
                                    {
                                        case "LIN":
                                            Program.LClient.eConnection -= LClient_eConnection;
                                            NavigationService.Navigate(new Octgn.Launcher.Lobby());
                    
                                            break;
                                        default:
                                           // MessageBox.Show(ConEvent, "ConEvent");
                                            break;
                                    }
                                }
                            )
                        );
                    }
                )
            );
            thread.Start();
    }

    private void btnCan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Program.LClient.eConnection -= LClient_eConnection;
        }
        catch (Exception ex)
        {
        }
        Program.LClient.Close("Exit Lobby",true);
        NavigationService.GoBack();
    }

    private void btnReg_Click(object sender, RoutedEventArgs e)
    {
        if (tbRegPass1.Password.Trim().Equals("") || tbRegPass1.Password.Trim().Length < 6)
            MessageBox.Show("Password must be at least 6 characters long.", "Error");
        else if (tbRegEmail.Text.Trim().Equals(""))
            MessageBox.Show("Email must not be empty.");
        else if (tbRegUsername.Text.Trim().Equals(""))
            MessageBox.Show("Username must not be empty.");
        else if (tbRegUsername.Text.Trim().Contains(" "))
            MessageBox.Show("Username can not contain spaces.");
        else
        {
            if (!tbRegPass1.Password.Equals(tbRegPass2.Password))
                MessageBox.Show("Password must match!", "Error");
            else
                Program.LClient.Register(tbRegEmail.Text, tbRegUsername.Text, tbRegPass1.Password);
        }

    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        //Program.LClient.Close("done");
        NavigationService.RemoveBackEntry();
    }
    private void funLogin()
    {
        Settings.Default.Email = tbLogEmail.Text;
        Settings.Default.Save();
        Program.LClient.Login(tbLogEmail.Text, tbLogPass.Password);
    }
    private void tbLogPass_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            funLogin();
    }

  }
}