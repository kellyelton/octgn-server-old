using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Windows.Media.Animation;
namespace Octgn.Launcher
{
    public sealed partial class Lobby : Page
    {

        private List<String> lsMessageHistory;
        private int intMessHistoryIndex = 0;

        public Lobby()
        {
            eSub();
            InitializeComponent();
            lsMessageHistory = new List<string>();
        }
        private void eSub()
        {
            try
            {
            Program.LClient.eLobbyChat += new Networking.LobbyClient.LobbyChatDelegate(LClient_eLobbyChat);
            Program.LClient.eUserEvent += new Networking.LobbyClient.UserDelegate(LClient_eUserEvent);
            Program.LClient.eConnection += new Networking.LobbyClient.ConnectionDelegate(LClient_eConnection);
            }
            catch (Exception e) { };
        }
        private void eUnSub()
        {
            try
            {
            Program.LClient.eLobbyChat -= LClient_eLobbyChat;
            Program.LClient.eUserEvent -= LClient_eUserEvent;
            Program.LClient.eConnection -= LClient_eConnection;
            }
            catch (Exception e) { };
        }
        void LClient_eConnection(string ConEvent)
        {
            System.Threading.Thread thread = new System.Threading.Thread
            (
                new System.Threading.ThreadStart
                (
                    delegate()
                    {
                        rtbChat.Dispatcher.Invoke
                        (
                            System.Windows.Threading.DispatcherPriority.Normal,
                            new Action
                            (
                                delegate()
                                {
                                    if (ConEvent.Equals("DC"))
                                    {
                                        try
                                        {
                                            eUnSub();
                                            Program.LClient.Close("Exit Lobby", true);
                                            NavigationService.GoBack();
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                                        }

                                    }
                                }
                            )
                        );
                    }
                )
            );
            thread.Start();
        }

        void LClient_eUserEvent(Networking.LobbyClient.User user, bool Connected)
        {
            System.Threading.Thread thread = new System.Threading.Thread
            (
                new System.Threading.ThreadStart
                (
                    delegate()
                    {
                        rtbChat.Dispatcher.Invoke
                        (
                            System.Windows.Threading.DispatcherPriority.Normal,
                            new Action
                            (
                                delegate()
                                {
                                    if (rtbChat == null)
                                        return;
                                    if (Connected)
                                    {
                                        if (!user.Username.Equals(""))
                                            rtbChat.AppendText("[" + user.Username + "]: Joined the lobby.\n");
                                    }
                                    else
                                    {
                                        if (!user.Username.Equals(""))
                                            rtbChat.AppendText("[" + user.Username + "]: Left the lobby.\n");
                                    }
                                    UpdateColors();
                                    Update_Online_Users();
                                }
                            )
                        );
                    }
                )
            );

            thread.Start();
        }

        void LClient_eLobbyChat(Octgn.Networking.LobbyClient.LobbyChatTypes type,string user, string chat)
        {
            System.Threading.Thread thread = new System.Threading.Thread
            (
                new System.Threading.ThreadStart
                (
                    delegate()
                    {
                        rtbChat.Dispatcher.Invoke
                        (
                            System.Windows.Threading.DispatcherPriority.Normal,
                            new Action
                            (
                                delegate()
                                {
                                    if (rtbChat == null)
                                        return;
                                    switch (type)
                                    {
                                        case Networking.LobbyClient.LobbyChatTypes.Global:
                                            rtbChat.AppendText("[" + user + "]: " + chat + "\n");
                                        break;
                                        case Networking.LobbyClient.LobbyChatTypes.System:
                                            rtbChat.AppendText("#" + user + ": " + chat + "\n");
                                        break;
                                        case Networking.LobbyClient.LobbyChatTypes.Whisper:
                                            String[] w = user.Split(new char[1] { ':' });
                                            rtbChat.AppendText("<" + w[0] + ">" +  w[1] + ": " + chat + "\n");
                                        break;
                                        case Networking.LobbyClient.LobbyChatTypes.Error:
                                            rtbChat.AppendText("!" + user + ": " + chat + "\n");
                                        break;
                                    }
                                    UpdateColors();
                                }
                            )
                        );
                    }
                )
            );

            thread.Start();
            
        }

        private delegate void NoArgsDelegate();

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
            //rtbChat.AppendText(new TextRange(rtbMess.Document.ContentStart, rtbMess.Document.ContentEnd).Text);
            //Program.LClient.Chat(new TextRange(rtbMess.Document.ContentStart, rtbMess.Document.ContentEnd).Text);
            if (!tbMess.Text.Trim().Equals(""))
            {
                lsMessageHistory.Add(tbMess.Text);
                Program.LClient.Chat(tbMess.Text);
                tbMess.Text = "";
            }
           // rtbChat.re
        }
        private void Update_Online_Users()
        {
            if (listBox1 == null)
                return;
            listBox1.Items.Clear();
            for (int i = 0; i < Program.LClient.OnlineUsers.Count; i++)
            {
                listBox1.Items.Add(Program.LClient.OnlineUsers[i].Username);
            }
            
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            Skylabs.Networking.SocketClient.SocketMessage sm = new Skylabs.Networking.SocketClient.SocketMessage("GETONLINELIST");
            Program.LClient.writeMessage(sm);
            Update_Online_Users();
            tbNickname.Text = "Your nickname: " + Program.LClient.strUserName;
        }

        private void UpdateColors()
        {
            if (rtbChat.Document == null)
                return;

            TextRange documentRange = new TextRange(rtbChat.Document.ContentStart, rtbChat.Document.ContentEnd);
            documentRange.ClearAllProperties();

            TextPointer navigator = rtbChat.Document.ContentStart;
            while (navigator.CompareTo(rtbChat.Document.ContentEnd) < 0)
            {
                TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
                if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
                {
                    //navigator.Parent.
                    CheckWordsInRun((Run)navigator.Parent);


                }
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);

            }

            for (int i = 0; i < m_tags.Count; i++)
            {
                TextRange range = new TextRange(m_tags[i].StartPosition, m_tags[i].EndPosition);
                switch (m_tags[i].Type)
                {
                    case Networking.LobbyClient.LobbyChatTypes.Global:
                        if (range.Text.Equals("[" + Program.LClient.strUserName + "]:"))
                        {
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        }
                        else
                        {
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        }
                    break;
                    case Networking.LobbyClient.LobbyChatTypes.Whisper:
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Orange));
                            range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                    break;
                    case Networking.LobbyClient.LobbyChatTypes.System:
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Gray));
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    break;
                    case Networking.LobbyClient.LobbyChatTypes.Error:
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    break;
                }
            }
            m_tags.Clear();
            //SendMessage(rtbChat.Handle, WM_VSCROLL, SB_BOTTOM, 0);
            rtbChat.ScrollToEnd();
        }
        new struct Tag
        {
            public TextPointer StartPosition;
            public TextPointer EndPosition;
            public string Word;
            public Networking.LobbyClient.LobbyChatTypes Type;

        }
        List<Tag> m_tags = new List<Tag>();
        void CheckWordsInRun(Run run)
        {
            string text = run.Text.Trim();
            if (text.Equals(""))
                return;
            int sIndex = 0;
                if (text[0] == '<')
                {
                    Tag t = new Tag();
                    int s = 0;
                    int e = text.IndexOf(':');
                    t.StartPosition = run.ContentStart.GetPositionAtOffset(s, LogicalDirection.Forward);
                    t.EndPosition = run.ContentStart.GetPositionAtOffset(e, LogicalDirection.Backward);
                    t.Word = text.Substring(s,text.Length - e);
                    t.Type = Networking.LobbyClient.LobbyChatTypes.Whisper;
                    m_tags.Add(t);
                    return;
                }
            String[] words = text.Split(new char[1]{' '});
            for(int i=0;i<words.Length;i++)
            {
                string word = words[i];

                for (int u = 0; u < Program.LClient.OnlineUsers.Count; u++)
                {
                    if (word.Equals("[" + Program.LClient.OnlineUsers[u].Username + "]:"))
                    {
                        Tag t = new Tag();
                        int s = text.IndexOf(word,sIndex);
                        sIndex = s + word.Length - 1;
                        t.StartPosition = run.ContentStart.GetPositionAtOffset(s, LogicalDirection.Forward);
                        t.EndPosition = run.ContentStart.GetPositionAtOffset(s + word.Length, LogicalDirection.Backward);
                        t.Word = word;
                        t.Type = Networking.LobbyClient.LobbyChatTypes.Global;
                        m_tags.Add(t);
                    }
                    else if (word.Equals("[" + Program.LClient.OnlineUsers[u].Username + "]"))
                    {
                        Tag t = new Tag();
                        int s = text.IndexOf(word, sIndex);
                        sIndex = s + word.Length - 1;
                        t.StartPosition = run.ContentStart.GetPositionAtOffset(s, LogicalDirection.Forward);
                        t.EndPosition = run.ContentStart.GetPositionAtOffset(s + word.Length, LogicalDirection.Backward);
                        t.Word = word;
                        t.Type = Networking.LobbyClient.LobbyChatTypes.Global;
                        m_tags.Add(t);
                    }
                    else if (word.Equals("!ERROR:"))
                    {
                        Tag t = new Tag();
                        int s = text.IndexOf(word, sIndex);
                        sIndex = s + word.Length - 1;
                        t.StartPosition = run.ContentStart.GetPositionAtOffset(s, LogicalDirection.Forward);
                        t.EndPosition = run.ContentStart.GetPositionAtOffset(s + word.Length, LogicalDirection.Backward);
                        t.Word = word;
                        t.Type = Networking.LobbyClient.LobbyChatTypes.Error;
                        m_tags.Add(t);
                    }
                    else if (word.Equals("#SYSTEM:"))
                    {
                        Tag t = new Tag();
                        int s = text.IndexOf(word, sIndex);
                        sIndex = s + word.Length - 1;
                        t.StartPosition = run.ContentStart.GetPositionAtOffset(s, LogicalDirection.Forward);
                        t.EndPosition = run.ContentStart.GetPositionAtOffset(s + word.Length, LogicalDirection.Backward);
                        t.Word = word;
                        t.Type = Networking.LobbyClient.LobbyChatTypes.System;
                        m_tags.Add(t);
                    }
                }    
            }
        }
        private bool isOnlineUserName(String user)
        {
            for (int u = 0; u < Program.LClient.OnlineUsers.Count; u++)
            {
                if (Program.LClient.OnlineUsers[u].Username.Equals(user))
                    return true;
            }
            return false;
        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                eUnSub();
                Program.LClient.Close("Exit Lobby", true);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void tbMess_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!tbMess.Text.Trim().Equals(""))
                {
                    lsMessageHistory.Add(tbMess.Text);
                    Program.LClient.Chat(tbMess.Text);
                    tbMess.Text = "";
                }
            }
            else if (e.Key == Key.Up)
            {
                intMessHistoryIndex++;
                if (intMessHistoryIndex >= lsMessageHistory.Count)
                    intMessHistoryIndex = 0;
                try
                {
                    tbMess.Text = lsMessageHistory[intMessHistoryIndex];
                }
                catch (Exception ex)
                {
                    tbMess.Text = "";
                }

            }
            else if (e.Key == Key.Down)
            {
                intMessHistoryIndex--;
                if (intMessHistoryIndex <= 0)
                    intMessHistoryIndex = lsMessageHistory.Count - 1;
                try
                {
                    tbMess.Text = lsMessageHistory[intMessHistoryIndex];
                }
                catch (Exception ex)
                {
                    tbMess.Text = "";
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                eUnSub();
                Program.LClient.Close("Exit Lobby", true);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            
        }

    }
}