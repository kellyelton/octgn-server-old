using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Octgn.Networking;
using Octgn.Properties;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

namespace Octgn.Lobby
{
  public sealed partial class Login : Page
  {

    public Login()
    {
      InitializeComponent();
      tbLogEmail.Text = Settings.Default.Email;
      Program.LClient.eLogEvent += new LobbyClient.ConnectionDelegate(LClient_eConnection);
    }

    private delegate void NoArgsDelegate();

    private void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        funLogin();
        
    }

    void LClient_eConnection(string ConEvent)
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
                        case "LSUCC":

                            Program.LClient.eLogEvent -= LClient_eConnection;
                            NavigationService.Navigate(new Octgn.Lobby.Lobby());
                        break;
                        case "RSUCC":
                            tbLogEmail.Text = tbRegEmail.Text;
                            tbRegEmail.Text = "";
                            tbRegUsername.Text = "";
                            tbRegPass1.Password = "";
                            tbRegPass2.Password = "";
                        break;
                        case "RERR":
                            tbRegPass1.Password = "";
                            tbRegPass2.Password = "";
                        break;
                        case "LERR":
                            tbLogPass.Password = "";
                        break;
                        default:
                            //MessageBox.Show(ConEvent, "ConEvent");

                        break;
                    }
                }
            )
        );
    }

    private void btnCan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Program.LClient.eLogEvent -= LClient_eConnection;
        }
        catch (Exception ex)
        {
        }
        Program.LClient.Close("Exit Lobby",true);
        Program.lwLobbyWindow.Close();
        Program.lwLobbyWindow = null;
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
            if (Regex.IsMatch(tbRegUsername.Text.Trim(), @"[^a-zA-Z0-9-_]"))
            {
                MessageBox.Show("Username contains invalid characters");
            }
            else
            {
                if (!tbRegPass1.Password.Equals(tbRegPass2.Password))
                    MessageBox.Show("Password must match!", "Error");
                else
                {
                    if (Regex.IsMatch(tbRegPass1.Password, @"[^a-zA-Z0-9-_\!\@\#\$\%\^\&\*\(\)\+\=\`\~\[\]\{\}\;\:\|\\\<\>\?\/\.\,]"))
                    {
                        MessageBox.Show("Password contains invalid characters");
                    }
                    else
                        Program.LClient.Register(tbRegEmail.Text, tbRegUsername.Text, tbRegPass1.Password);
                }
            }
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

    private void tbLogEmail_TextChanged(object sender, TextChangedEventArgs e)
    {
        
    }

  }
}