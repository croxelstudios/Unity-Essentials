using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CameraExtension_IsObjectInFrustrum
{
    static Camera[] cameras;
    static Dictionary<Camera, Plane[]> camFrustrumPlanes;
    static int frame;

    public static bool IsVisibleBySceneCameras(this Renderer renderer)
    {
        if (!renderer.isVisible)
            return false;

        int count = Camera.allCamerasCount;
        if ((cameras == null) || (cameras.Length < count))
            cameras = new Camera[count];

        Camera.GetAllCameras(cameras);

        if (!cameras.IsNullOrEmpty())
            foreach (Camera cam in cameras)
                if ((cam != null) && cam.enabled &&
                    ((LayerMask)cam.cullingMask).ContainsLayer(renderer.gameObject.layer) &&
                    cam.IsObjectInFrustrum(renderer)) return true;

#if UNITY_EDITOR
        if (SceneView.sceneViews != null)
            foreach (SceneView view in SceneView.sceneViews)
                if (view.camera.IsObjectInFrustrum(renderer)) return true;
#endif

        return false;
    }

    public static bool IsObjectInFrustrum(this Camera cam, Renderer renderer)
    {
        return GeometryUtility.TestPlanesAABB(GetFrustrumPlanes(cam), renderer.bounds);
    }

    public static bool IsObjectInFrustrum(this Camera[] cams, Renderer renderer)
    {
        foreach (Camera cam in cams) if (cam.IsObjectInFrustrum(renderer)) return true;
        return false;
    }

    public static bool IsObjectInFrustrum(this Camera cam, Bounds bounds)
    {
        return GeometryUtility.TestPlanesAABB(GetFrustrumPlanes(cam), bounds);
    }

    public static bool IsObjectInFrustrum(this Camera[] cams, Bounds bounds)
    {
        foreach (Camera cam in cams) if (cam.IsObjectInFrustrum(bounds)) return true;
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
            camFrustrumPlanes = camFrustrumPlanes.ClearOrCreate();
            camFrustrumPlanes.Set(cam, GeometryUtility.CalculateFrustumPlanes(cam));
        }

        return camFrustrumPlanes[cam];
    }
}
