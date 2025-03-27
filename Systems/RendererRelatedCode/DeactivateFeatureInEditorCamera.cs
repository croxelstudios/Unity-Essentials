using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class DeactivateFeatureInEditorCamera : MonoBehaviour
{
    [SerializeField]
    ScriptableRendererFeature feature;
    [SerializeField]
    Camera[] extraCamsToDeactivateIn;

    bool deactivated;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
    }

    void BeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if ((feature != null) && feature.isActive)
        {
            if (cam.name == "SceneCamera")
            {
                feature.SetActive(false);
                deactivated = true;
            }
            else
                for (int i = 0; i < extraCamsToDeactivateIn.Length; i++)
                    if (cam == extraCamsToDeactivateIn[i])
                    {
                        feature.SetActive(false);
                        deactivated = true;
                    }
        }
    }

    void EndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (deactivated)
            feature.SetActive(true);
    }
}
