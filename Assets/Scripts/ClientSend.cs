using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        Client.Instance.Tcp.SendData(_packet);
    }



    private static void SendUDPData(Packet _packet) {
        _packet.WriteLength();
        Client.Instance.Udp.SendData(_packet);
    }



    #region Packets

    public static void WelcomeRecieved() {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) {
            _packet.Write(Client.Instance.MyId);
            _packet.Write(UiManager.Instance.usernameFied.text);

            SendTCPData(_packet);
        };
    }

    public static void PlayerMovement(bool[] _inputs) {

        using(Packet _packet = new Packet((int)ClientPackets.playerMovement)) {

            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs) {
                _packet.Write(_input);
            }

            _packet.Write(GameManager.players[Client.Instance.MyId].transform.rotation);

            SendUDPData(_packet);
        }
    }

  

    #endregion
}
