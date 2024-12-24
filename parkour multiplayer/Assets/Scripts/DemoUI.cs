using Unity.Netcode;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    
    public void startHost()
    {
        Destroy(GameObject.Find("Camera"));
        NetworkManager.Singleton.StartHost();
        Destroy(GameObject.Find("Canvas"));
    }
    public void startClient()
    {
        Destroy(GameObject.Find("Camera"));
        NetworkManager.Singleton.StartClient();
        Destroy(GameObject.Find("Canvas"));
    }
}
