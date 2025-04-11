using UnityEngine;
using TMPro;

public class SetUsername : MonoBehaviour
{
    public TMP_Text usernameText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            string username = SessionManager.Instance.GetUsername();
            usernameText.text = $"Welcome, {username}!";
        }
        catch 
        {

            usernameText.text = "Not logged in";
        }

    }

}
