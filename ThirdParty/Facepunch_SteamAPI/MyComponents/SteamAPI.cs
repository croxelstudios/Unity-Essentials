using Steamworks;
using UnityEngine;

public class SteamAPI : MonoBehaviour
{
    [SerializeField]
    uint appID = 0;

    private void Awake()
    {
        try { SteamClient.Init(appID); }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log("Couldn't open Steam -- " + e);
#endif
            // Something went wrong - it's one of these:
            //
            //     Steam is closed?
            //     Can't find steam_api dll?
            //     Don't have permission to play app?
        }
    }

    private void OnDestroy()
    {
        if (SteamClient.IsValid)
            SteamClient.Shutdown();
    }
}
