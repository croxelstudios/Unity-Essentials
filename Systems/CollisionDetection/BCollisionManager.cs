using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BCollisionManager : BColliderInteractor
{
    [MinValue(0f)]
    [SerializeField]
    protected float minImpact = 0f;
    [PropertyOrder(2)]
    [SerializeField]
    [Range(0f, 180f)]
    float angleRange = 180f;
    [Indent]
    [PropertyOrder(2)]
    [ShowIf("@angleRange < 180f")]
    [SerializeField]
    Vector3 centerNormal = Vector3.up;
    [Indent]
    [PropertyOrder(2)]
    [ShowIf("@angleRange < 180f")]
    [SerializeField]
    Space normalSpace = Space.World;

    protected Dictionary<NDCollider, NDContactPoint[]> collisions;

    NDRigidbody rigid;
    CollisionManager_Detector detector;
    bool detectorEnabled;

    protected override void Awake()
    {
        collisions = new Dictionary<NDCollider, NDContactPoint[]>();
        base.Awake();

        rigid = NDRigidbody.GetNDRigidbodyFrom(gameObject, Scope.inParents);
        if (rigid != null) SetUpDetector(rigid.gameObject);
        else SetUpDetector(gameObject);
    }

    protected virtual void OnEnable()
    {
        EnableDetector();
    }

    protected virtual void OnDisable()
    {
        if (collisions.Count > 0)
        {
            foreach (NDCollider col in collisions.Keys)
                OnColExit(collisions[col]);
            collisions.Clear();
            OnLastColExit();
        }
        DisableDetector();
    }

    List<NDCollider> aux;

    protected virtual void FixedUpdate()
    {
        if (!HasEnabledCollider())
            OnDisable();
        else EnableDetector();

        aux = aux.ClearOrCreate();
        foreach (NDCollider col in collisions.Keys)
        {
            if (col.IsNull() ||
                (!col.enabled) ||
                (!col.gameObject.activeInHierarchy))
                aux.Add(col);
        }

        for (int i = 0; i < aux.Count; i++)
        {
            collisions.Remove(aux[i]);
            if (collisions.Count == 0) OnLastColExit();
            OnColExit(null);
        }
    }

    //TO DO: Collison trigger is called even when impact is greater than maxImpact.
    //This was intended at first but makes no sense in retrospect
    public void CollisionStay(NDCollision collision)
    {
        if (IsThisEnabled() && (minImpact > Mathf.Epsilon) &&
            CheckCollision(collision.gameObject, out CustomTag otherTag) &&
            CheckImpact(collision, out NDContactPoint[] points, out float impact))
        {
            OnColEnter(points, impact);
            LaunchCustomTag(otherTag);
        }
    }

    public void CollisionEnter(NDCollision collision)
    {
        if (IsThisEnabled() &&
            CheckCollision(collision.gameObject, out CustomTag otherTag) &&
            CheckImpact(collision, out NDContactPoint[] points, out float impact))
        {
            int prevCount = collisions.Count;
            if (!collisions.ContainsKey(collision.collider))
                collisions.Add(collision.collider, collision.contacts);
            if (prevCount == 0) OnFirstColEnter();
            OnColEnter(points, impact);
            LaunchCustomTag(otherTag);
        }
    }

    public void CollisionExit(NDCollision collision)
    {
        if (IsThisEnabled() && CheckCollision(collision.gameObject))
        {
            collisions.Remove(collision.collider);
            if (collisions.Count == 0) OnLastColExit();
            OnColExit(collision.contacts);
        }
    }

    protected virtual bool CheckImpact(NDCollision collision, int point, out float impact)
    {
        Vector3 normal = collision.contacts[point].normal;

        Vector3 projectedVelocity = Vector3.Project(collision.relativeVelocity, normal);
        impact = projectedVelocity.magnitude * Mathf.Sign(Vector3.Dot(normal, projectedVelocity));
        impact = Mathf.Max(impact, 0f);

        Vector3 compareNormal = (normalSpace == Space.World) ? centerNormal :
            transform.TransformDirection(centerNormal);

        return (impact >= minImpact) && NormalIsInRange(normal);
    }

    protected virtual bool CheckImpact(NDCollision collision, out NDContactPoint[] points,
        out float impact)
    {
        impact = 0f;
        if ((collision.contactCount <= 0) && (minImpact > Mathf.Epsilon))
        {
            points = null;
            return false;
        }

        List<NDContactPoint> lPoints = new List<NDContactPoint>();

        Vector3 normal = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (CheckImpact(collision, i, out impact))
                lPoints.Add(collision.contacts[i]);
            normal += collision.contacts[i].normal;
        }

        if (lPoints.Count > 0)
        {
            points = lPoints.ToArray();
            return true;
        }
        else
        {
            normal = normal.normalized;

            Vector3 projectedVelocity = Vector3.Project(collision.relativeVelocity, normal);
            float sqrMag = projectedVelocity.sqrMagnitude * Mathf.Sign(Vector3.Dot(normal, projectedVelocity));

            points = collision.contacts;
            return (sqrMag > (minImpact * minImpact)) && NormalIsInRange(normal);
        }
    }

    bool NormalIsInRange(Vector3 normal)
    {
        if (angleRange >= 180f)
            return true;

        Vector3 compareNormal = (normalSpace == Space.World) ? centerNormal :
            transform.TransformDirection(centerNormal);

        return Vector3.Angle(normal, compareNormal) < angleRange;
    }

    public virtual void OnFirstColEnter()
    {

    }

    public virtual void OnLastColExit()
    {

    }

    public virtual void OnColEnter(NDContactPoint[] contacts, float impact)
    {

    }

    public virtual void OnColExit(NDContactPoint[] collision)
    {

    }

    protected override bool HasEnabledCollider()
    {
        if ((rigid != null) && (rigid.gameObject == gameObject))
            return true;

        return base.HasEnabledCollider();
    }

    void SetUpDetector(GameObject obj)
    {
        detector = obj.GetComponent<CollisionManager_Detector>();
        if (detector == null)
            detector = obj.AddComponent<CollisionManager_Detector>();
    }

    void EnableDetector()
    {
        if ((detector != null) && (!detectorEnabled))
        {
            detector.enter += CollisionEnter;
            detector.stay += CollisionStay;
            detector.exit += CollisionExit;
            detectorEnabled = true;
        }
    }

    void DisableDetector()
    {
        if ((detector != null) && detectorEnabled)
        {
            detector.enter -= CollisionEnter;
            detector.stay -= CollisionStay;
            detector.exit -= CollisionExit;
            detectorEnabled = false;
        }
    }
}
