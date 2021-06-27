using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer {
    class Client {

        public static int DataBufferSize = 4096;
        public int Id;

        public Player player;

        public TCP Tcp;
        public UDP Udp;

        public Client(int _clientId) { 
            Id = _clientId; 
            Tcp = new TCP(Id); 
            Udp = new UDP(Id); 
        }

        public class TCP {

            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private Packet recievedData;
            private byte[] recieveBuffer;

            public TCP(int _id) { id = _id; }

            public void Connect(TcpClient _sockets) {
                socket = _sockets;
                socket.ReceiveBufferSize = DataBufferSize;
                socket.SendBufferSize = DataBufferSize;

                stream = socket.GetStream();


                recievedData = new Packet();
                recieveBuffer = new byte[DataBufferSize];

                stream.BeginRead(recieveBuffer, 0, DataBufferSize, RecieveCallback, null);

                // Send welcome packet
                ServerSend.Welcome(id, "Welcome to the Server!");
            }

            public void SendData(Packet _packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }

                }
                catch (Exception _ex) {

                    Console.WriteLine($"ERROR sending data to player {id} via TCP: {_ex}");
                }
            }


            private void RecieveCallback(IAsyncResult _result) {

                try {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0) {
                        // Disconnect
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(recieveBuffer, _data, _byteLength);

                    recievedData.Reset(HandleData(_data));

                    stream.BeginRead(recieveBuffer, 0, DataBufferSize, RecieveCallback, null);
                }

                catch (Exception _ex) {

                    Console.WriteLine($"Error recieving TCP data: {_ex}");
                    // Disconnect Client
                }
            }

            private bool HandleData(byte[] _data) {
                int _packetLength = 0;

                recievedData.SetBytes(_data);

                // The first bit of data in any msg is the length of data.
                // an int is 4 bytes, soo Anything more than 4 contains the int, soo contains data.
                if (recievedData.UnreadLength() >= 4) {

                    _packetLength = recievedData.ReadInt();

                    if (_packetLength <= 0) { return true; }

                }

                while (_packetLength > 0 && _packetLength <= recievedData.UnreadLength()) {

                    byte[] _packetBytes = recievedData.ReadBytes(_packetLength);

                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet _packet = new Packet(_packetBytes)) {

                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id ,_packet);
                        }

                    });

                    _packetLength = 0;

                    // If data still to be read.
                    if (recievedData.UnreadLength() >= 4) {

                        _packetLength = recievedData.ReadInt();

                        if (_packetLength <= 0) { return true; }

                    }
                }

                if (_packetLength <= 1) {
                    return true;
                }

                return false;
            }

        }

        public class UDP {

            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id) {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint) {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet) {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData) {

                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() => {

                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }


                });
            }
        }
         
        public void SendIntoGame(string _playerName) {
            player = new Player(Id, _playerName, new Vector3(0, 0, 0));

            foreach (Client _client in Server.Clients.Values) {

                if (_client.player != null) {

                    if (_client.Id != Id) {
                        ServerSend.SpawnPlayer(Id, _client.player);
                    }
                }
            }

            foreach (Client _client in Server.Clients.Values) {

                if (_client.player != null) {

                    ServerSend.SpawnPlayer(_client.Id, player);

                }

            }
        }
    }
}
