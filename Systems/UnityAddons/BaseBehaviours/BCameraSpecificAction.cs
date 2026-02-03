using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class BCameraSpecificAction : MonoBehaviour
{
    protected Camera currentCam;

    protected virtual void OnEnable()
    {
        currentCam = transform.GetComponent<Camera>();
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
    }

    protected virtual void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
    }

    void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == currentCam) StartRendering();
    }

    void EndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == currentCam) StopRendering();
    }

    protected virtual void StartRendering()
    {

    }

    protected virtual void StopRendering()
    {

    }
}
