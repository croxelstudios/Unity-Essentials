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

    public static void ViewCone(Transform transform,
        float radius, float angle, Color color, int radialDiv = RADIAL_RAY_DIV)
    {
        ViewCone(transform.position, transform.forward, transform.up, radius, angle, color, radialDiv);
    }

    public static void ViewCone(Vector3 position, Vector3 forward, Vector3 up,
        float radius, float angle, Color color, int radialDiv = RADIAL_RAY_DIV)
    {
        forward = forward.normalized;
        up = Vector3.ProjectOnPlane(up, forward).normalized;

        float angdif = 360f / radialDiv;
        float innerAngdif = angle / INNER_DIV;
        float stepsDif = radius / STEPS_DIV;
        Gizmos.color = color;
        Vector3 prevDir = forward;
        for (int i = 1; i <= radialDiv; i++)
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

        Gizmos.color = Color.white;
    }

    public static void CameraFrustrum(Vector3 position, Quaternion rotation,
        float farClip, float nearClip, float fov, float aspect, Color color)
    {
        Vector3 forward = rotation * Vector3.forward;
        Vector3 up = rotation * Vector3.up;
        Vector3 right = rotation * Vector3.right;
        float tan = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float farHeight = tan * farClip;
        float nearHeight = tan * nearClip;
        float farWidth = farHeight * aspect;
        float nearWidth = nearHeight * aspect;
        Vector3 farTopLeft = position + (forward * farClip) + (up * farHeight) - (right * farWidth);
        Vector3 farTopRight = position + (forward * farClip) + (up * farHeight) + (right * farWidth);
        Vector3 farBottomLeft = position + (forward * farClip) - (up * farHeight) - (right * farWidth);
        Vector3 farBottomRight = position + (forward * farClip) - (up * farHeight) + (right * farWidth);
        Vector3 nearTopLeft = position + (forward * nearClip) + (up * nearHeight) - (right * nearWidth);
        Vector3 nearTopRight = position + (forward * nearClip) + (up * nearHeight) + (right * nearWidth);
        Vector3 nearBottomLeft = position + (forward * nearClip) - (up * nearHeight) - (right * nearWidth);
        Vector3 nearBottomRight = position + (forward * nearClip) - (up * nearHeight) + (right * nearWidth);

        Gizmos.color = color;
        Gizmos.DrawLine(farTopLeft, farTopRight);
        Gizmos.DrawLine(farTopRight, farBottomRight);
        Gizmos.DrawLine(farBottomRight, farBottomLeft);
        Gizmos.DrawLine(farBottomLeft, farTopLeft);
        Gizmos.DrawLine(nearTopLeft, nearTopRight);
        Gizmos.DrawLine(nearTopRight, nearBottomRight);
        Gizmos.DrawLine(nearBottomRight, nearBottomLeft);
        Gizmos.DrawLine(nearBottomLeft, nearTopLeft);
        Gizmos.DrawLine(farTopLeft, nearTopLeft);
        Gizmos.DrawLine(farTopRight, nearTopRight);
        Gizmos.DrawLine(farBottomLeft, nearBottomLeft);
        Gizmos.DrawLine(farBottomRight, nearBottomRight);
        Gizmos.color = Color.white;
    }
#endif
}
