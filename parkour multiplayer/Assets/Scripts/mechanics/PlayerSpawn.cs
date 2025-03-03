using Unity.Netcode;
using UnityEngine;


//This script is used to manually spawn the player when the scene is changed
public class PlayerSpawn : NetworkBehaviour
{
    void Start()
    {
        /* 
         * The start function just spawns all of the connected clients 
         * It iterates over through the ConnectedClientsIds and spawns a player for each entry
        */
        GameObject playerPrefab = Resources.Load<GameObject>("brycePrefab");

        if (IsServer)
        {
            foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
            {
                GameObject playerInstance = Instantiate(playerPrefab);
                NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                networkObject.SpawnWithOwnership(clientID);
            }
        }
    }

}
