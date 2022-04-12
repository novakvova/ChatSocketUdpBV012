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
        List<Client> clients = new List<Client>();

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
            while(true)
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
                Paket clientData = new Paket(dataStream);
                allDone.Set();

            }
            catch (Exception ex)
            {

                Console.WriteLine("Problem read data");
            }
        }
    }
}
