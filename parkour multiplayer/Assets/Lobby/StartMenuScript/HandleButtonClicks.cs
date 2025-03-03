using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleButtonClicks : MonoBehaviour
{
    [SerializeField] GameObject CreateLobbyButton;
    [SerializeField] GameObject JoinLobbyButton;
    [SerializeField] GameObject CreateLobbyInterface;
    [SerializeField] GameObject JoinLobbyInterface;
    public void CreateLobby()
    {
        CreateLobbyButton.SetActive(false);
        JoinLobbyButton.SetActive(false);
        CreateLobbyInterface.SetActive(true);
    }

    public void JoinLobby()
    {
        CreateLobbyButton.SetActive(false);
        JoinLobbyButton.SetActive(false);
        JoinLobbyInterface.SetActive(true);
    }
}

