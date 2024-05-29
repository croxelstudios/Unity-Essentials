using UnityEngine;

public class TiltFromTargetPosition : MonoBehaviour
{
    [SerializeField]
    [TagSelector]
    string targetTag = "Player";
    [SerializeField]
    float multiplier = 1f;
    [SerializeField]
    Vector2Int limits = new Vector2Int(-180, 180);
    [SerializeField]
    Vector3 tiltAxis = Vector3.forward;
    [SerializeField]
    Vector3 positionAxis = Vector3.right;
    [SerializeField]
    bool update = false;
    [SerializeField]
    bool localPos = true;
    //TO DO: Update mode is actually too resource intensive, and it should work on some kind of List better

    GameObject[] targets;
    float currentRotation;

    void Awake()
    {
        if (!update) targets = GameObject.FindGameObjectsWithTag(targetTag);
        currentRotation = 0f;
    }

    void Update()
    {
        if (update) targets = GameObject.FindGameObjectsWithTag(targetTag);
        float posAlongAxis = 0f;
        if (targets.Length != 0)
        {
            foreach(GameObject target in targets)
            {
                Vector3 pos = localPos ? target.transform.position - transform.position : target.transform.position;
                if (Vector3.Angle(positionAxis, pos) <= 90f)
                    posAlongAxis += Vector3.Project(pos, positionAxis).magnitude;
                else posAlongAxis -= Vector3.Project(pos, positionAxis).magnitude;
            }
            posAlongAxis /= targets.Length;
            posAlongAxis *= multiplier;
            posAlongAxis = Mathf.Clamp(posAlongAxis, limits.x, limits.y);
        }
        float dif = posAlongAxis - currentRotation;
        currentRotation = posAlongAxis;
        transform.localRotation *= Quaternion.AngleAxis(dif, tiltAxis);
    }
}
