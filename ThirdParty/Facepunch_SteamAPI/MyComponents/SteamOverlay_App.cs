using Sirenix.OdinInspector;
using Steamworks;
using UnityEngine;

public class SteamOverlay_App : MonoBehaviour
{
    [SerializeField]
    uint appID = 2545220;
    [LabelText("Use UTM Tags")]
    [SerializeField]
    bool useUTMTags = false;
    [ShowIf("useUTMTags")]
    [SerializeField]
    string campaign = "ann";
    [ShowIf("useUTMTags")]
    [SerializeField]
    string medium = "ingame";
    [ShowIf("useUTMTags")]
    [SerializeField]
    string source = "g";

    public void OpenAppPage()
    {
        if (SteamClient.IsValid && SteamUtils.IsOverlayEnabled)
            SteamFriends.OpenStoreOverlay(appID);
        else
            Application.OpenURL("https://store.steampowered.com/app/" + appID + "/"
                + (useUTMTags ?
                "?utm_source=" + source +
                "&utm_medium=" + medium +
                "&utm_campaign=" + campaign
                : ""));
    }
}
