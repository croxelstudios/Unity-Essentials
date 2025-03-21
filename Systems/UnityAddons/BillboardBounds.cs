using UnityEngine;

public struct BillboardBounds
{
    public Vector3 center;
    public Vector3 normal;
    public Vector3 planeUp;
    public Vector3 planeRight;
    public Vector2 extents;

    public BillboardBounds(Vector3 center, Vector3 normal, Vector3 planeUp, Vector3 extents)
    {
        this.center = center;
        this.normal = normal;
        this.planeUp = planeUp;
        planeRight = Vector3.Cross(normal, planeUp);
        this.extents = extents;
    }

    public BillboardBounds(Bounds bounds, Vector3 direction)
    {
        center = bounds.center;
        normal = direction.normalized;

        planeUp = Vector3.ProjectOnPlane(Vector2.up, direction).normalized;
        planeRight = Vector3.Cross(normal, planeUp);

        extents = CreateExtents(bounds, direction, planeUp);
    }

    public BillboardBounds(Bounds bounds, Vector3 direction, Vector3 upDirection)
    {
        center = bounds.center;
        normal = direction.normalized;

        planeUp = Vector3.ProjectOnPlane(upDirection, direction).normalized;
        planeRight = Vector3.Cross(normal, planeUp);

        extents = CreateExtents(bounds, direction, planeUp);
    }

    public void Draw(Color color)
    {
        Vector3 cornerRT = center + extents.InterpretVector2(normal, planeUp);
        Vector3 cornerRB = center + extents.InterpretVector2(-normal, -planeUp);
        Vector3 cornerLB = center + extents.InterpretVector2(normal, -planeUp);
        Vector3 cornerLT = center + extents.InterpretVector2(-normal, planeUp);

        Debug.DrawLine(cornerRT, cornerRB, color);
        Debug.DrawLine(cornerRB, cornerLB, color);
        Debug.DrawLine(cornerLB, cornerLT, color);
        Debug.DrawLine(cornerLT, cornerRT, color);
    }

    public Vector3 TransformPoint(Vector2 point)
    {
        return center + (point.x * extents.x * planeRight) + (point.y * extents.y * planeUp);
    }

    static Vector3 CreateExtents(Bounds bounds, Vector3 direction, Vector3 upDirection)
    {
        Vector3[] corners = new Vector3[8];
        corners[0] = bounds.extents;
        corners[1] = Vector3.Scale(bounds.extents, new Vector3(1, 1, -1));
        corners[2] = Vector3.Scale(bounds.extents, new Vector3(1, -1, 1));
        corners[3] = Vector3.Scale(bounds.extents, new Vector3(-1, 1, 1));
        corners[4] = -corners[0];
        corners[5] = -corners[1];
        corners[6] = -corners[2];
        corners[7] = -corners[3];

        Bounds bounds2D = new Bounds(Vector3.zero, Vector3.zero);
        for (int i = 0; i < corners.Length; i++)
            bounds2D.Encapsulate(corners[i].InterpretVector3Back(direction, upDirection));

        return bounds2D.extents;
    }

    public static float BillboardRadius(Bounds bounds, Vector3 direction)
    {
        Vector3[] corners = new Vector3[8];
        corners[0] = bounds.extents;
        corners[1] = Vector3.Scale(bounds.extents, new Vector3(1, 1, -1));
        corners[2] = Vector3.Scale(bounds.extents, new Vector3(1, -1, 1));
        corners[3] = Vector3.Scale(bounds.extents, new Vector3(-1, 1, 1));
        corners[4] = -corners[0];
        corners[5] = -corners[1];
        corners[6] = -corners[2];
        corners[7] = -corners[3];

        float max = 0f;
        for (int i = 0; i < corners.Length; i++)
        {
            float mag = Vector3.ProjectOnPlane(corners[i], direction).magnitude;
            if (mag > max) max = mag;
        }

        return max;
    }
}
