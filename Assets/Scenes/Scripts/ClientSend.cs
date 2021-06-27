using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        Client.Instance.Tcp.SendData(_packet);
    }

    #region Packets

    public static void WelcomeRecieved() {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) {
            _packet.Write(Client.Instance.MyId);
            _packet.Write(UiManager.Instance.usernameFied.text);

            SendTCPData(_packet);
        };
    }

    #endregion
}
