using UnityEngine;

public class CollisionEvents : BCollisionManager
{
    [SerializeField]
    float maxImpact = 3f;
    [SerializeField]
    Transform[] toContactPoints = null;
    [SerializeField]
    protected DXFloatEvent entered = null;
    [SerializeField]
    protected DXEvent exited = null;

    public override void OnColEnter(NDContactPoint[] contacts, float impact)
    {
        if ((toContactPoints != null) && (toContactPoints.Length > 0))
        {
            for (int i = 0; i < toContactPoints.Length; i++)
            {
                Transform tr = toContactPoints[i];
                NDContactPoint contact = contacts[i % contacts.Length];
                tr.position = contact.point;
                tr.up = contact.normal;
            }
            if (toContactPoints.Length < contacts.Length)
            {
                int i = toContactPoints.Length - 1;
                Transform tr = toContactPoints[i];
                Vector3 point = Vector3.zero;
                Vector3 normal = Vector3.zero;
                for (int j = i; j < contacts.Length; j++)
                {
                    point += contacts[j].point;
                    normal += contacts[j].normal;
                }
                point /= contacts.Length - i;
                normal = normal.normalized;
                tr.position = point;
                tr.up = normal;
            }
        }
        entered?.Invoke(Mathf.Clamp01(Mathf.InverseLerp(minImpact, maxImpact, impact)));
    }

    public override void OnColExit(NDCollision collision)
    {
        exited?.Invoke();
    }
}
