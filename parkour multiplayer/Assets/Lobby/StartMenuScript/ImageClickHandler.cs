using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//This script will take input of start scene
//Will disable floating text (Click anywhere to start)
//Will disable raycast target on background image
public class ImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject lobbyBack;
    UserName userName;
    [SerializeField] GameObject inputFieldUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        DisableText();
        DisableRaycastOnImage();
        userName = Resources.Load<UserName>("UserName");
        if (userName.Name.Equals(""))
            inputUserName();
        else
            EnableFirstLobbyInterface();

    }

    
    void inputUserName()
    {
        inputFieldUI.SetActive(true);        
    }

    void DisableText()
    {
        /*
        This function just disables the floating text which says "Click Anywhere to Start"
        */
        try
        {
            GameObject floatingText = GameObject.Find("Floating Text");
            floatingText.SetActive(false);
        }
        catch(Exception e) 
        {
            Debug.LogError(e);
        }
    }

    void DisableRaycastOnImage()
    {
        /*
          * This function disables the raycast target of the image
        */
        try
        {
            Image startImage = GameObject.Find("Start Image").GetComponent<Image>();
            startImage.raycastTarget = false;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void EnableFirstLobbyInterface()
    {
        try
        {
            lobbyBack.SetActive(true);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}