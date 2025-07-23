using UnityEngine;

public static class VectorExtension_DrawArrow
{
    const int RADIAL_DIV = 24;
    const int RADIAL_RAY_DIV = 8;
    const int INNER_DIV = 12;
    const int STEPS_DIV = 2;

    public static void DrawCircle(this Vector3 point, Vector3 normal, Vector3 up, float radius, Color color)
    {
        float angdif = 360f / RADIAL_DIV;

        Vector3 prevDir = up = up * radius;
        for (int i = 1; i <= RADIAL_DIV; i++)
        {
            float ang = angdif * i;
            Vector3 direction = Quaternion.AngleAxis(ang, normal) * up;

            Vector3 origin = point + prevDir;
            Vector3 target = point + direction;
            Debug.DrawLine(origin, target, color);

            prevDir = direction;
        }
    }

    public static void DrawViewCone(this Vector3 position, Vector3 forward, Vector3 up, float radius, float angle, Color color)
    {
        forward = forward.normalized;
        up = Vector3.ProjectOnPlane(up, forward).normalized;

        float angdif = 360f / RADIAL_RAY_DIV;
        float innerAngdif = angle / INNER_DIV;
        float stepsDif = radius / STEPS_DIV;
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
                    Debug.DrawLine(origin, target, color);
                }

                prevDir = direction;
            }

            Debug.DrawRay(position, prevDir * radius, color);
        }

        for (int k = 1; k <= STEPS_DIV; k++)
        {
            float rad = stepsDif * k * Mathf.Cos(angle * Mathf.Deg2Rad);
            DrawCircle(position + (forward * rad), forward, up, rad, color);
        }
    }

    public static void DrawArrowHead(this Vector3 point, Vector3 forward, Vector3 up, float size, float angle, Color color)
    {
        forward = forward.normalized;
        up = Vector3.ProjectOnPlane(up, forward).normalized;

        float angdif = 360f / RADIAL_RAY_DIV;
        Vector3 dir = -forward * size;

        for (int i = 1; i <= RADIAL_RAY_DIV; i++)
        {
            float ang = angdif * i;

            Vector3 axis = Quaternion.AngleAxis(ang, dir) * up;

            Vector3 origin = point + dir + (axis * Mathf.Sin(angle * Mathf.Deg2Rad) * size);
            Vector3 target = point + dir;
            Debug.DrawLine(origin, target, color);

            Debug.DrawLine(point, origin, color);
        }

        DrawCircle(point + dir, forward, up, Mathf.Sin(angle * Mathf.Deg2Rad) * size, color);
    }

    public static void DrawArrow(this Vector3 forward, Vector3 position, Vector3 up, Color color)
    {
        DrawArrowHead(position + forward, forward, up, forward.magnitude * 0.1f, 30f, color);
        Debug.DrawRay(position, forward, color);
    }
}
