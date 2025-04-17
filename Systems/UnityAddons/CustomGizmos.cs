using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CustomGizmos
{
#if UNITY_EDITOR
    const int RADIAL_DIV = 24;
    const int RADIAL_RAY_DIV = 8;
    const int INNER_DIV = 12;
    const int STEPS_DIV = 2;

    public static void Circle(Vector3 point, Vector3 normal, Vector3 up, float radius)
    {
        float angdif = 360f / RADIAL_DIV;

        Vector3 prevDir = up = up * radius;
        for (int i = 1; i <= RADIAL_DIV; i++)
        {
            float ang = angdif * i;
            Vector3 direction = Quaternion.AngleAxis(ang, normal) * up;

            Vector3 origin = point + prevDir;
            Vector3 target = point + direction;
            Gizmos.DrawLine(origin, target);

            prevDir = direction;
        }
    }

    public static void ViewCone(Transform transform, float radius, float angle, Color color)
    {
        ViewCone(transform.position, transform.forward, transform.up, radius, angle, color);
    }

    public static void ViewCone(Vector3 position, Vector3 forward, Vector3 up, float radius, float angle, Color color)
    {
        up = Vector3.ProjectOnPlane(up, forward).normalized;

        float angdif = 360f / RADIAL_RAY_DIV;
        float innerAngdif = angle / INNER_DIV;
        float stepsDif = radius / STEPS_DIV;
        Gizmos.color = color;
        Vector3 prevDir = forward;
        for (int i = 1; i <= RADIAL_RAY_DIV; i++)
        {
            float ang = angdif * i;
            prevDir = forward;
            for (int j = 1; j <= INNER_DIV; j++)
            {
                float innerAng = innerAngdif * j;
                Vector3 axis = Quaternion.AngleAxis(ang, forward) * up;
                Vector3 direction = Quaternion.AngleAxis(innerAng, axis) * forward;

                for (int k = 1; k <= STEPS_DIV; k++)
                {
                    float rad = stepsDif * k;
                    Vector3 origin = position + (prevDir * rad);
                    Vector3 target = position + (direction * rad);
                    Gizmos.DrawLine(origin, target);
                }

                prevDir = direction;
            }

            Gizmos.DrawRay(position, prevDir * radius);
        }

        for (int k = 1; k <= STEPS_DIV; k++)
        {
            float dist = stepsDif * k * Mathf.Cos(angle * Mathf.Deg2Rad);
            float rad = stepsDif * k * Mathf.Sin(angle * Mathf.Deg2Rad);
            Circle(position + (forward * dist), forward, up, rad);
        }
    }
#endif
}
