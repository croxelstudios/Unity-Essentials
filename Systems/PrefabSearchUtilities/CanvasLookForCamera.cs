using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CanvasLookForCamera : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    [OnValueChanged("SetCamera")]
    string cameraTag = "MainCamera";

    Canvas canv;

    void OnEnable()
    {
        if (canv == null)
            canv = GetComponent<Canvas>();
        SetCamera();
    }

    void SetCamera()
    {
        Camera cam = cameraTag.FindComponentWithTag<Camera>();

        if (cam != null)
        {
            //canv.renderMode = RenderMode.ScreenSpaceCamera;
            canv.worldCamera = cam;
        }
        //else canv.renderMode = RenderMode.ScreenSpaceOverlay;
    }
}
