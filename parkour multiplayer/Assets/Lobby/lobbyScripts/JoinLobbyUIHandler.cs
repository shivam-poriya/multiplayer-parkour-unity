using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

//This script just handle the UI of joining the lobby
//It does not connects the user to a specific lobby
public class JoinLobbyUIHandler : MonoBehaviour
{
    QueryResponse lobbiesAvailable;
    GameObject contentScrollView;
    GameObject lobbyEntryPrefab;
    StoreLobbyID infoScript;
    [HideInInspector]
    public bool refresh=false;
    [HideInInspector] 
    public bool hasJoinedLobby = false;
    [HideInInspector]
    public Lobby clientJoinedLobby;
    GameObject listLobbies;
    GameObject joinedLobby;
    JoinedLobbyClient joinedLobbyClient;

    private async void Start()
    {
        await SignIn();
        contentScrollView = GameObject.Find("Content1");
        lobbyEntryPrefab = Resources.Load<GameObject>("LobbyEntry");
        listLobbies = gameObject.transform.Find("lobbyList").gameObject;
        joinedLobby = gameObject.transform.Find("JoinedLobby").gameObject;
        await AvailableLobbies();
    }
    async Task SignIn()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void Update()
    {
        if(refresh) 
        {
            refresh = false;
            await AvailableLobbies(); 
        }
        if(hasJoinedLobby)
        {
            hasJoinedLobby = false;
            changeUI();
        }
    }

    void DeleteExistingLobby()
    {
        /* 
         * This function deletes the existing record of lobby to print to prevent the duplication entry when refreshed the lobby list  
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


    void PrintAvailableLobby()
    {
        /*
         * This function prints all the available lobbies
         * First it calls the delete function
         * Then for each lobby it creates an instance in a scroll view
         * And sets the name of lobby
         * Number of players joined in that lobby
         * And also sets the Lobby Id to the join button
        */
        DeleteExistingLobby();
        foreach(var availableLobby in lobbiesAvailable.Results)
        {
            GameObject lobbyInstantiated = Instantiate(lobbyEntryPrefab,contentScrollView.transform);
            TMP_Text lobbyName = lobbyInstantiated.transform.Find("Name").GetComponent<TMP_Text>();
            TMP_Text joinedPlayers = lobbyInstantiated.transform.Find("playersJoined").GetComponent<TMP_Text>();
            infoScript = lobbyInstantiated.GetComponent<StoreLobbyID>();
            infoScript.lobbyID = availableLobby.Id;
            lobbyName.text = availableLobby.Name;
            joinedPlayers.text = availableLobby.Players.Count+"/"+availableLobby.MaxPlayers;
        }
    }

    async Task AvailableLobbies()
    {
        /* 
         * This function gets all of the available lobbies 
         * And stores it in a class field
         * Calls printAvailableLobby to print the lobbies
        */
        try 
        { 
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            lobbiesAvailable = queryResponse;
            PrintAvailableLobby();
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    void changeUI()
    {
        /* 
         * This function will simply disable the available lobby ui and will enable the joined lobby client ui 
         * It also sets reference of joined lobby
        */
        listLobbies.SetActive(false);
        joinedLobby.SetActive(true);
        joinedLobbyClient = joinedLobby.GetComponent<JoinedLobbyClient>();
        joinedLobbyClient.joinedLobby = clientJoinedLobby;
    }
}
