using UnityEngine;

public class RenderersVisible : DXMonoBehaviour
{
    [SerializeField]
    ObjectRef<Camera> cam = new ObjectRef<Camera>("Camera", "MainCamera");
    [SerializeField]
    DXEvent becameVisible = null;
    [SerializeField]
    DXEvent becameInvisible = null;

    Renderer[] renderers;
    CullingGroup cullingGroup;
    Vector3 pos;
    Vector3 scale;
    BoundingSphere sphere;
    bool prevVisible;

    void OnEnable()
    {
        pos = transform.position;
        scale = transform.lossyScale;
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        Camera c = cam; 
        if (c != null)
        {
            cullingGroup = new CullingGroup();
            Bounds bounds = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                bounds.Encapsulate(rends[i].bounds);
            sphere = bounds.BoundingSphere();
            cullingGroup.SetBoundingSphereCount(1);
            cullingGroup.SetBoundingSpheres(new BoundingSphere[] { sphere });
            cullingGroup.onStateChanged += OnStateChanged;
            cullingGroup.targetCamera = c;
        }
        else renderers = rends;
    }

    void FixedUpdate()
    {
        if (cullingGroup != null)
        {
            if (transform.hasChanged)
            {
                if ((transform.position != pos) || (transform.lossyScale != scale))
                {
                    sphere.position += transform.position - pos;
                    pos = transform.position;

                    sphere.radius *= Mathf.Max(
                        transform.lossyScale.x / scale.x, transform.lossyScale.y / scale.y, transform.lossyScale.z / scale.z);
                    scale = transform.lossyScale;

                    cullingGroup.SetBoundingSpheres(new BoundingSphere[] { sphere });
                }
            }
        }
        else
        {
            bool currentVisible = false;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].isVisible)
                {
                    currentVisible = true;
                    break;
                }
            if (currentVisible)
            {
                if (!prevVisible)
                {
                    becameInvisible?.Invoke();
                    prevVisible = true;
                }
            }
            else
            {
                if (prevVisible)
                {
                    becameVisible?.Invoke();
                    prevVisible = false;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (cullingGroup != null)
            transform.hasChanged = false;
    }

    void OnStateChanged(CullingGroupEvent ev)
    {
        if (ev.hasBecomeVisible)
            becameVisible.Invoke();
        else if (ev.hasBecomeInvisible)
            becameInvisible.Invoke();
    }

    void OnDisable()
    {
        cullingGroup.Dispose();
    }
}
