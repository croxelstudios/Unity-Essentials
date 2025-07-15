using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class NDRigidbody
{
    static Dictionary<Rigidbody2D, NDRigidbody> rigids2;
    static Dictionary<Rigidbody, NDRigidbody> rigids3;

    public Rigidbody2D rigid2;
    public Rigidbody rigid3;
    public bool is2D { get { return rigid2 != null; } }
    public bool is3D { get { return rigid3 != null; } }
    GameObject _gameObject;
    public GameObject gameObject
    {
        get
        {
            if (_gameObject == null)
            {
                if (is2D)
                {
                    if (rigid2.gameObject != null)
                        _gameObject = rigid2.gameObject;
                }
                else
                {
                    if (rigid3.gameObject != null)
                        _gameObject = rigid3.gameObject;
                }
            }
            return _gameObject;
        }
    }
    Transform _transform;
    public Transform transform
    {
        get
        {
            if (_transform == null)
            {
                if (is2D) _transform = rigid2.transform;
                else _transform = rigid3.transform;
            }
            return _transform;
        }
    }
    public string tag
    {
        get
        {
            if (is2D) return rigid2.tag;
            else return rigid3.tag;
        }
    }
    public int layer { get { return gameObject.layer; } }
    public Vector3 linearVelocity
    {
        get
        {
            if (is2D) return rigid2.linearVelocity;
            else return rigid3.linearVelocity;
        }

        set
        {
            if (is2D)
            {
                if (rigid2.bodyType != RigidbodyType2D.Static)
                    rigid2.linearVelocity = value;
            }
            else rigid3.linearVelocity = value;
        }
    }
    public Vector3 angularVelocity
    {
        get
        {
            if (is2D) return new Vector3(0f, 0f, rigid2.angularVelocity);
            else return rigid3.angularVelocity;
        }

        set
        {
            if (is2D)
            {
                if (rigid2.bodyType != RigidbodyType2D.Static)
                    rigid2.angularVelocity = value.z;
            }
            else rigid3.angularVelocity = value;
        }
    }
    public float linearDamping
    {
        get
        {
            if (is2D) return rigid2.linearDamping;
            else return rigid3.linearDamping;
        }

        set
        {
            if (is2D) rigid2.linearDamping = value;
            else rigid3.linearDamping = value;
        }
    }
    public float angularDamping
    {
        get
        {
            if (is2D) return rigid2.angularDamping;
            else return rigid3.angularDamping;
        }

        set
        {
            if (is2D) rigid2.angularDamping = value;
            else rigid3.angularDamping = value;
        }
    }
    public bool isKinematic
    {
        get
        {
            if (is2D) return rigid2.bodyType == RigidbodyType2D.Kinematic;
            else return rigid3.isKinematic;
        }

        set
        {
            if (is2D) rigid2.bodyType = value ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            else rigid3.isKinematic = value;
        }
    }
    public bool useGravity
    {
        get
        {
            if (is2D) return rigid2.gravityScale > Mathf.Epsilon;
            else return rigid3.useGravity;
        }

        set
        {
            if (is2D) rigid2.gravityScale = value ? 1f : 0f;
            else rigid3.useGravity = value;
        }
    }
    public float gravityScale
    {
        get
        {
            if (is2D) return rigid2.gravityScale;
            else return rigid3.useGravity ? 1f : 0f;
        }

        set
        {
            if (is2D) rigid2.gravityScale = value;
            else rigid3.useGravity = (value > 0.5f) ? true : false;
        }
    }
    public RigidbodyConstraints constraints
    {
        get
        {
            if (is2D)
            {
                RigidbodyConstraints cons3 = new RigidbodyConstraints();
                if ((rigid2.constraints & RigidbodyConstraints2D.FreezePositionX) != 0)
                    cons3 |= RigidbodyConstraints.FreezePositionX;
                if ((rigid2.constraints & RigidbodyConstraints2D.FreezePositionY) != 0)
                    cons3 |= RigidbodyConstraints.FreezePositionY;
                if ((rigid2.constraints & RigidbodyConstraints2D.FreezeRotation) != 0)
                    cons3 |= RigidbodyConstraints.FreezeRotationZ;
                return cons3;
            }
            else return rigid3.constraints;
        }

        set
        {
            if (is2D)
            {
                RigidbodyConstraints2D cons2 = new RigidbodyConstraints2D();
                if ((value & RigidbodyConstraints.FreezePositionX) != 0)
                    cons2 |= RigidbodyConstraints2D.FreezePositionX;
                if ((value & RigidbodyConstraints.FreezePositionY) != 0)
                    cons2 |= RigidbodyConstraints2D.FreezePositionY;
                if ((value & RigidbodyConstraints.FreezeRotationZ) != 0)
                    cons2 |= RigidbodyConstraints2D.FreezeRotation;
                rigid2.constraints = cons2;
            }
            else rigid3.constraints = value;
        }
    }
    public Vector3 worldCenterOfMass
    {
        get
        {
            if (is2D) return rigid2.worldCenterOfMass;
            else return rigid3.worldCenterOfMass;
        }
    }
    public Vector3 position
    {
        get
        {
            if (is2D) return rigid2.position;
            else return rigid3.position;
        }
    }
    public Quaternion rotation
    {
        get
        {
            if (is2D) return Quaternion.Euler(0f, 0f, rigid2.rotation);
            else return rigid3.rotation;
        }
    }

    public Bounds GetBounds()
    {
        if (is2D) return rigid2.GetBounds();
        else return rigid3.GetBounds();
    }

    public Vector3 Cast(Vector3 speed)
    {
        if (is2D)
        {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            rigid2.Cast(speed, hits);
            return speed * hits[0].fraction;
        }
        else
        {
            rigid3.SweepTest(speed, out RaycastHit hit);
            return speed.normalized * hit.distance;
        }
    }

    public bool IsNull()
    {
        return (rigid2 == null) && (rigid3 == null);
    }

    NDRigidbody(Rigidbody2D rigid)
    {
        rigid2 = rigid;
        rigid3 = null;
    }

    NDRigidbody(Rigidbody rigid)
    {
        rigid2 = null;
        rigid3 = rigid;
    }

    public static NDRigidbody ND(Rigidbody2D rigid)
    {
        if (rigid == null) return null;

        if (rigids2 == null)
        {
            rigids2 = new Dictionary<Rigidbody2D, NDRigidbody>();
            SceneManager.sceneUnloaded += ResetDictionary2;
        }

        NDRigidbody nd;
        if (!rigids2.TryGetValue(rigid, out nd))
        {
            nd = new NDRigidbody(rigid);
            rigids2.Add(rigid, nd);
        }
        return nd;
    }

    public static NDRigidbody ND(Rigidbody rigid)
    {
        if (rigid == null) return null;

        if (rigids3 == null)
        {
            rigids3 = new Dictionary<Rigidbody, NDRigidbody>();
            SceneManager.sceneUnloaded += ResetDictionary3;
        }

        NDRigidbody nd;
        if (!rigids3.TryGetValue(rigid, out nd))
        {
            nd = new NDRigidbody(rigid);
            rigids3.Add(rigid, nd);
        }
        return nd;
    }

    static void ResetDictionary2(Scene scene)
    {
        rigids2.SmartClear();
    }

    static void ResetDictionary3(Scene scene)
    {
        rigids3.SmartClear();
    }

    public static NDRigidbody GetNDRigidbodyFrom(GameObject go, Scope scope = Scope.inThis)
    {
        NDRigidbody result = null;
        Rigidbody rigid3;
        switch (scope)
        {
            case Scope.inParents:
                rigid3 = go.GetComponentInParent<Rigidbody>();
                break;
            case Scope.inChildren:
                rigid3 = go.GetComponentInChildren<Rigidbody>();
                break;
            default:
                rigid3 = go.GetComponent<Rigidbody>();
                break;
        }
        if (rigid3 != null) result = rigid3.ND();
        else
        {
            Rigidbody2D rigid2;
            switch (scope)
            {
                case Scope.inParents:
                    rigid2 = go.GetComponentInParent<Rigidbody2D>();
                    break;
                case Scope.inChildren:
                    rigid2 = go.GetComponentInChildren<Rigidbody2D>();
                    break;
                default:
                    rigid2 = go.GetComponent<Rigidbody2D>();
                    break;
            }
            if (rigid2 != null) result = rigid2.ND();
            else result = null;
        }
        return result;
    }

    public enum Scope { inThis, inParents, inChildren }

    public void AddForce(Vector2 force, ForceMode forceMode)
    {
        AddForce((Vector3)force, forceMode);
    }

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        if (is2D)
        {
            switch (forceMode)
            {
                case ForceMode.VelocityChange:
                    rigid2.linearVelocity += (Vector2)force;
                    break;
                case ForceMode.Acceleration: //WARNING: It is not implemented. TO DO: Add a Coroutine to manage this?
                    rigid2.linearVelocity += (Vector2)force;
                    break;
                case ForceMode.Impulse:
                    rigid2.AddForce(force, ForceMode2D.Impulse);
                    break;
                default:
                    rigid2.AddForce(force, ForceMode2D.Force);
                    break;
            }
        }
        else rigid3.AddForce(force, forceMode);
    }

    public void AddTorque(float torque, ForceMode forceMode)
    {
        AddForce(Vector3.forward * torque, forceMode);
    }

    public void AddTorque(Vector3 torque, ForceMode forceMode)
    {
        if (is2D)
        {
            switch (forceMode)
            {
                case ForceMode.VelocityChange:
                    rigid2.angularVelocity += torque.z;
                    break;
                case ForceMode.Acceleration: //WARNING: It is not implemented. TO DO: Add a Coroutine to manage this?
                    rigid2.angularVelocity += torque.z;
                    break;
                case ForceMode.Impulse:
                    rigid2.AddTorque(torque.z, ForceMode2D.Impulse);
                    break;
                default:
                    rigid2.AddTorque(torque.z, ForceMode2D.Force);
                    break;
            }
        }
        else rigid3.AddTorque(torque, forceMode);
    }

    public void MovePosition(Vector3 position)
    {
        if (is2D) rigid2.MovePosition(position);
        else rigid3.MovePosition(position);
    }

    List<NDRaycastHit> tmpHits;

    public NDRaycastHit[] TunnelCastToThis(Vector3 origin, Vector3 direction, float radius, float distance)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, is2D);

        tmpHits = tmpHits.ClearOrCreate();

        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider.attachedRigidbody == this) && (hits[i].distance >= offset))
                tmpHits.Add(hits[i]);
        return tmpHits.ToArray();
    }

    public NDRaycastHit[] TunnelCastExceptThis(Vector3 origin, Vector3 direction, float radius, float distance,
        bool includeTriggers = false)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, is2D);

        tmpHits = tmpHits.ClearOrCreate();

        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider.attachedRigidbody != this) &&
                (includeTriggers || !hits[i].isTrigger) && (hits[i].distance >= offset))
                tmpHits.Add(hits[i]);
        return tmpHits.ToArray();
    }

    public NDRaycastHit[] TunnelCastToThis(Vector3 origin, Vector3 direction, float radius, float distance,
        LayerMask mask)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, mask, is2D);

        tmpHits = tmpHits.ClearOrCreate();

        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider.attachedRigidbody == this) && (hits[i].distance >= offset))
                tmpHits.Add(hits[i]);
        return tmpHits.ToArray();
    }

    public NDRaycastHit[] TunnelCastExceptThis(Vector3 origin, Vector3 direction, float radius, float distance,
        LayerMask mask, bool includeTriggers = false)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, mask, is2D);

        tmpHits = tmpHits.ClearOrCreate();

        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider.attachedRigidbody != this) &&
                (includeTriggers || !hits[i].isTrigger) && (hits[i].distance >= offset))
                tmpHits.Add(hits[i]);
        return tmpHits.ToArray();
    }

    //TO DO: Full rigidbody casts.
}

[Serializable]
public class NDCollider
{
    static Dictionary<Collider2D, NDCollider> cols2;
    static Dictionary<Collider, NDCollider> cols3;

    public Collider2D col2;
    public Collider col3;
    public bool is2D { get { return col2 != null; } }
    public bool is3D { get { return col3 != null; } }
    public bool enabled
    {
        get
        {
            if (IsNull()) return false;
            if (is2D) return col2.enabled;
            else return col3.enabled;
        }
    }
    public bool isTrigger
    {
        get
        {
            if (is2D) return col2.isTrigger;
            else return col3.isTrigger;
        }
    }

    NDRigidbody _attachedRigidbody;
    public NDRigidbody attachedRigidbody
    {
        get
        {
            if (IsNull()) return null;
            if (is2D)
            {
                if (col2.attachedRigidbody != null)
                {
                    if ((_attachedRigidbody == null) || (_attachedRigidbody.rigid2 != col2.attachedRigidbody))
                        _attachedRigidbody = col2.attachedRigidbody.ND();
                    return _attachedRigidbody;
                }
                else return null;
            }
            else if (is3D)
            {
                if (col3.attachedRigidbody != null)
                {
                    if ((_attachedRigidbody == null) || (_attachedRigidbody.rigid3 != col3.attachedRigidbody))
                        _attachedRigidbody = col3.attachedRigidbody.ND();
                    return _attachedRigidbody;
                }
                else return null;
            }
            else return null;
        }
    }
    
    GameObject _gameObject;
    public GameObject gameObject
    {
        get
        {
            if (_gameObject == null)
            {
                if (IsNull()) _gameObject = null;
                if (is2D)
                {
                    if (col2.gameObject != null)
                        _gameObject = col2.gameObject;
                }
                else
                {
                    if (col3.gameObject != null)
                        _gameObject = col3.gameObject;
                }
            }
            return _gameObject;
        }
    }

    Transform _transform;
    public Transform transform
    {
        get
        {
            if (_transform == null)
            {
                if (IsNull()) _transform = null;
                if (is2D) _transform = col2.transform;
                else _transform = col3.transform;
            }
            return _transform;
        }
    }

    public Bounds bounds
    {
        get
        {
            if (IsNull()) return default;
            if (is2D) return col2.bounds;
            else return col3.bounds;
        }
    }

    public string tag
    {
        get
        {
            if (IsNull()) return null;
            if (is2D) return col2.tag;
            else return col3.tag;
        }
    }
    public int layer { get { return gameObject.layer; } }

    public NDCollider(Collider2D col)
    {
        col2 = col;
        col3 = null;
        _attachedRigidbody = col2.attachedRigidbody.ND();
    }

    public NDCollider(Collider col)
    {
        col2 = null;
        col3 = col;
        _attachedRigidbody = col3.attachedRigidbody.ND();
    }

    public static NDCollider ND(Collider2D collider)
    {
        if (collider == null) return null;

        if (cols2 == null)
        {
            cols2 = new Dictionary<Collider2D, NDCollider>();
            SceneManager.sceneUnloaded += ResetDictionary2;
        }

        NDCollider nd;
        if (!cols2.TryGetValue(collider, out nd))
        {
            nd = new NDCollider(collider);
            cols2.Add(collider, nd);
        }
        return nd;
    }

    public static NDCollider ND(Collider collider)
    {
        if (collider == null) return null;

        if (cols3 == null)
        {
            cols3 = new Dictionary<Collider, NDCollider>();
            SceneManager.sceneUnloaded += ResetDictionary3;
        }

        NDCollider nd;
        if (!cols3.TryGetValue(collider, out nd))
        {
            nd = new NDCollider(collider);
            cols3.Add(collider, nd);
        }
        return nd;
    }

    static void ResetDictionary2(Scene scene)
    {
        cols2.Clear();
    }

    static void ResetDictionary3(Scene scene)
    {
        cols3.Clear();
    }

    public static NDCollider GetNDColliderFrom(GameObject go, Scope scope = Scope.inThis)
    {
        NDCollider result = null;
        Collider col3;
        switch (scope)
        {
            case Scope.inParents:
                col3 = go.GetComponentInParent<Collider>();
                break;
            case Scope.inChildren:
                col3 = go.GetComponentInChildren<Collider>();
                break;
            default:
                col3 = go.GetComponent<Collider>();
                break;
        }
        if (col3 != null) result = col3.ND();
        else
        {
            Collider2D col2;
            switch (scope)
            {
                case Scope.inParents:
                    col2 = go.GetComponentInParent<Collider2D>();
                    break;
                case Scope.inChildren:
                    col2 = go.GetComponentInChildren<Collider2D>();
                    break;
                default:
                    col2 = go.GetComponent<Collider2D>();
                    break;
            }
            if (col2 != null) result = col2.ND();
            else result = null;
        }
        return result;
    }

    public static NDCollider GetNDColliderFrom(Transform tr, Scope scope = Scope.inThis)
    {
        return GetNDColliderFrom(tr.gameObject, scope);
    }

    public static NDCollider[] GetNDCollidersFrom(GameObject go, Scope scope = Scope.inThis)
    {
        List<NDCollider> result = new List<NDCollider>();
        Collider[] col3s;
        switch (scope)
        {
            case Scope.inParents:
                col3s = go.GetComponentsInParent<Collider>();
                break;
            case Scope.inChildren:
                col3s = go.GetComponentsInChildren<Collider>();
                break;
            default:
                col3s = go.GetComponents<Collider>();
                break;
        }
        foreach (Collider col in col3s) result.Add(col.ND());

        Collider2D[] col2s;
        switch (scope)
        {
            case Scope.inParents:
                col2s = go.GetComponentsInParent<Collider2D>();
                break;
            case Scope.inChildren:
                col2s = go.GetComponentsInChildren<Collider2D>();
                break;
            default:
                col2s = go.GetComponents<Collider2D>();
                break;
        }
        foreach (Collider2D col in col2s) result.Add(col.ND());

        return result.ToArray();
    }

    public static NDCollider[] GetNDCollidersFrom(Transform tr, Scope scope = Scope.inThis)
    {
        return GetNDCollidersFrom(tr.gameObject, scope);
    }

    public LayerMask GetLayerCollisionMask()
    {
        if (is2D) return Physics2D.GetLayerCollisionMask(layer);
        else
        {
            int finalMask = 0;
            for (int i = 0; i < 32; i++)
                if (!Physics.GetIgnoreLayerCollision(layer, i)) finalMask = finalMask | (1 << i);
            return finalMask;
        }
    }

    bool IsNull()
    {
        return (col2 == null) && (col3 == null);
    }

    public bool IsTransformAttached(Transform tr)
    {
        if (transform.IsChildOf(tr)) return true;
        else if ((attachedRigidbody != null) && tr.IsChildOf(attachedRigidbody.transform)) return true;
        else return false;
    }

    public NDRaycastHit[] TunnelCastToThis(Vector3 origin, Vector3 direction, float radius, float distance)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, is2D);
        List<NDRaycastHit> finalHits = new List<NDRaycastHit>();
        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider == this) && (hits[i].distance >= offset)) finalHits.Add(hits[i]);
        return finalHits.ToArray();
    }

    public NDRaycastHit[] TunnelCastExceptThis(Vector3 origin, Vector3 direction, float radius, float distance,
        bool includeTriggers = false)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, is2D);
        List<NDRaycastHit> finalHits = new List<NDRaycastHit>();
        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider != this) &&
                (includeTriggers || !hits[i].isTrigger) && (hits[i].distance >= offset))
                finalHits.Add(hits[i]);
        return finalHits.ToArray();
    }

    public NDRaycastHit[] TunnelCastToThis(Vector3 origin, Vector3 direction, float radius, float distance,
        LayerMask mask)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, mask, is2D);
        List<NDRaycastHit> finalHits = new List<NDRaycastHit>();
        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider == this) && (hits[i].distance >= offset)) finalHits.Add(hits[i]);
        return finalHits.ToArray();
    }

    public NDRaycastHit[] TunnelCastExceptThis(Vector3 origin, Vector3 direction, float radius, float distance,
        LayerMask mask, bool includeTriggers = false)
    {
        float offset = NDPhysics.DefaultContactOffset(is2D);
        NDRaycastHit[] hits = NDPhysics.RadiusCastAll(origin - (direction.normalized * offset),
            direction, radius, distance + offset, mask, is2D);
        List<NDRaycastHit> finalHits = new List<NDRaycastHit>();
        for (int i = 0; i < hits.Length; i++)
            if ((hits[i].collider != this) &&
                (includeTriggers || !hits[i].isTrigger) && (hits[i].distance >= offset))
                finalHits.Add(hits[i]);
        return finalHits.ToArray();
    }

    public enum Scope { inThis, inParents, inChildren }
}

public static class NDPhysics
{
    public static NDCollider[] OverlapRadiusAll(Vector3 position, float radius, LayerMask layerMask, bool is2D)
    {
        NDCollider[] result = null;
        if (is2D)
        {
            Collider2D[] inRange = Physics2D.OverlapCircleAll(position, radius, layerMask);
            result = new NDCollider[inRange.Length];
            for (int i = 0; i < inRange.Length; i++)
                result[i] = inRange[i].ND();
        }
        else
        {
            Collider[] inRange = Physics.OverlapSphere(position, radius, layerMask);
            result = new NDCollider[inRange.Length];
            for (int i = 0; i < inRange.Length; i++)
                result[i] = inRange[i].ND();
        }
        return result;
    }

    /// <summary>
    /// Gets all colliders in a box area. When 'quat' value isn't specified, 2D physics are used.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <param name="layerMask"></param>
    /// <param name="quat"></param>
    /// <returns></returns>
    public static NDCollider[] OverlapBoxAll(Vector3 position, Vector3 size,
        LayerMask layerMask, Quaternion quat = default)
    {
        NDCollider[] result = null;
        if (quat.w == 0f)
        {
            Collider2D[] inRange = Physics2D.OverlapBoxAll(position, size, layerMask);
            result = new NDCollider[inRange.Length];
            for (int i = 0; i < inRange.Length; i++)
                result[i] = inRange[i].ND();
        }
        else
        {
            Collider[] inRange = Physics.OverlapBox(position, size, quat, layerMask);
            result = new NDCollider[inRange.Length];
            for (int i = 0; i < inRange.Length; i++)
                result[i] = inRange[i].ND();
        }
        return result;
    }

    public static LayerMask GetLayerCollisionMask(int layer, bool is2D)
    {
        LayerMask layerMask = default;
        if (is2D)
        {
            for (int i = 0; i < 32; i++)
                if (!Physics2D.GetIgnoreLayerCollision(layer, i))
                    layerMask = layerMask.AddLayer(i);
        }
        else
        {
            for (int i = 0; i < 32; i++)
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                    layerMask = layerMask.AddLayer(i);
        }
        return layerMask;
    }

    public static NDRigidbody[] GetRigidbodies(this NDCollider[] colliders)
    {
        List<NDRigidbody> rigids = new List<NDRigidbody>();
        foreach (NDCollider col in colliders)
        {
            NDRigidbody rigid = col.attachedRigidbody;
            if ((rigid != null) && (!rigids.Contains(rigid))) rigids.Add(rigid);
        }
        return rigids.ToArray();
    }

    public static bool Raycast(Vector3 position, Vector3 direction, out NDRaycastHit hit, float distance, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D hit2 = Physics2D.Raycast(position, direction, distance);
            hit = new NDRaycastHit(hit2);
            return hit2;
        }
        else
        {
            bool didHit = Physics.Raycast(position, direction, out RaycastHit hit3, distance);
            hit = new NDRaycastHit(hit3, distance);
            return didHit;
        }
    }

    public static bool Raycast(Vector3 position, Vector3 direction, out NDRaycastHit hit, float distance,
        LayerMask mask, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D hit2 = Physics2D.Raycast(position, direction, distance, mask);
            hit = new NDRaycastHit(hit2);
            return hit2;
        }
        else
        {
            bool didHit = Physics.Raycast(position, direction, out RaycastHit hit3, distance, mask);
            hit = new NDRaycastHit(hit3, distance);
            return didHit;
        }
    }

    public static NDRaycastHit[] RaycastAll(Vector3 position, Vector3 direction, float distance, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D[] hit2 = Physics2D.RaycastAll(position, direction, distance);
            NDRaycastHit[] hits = new NDRaycastHit[hit2.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit2[i]);
            return hits;
        }
        else
        {
            RaycastHit[] hit3 = Physics.RaycastAll(position, direction, distance);
            NDRaycastHit[] hits = new NDRaycastHit[hit3.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit3[i], distance);
            return hits;
        }
    }

    public static NDRaycastHit[] RaycastAll(Vector3 position, Vector3 direction, float distance,
        LayerMask mask, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D[] hit2 = Physics2D.RaycastAll(position, direction, distance, mask);
            NDRaycastHit[] hits = new NDRaycastHit[hit2.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit2[i]);
            return hits;
        }
        else
        {
            RaycastHit[] hit3 = Physics.RaycastAll(position, direction, distance, mask);
            NDRaycastHit[] hits = new NDRaycastHit[hit3.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit3[i], distance);
            return hits;
        }
    }

    public static bool RadiusCast(Ray ray, float radius,
        float distance, out NDRaycastHit hit, bool is2D)
    {
        return RadiusCast(ray.origin, ray.direction, radius, distance, out hit, is2D);
    }

    public static bool RadiusCast(Vector3 origin, Vector3 direction, float radius,
        float distance, out NDRaycastHit hit, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D hit2 = Physics2D.CircleCast(origin, radius, direction, distance);
            hit = new NDRaycastHit(hit2);
            return hit2;
        }
        else
        {
            bool didHit = Physics.SphereCast(origin, radius, direction, out RaycastHit hit3, distance);
            hit = new NDRaycastHit(hit3, distance);
            return didHit;
        }
    }

    public static bool RadiusCast(Ray ray, float radius,
        float distance, out NDRaycastHit hit, LayerMask mask, bool is2D)
    {
        return RadiusCast(ray.origin, ray.direction, radius, distance, out hit, mask, is2D);
    }

    public static bool RadiusCast(Vector3 origin, Vector3 direction, float radius,
        float distance, out NDRaycastHit hit, LayerMask mask, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D hit2 = Physics2D.CircleCast(origin, radius, direction, distance, mask);
            hit = new NDRaycastHit(hit2);
            return hit2;
        }
        else
        {
            bool didHit = Physics.SphereCast(origin, radius, direction, out RaycastHit hit3, distance, mask);
            hit = new NDRaycastHit(hit3, distance);
            return didHit;
        }
    }

    public static NDRaycastHit[] RadiusCastAll(Ray ray, float radius, float distance, bool is2D)
    {
        return RadiusCastAll(ray.origin, ray.direction, radius, distance, is2D);
    }

    public static NDRaycastHit[] RadiusCastAll(Vector3 origin, Vector3 direction, float radius,
        float distance, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D[] hit2 = Physics2D.CircleCastAll(origin, radius, direction, distance);
            NDRaycastHit[] hits = new NDRaycastHit[hit2.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit2[i]);
            return hits;
        }
        else
        {
            RaycastHit[] hit3 = Physics.SphereCastAll(origin, radius, direction, distance);
            NDRaycastHit[] hits = new NDRaycastHit[hit3.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit3[i], distance);
            return hits;
        }
    }

    public static NDRaycastHit[] RadiusCastAll(Ray ray, float radius, float distance, LayerMask mask, bool is2D)
    {
        return RadiusCastAll(ray.origin, ray.direction, radius, distance, mask, is2D);
    }

    public static NDRaycastHit[] RadiusCastAll(Vector3 origin, Vector3 direction, float radius,
        float distance, LayerMask mask, bool is2D)
    {
        if (is2D)
        {
            RaycastHit2D[] hit2 = Physics2D.CircleCastAll(origin, radius, direction, distance, mask);
            NDRaycastHit[] hits = new NDRaycastHit[hit2.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit2[i]);
            return hits;
        }
        else
        {
            RaycastHit[] hit3 = Physics.SphereCastAll(origin, radius, direction, distance, mask);
            NDRaycastHit[] hits = new NDRaycastHit[hit3.Length];
            for (int i = 0; i < hits.Length; i++)
                hits[i] = new NDRaycastHit(hit3[i], distance);
            return hits;
        }
    }

    public static NDRigidbody ND(this Rigidbody2D rigid)
    {
        return NDRigidbody.ND(rigid);
    }

    public static NDRigidbody ND(this Rigidbody rigid)
    {
        return NDRigidbody.ND(rigid);
    }

    public static NDCollision ND(this Collision2D collision)
    {
        return NDCollision.ND(collision);
    }

    public static NDCollision ND(this Collision collision)
    {
        return NDCollision.ND(collision);
    }

    public static NDCollider ND(this Collider2D collider)
    {
        return NDCollider.ND(collider);
    }

    public static NDCollider ND(this Collider collider)
    {
        return NDCollider.ND(collider);
    }

    public static bool IsNull(this NDCollider collider)
    {
        return (collider == null) || ((collider.col2 == null) && (collider.col3 == null));
    }

    public static float DefaultContactOffset(bool is2D)
    {
        if (is2D) return Physics2D.defaultContactOffset;
        else return Physics.defaultContactOffset;
    }

    //Layer matrix
    private static Dictionary<int, int> masksByLayer;

    private static Dictionary<int, int> masksByLayer2D;

    public static void Init()
    {
        masksByLayer = new Dictionary<int, int>();
        for (int i = 0; i < 32; i++)
        {
            int mask = 0;
            for (int j = 0; j < 32; j++)
            {
                if (!Physics.GetIgnoreLayerCollision(i, j))
                {
                    mask |= 1 << j;
                }
            }
            masksByLayer.Add(i, mask);
        }

        masksByLayer2D = new Dictionary<int, int>();
        for (int i = 0; i < 32; i++)
        {
            int mask = 0;
            for (int j = 0; j < 32; j++)
            {
                if (!Physics2D.GetIgnoreLayerCollision(i, j))
                {
                    mask |= 1 << j;
                }
            }
            masksByLayer2D.Add(i, mask);
        }
    }

    public static int MaskForLayer(int layer, bool is2D)
    {
        if (is2D)
        {
            if (masksByLayer2D == null) Init();
            return masksByLayer2D[layer];
        }
        else
        {
            if (masksByLayer == null) Init();
            return masksByLayer[layer];
        }
    }

    public static int MaskForLayer(NDRigidbody rb)
    {
        return MaskForLayer(rb.layer, rb.is2D);
    }
}

public struct NDRaycastHit
{
    public RaycastHit2D hit2;
    public RaycastHit hit3;
    readonly bool is2D;
    public bool is3D { get { return !is2D; } }
    public bool isTrigger
    {
        get { return collider.isTrigger; }
    }
    public NDCollider collider
    {
        get
        {
            if (is2D) return hit2.collider.ND();
            else return hit3.collider.ND();
        }
    }
    public float distance
    {
        get
        {
            if (is2D) return hit2.distance;
            else return hit3.distance;
        }
    }
    public float fraction;
    public Vector3 normal
    {
        get
        {
            if (is2D) return hit2.normal;
            else return hit3.normal;
        }
    }
    public Vector3 point
    {
        get
        {
            if (is2D) return hit2.point;
            else return hit3.point;
        }
    }
    NDRigidbody _rigidbody;
    public NDRigidbody rigidbody
    {
        get
        {
            if (is2D)
            {
                if (hit2.rigidbody != null)
                {
                    if ((_rigidbody == null) || (_rigidbody.rigid2 != hit2.rigidbody))
                        _rigidbody = hit2.rigidbody.ND();
                    return _rigidbody;
                }
                else return null;
            }
            else
            {
                if (hit3.rigidbody != null)
                {
                    if ((_rigidbody == null) || (_rigidbody.rigid3 != hit3.rigidbody))
                        _rigidbody = hit3.rigidbody.ND();
                    return _rigidbody;
                }
                else return null;
            }
        }
    }
    public Transform transform
    {
        get
        {
            if (is2D) return hit2.transform;
            else return hit3.transform;
        }
    }

    public NDRaycastHit(RaycastHit2D hit)
    {
        hit2 = hit;
        hit3 = default;
        _rigidbody = hit2.rigidbody.ND();
        is2D = true;
        fraction = hit.fraction;
    }

    public NDRaycastHit(RaycastHit hit, float maxDistance)
    {
        hit2 = default;
        hit3 = hit;
        _rigidbody = hit3.rigidbody.ND();
        is2D = false;
        fraction = Mathf.InverseLerp(0f, maxDistance, hit.distance);
    }
}

public class NDCollision
{
    static Dictionary<Collision2D, NDCollision> cols2;
    static Dictionary<Collision, NDCollision> cols3;

    public Collision2D collision2;
    public Collision collision3;
    public bool wasOnStay;
    public bool is2D { get { return collision2 != null; } }
    public bool is3D { get { return collision3 != null; } }

    NDCollider _collider;
    public NDCollider collider
    {
        get
        {
            if (_collider == null)
            {
                if (is2D) _collider = collision2.otherCollider.ND();
                else _collider = collision3.collider.ND();
            }
            return _collider;
        }
    }

    GameObject _gameObject;
    public GameObject gameObject
    {
        get
        {
            if (_gameObject == null)
            {
                if (is2D) _gameObject = collision2.otherCollider.gameObject;
                else _gameObject = collision3.gameObject;
            }
            return _gameObject;
        }
    }
    public int contactCount
    {
        get
        {
            if (is2D) return collision2.contactCount;
            else return collision3.contactCount;
        }
    }
    public NDContactPoint[] contacts
    {
        get
        {
            if (is2D)
            {
                ContactPoint2D[] con2 = new ContactPoint2D[collision2.contactCount];
                collision2.GetContacts(con2);
                NDContactPoint[] result = new NDContactPoint[con2.Length];
                for (int i = 0; i < con2.Length; i++)
                    result[i] = new NDContactPoint(con2[i]);
                return result;
            }
            else
            {
                ContactPoint[] con3 = new ContactPoint[collision3.contactCount];
                collision3.GetContacts(con3);
                NDContactPoint[] result = new NDContactPoint[con3.Length];
                for (int i = 0; i < con3.Length; i++)
                    result[i] = new NDContactPoint(con3[i]);
                return result;
            }
        }
    }

    Transform _transform;
    public Transform transform
    {
        get
        {
            if (_transform == null)
            {
                if (is2D) _transform = collision2.otherCollider.transform;
                else _transform = collision3.transform;
            }
            return transform;
        }
    }
    public Vector3 relativeVelocity
    {
        get
        {
            if (is2D) return collision2.relativeVelocity;
            else return collision3.relativeVelocity;
        }
    }

    NDCollision(Collision2D collision, bool wasOnStay = false)
    {
        collision2 = collision;
        collision3 = null;
        this.wasOnStay = wasOnStay;
    }

    NDCollision(Collision collision, bool wasOnStay = false)
    {
        collision2 = null;
        collision3 = collision;
        this.wasOnStay = wasOnStay;
    }

    public static NDCollision ND(Collision2D collision)
    {
        if (collision == null) return null;

        if (cols2 == null)
        {
            cols2 = new Dictionary<Collision2D, NDCollision>();
            SceneManager.sceneUnloaded += ResetDictionary2;
        }

        NDCollision nd;
        if (!cols2.TryGetValue(collision, out nd))
        {
            nd = new NDCollision(collision);
            cols2.Add(collision, nd);
        }
        return cols2[collision];
    }

    public static NDCollision ND(Collision collision)
    {
        if (collision == null) return null;

        if (cols3 == null)
        {
            cols3 = new Dictionary<Collision, NDCollision>();
            SceneManager.sceneUnloaded += ResetDictionary3;
        }

        NDCollision nd;
        if (!cols3.TryGetValue(collision, out nd))
        {
            nd = new NDCollision(collision);
            cols3.Add(collision, nd);
        }
        else cols3[collision].Reset();

        return cols3[collision];
    }

    static void ResetDictionary2(Scene scene)
    {
        cols2.Clear();
    }

    static void ResetDictionary3(Scene scene)
    {
        cols3.Clear();
    }

    public void Reset()
    {
        _collider = null;
        _gameObject = null;
        _transform = null;
    }
}

public struct NDContactPoint
{
    ContactPoint2D con2;
    ContactPoint con3;
    readonly bool is2D;
    public bool is3D { get { return !is2D; } }
    public NDCollider collider
    {
        get
        {
            if (is2D) return con2.collider.ND();
            else return con3.thisCollider.ND();
        }
    }
    public Vector3 normal
    {
        get
        {
            if (is2D) return con2.normal;
            else return con3.normal;
        }
    }
    public NDCollider otherCollider
    {
        get
        {
            if (is2D) return con2.otherCollider.ND();
            else return con3.otherCollider.ND();
        }
    }
    public Vector3 point
    {
        get
        {
            if (is2D) return con2.point;
            else return con3.point;
        }
    }
    public float separation
    {
        get
        {
            if (is2D) return con2.separation;
            else return con3.separation;
        }
    }

    public NDContactPoint(ContactPoint2D contact)
    {
        con2 = contact;
        con3 = new ContactPoint();
        is2D = true;
    }

    public NDContactPoint(ContactPoint contact)
    {
        con2 = new ContactPoint2D();
        con3 = contact;
        is2D = false;
    }
}
