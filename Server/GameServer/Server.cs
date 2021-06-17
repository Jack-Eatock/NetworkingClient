using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer {
    class Server {

        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

        private static TcpListener tcpListener;

        public static void Start(int _maxPlayers, int _port)  {

            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server!");

            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started successfully on {Port}. ");
        }

        private static void TCPConnectCallback(IAsyncResult _result) {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint} ");

            // Iterate through each client, find the next empty slot and conenct to it.
            for (int i = 1; i <= MaxPlayers; i++) {
                if (Clients[i].Tcp.socket == null) {
                    Clients[i].Tcp.Connect(_client);
                    return;
                }
            }

            // If loop completes all iterations server must be full
            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect!");
        }

        private static void InitializeServerData() {
            for (int i = 1; i <= MaxPlayers; i++) {
                Clients.Add(i, new Client(i));
            }
        }
    }
}
