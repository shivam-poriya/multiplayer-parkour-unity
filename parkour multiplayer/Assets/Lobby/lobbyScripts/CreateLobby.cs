using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

//This script handles the UI for creatingLobby
//It also implements actual lobby creation and managing etc
public class CreateLobby : MonoBehaviour
{
    float heartBeatTime;
    UserName userName;
    TMP_Text lobbyName;
    Allocation allocationRelay;
    Lobby hostLobby;
    GameObject contentScrollView;
    GameObject hostPlayerEntry;
    float lobbyUpdatePoll = 5;
    int numberOfPlayersJoined=0;
    StorePlayerId storePlayerId;
    private async void Start()
    {
        ChangeLobbyName();
        await SignIn();
        await HostLobby();
        contentScrollView = GameObject.Find("Content3");
        hostPlayerEntry = Resources.Load<GameObject>("HostPlayerEntry");
        printJoinedPlayer();
    }

    private async void Update()
    {
        sendHeartBeat();
        await pollForLobbyUpdate();
    }

    void ChangeLobbyName()
    {
        userName = Resources.Load<UserName>("UserName");
        lobbyName = GameObject.Find("Lobby Name").GetComponent<TMP_Text>();
        lobbyName.text = userName.Name;
    }

    async Task SignIn()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    
    async Task HostLobby()
    {
        /* 
         * This function starts a lobby with options
         * In lobby's Data field it stores relayJoinCode
         * In lobby's player data field it stores user name of player
        */
        string name = userName.Name;
        try
        {
            string relayJoinCode = await CreateRelay();
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Player = new Unity.Services.Lobbies.Models.Player()
                {
                    Data = new Dictionary<string,PlayerDataObject>()
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name) }
                    }
                },
                Data = new Dictionary<string,DataObject>()
                {
                    { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };
            hostLobby = await LobbyService.Instance.CreateLobbyAsync(name, 4, options);
            numberOfPlayersJoined = hostLobby.Players.Count;
            //setting the relay server data to the network before calling start host
            startHostNet();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    void startHostNet()
    {
        /* 
         * This function calls the startHost method in network manager
         * It first sets the created relay data to relay server
        */
        RelayServerData relayServerData = new RelayServerData(allocationRelay, "udp");
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        transport.SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
    }

    private async void sendHeartBeat()
    {
        /*
         * This function sends constant heartbeat to the server to keep the lobby visible after 30 seconds of its creation
         * It sends a heart beat ping signal once every 15 seconds
        */
        if (hostLobby != null)
        {
            heartBeatTime -= Time.deltaTime;
            if (heartBeatTime <= 0f)
            {
                heartBeatTime = 15f;
                await Lobbies.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    async Task<string> CreateRelay()
    {
        /*
         * It creates a relay and returns a relay join code
         * It also stores the Allocation reference to a class field
        */
        try
        {
            Allocation relayAllocation = await RelayService.Instance.CreateAllocationAsync(4);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(relayAllocation.AllocationId);
            Debug.Log("Relay Created");
            allocationRelay = relayAllocation;
            return relayJoinCode;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
        }
        return null;
    }

    async Task pollForLobbyUpdate()
    {
        /* 
         * It updates lobby every 5 seconds to check if any new player is joined or anyone left 
         * If any new players is joined or any player left the lobby it immediately calls the printJoinedPlayer
        */
        lobbyUpdatePoll -= Time.deltaTime;
        if(lobbyUpdatePoll<=0f)
        {
            lobbyUpdatePoll = 5f;
            hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
            if(hostLobby.Players.Count!=numberOfPlayersJoined)
            {
                numberOfPlayersJoined = hostLobby.Players.Count;
                printJoinedPlayer();
            }
        }
    }

    void printJoinedPlayer()
    {
        /* 
         * prints all the joined player in a lobby in Joined Player List 
         * It also sets playerName
         * Stores playerId to attached script
        */
        deleteExistingPlayer();
        foreach(var joinedPlayer in hostLobby.Players)
        {
            GameObject playerInstantiated = Instantiate(hostPlayerEntry,contentScrollView.transform);
            TMP_Text namePlayer = playerInstantiated.GetComponentInChildren<TMP_Text>();
            if(joinedPlayer.Id == AuthenticationService.Instance.PlayerId)
            {
                GameObject kickImage = playerInstantiated.transform.Find("Kick").gameObject;
                kickImage.SetActive(false);
                namePlayer.text = joinedPlayer.Data["PlayerName"].Value + " (You)";
            }
            else
            {
                namePlayer.text = joinedPlayer.Data["PlayerName"].Value;
            }
            storePlayerId = playerInstantiated.GetComponent<StorePlayerId>();
            storePlayerId.playerId = joinedPlayer.Id;
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
}