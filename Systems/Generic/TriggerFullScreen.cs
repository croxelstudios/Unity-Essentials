using UnityEngine;

public class TriggerFullScreen : MonoBehaviour
{
    void Awake()
    {
        if (!PlayerPrefs.HasKey("Fullscreen"))
        {
            PlayerPrefs.SetString("Fullscreen", "y");
            PlayerPrefs.Save();
        }

        if (!PlayerPrefs.HasKey("ResW"))
        {
            PlayerPrefs.SetInt("ResW", Screen.currentResolution.width);
            PlayerPrefs.Save();
        }

        if (!PlayerPrefs.HasKey("ResH"))
        {
            PlayerPrefs.SetInt("ResH", Screen.currentResolution.height);
            PlayerPrefs.Save();
        }
    }

    public void Trigger()
    {
        if (PlayerPrefs.GetString("Fullscreen", "y") == "y")
        {
            PlayerPrefs.SetString("Fullscreen", "n");
            Screen.SetResolution(PlayerPrefs.GetInt("ResW"), PlayerPrefs.GetInt("ResH"), false);
        }
        else
        {
            PlayerPrefs.SetString("Fullscreen", "y");
            PlayerPrefs.SetInt("ResW", Screen.width);
            PlayerPrefs.SetInt("ResH", Screen.height);
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        if (PlayerPrefs.GetString("Fullscreen", "y") == "n")
        {
            PlayerPrefs.SetInt("ResW", Screen.width);
            PlayerPrefs.SetInt("ResH", Screen.height);
        }
    }
}
