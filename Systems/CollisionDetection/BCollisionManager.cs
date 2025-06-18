using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BCollisionManager : MonoBehaviour
{
    //[SerializeField]
    //[Tooltip("Determines if it should also check the tag of the attached rigidbody")]
    bool checkRigidbodyTag = false;
    [SerializeField]
    [TagSelector]
    [Tooltip("Will fire on any collision if this array is empty")]
    string[] detectionTags = null;
    [SerializeField]
    LayerMask layerMask = -1;
    [SerializeField]
    float minImpact = 0f;
    //TO DO: Only detect hits withing a range of a Normal

    CollisionManager_Detector detector;
    List<NDCollision> collisions;
    NDCollider[] selfColliders;

    protected virtual void Awake()
    {
        collisions = new List<NDCollision>();
        selfColliders = NDCollider.GetNDCollidersFrom(gameObject);

        NDRigidbody rigid = NDRigidbody.GetNDRigidbodyFrom(gameObject, NDRigidbody.Scope.inParents);
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

    bool IsThisEnabled()
    {
        return this.IsActiveAndEnabled() && HasEnabledCollider();
    }

    public void CollisionStay(NDCollision collision)
    {
        if (IsThisEnabled() && (minImpact > Mathf.Epsilon) && CheckCollision(collision.gameObject) &&
            CheckImpact(collision, out NDContactPoint[] points, out float impact))
        {
            OnColEnter(collision);
            OnColEnter(points, impact);
        }
    }

    public void CollisionEnter(NDCollision collision)
    {
        if (IsThisEnabled() && CheckCollision(collision.gameObject)
            && CheckImpact(collision, out NDContactPoint[] points, out float impact))
        {
            int prevCount = collisions.Count;
            if (!collisions.Contains(collision))
                collisions.Add(collision);
            if (prevCount == 0) OnFirstColEnter();
            OnColEnter(collision);
            OnColEnter(points, impact);
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

        return impact > minImpact;
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
            return sqrMag > (minImpact * minImpact);
        }
    }

    protected virtual bool CheckCollision(GameObject other)
    {
        if ((detectionTags == null || detectionTags.Contains(other.tag) || detectionTags.Length == 0)
            && layerMask.ContainsLayer(other.layer))
            return true;
        else if (checkRigidbodyTag)
        {
            NDRigidbody rigid = NDRigidbody.GetNDRigidbodyFrom(other, NDRigidbody.Scope.inParents);
            if ((rigid != null) && detectionTags.Contains(rigid.tag) && layerMask.ContainsLayer(rigid.layer))
                return true;
        }
        return false;
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

    bool HasEnabledCollider()
    {
        if ((selfColliders == null) || (selfColliders.Length <= 0))
            return false;
        for (int i = 0; i < selfColliders.Length; i++)
            if ((!selfColliders[i].IsNull()) && selfColliders[i].enabled)
                return true;
        return false;
    }

    void SetUpDetector(GameObject obj)
    {
        detector = obj.GetComponent<CollisionManager_Detector>();
        if (detector == null)
            detector = obj.AddComponent<CollisionManager_Detector>();
    }

    void EnableDetector()
    {
        if (detector != null)
        {
            detector.enter += CollisionEnter;
            detector.stay += CollisionStay;
            detector.exit += CollisionExit;
        }
    }

    void DisableDetector()
    {
        if (detector != null)
        {
            detector.enter -= CollisionEnter;
            detector.stay -= CollisionStay;
            detector.exit -= CollisionExit;
        }
    }
}
