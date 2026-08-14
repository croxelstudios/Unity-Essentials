using Sirenix.OdinInspector;
using UnityEngine;

public class CollisionEvents : BCollisionManager
{
    [MinValue(0f)]
    [HorizontalGroup("maxImp")]
    [SerializeField]
    float maxImpact = 3f;
    [HorizontalGroup("maxImp", LabelWidth = 120, Width = 137)]
    [SerializeField]
    bool limitByMaxImpact = false;
    [PropertyOrder(5)]
    [SerializeField]
    Transform[] toContactPoints = null;
    [SerializeField]
    bool lerpValue = true;
    [PropertyOrder(5)]
    [SerializeField]
    protected DXFloatEvent entered = null;
    [PropertyOrder(5)]
    [SerializeField]
    protected DXEvent exited = null;

    public override void OnColEnter(NDContactPoint[] contacts, float impact)
    {
        if (limitByMaxImpact && (impact > maxImpact))
            return;

        if ((toContactPoints != null) && (toContactPoints.Length > 0))
        {
            for (int i = 0; i < toContactPoints.Length; i++)
            {
                Transform tr = toContactPoints[i];
                NDContactPoint contact = contacts[i % contacts.Length];
                tr.position = contact.point;
                tr.forward = contact.normal;
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
                tr.forward = normal;
            }
        }
        float result = lerpValue ?
            Mathf.Clamp01(Mathf.InverseLerp(minImpact, maxImpact, impact)) : impact;
        entered?.Invoke(result);
    }

    public override void OnColExit(NDContactPoint[] collision)
    {
        exited?.Invoke();
    }
}
