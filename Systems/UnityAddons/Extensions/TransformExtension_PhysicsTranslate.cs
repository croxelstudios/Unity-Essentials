using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class TransformExtension_PhysicsTranslate
{
    static Dictionary<Transform, NDRigidbody> rigidbodies;

    public static Vector3 PhysicsPosition(this Transform tr)
    {
        if (tr == null)
            return Vector3.zero;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (rigid != null)
            return rigid.position;
        else return tr.position;
    }

    public static Quaternion PhysicsRotation(this Transform tr)
    {
        if (tr == null)
            return Quaternion.identity;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (rigid != null)
            return rigid.rotation;
        else return tr.rotation;
    }

    public static void PhysicsSetPosition(this Transform tr, Vector3 position, bool interpolate = false)
    {
        if (tr == null)
            return;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (rigid != null)
        {
            if (interpolate)
                rigid.MovePosition(position);
            else rigid.position = position;
        }
        else tr.position = position;
    }

    public static void PhysicsSetRotation(this Transform tr, Quaternion rotation, bool interpolate = false)
    {
        if (tr == null)
            return;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (rigid != null)
        {
            if (interpolate)
                rigid.MoveRotation(rotation);
            else rigid.rotation = rotation;
        }
        else tr.rotation = rotation;
    }

    public static void PhysicsTranslate(this Transform tr, float x, float y, float z, Space space = Space.Self, bool teleport = false)
    {
        tr.PhysicsTranslate(new Vector3(x, y, z), teleport, space);
    }

    public static void PhysicsTranslate(this Transform tr, float x, float y, float z, bool teleport, Space space = Space.Self)
    {
        tr.PhysicsTranslate(new Vector3(x, y, z), teleport, space);
    }

    public static void PhysicsTranslate(this Transform tr, float x, float y, float z, Transform relativeTo, bool teleport = false)
    {
        tr.PhysicsTranslate(new Vector3(x, y, z), teleport, relativeTo);
    }

    public static void PhysicsTranslate(this Transform tr, float x, float y, float z, bool teleport, Transform relativeTo)
    {
        tr.PhysicsTranslate(new Vector3(x, y, z), teleport, relativeTo);
    }

    public static void PhysicsTranslate(this Transform tr, Vector3 translation, Space space = Space.Self, bool teleport = false)
    {
        tr.PhysicsTranslate(translation, teleport, space);
    }

    public static void PhysicsTranslate(this Transform tr, Vector3 translation, bool teleport, Space space = Space.Self)
    {
        if (tr == null)
            return;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (rigid != null)
        {
            if (space == Space.Self)
                translation = tr.TransformVector(translation);
            translation += tr.position;
            if (teleport) rigid.position = translation;
            else rigid.MovePosition(translation);
        }
        else tr.DXTranslate(translation, space);
    }

    public static void PhysicsTranslate(this Transform tr, Vector3 translation, Transform relativeTo, bool teleport = false)
    {
        tr.PhysicsTranslate(translation, teleport, relativeTo);
    }

    public static void PhysicsTranslate(this Transform tr, Vector3 translation, bool teleport, Transform relativeTo)
    {
        if (tr == null)
            return;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (rigid != null)
        {
            if (relativeTo != null)
                translation = relativeTo.TransformVector(translation);
            translation += tr.position;
            if (teleport) rigid.position = translation;
            else rigid.MovePosition(translation);
        }
        else tr.Translate(translation, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 axis, float angle, Space space = Space.Self, bool teleport = false)
    {
        tr.PhysicsRotate(Quaternion.AngleAxis(angle, axis), teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 axis, float angle, bool teleport, Space space = Space.Self)
    {
        tr.PhysicsRotate(Quaternion.AngleAxis(angle, axis), teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, float xAngle, float yAngle, float zAngle, Space space = Space.Self, bool teleport = false)
    {
        tr.PhysicsRotate(Quaternion.Euler(xAngle, yAngle, zAngle), teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, float xAngle, float yAngle, float zAngle, bool teleport, Space space = Space.Self)
    {
        tr.PhysicsRotate(Quaternion.Euler(xAngle, yAngle, zAngle), teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 eulerAngles, Space space = Space.Self, bool teleport = false)
    {
        tr.PhysicsRotate(Quaternion.Euler(eulerAngles), teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 eulerAngles, bool teleport, Space space = Space.Self)
    {
        tr.PhysicsRotate(Quaternion.Euler(eulerAngles), teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, Quaternion rotation, Space space = Space.Self, bool teleport = false)
    {
        tr.PhysicsRotate(rotation, teleport, space);
    }

    public static void PhysicsRotate(this Transform tr, Quaternion rotation, bool teleport, Space space = Space.Self)
    {
        tr.PhysicsRotate(rotation, teleport, (space == Space.Self) ? tr : null);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 axis, float angle, Transform relativeTo, bool teleport = false)
    {
        tr.PhysicsRotate(Quaternion.AngleAxis(angle, axis), teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 axis, float angle, bool teleport, Transform relativeTo)
    {
        tr.PhysicsRotate(Quaternion.AngleAxis(angle, axis), teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, float xAngle, float yAngle, float zAngle, Transform relativeTo, bool teleport = false)
    {
        tr.PhysicsRotate(Quaternion.Euler(xAngle, yAngle, zAngle), teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, float xAngle, float yAngle, float zAngle, bool teleport, Transform relativeTo)
    {
        tr.PhysicsRotate(Quaternion.Euler(xAngle, yAngle, zAngle), teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 eulerAngles, Transform relativeTo, bool teleport = false)
    {
        tr.PhysicsRotate(Quaternion.Euler(eulerAngles), teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, Vector3 eulerAngles, bool teleport, Transform relativeTo)
    {
        tr.PhysicsRotate(Quaternion.Euler(eulerAngles), teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, Quaternion rotation, Transform relativeTo, bool teleport = false)
    {
        tr.PhysicsRotate(rotation, teleport, relativeTo);
    }

    public static void PhysicsRotate(this Transform tr, Quaternion rotation, bool teleport, Transform relativeTo)
    {
        if (tr == null)
            return;

        NDRigidbody rigid = tr.GetNDRigidbody();

        if (relativeTo != null) rotation = rotation.Add(relativeTo.rotation);
        else rotation = tr.rotation.Add(rotation);

        if (rigid != null)
        {
            if (teleport) rigid.rotation = rotation;
            else rigid.MoveRotation(rotation);
        }
        else tr.rotation = rotation;
    }

    public static NDRigidbody GetNDRigidbody(this Transform tr)
    {
        TryCreateDictionary();

        NDRigidbody rigid;
        if (!rigidbodies.TryGetValue(tr, out rigid))
        {
            rigid = NDRigidbody.GetNDRigidbodyFrom(tr.gameObject);
            rigidbodies.Add(tr, rigid);
        }

        return rigid;
    }

    static void TryCreateDictionary()
    {
        if (rigidbodies == null)
        {
            rigidbodies = new Dictionary<Transform, NDRigidbody>();
            SceneManager.sceneUnloaded -= SceneUnloaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
    }

    static void SceneUnloaded(Scene scene)
    {
        rigidbodies.Clear();
    }
}
