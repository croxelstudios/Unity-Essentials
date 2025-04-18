using UnityEngine;

public struct BillboardBounds
{
    Vector3 _center;
    public Vector3 center {  get { return _center; } }
    Vector3 _normal;
    public Vector3 normal { get { return _normal; } }
    Vector3 _planeUp;
    public Vector3 planeUp { get { return _planeUp; } }
    Vector3 _planeRight;
    public Vector3 planeRight { get { return _planeRight; } }
    Vector2 _extents;
    public Vector3 extents { get { return _extents; } }

    bool cRT;
    bool cRB;
    bool cLB;
    bool cLT;

    Vector3 _cornerRT;
    public Vector3 cornerRT
    {
        get
        {
            if (!cRT)
            {
                _cornerRT = _center + _extents.InterpretVector2(_normal, _planeUp);
                cRT = true;
            }
            return _cornerRT;
        }
    }
    Vector3 _cornerRB;
    public Vector3 cornerRB
    {
        get
        {
            if (!cRB)
            {
                _cornerRB = _center + _extents.InterpretVector2(-_normal, -_planeUp);
                cRB = true;
            }
            return _cornerRB;
        }
    }
    Vector3 _cornerLB;
    public Vector3 cornerLB
    {
        get
        {
            if (!cLB)
            {
                _cornerLB = _center + _extents.InterpretVector2(_normal, -_planeUp);
                cLB = true;
            }
            return _cornerLB;
        }
    }
    Vector3 _cornerLT;
    public Vector3 cornerLT
    {
        get
        {
            if (!cLT)
            {
                _cornerLT = _center + _extents.InterpretVector2(-_normal, _planeUp);
                cLT = true;
            }
            return _cornerLT;
        }
    }

    public BillboardBounds(Vector3 center, Vector3 normal, Vector3 planeUp, Vector3 extents)
    {
        _center = center;
        _normal = normal.normalized;
        _planeUp = planeUp.normalized;
        _planeRight = Vector3.Cross(normal, planeUp);
        _extents = extents;

        cRT = false;
        cRB = false;
        cLB = false;
        cLT = false;

        _cornerRT = Vector3.zero;
        _cornerRB = Vector3.zero;
        _cornerLB = Vector3.zero;
        _cornerLT = Vector3.zero;
    }

    public BillboardBounds(Bounds bounds, Vector3 direction)
    {
        _center = bounds.center;
        _normal = direction.normalized;

        _planeUp = Vector3.ProjectOnPlane(Vector2.up, direction).normalized;
        _planeRight = Vector3.Cross(_normal, _planeUp);

        _extents = CreateExtents(bounds, direction, _planeUp);

        cRT = false;
        cRB = false;
        cLB = false;
        cLT = false;

        _cornerRT = Vector3.zero;
        _cornerRB = Vector3.zero;
        _cornerLB = Vector3.zero;
        _cornerLT = Vector3.zero;
    }

    public BillboardBounds(Bounds bounds, Vector3 direction, Vector3 upDirection)
    {
        _center = bounds.center;
        _normal = direction.normalized;

        _planeUp = Vector3.ProjectOnPlane(upDirection, direction).normalized;
        _planeRight = Vector3.Cross(_normal, _planeUp);

        _extents = CreateExtents(bounds, direction, _planeUp);

        cRT = false;
        cRB = false;
        cLB = false;
        cLT = false;

        _cornerRT = Vector3.zero;
        _cornerRB = Vector3.zero;
        _cornerLB = Vector3.zero;
        _cornerLT = Vector3.zero;
    }

    public void Draw(Color color)
    {
        Debug.DrawLine(cornerRT, cornerRB, color);
        Debug.DrawLine(cornerRB, cornerLB, color);
        Debug.DrawLine(cornerLB, cornerLT, color);
        Debug.DrawLine(cornerLT, cornerRT, color);
    }

    public Vector3 TransformPoint(Vector2 point)
    {
        return _center + (point.x * _extents.x * _planeRight) + (point.y * _extents.y * _planeUp);
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
