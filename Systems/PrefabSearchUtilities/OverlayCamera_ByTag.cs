using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class OverlayCamera_ByTag : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    [OnValueChanged("SetOverlay")]
    string overlayTag = "MainCamera";
    //TO DO: Extra tags?

    Camera thisCamera;

    void OnEnable()
    {
        if (thisCamera == null)
            thisCamera = GetComponent<Camera>();
        SetOverlay();
    }

    void SetOverlay()
    {
        Camera cam = FindWithTag.OnlyEnabled<Camera>(overlayTag);

        if (cam != null)
        {
            UniversalAdditionalCameraData overlayData =
                cam.GetUniversalAdditionalCameraData();
            if (overlayData.renderType == CameraRenderType.Overlay)
            {
                UniversalAdditionalCameraData cameraData =
                    thisCamera.GetUniversalAdditionalCameraData();
                if (!cameraData.cameraStack.Contains(cam))
                    cameraData.cameraStack.Add(cam);
            }
        }
    }
}
