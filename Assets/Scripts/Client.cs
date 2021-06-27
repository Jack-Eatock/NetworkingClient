using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour {
    public static Client Instance;
    public static int DataBufferSize = 4069;

    public string Ip = "127.0.0.1";
    public int Port = 26950;
    public int MyId = 0;

    public TCP Tcp;
    public UDP Udp;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake() {

        // Ensures only one. Signleton method.
        if (Instance == null) {
            Instance = this;
        }
        else if (Instance != this) {
            Debug.Log("Instance already created, destroying object!");
            Destroy(this);
        }
    }

    private void Start() {
        Tcp = new TCP();
        Udp = new UDP();
    }

    public void ConnectToServer() {
        InitializeClientData();

        Tcp.Connect();
    }

    public class TCP {
        public TcpClient sockets;

        private NetworkStream stream;

        private Packet RecievedData;

        private byte[] recieveBuffer;

        public void Connect() {

            sockets = new TcpClient {
                ReceiveBufferSize = DataBufferSize,
                SendBufferSize = DataBufferSize
            };

            recieveBuffer = new byte[DataBufferSize];
            sockets.BeginConnect(Instance.Ip, Instance.Port, ConnectCallback, sockets);

        }

        private void ConnectCallback(IAsyncResult _result) {
            sockets.EndConnect(_result);

            if (!sockets.Connected) {
                return;
            }

            stream = sockets.GetStream();

            RecievedData = new Packet();

            stream.BeginRead(recieveBuffer, 0, DataBufferSize, RecieveCallback, null);
        }

        public void SendData(Packet _packet) {
            try {
                if (sockets != null) {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex) {

                Debug.Log($"Error sending data to server via TCP: {_ex} ");
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

                // Handle Data
                RecievedData.Reset(HandleData(_data));
                stream.BeginRead(recieveBuffer, 0, DataBufferSize, RecieveCallback, null);
            }
            catch (Exception) {

                // Disconnect
            }
        }


        private bool HandleData(byte[] _data) {
            int _packetLength = 0;

            RecievedData.SetBytes(_data);

            // The first bit of data in any msg is the length of data.
            // an int is 4 bytes, soo Anything more than 4 contains the int, soo contains data.
            if (RecievedData.UnreadLength() >= 4) {

                _packetLength = RecievedData.ReadInt();

                if (_packetLength <= 0) { return true; }

            }

            while (_packetLength > 0 && _packetLength <= RecievedData.UnreadLength()) {

                byte[] _packetBytes = RecievedData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {

                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }

                });

                _packetLength = 0;

                // If data still to be read.
                if (RecievedData.UnreadLength() >= 4) {

                    _packetLength = RecievedData.ReadInt();

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

        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() {
            endPoint = new IPEndPoint(IPAddress.Parse(Instance.Ip), Instance.Port);
        }

        public void Connect(int _localPort) {
            socket = new UdpClient(_localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(RecieveCallback, null);

            // Initial Packet sent to open port and prepare for flow
            using (Packet _packet = new Packet()) {
                SendData(_packet);
            }
        }


        public void SendData(Packet _packet) {
            try {
                _packet.InsertInt(Instance.MyId);
                if (socket != null) {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }

            }
            catch (Exception _ex) {

                Debug.Log($"Error sending data to server via UDP: {_ex} ");
            }
        }

        private void RecieveCallback(IAsyncResult _result) {
            try {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(RecieveCallback, null);

                if (_data.Length < 4) {

                    // Disconnect?
                    return;
                }

                HandleData(_data);
            }
            catch (Exception) {

                // Disconnect?
            }
        }

        private void HandleData(byte[] _data) {

            // Remove the length of the packet from the begining soo we just have data.
            using (Packet _packet = new Packet(_data)) {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            };

            // Now that the length has been removed from the begining, process the data within the packet
            // first the id.
            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_data)) {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }


    }


    private void InitializeClientData() {
        packetHandlers = new Dictionary<int, PacketHandler>() {
            {(int) ServerPackets.welcome, ClientHandle.Welcome},
            {(int) ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            {(int) ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int) ServerPackets.playerRotation, ClientHandle.PlayerRotation},
        };

        Debug.Log("Initizalized Client Data");
    }

}
