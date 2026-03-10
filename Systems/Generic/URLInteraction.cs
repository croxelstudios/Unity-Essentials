using UnityEngine;

public class URLInteraction : MonoBehaviour
{
    [SerializeField]
    string url = "https://www.google.com/";

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
