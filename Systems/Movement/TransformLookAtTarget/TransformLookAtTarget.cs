using UnityEngine;

[ExecuteAlways]
public class TransformLookAtTarget : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Will look at closest if multiple with same tag are detected")]
    [TagSelector]
    string targetTag = "MainCamera";
    [SerializeField]
    Transform targetOverride = null;
    [SerializeField]
    Vector3 targetOffset = Vector3.zero;
    [SerializeField]
    bool lookAway = true;
    [Header("Rotation Range")]
    [SerializeField]
    Vector3 angleOffset = Vector3.zero;
    [SerializeField]
    [Range(0f, 180f)]
    float X = 20f;
    [SerializeField]
    [Range(0f, 180f)]
    float Y = 180f;
    [SerializeField]
    [Range(0f, 180f)]
    float Z = 0f;
    [SerializeField]
    bool updateTaggedObjects = false;
    [SerializeField]
    float smoothTime = 0f;
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    Transform[] targets;
    Vector3 tmp;
    Quaternion prevRot;

    void OnEnable()
    {
        if (targetOverride == null)
            if (!updateTaggedObjects) targets = GetTaggedTransforms(targetTag);
        prevRot = transform.localRotation;
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()
#if UNITY_EDITOR
            && Application.isPlaying
#endif
            ) DoUpdate(timeMode.DeltaTime());
    }

    void LateUpdate()
    {
        if (timeMode.IsSmooth()
#if UNITY_EDITOR
            || !Application.isPlaying
#endif
            ) DoUpdate(timeMode.DeltaTime());
    }

    void DoUpdate(float deltaTime)
    {
        if (targetOverride == null)
        {
            if (updateTaggedObjects) targets = GetTaggedTransforms(targetTag);
            else
            {
                bool isNull = targets == null;
                if (!isNull)
                    for (int i = 0; i < targets.Length; i++)
                        if (targets[i] == null)
                        {
                            isNull = true;
                            break;
                        }
                if (isNull) targets = GetTaggedTransforms(targetTag);
            }
        }
        SetRotation(deltaTime);
    }

    void SetRotation(float deltaTime)
    {
        Vector3 eulers = GetEulers();

        if ((smoothTime <= 0f)
#if UNITY_EDITOR
            || !Application.isPlaying
#endif
            )
            transform.localEulerAngles = eulers;
        else
        {
            Quaternion quat = Quaternion.Euler(eulers);
            transform.localRotation = prevRot.SmoothDamp(
                quat, ref tmp, smoothTime, Mathf.Infinity, deltaTime);
            prevRot = transform.localRotation;
        }
    }

    Vector3 GetEulers()
    {
        if (((targets != null) && (targets.Length > 0)) || (targetOverride != null))
        {
            Quaternion rot = transform.rotation;
            transform.LookAt(((targetOverride != null) ? targetOverride :
                targets.GetClosest(transform)).position + targetOffset);
            Vector3 lookAtEuler = transform.localEulerAngles.Loop(-180f, 180f);

            float x = (Mathf.Abs(lookAtEuler.x) > X) ? (Mathf.Sign(lookAtEuler.x) * X) : lookAtEuler.x;
            float y = ((Mathf.Abs(lookAtEuler.y) > Y) ? (Mathf.Sign(lookAtEuler.y) * Y) : lookAtEuler.y);
            float z = (Mathf.Abs(lookAtEuler.z) > Z) ? (Mathf.Sign(lookAtEuler.z) * Z) : lookAtEuler.z;

            if (lookAway)
            {
                x = -x;
                y += 180f;
            }

            Vector3 finalEuler = new Vector3(x, y, z);

            transform.localEulerAngles = finalEuler;
            transform.Rotate(angleOffset);
            finalEuler = transform.localEulerAngles;

            transform.rotation = rot;

            return finalEuler;
        }
        else return transform.localEulerAngles;
    }

    public void ResetRotation()
    {
        transform.localRotation = Quaternion.identity;
    }

    public void UpdateTaggedObjects()
    {
        targets = GetTaggedTransforms(targetTag);
    }

    public void SetTarget(Transform newTargetOverride)
    {
        targetOverride = newTargetOverride;
    }

    public void ChangeTag(string tag)
    {
        targetTag = tag;
        UpdateTaggedObjects();
    }

    Transform[] GetTaggedTransforms(string tag)
    {
        if (!string.IsNullOrEmpty(targetTag))
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(targetTag);
            Transform[] targets = new Transform[objs.Length];
            for (int i = 0; i < objs.Length; i++) targets[i] = objs[i].transform;
            return targets;
        }
        else return null;
    }
}
