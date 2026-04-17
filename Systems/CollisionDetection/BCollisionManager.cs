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

    List<NDCollision> collisions;

    NDRigidbody rigid;
    CollisionManager_Detector detector;
    bool detectorEnabled;

    protected override void Awake()
    {
        collisions = new List<NDCollision>();
        base.Awake();

        rigid = NDRigidbody.GetNDRigidbodyFrom(gameObject, NDRigidbody.Scope.inParents);
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
            foreach (NDCollision col in collisions)
                OnColExit(col);
            collisions.Clear();
            OnLastColExit();
        }
        DisableDetector();
    }

    void FixedUpdate()
    {
        if (!HasEnabledCollider())
            OnDisable();
        else EnableDetector();

        for (int i = collisions.Count - 1; i > -1; i--)
        {
            if (collisions[i].collider.IsNull() ||
                (!collisions[i].collider.enabled) ||
                (!collisions[i].collider.gameObject.activeInHierarchy))
            {
                collisions.RemoveAt(i);
                if (collisions.Count == 0) OnLastColExit();
                OnColExit(null);
            }
        }
    }

    //TO DO: Something here is making the collison trigger even when impact is greater than maxImpact.
    //Probably CollisionStay is being called after the impact force has been reduced.
    public void CollisionStay(NDCollision collision)
    {
        if (IsThisEnabled() && (minImpact > Mathf.Epsilon) &&
            CheckCollision(collision.gameObject, out CustomTag otherTag) &&
            CheckImpact(collision, out NDContactPoint[] points, out float impact))
        {
            OnColEnter(collision);
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
            if (!collisions.Contains(collision))
                collisions.Add(collision);
            if (prevCount == 0) OnFirstColEnter();
            OnColEnter(collision);
            OnColEnter(points, impact);
            LaunchCustomTag(otherTag);
        }
    }

    public void CollisionExit(NDCollision collision)
    {
        if (IsThisEnabled() && CheckCollision(collision.gameObject))
        {
            collisions.Remove(collision);
            if (collisions.Count == 0) OnLastColExit();
            OnColExit(collision);
        }
    }

    protected virtual bool CheckImpact(NDCollision collision, int point, out float impact)
    {
        Vector3 normal = collision.contacts[point].normal;

        Vector3 projectedVelocity = Vector3.Project(collision.relativeVelocity, normal);
        impact = projectedVelocity.magnitude * Mathf.Sign(Vector3.Dot(normal, projectedVelocity));

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

    public virtual void OnColEnter(NDCollision collision)
    {

    }

    public virtual void OnColEnter(NDContactPoint[] contacts, float impact)
    {

    }

    public virtual void OnColExit(NDCollision collision)
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
