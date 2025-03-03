using UnityEngine;
using UnityEngine.EventSystems;
//This script is used to refresh the list of available lobby
//This script is attached to a refresh button
//This script just makes a boolean of JoinLobbyUIHandler script true 
public class RefreshAvailableLobby : MonoBehaviour, IPointerClickHandler
{
    
    JoinLobbyUIHandler joinLobbyUIHandler;
    private void Start()
    {
        joinLobbyUIHandler = GetComponentInParent<JoinLobbyUIHandler>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        joinLobbyUIHandler.refresh = true;
    }
}
