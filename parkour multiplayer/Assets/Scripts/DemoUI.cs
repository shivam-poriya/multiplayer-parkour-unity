using Unity.Netcode;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    
    public void startHost()
    {
        Destroy(GameObject.Find("Canvas"));
        NetworkManager.Singleton.StartHost();
    }
    public void startClient()
    {
        Destroy(GameObject.Find("Canvas"));
        NetworkManager.Singleton.StartClient();
    }
}
