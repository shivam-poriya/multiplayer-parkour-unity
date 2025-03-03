using UnityEngine;
using TMPro;
//This script is used to Input userName for player
public class UserNameInputHandler : MonoBehaviour
{
    private TMP_InputField userNameInputField;
    private UserName userName;
    [SerializeField] GameObject lobbyBack;
    [SerializeField] GameObject InputFieldUI;

    public void OnSubmit()
    {
        userName = Resources.Load<UserName>("UserName");
        userNameInputField = GetComponent<TMP_InputField>();
        userName.Name = userNameInputField.text;
    }

    public void onDone()
    {
        userName = Resources.Load<UserName>("UserName");
        if (!userName.Name.Equals(""))
        {
            InputFieldUI.SetActive(false);
            lobbyBack.SetActive(true);
        }
    }


}


