using UnityEngine;

public static class VectorExtension_RotateTowards
{
    public static Vector2 RotateTowards(this Vector2 current, Vector2 target, float speed)
    {
        if (current.normalized != target.normalized)
        {
            float dif = Vector2.SignedAngle(current, target);
            if (Mathf.Abs(dif) < speed) return target;
            else return current.Rotate(speed * Mathf.Sign(dif));
        }
        else return current;
    }

    public static Vector3 RotateTowards(this Vector3 current, Vector3 target, float speed)
    {
        if (current.normalized != target.normalized)
        {
            Quaternion dif = Quaternion.FromToRotation(current, target);
            dif.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle < speed) return target;
            else return Quaternion.AngleAxis(speed, axis) * current;
        }
        else return current;
    }

    //TO DO: 4D?
}
