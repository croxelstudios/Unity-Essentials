using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CanvasCamera_ByTag : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    [OnValueChanged("SetCamera")]
    string cameraTag = "MainCamera";
    //TO DO: Extra tags?

    Canvas canv;

    void OnEnable()
    {
        SetCamera();
    }

    void SetCamera()
    {
        if (canv == null)
            canv = GetComponent<Canvas>();

        Camera cam = FindWithTag.OnlyEnabled<Camera>(cameraTag);

        if (cam != null)
        {
            //canv.renderMode = RenderMode.ScreenSpaceCamera;
            canv.worldCamera = cam;
        }
        //else canv.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    public void UpdateTag(string tag)
    {
        cameraTag = tag;
        SetCamera();
    }
}
