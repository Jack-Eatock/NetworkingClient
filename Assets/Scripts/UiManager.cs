using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    public GameObject startMenu;
    public InputField usernameFied;

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

    public void ConnectToServer() {
        startMenu.SetActive(false);
        usernameFied.interactable = false;
        Client.Instance.ConnectToServer();
    }
}
