using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer {
    class Client {
        public static int DataBufferSize = 4096;
        public int Id;
        public TCP Tcp;
        
        public Client(int _clientId) {
            Id = _clientId;
            Tcp = new TCP(Id);
        }

        public class TCP {

            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private byte[] recieveBuffer;
            
            public TCP(int _id) { id = _id; }
               
            public void Connect(TcpClient _sockets) {
                socket = _sockets;
                socket.ReceiveBufferSize = DataBufferSize;
                socket.SendBufferSize = DataBufferSize;

                stream = socket.GetStream();
                recieveBuffer = new byte[DataBufferSize];

                stream.BeginRead(recieveBuffer, 0, DataBufferSize, RecieveCallback, null);

                // Send welcome packet
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

                    // Handle Data

                    stream.BeginRead(recieveBuffer, 0 , DataBufferSize, RecieveCallback, null);
                }
                catch (Exception _ex) {

                    Console.WriteLine($"Error recieving TCP data: {_ex}");
                    // Disconnect Client
                }
            }
           
        }

    }
}
