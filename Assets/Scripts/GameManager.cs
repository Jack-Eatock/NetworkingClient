using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject PlayerPrefab;

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

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation) {
        GameObject _player;

        // Local user
        if (_id == Client.Instance.MyId) {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }

        // Other user
        else {
            _player = Instantiate(PlayerPrefab, _position, _rotation);
        }

        PlayerManager playerManager = _player.GetComponent<PlayerManager>();

        playerManager.id = _id;
        playerManager.username = _username;
        players.Add(_id, playerManager);
    }
}
