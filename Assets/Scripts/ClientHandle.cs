using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
 
    public static void Welcome (Packet _packet) {

        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log("Message Recieved from server:" + _msg);
        Client.Instance.MyId = _myId;

        ClientSend.WelcomeRecieved();

        Client.Instance.Udp.Connect(((IPEndPoint)Client.Instance.Tcp.sockets.Client.LocalEndPoint).Port);
    }
    

    public static void SpawnPlayer(Packet _packet) {

        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.Instance.SpawnPlayer(_id, _username, _position, _rotation);


    }

    public static void PlayerPosition(Packet _packet) {

        if (GameManager.players.Count == 0) { return; }

        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (Client.Instance.MyId == _id) {
            GameManager.players[_id].GetComponent<PlayerController>().UpdatePosFromServer(_position);
        }
        else {
            GameManager.players[_id].transform.position = _position; 
        }
      
    }

    
    public static void PlayerRotation(Packet _packet) {

        if (GameManager.players.Count == 0) { return; }

        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (Client.Instance.MyId == _id) {
            GameManager.players[_id].GetComponent<PlayerController>().UpdatePosFromServer(_rotation);
        }
        else {
            GameManager.players[_id].transform.rotation = _rotation; 
        }
    }

    public static void PlayerDisconnected(Packet _packet) {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }
}
