using ChatLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerChatConsole
{
    class Program
    {
        

        private struct Client
        {
            public EndPoint endPoint;
            public string name;
        }
        private static List<Client> listClients = new List<Client>();

        // Server socket
        private static Socket serverSocket;

        // Data stream
        private static byte[] dataStream = new byte[1024];
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            //Console.WriteLine(Encoding.UTF8.GetBytes("vvПривіт").Length);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(ip, 2000);
            serverSocket.Bind(endPoint);

            Console.WriteLine("Server Start --------");
            while (true)
            {
                allDone.Reset();
                IPEndPoint client = new IPEndPoint(ip, 0);
                EndPoint epSender = (EndPoint)client;
                serverSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epSender,
                    new AsyncCallback(ReceiveData), epSender);
                allDone.WaitOne();
            }

            //Console.ReadKey();
        }

        private static void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                allDone.Set();
                Paket clientData = new Paket(dataStream);
                //дані що відправляє серввер усім клієнтам
                Paket sendData = new Paket();
                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;
                // Receive all data
                serverSocket.EndReceiveFrom(asyncResult, ref epSender);
                // Start populating the packet to be sent
                sendData.TypeMessage = clientData.TypeMessage;
                sendData.UserName = clientData.UserName;

                switch (clientData.TypeMessage)
                {
                    case DataIdentifier.Message:
                        sendData.Message = string.Format("{0}: {1}", clientData.UserName, clientData.Message);
                        break;
                    case DataIdentifier.LogIn:
                        // Populate client object
                        Client client = new Client();
                        client.endPoint = epSender;
                        client.name = clientData.UserName;

                        // Add client to list
                        listClients.Add(client);

                        sendData.Message = string.Format("-- {0} користувач зайшов в чат --", clientData.UserName);
                        break;

                    case DataIdentifier.LogOut:
                        // Remove current client from list
                        foreach (Client c in listClients)
                        {
                            if (c.endPoint.Equals(epSender))
                            {
                                listClients.Remove(c);
                                break;
                            }
                        }
                        sendData.Message = string.Format("-- {0} користувач покинув чат --", clientData.UserName);
                        break;
                }

                //Отримує масив байт, який будемо розсилати
                byte[] data = sendData.GetDataStream();

                foreach (Client client in listClients)
                {
                    if (client.endPoint != epSender || sendData.TypeMessage != DataIdentifier.LogIn)
                    {
                        // Broadcast to all logged on users
                        serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(SendData), client.endPoint);
                    }
                }

                // Listen for more connections again...

                //serverSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                Console.WriteLine(sendData.Message);



            }
            catch (Exception ex)
            {

                Console.WriteLine("Problem read data");
            }
        }

        public static void SendData(IAsyncResult asyncResult)
        {
            try
            {
                serverSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendData Error: " + ex.Message);
            }
        }
    }
}
