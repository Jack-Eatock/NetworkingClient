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
    }

    public void ConnectToServer() {
        Tcp.Connect();
    }

    public class TCP {
        public TcpClient sockets;

        private NetworkStream stream;
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

            if (sockets.Connected) {
                return;
            }

            stream = sockets.GetStream();

            stream.BeginRead(recieveBuffer, 0, DataBufferSize, RecieveCallback, null);
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
            catch (Exception) {

                // Disconnect
            }
        }
    }
}