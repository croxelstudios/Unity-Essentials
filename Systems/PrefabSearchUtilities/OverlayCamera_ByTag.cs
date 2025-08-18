using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class OverlayCamera_ByTag : BByTag<Camera>
{
    [SerializeField]
    [TagSelector]
    string overlayTag = "MainCamera";

    Camera thisCamera;

    void Reset()
    {
        targetTag = "MainCamera";
        extraTags = null;
        updateMode = ByTagUpdateMode.DontUpdate;
    }

    protected override void InitIfNull()
    {
        if (thisCamera == null)
            thisCamera = GetComponent<Camera>();
    }

    protected override void SetSource(Camera target)
    {
        UniversalAdditionalCameraData overlayData = target.GetUniversalAdditionalCameraData();

        if (overlayData.renderType == CameraRenderType.Overlay)
        {
            UniversalAdditionalCameraData cameraData = thisCamera.GetUniversalAdditionalCameraData();

            for (int i = cameraData.cameraStack.Count - 1; i >= 0; i--)
            {
                if (cameraData.cameraStack[i] == null)
                    cameraData.cameraStack.RemoveAt(i);
            }

            if (!cameraData.cameraStack.Contains(target))
                cameraData.cameraStack.Add(target);
        }
    }
}
