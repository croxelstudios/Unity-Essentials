using UnityEngine;
using UnityEngine.SceneManagement;

public static class CameraExtension_IsCameraInScene
{
    public static bool IsCameraInScene(this Camera cam, Scene scene, bool includeSceneCamera = true)
    {
        if (cam.name == "SceneCamera")
        {
            if (includeSceneCamera) return true;
            else return false;
        }
        else
        {
            if (cam.gameObject.scene == scene) return true;
            else return false;
        }
    }
}
