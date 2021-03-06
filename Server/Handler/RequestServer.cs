using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using Collector;
using Server;
using Sql;
using ServerData;

namespace Handler
{
    class RequestServer
    {
        private static Socket ListenerSocket;
        private static List<ClientData> _Clients = new List<ClientData>();
        public static void run() {
            ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(Packet.IPV4Address()), 4242);
            ListenerSocket.Bind(ep);
            Thread Listentrd = new Thread(ListenThread);
            Listentrd.Start();
        }

        public static void Data_IN(object Socket) {
            Socket ClientSocket = (Socket)Socket;
            byte[] buffer;
            int readBytes;
            try {
                while (true) 
                {
                    buffer = new byte[ClientSocket.SendBufferSize];
                    readBytes = ClientSocket.Receive(buffer);
                    if (readBytes > 0) 
                    {
                        byte[] readBuffer = new byte[readBytes]; 
                        Array.Copy(buffer, readBuffer, readBytes);
                        dataManager(new Packet(readBuffer));
                    }
                }
            } catch (SocketException e) {
                Console.WriteLine(e);
            }
        }

        public static void dataManager(Packet p)
        {
            switch (p.PacketType)
            {
                case PacketType.chat:
                    foreach (ClientData c in _Clients)
                    {
                        c.ClientSocket.Send(p.toBytes());
                    }
                    break;
                case PacketType.NewClan:
                    Command.newClan(p.Gdata[0]);
                    break;
            }
        }

        private static void ListenThread() {
            while (true) 
            {
                ListenerSocket.Listen(0);
                _Clients.Add(new ClientData(ListenerSocket.Accept()));
            }
        }

    }
}