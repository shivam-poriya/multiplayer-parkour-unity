using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;

//This script is used to make player join a specific lobby
//This script is attached to the join button of each lobby
//This script also joins client to relay through relay join code
//And also calls the startclient() in network manager
public class LobbyJoinClick : MonoBehaviour, IPointerClickHandler
{
    StoreLobbyID lobbyEntry;
    string relayJoinCode;
    UserName userName;
    Lobby joinedLobby;
    JoinLobbyUIHandler joinLobbyUIHandler;
    public async void OnPointerClick(PointerEventData eventData)
    {
        lobbyEntry = GetComponentInParent<StoreLobbyID>();
        userName = Resources.Load<UserName>("UserName");
        joinLobbyUIHandler = GameObject.Find("Client").GetComponent<JoinLobbyUIHandler>();
        await JoinLobby();
    }

    async Task JoinLobby()
    {
        /* 
         * This function will connect the player with specific lobby 
         * First it will join the lobby with options(playerName)
         * then it will retrieve the relay join code from lobby's custom data field
         * At last it will call the joinRelay function
        */
        string lobbyID = lobbyEntry.lobbyID;
        string name = userName.Name;
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions()
            {
                Player = new Unity.Services.Lobbies.Models.Player
                {
                    Data = new System.Collections.Generic.Dictionary<string, PlayerDataObject> 
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name) }
                    }
                }
            };
            Lobby lobbyJoinedTemp = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID, options);
            joinedLobby = lobbyJoinedTemp;
            relayJoinCode = joinedLobby.Data["relayJoinCode"].Value;
            joinLobbyUIHandler.clientJoinedLobby = joinedLobby;
            joinLobbyUIHandler.hasJoinedLobby = true;
            await JoinRelay();
        }
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
    async Task JoinRelay()
    {
        /* 
         * This function will join relay with relayJoinCode 
         * It will set the relay server data to networkManager
         * At last call the startClient() method
        */
        try
        {
            JoinAllocation allocationRelay = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            RelayServerData relayServerData = new RelayServerData(allocationRelay, "udp");
            var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            transport.SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    
}