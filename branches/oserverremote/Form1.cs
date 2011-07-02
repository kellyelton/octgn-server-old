using System;
using System.Text;
using System.Windows.Forms;
using Skylabs.NetShit;

namespace oserverremote
{
    public partial class Form1 : Form
    {
        RawShitSock sock = new RawShitSock();

        public Form1()
        {
            InitializeComponent();
            sock.onConnectionEvent += new RawShitSock.dConnectionEvent(sock_onConnectionEvent);
            sock.onInput += new RawShitSock.dOnInput(sock_onInput);
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

        private void sock_onInput(object Sender, ShitBag bag)
        {
            addline("Input: " + Encoding.ASCII.GetString(bag.buffer));
            PingMessage pm = new PingMessage();
            sock.WriteData(Encoding.ASCII.GetBytes(pm.getMessage()));
        }

        private void sock_onConnectionEvent(object Sender, ConnectionEvent e)
        {
            if(e.Event == ConnectionEvent.eConnectionEvent.eceConnect)
            {
                addline("#Connected");
                SocketMessage sm = new SocketMessage("RC");
                sm.getMessage();
                sock.WriteData(Encoding.ASCII.GetBytes(sm.getMessage()));
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
            sock.WriteData(Encoding.ASCII.GetBytes(sm.getMessage()));
            addline("Output: " + sm.getMessage());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SocketMessage sm = new SocketMessage("LOG");
            sm.Arguments.Add(tbUsername.Text);
            sm.Arguments.Add(tbPassword.Text);
            sm.Arguments.Add("1.0.1.17");
            sock.WriteData(Encoding.ASCII.GetBytes(sm.getMessage()));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SocketMessage sm = new SocketMessage("1");
            sm.Arguments.Add(textBox1.Text);
            sock.WriteData(Encoding.ASCII.GetBytes(sm.getMessage()));
            addline("Output: " + sm.getMessage());
        }
    }
}