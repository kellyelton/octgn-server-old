using System;
using System.Windows.Forms;
using Skylabs.NetShit;

namespace oserverremote
{
    public partial class Form1 : Form
    {
        ShitSock sock = new ShitSock();

        public Form1()
        {
            InitializeComponent();
            sock.onConnectionEvent += new RawShitSock.dConnectionEvent(sock_onConnectionEvent);
            sock.onSocketMessageInput += new ShitSock.dOnSockMessageInput(sock_onSocketMessageInput);
        }

        private void sock_onSocketMessageInput(object Sender, SocketMessage sm)
        {
            addline("Input: " + sm.getMessage());
        }

        private void addline(string str)
        {
            this.Invoke(new Action(delegate
            {
                string[] temp = textBox2.Lines;
                Array.Resize(ref temp, temp.Length + 1);
                temp[temp.Length - 1] = str;
                textBox2.Lines = temp;
                textBox2.Select(textBox2.Text.Length - 1, 0);
                textBox2.ScrollToCaret();
                //textBox2.Lines = textBox2.Text + '\n' + str;
            }));
        }

        private void sock_onConnectionEvent(object Sender, ConnectionEvent e)
        {
            if(e.Event == ConnectionEvent.eConnectionEvent.eceConnect)
            {
                addline("#Connected");
                SocketMessage sm = new SocketMessage("RC");
                sm.getMessage();
                sock.writeMessage(sm);
                //sock.writeMessage(sm);
            }
            else
            {
                addline("#Disconnected");
                sock.Connect("www.skylabsonline.com", 5583);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(!sock.Connected)
            {
                sock.Connect("www.skylabsonline.com", 5583);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(button2.Text == "Start")
            {
                button2.Text = "Stop";
                timer1.Enabled = true;
            }
            else
            {
                button2.Text = "Start";
                timer1.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SocketMessage sm = new SocketMessage("1");
            sm.Arguments.Add(textBox1.Text);
            sock.writeMessage(sm);
            addline("Output: " + sm.getMessage());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SocketMessage sm = new SocketMessage("LOG");
            sm.Arguments.Add(tbUsername.Text);
            sm.Arguments.Add(tbPassword.Text);
            sm.Arguments.Add("1.0.2.1");

            sock.writeMessage(sm);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SocketMessage sm = new SocketMessage("1");
            sm.Arguments.Add(textBox1.Text);
            sock.writeMessage(sm);
            addline("Output: " + sm.getMessage());
        }
    }
}