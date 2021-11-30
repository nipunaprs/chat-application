using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {

        TcpClient _client;
        byte[] _buffer = new byte[1024];
        static Encoding enc8 = Encoding.UTF8;
        bool firstmsgsent = false;

        public Form1()
        {
            InitializeComponent();

            _client = new TcpClient();

        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Connect to the remote server. The IP address and port # could be
            // picked up from a settings file.
            _client.Connect("192.168.0.189", 3005);

            // Start reading the socket and receive any incoming messages
            _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, Server_MessageRecieved, null);

        }

        private void Server_MessageRecieved(IAsyncResult ar)
        {
            if(ar.IsCompleted)
            {
                //recieve message
                var bytesIn = _client.GetStream().EndRead(ar);
                if(bytesIn > 0)
                {
                    var tmp = new byte[bytesIn];
                    Array.Copy(_buffer, 0, tmp, 0, bytesIn);
                    var str = Encoding.UTF8.GetString(tmp);

                    if (firstmsgsent == false)
                    {
                        label1.Text = "Number of chatrooms is: " + str;
                        firstmsgsent = true;
                    }
                    else
                    {
                        
                        BeginInvoke((Action)(() =>
                        {
                            listBox1.Items.Add(str);
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                        }));
                    }

                    
                    
                }

                Array.Clear(_buffer, 0, _buffer.Length);
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, Server_MessageRecieved, null);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var msg = Encoding.UTF8.GetBytes(textBox1.Text);
            _client.GetStream().Write(msg);
            //removed stuff below from write
            //0, msg.Length
            textBox1.Text = "";
            textBox1.Focus();

        }
    }
}
