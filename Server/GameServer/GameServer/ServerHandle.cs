using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {
    class ServerHandle {

        public static void WelcomeRecieved(int _fromClient, Packet _packet) {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

             //Console.WriteLine($"{Server.Clients[_fromClient].Tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
             Console.WriteLine($"{Server.Clients[_fromClient].Tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient} with username {_username} .");

            if (_fromClient != _clientIdCheck) {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }

            // Send player to game.
        }

    }
}
