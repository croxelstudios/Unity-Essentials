using UnityEngine;

public static class CameraExtension_IsObjectInFrustrum
{
    public static bool IsObjectInFrustrum(this Camera cam, Renderer renderer)
    {
        //TO DO: Caused massive lag in Serfs Galore
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), renderer.bounds);
    }

    public static bool IsObjectInFrustrum(this Camera[] cams, Renderer renderer)
    {
        foreach (Camera cam in cams) if (cam.IsObjectInFrustrum(renderer)) return true;
        return false;
    }
}
