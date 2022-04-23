using ChatLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForms
{
    public partial class ClientForm : Form
    {
        #region Private Members

        // Client socket
        private Socket clientSocket;

        // Client name
        private string name;

        // Server End Point
        private EndPoint epServer;

        // Data stream
        private byte[] dataStream = new byte[1024];

        // Display message delegate
        private delegate void DisplayMessageDelegate(string message);
        private DisplayMessageDelegate displayMessageDelegate = null;

        #endregion
        public ClientForm()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            // Initialise delegate
            this.displayMessageDelegate = new DisplayMessageDelegate(this.DisplayMessage);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Initialise a packet object to store the data to be sent
                Paket sendData = new Paket();
                sendData.UserName = this.name;
                sendData.Message = txtMessage.Text.Trim();
                sendData.TypeMessage = DataIdentifier.Message;

                // Get packet as byte array
                byte[] byteData = sendData.GetDataStream();

                // Send packet to the server
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                txtMessage.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (this.clientSocket != null)
                {
                    // Initialise a packet object to store the data to be sent
                    Paket sendData = new Paket();
                    sendData.TypeMessage = DataIdentifier.LogOut;
                    sendData.UserName = this.name;
                    sendData.Message = "";

                    // Get packet as byte array
                    byte[] byteData = sendData.GetDataStream();

                    // Send packet to the server
                    this.clientSocket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer);

                    // Close the socket
                    this.clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Closing Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.name = txtName.Text.Trim();

                // Initialise a packet object to store the data to be sent
                Paket sendData = new Paket();
                sendData.UserName = this.name;
                sendData.Message = "";
                sendData.TypeMessage = DataIdentifier.LogIn;

                // Initialise socket
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Initialise server IP
                IPAddress serverIP = IPAddress.Parse(txtServerIP.Text.Trim());

                // Initialise the IPEndPoint for the server and use port 30000
                IPEndPoint server = new IPEndPoint(serverIP, 2000);

                // Initialise the EndPoint for the server
                epServer = (EndPoint)server;

                // Get packet as byte array
                byte[] data = sendData.GetDataStream();

                // Send data to server
                clientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                // Initialise data stream
                this.dataStream = new byte[1024];

                // Begin listening for broadcasts
                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }



        #region Send And Receive

        private void SendData(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send Data: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                // Receive all data
                this.clientSocket.EndReceive(ar);

                // Initialise a packet object to store the received data
                Paket receivedData = new Paket(this.dataStream);

                // Update display through a delegate
                if (receivedData.Message != null)
                    this.Invoke(this.displayMessageDelegate, new object[] { receivedData.Message });

                // Reset data stream
                this.dataStream = new byte[1024];

                // Continue listening for broadcasts
                clientSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show("Receive Data: " + ex.Message, "UDP Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion


        private void DisplayMessage(string messge)
        {
            rtxtConversation.Text += messge + Environment.NewLine;
        }
    }
}
