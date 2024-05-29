using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CanvasLookForCamera : MonoBehaviour
{
    Canvas canv;

    [SerializeField]
    [TagSelector]
    string cameraTag = "MainCamera";
    [SerializeField]
    UpdateMode updateMode = UpdateMode.DontUpdate;

    enum UpdateMode { DontUpdate, Update, UpdateWhenNull }

    void OnEnable()
    {
        if (canv == null) canv = GetComponent<Canvas>();
        if (updateMode != UpdateMode.Update) SetCamera();
    }

    void Update()
    {
        if ((updateMode == UpdateMode.Update) ||
            ((updateMode == UpdateMode.UpdateWhenNull) &&
            (canv.worldCamera == null)))
            SetCamera();
    }

    void SetCamera()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(cameraTag);
        Camera cam = null;
        foreach (GameObject obj in objs)
        {
            cam = obj.GetComponent<Camera>();
            if (cam != null)
            {
                if (cam.enabled) break;
                else cam = null;
            }
        }

        if (cam != null)
        {
            canv.renderMode = RenderMode.ScreenSpaceCamera;
            canv.worldCamera = cam;
        }
        else canv.renderMode = RenderMode.ScreenSpaceOverlay;
    }
}
