using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CameraExtension_IsObjectInFrustrum
{
    static Camera[] cameras;
    static Dictionary<Camera, Plane[]> camFrustrumPlanes;
    static int frame;

    public static bool IsVisibleBySceneCameras(this Renderer renderer, GameObject cachedGameObject, bool skipIsVisible = false)
    {
        return renderer.IsVisibleBySceneCameras(-1, cachedGameObject, skipIsVisible);
    }

    public static bool IsVisibleBySceneCameras(this Renderer renderer, int cachedLayer, bool skipIsVisible = false)
    {
        return renderer.IsVisibleBySceneCameras(-1, cachedLayer, skipIsVisible);
    }

    public static bool IsVisibleBySceneCameras(this Renderer renderer,
        float maxFar, GameObject cachedGameObject, bool skipIsVisible = false)
    {
        return renderer.IsVisibleBySceneCameras(maxFar, (cachedGameObject != null) ? cachedGameObject.layer : -1, skipIsVisible);
    }

    public static bool IsVisibleBySceneCameras(this Renderer renderer, float maxFar = -1, int cachedLayer = -1, bool skipIsVisible = false)
    {
        if (!(skipIsVisible || renderer.isVisible))
            return false;

        int count = Camera.allCamerasCount;
        if ((cameras == null) || (cameras.Length < count))
            cameras = new Camera[count];

        Camera.GetAllCameras(cameras);

        if (!cameras.IsNullOrEmpty())
            foreach (Camera cam in cameras)
                if ((cam != null) && cam.enabled &&
                    ((LayerMask)cam.cullingMask).ContainsLayer(
                        (cachedLayer < 0) ? renderer.gameObject.layer : cachedLayer) &&
                    cam.IsObjectInFrustrum(renderer, maxFar))
                    return true;

#if UNITY_EDITOR
        if ((!Application.isPlaying) && (SceneView.sceneViews != null))
            foreach (SceneView view in SceneView.sceneViews)
                if (view.camera.IsObjectInFrustrum(renderer, maxFar))
                    return true;
#endif

        return false;
    }

    public static bool IsObjectInFrustrum(this Camera cam, Renderer renderer, float maxFar = -1)
    {
        return cam.IsObjectInFrustrum(renderer.bounds, maxFar);
    }

    public static bool IsObjectInFrustrum(this Camera[] cams, Renderer renderer, float maxFar = -1)
    {
        foreach (Camera cam in cams) if (cam.IsObjectInFrustrum(renderer, maxFar)) return true;
        return false;
    }

    public static bool IsObjectInFrustrum(this Camera cam, Bounds bounds, float maxFar = -1)
    {
        Plane[] planes = cam.GetFrustrumPlanes();
        if (maxFar > 0)
        {
            float dif = cam.farClipPlane;
            if (dif > maxFar)
                planes[5].Translate(-planes[5].normal * (dif - maxFar));
        }
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    public static bool IsObjectInFrustrum(this Camera[] cams, Bounds bounds, float maxFar = -1)
    {
        foreach (Camera cam in cams) if (cam.IsObjectInFrustrum(bounds, maxFar)) return true;
        return false;
    }

    public static Plane[] GetFrustrumPlanes(this Camera cam)
    {
        int currentFrame =
#if UNITY_EDITOR
            (!Application.isPlaying) ? EditorTime.frameCount :
#endif
        Time.frameCount;

        if ((!camFrustrumPlanes.NotNullContainsKey(cam)) || (currentFrame != frame))
        {
            if (camFrustrumPlanes == null)
                camFrustrumPlanes = new Dictionary<Camera, Plane[]>();
            else camFrustrumPlanes = camFrustrumPlanes.ClearNulls();
            camFrustrumPlanes.Set(cam, GeometryUtility.CalculateFrustumPlanes(cam));
            frame = currentFrame;
        }

        return (Plane[])camFrustrumPlanes[cam].Clone();
    }
}
