using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
//This script is used to maintain joined lobby client UI
public class JoinedLobbyClient : MonoBehaviour
{
    public Lobby joinedLobby;
    GameObject contentScrollView;
    GameObject clientPlayerEntry;
    UserName userName;
    TMP_Text lobbyName;
    float lobbyUpdatePoll = 5;
    int numberOfPlayersJoined = 0;

    private void Start()
    {
        userName = Resources.Load<UserName>("UserName");
        ChangeLobbyName();
        contentScrollView = GameObject.Find("Content2");
        clientPlayerEntry = Resources.Load<GameObject>("ClientPlayerEntry");
        printJoinedPlayer();
    }

    void ChangeLobbyName()
    {
        lobbyName = GameObject.Find("LobbyNameClientJoined").GetComponent<TMP_Text>();
        lobbyName.text = joinedLobby.Players[0].Data["PlayerName"].Value;
    }

    private async void Update()
    {
        await pollForLobbyUpdate();
    }

    void printJoinedPlayer()
    {
        /* 
         * prints all the joined player in a lobby in Joined Player List 
         * It also sets playerName
         * Stores playerId to attached script
        */
        deleteExistingPlayer();
        foreach (var joinedPlayer in joinedLobby.Players)
        {
            GameObject playerInstantiated = Instantiate(clientPlayerEntry, contentScrollView.transform);
            TMP_Text namePlayer = playerInstantiated.GetComponentInChildren<TMP_Text>();
            if (joinedPlayer.Id == AuthenticationService.Instance.PlayerId)
            {
                namePlayer.text = joinedPlayer.Data["PlayerName"].Value + " (You)";
            }
            else
            {
                namePlayer.text = joinedPlayer.Data["PlayerName"].Value;
            }
        }
    }
    void deleteExistingPlayer()
    {
        /* 
         * Deletes existing entry of player in Joined Player List 
        */
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in contentScrollView.transform)
        {
            children.Add(child.gameObject);
        }
        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }

    async Task pollForLobbyUpdate()
    {
        /* 
         * It updates lobby every 5 seconds to check if any new player is joined or anyone left 
         * If any new players is joined or any player left the lobby it immediately calls the printJoinedPlayer
        */
        lobbyUpdatePoll -= Time.deltaTime;
        if (lobbyUpdatePoll <= 0f)
        {
            lobbyUpdatePoll = 5f;
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            if (joinedLobby.Players.Count != numberOfPlayersJoined)
            {
                numberOfPlayersJoined = joinedLobby.Players.Count;
                printJoinedPlayer();
            }
        }
    }
}