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
        //Create nessasary variables for TCP, buffer and other booleans
        TcpClient _client;
        byte[] _buffer = new byte[1024];
        static Encoding enc8 = Encoding.UTF8;
        bool firstmsgsent = false;
        bool closed = false;

        public Form1()
        {
            InitializeComponent();
            //Add a message box that tells what to do

            //When the fomr loads create a new TCP connection for TcpClient
            _client = new TcpClient();

        }

        //Function runs when Form is shown
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            
            

            //use this ip if running locally --> 192.168.0.189
            //use this ip if running on Google Server --> connect using external ip at that time
            _client.Connect("192.168.0.189", 3005);
            
            

            // Start reading the socket and receive any incoming messages
            _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, Server_MessageRecieved, null);

        }

        //Method runs everytime getStream recieves something and is used to intepret messages and core functionality
        private void Server_MessageRecieved(IAsyncResult ar)
        {
            //If sync is completed and client isn't quit, try to read message
            if(ar.IsCompleted && closed == false)
            {
                //recieve message
                var bytesIn = _client.GetStream().EndRead(ar);
                //If there are bytes then attempt to read
                if(bytesIn > 0)
                {
                    //Read the bytes in and get string from encoding
                    var tmp = new byte[bytesIn];
                    Array.Copy(_buffer, 0, tmp, 0, bytesIn);
                    var str = Encoding.UTF8.GetString(tmp);

                    //If firstmessage wasn't sent run this
                    if (firstmsgsent == false)
                    {
                        //The first message from the server will be the number of rooms so parse as an integer
                        int number = Int32.Parse(str);
                        //Run for loop to fill combobox with number of rooms
                        for(int i = 0; i < number; i++)
                        {
                            int index = i + 1;
                            comboBox1.Items.Add(index);
                        }
                        //Set the default room to chatroom 1
                        comboBox1.SelectedIndex = 0;
                        listBox1.Items.Add("Welcome to chatroom " + comboBox1.SelectedItem);
                        firstmsgsent = true;
                    }
                    else
                    {
                        //If not first message, add any incoming messages on the main thread to the listbox
                        BeginInvoke((Action)(() =>
                        {
                            listBox1.Items.Add(str);
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                        }));
                    }

                    
                    
                }
                //Clear the array and call the read function again to see if anything new is coming
                Array.Clear(_buffer, 0, _buffer.Length);
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, Server_MessageRecieved, null);
            }
        }

        //Run when the send button is clicked
        private void button1_Click(object sender, EventArgs e)
        {
            //Gets chatroom number and creats message with the username and chatroom number appended. The chatroom number will always be appended to begenining.
            var chatroomNum = comboBox1.SelectedItem;
            var msg = Encoding.UTF8.GetBytes(chatroomNum + textBox2.Text + ": " + textBox1.Text);
            //Write the message to the server
            _client.GetStream().Write(msg);
            //Reset the text box and refresh
            textBox1.Text = "";
            textBox1.Focus();

        }

        //Run if the change room button is pressed
        private void button2_Click(object sender, EventArgs e)
        {
            //Run once a new room a selected and change room is pressed
            var chatroomNum = comboBox1.SelectedItem;
            var msg = Encoding.UTF8.GetBytes("/" + chatroomNum);

            //Send to server with a "/" b/c that indicates a room change
            _client.GetStream().Write(msg);
            //Clear the main textbox
            listBox1.Items.Clear();
            listBox1.Items.Add("Welcome to chatroom " + chatroomNum);

        }

        //Method run if quit button is pressed
        private void button3_Click(object sender, EventArgs e)
        {
            //Get username and send a message saying this user has closed the app
            string username = textBox2.Text;
            var msg = Encoding.UTF8.GetBytes(username + " has closed messenger app!");
            _client.GetStream().Write(msg);
            //Set the closed variable to true so not listening to any new messages and close connection
            closed = true;
            _client.GetStream().Close();
            _client.Close();
            //Indicate to client that app is done.
            MessageBox.Show("Connection Closed. Please exit app.");
        }
    }
}
