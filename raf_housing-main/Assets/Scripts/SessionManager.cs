using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string AuthToken { get; private set; }
    private string username = null;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetAuthToken(string token)
    {
        AuthToken = token;
    }

    public void SetUsername(string name)
    {
        username = name;
    }

    public string GetUsername()
    {
        return username;
    }
}
