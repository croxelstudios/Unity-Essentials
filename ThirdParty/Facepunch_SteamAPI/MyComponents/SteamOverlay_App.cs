using Steamworks;
using UnityEngine;

public class SteamOverlay_App : MonoBehaviour
{
    [SerializeField]
    uint appID = 2545220;

    public void OpenAppPage()
    {
        if (SteamClient.IsValid && SteamUtils.IsOverlayEnabled)
            SteamFriends.OpenStoreOverlay(appID);
        else
            Application.OpenURL("https://store.steampowered.com/app/" + appID + "/");
    }
}
