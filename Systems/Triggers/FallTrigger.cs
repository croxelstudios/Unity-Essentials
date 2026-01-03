using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    [SerializeField]
    Vector3 direction = Vector3.down;
    [SerializeField]
    Space space = Space.World;
    [SerializeField]
    float distance = 5f;
    [SerializeField]
    DXEvent entered = null;
    [SerializeField]
    DXEvent exited = null;
    [SerializeField]
    ScaledTimeMode timeMode = ScaledTimeMode.FixedUpdate;

    bool isIn;

    void Update()
    {
        if (timeMode.IsSmooth()) OnUpdate();
    }

    void FixedUpdate()
    {
        if (timeMode.IsFixed()) OnUpdate();
    }

    void OnUpdate()
    {
        if (CheckTransform())
        {
            if (!isIn)
            {
                entered?.Invoke();
                isIn = true;
            }
        }
        else
        {
            if (isIn)
            {
                exited?.Invoke();
                isIn = false;
            }
        }
    }

    bool CheckTransform()
    {
        Vector3 position = space == Space.Self ? transform.localPosition : transform.position;

        if (Vector3.Dot(position, direction) < 0f)
            return false;

        Vector3 proj = Vector3.Project(position, direction);
        return proj.magnitude > distance;
    }
}
