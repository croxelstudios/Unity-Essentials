using UnityEngine;

public static class VectorExtension_RotateTowards
{
    public static Vector2 RotateTowards(this Vector2 current, Vector2 target, float speed) //TO DO: Should just use Rotate function
    {
        float aCurrent = Vector2.SignedAngle(Vector2.up, current);
        float aTarget = Vector2.SignedAngle(Vector2.up, target);

        if (aCurrent != aTarget)
        {
            float dif = Vector2.SignedAngle(current, target);
            if (Mathf.Abs(dif) < speed) return target;
            else
            {
                float sign = Mathf.Sign(dif);
                aCurrent = Mathf.Repeat(aCurrent + 180f + (speed * sign), 360f) - 180f;
                if (Mathf.Sign(Vector2.SignedAngle(current, target)) != sign) aCurrent = aTarget;

                return new Vector2(-Mathf.Sin(aCurrent * Mathf.Deg2Rad),
                    Mathf.Cos(aCurrent * Mathf.Deg2Rad)) * current.magnitude;
            }
        }
        else return current;
    }

    //TO DO: 3D Version
}
