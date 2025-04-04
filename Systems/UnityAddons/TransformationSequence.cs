using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct RotationPath
{
    public Quaternion origin;
    public Quaternion target;
    public Quaternion dif;
    public Quaternion[] path;
    public RotationMode mode;
    public float magnitude;
    public float difAngle;
    public Vector3 difAxis;

    public RotationPath(Quaternion origin, Quaternion target, RotationMode mode = RotationMode.Shortest)
    {
        this.origin = origin;
        this.target = target;
        this.mode = mode;
        dif = target.Subtract(origin, mode);
        dif.ToAngleAxis(out difAngle, out difAxis);
        path = new Quaternion[] { origin, target };
        magnitude = dif.Angle();
    }

    public RotationPath(Quaternion[] rotations, RotationMode mode = RotationMode.Shortest)
    {
        origin = rotations[0];
        target = rotations[rotations.Length - 1];
        this.mode = mode;
        path = rotations;
        dif = target.Subtract(origin, mode);
        dif.ToAngleAxis(out difAngle, out difAxis);
        magnitude = 0f;
        CalculateMagnitude();
    }

    //TO DO
    public void ProjectOnPlane(Vector3 normal)
    {
    }

    public void CalculateMagnitude()
    {
        if (IsComplexPath())
        {
            Quaternion previousCorner = path[0];
            float lengthSoFar = 0f;
            int i = 1;
            while (i < path.Length)
            {
                Quaternion currentCorner = path[i];
                lengthSoFar += currentCorner.AngleDistance(previousCorner);
                previousCorner = currentCorner;
                i++;
            }
            magnitude = lengthSoFar;
        }
        else magnitude = dif.Angle();
    }

    public Quaternion RotationAlong(float distance)
    {
        if (IsComplexPath())
        {
            float tangle;
            Vector3 taxis;

            Quaternion previousCorner = path[0];
            float wholeDist = 0f;
            int i = 1;
            while (i < path.Length)
            {
                Quaternion currentCorner = path[i];

                currentCorner.Subtract(previousCorner).ToAngleAxis(out tangle, out taxis);

                if ((wholeDist + tangle) > distance)
                    return previousCorner.Add(Quaternion.AngleAxis(distance - wholeDist, taxis));
                wholeDist += tangle;
                previousCorner = currentCorner;
                i++;
            }

            path[path.Length - 2].Subtract(previousCorner).ToAngleAxis(out tangle, out taxis);

            return previousCorner.Add(Quaternion.AngleAxis(distance - wholeDist, taxis));
        }
        else return origin.Add(Quaternion.AngleAxis(distance, difAxis));
    }

    public Quaternion DisplacementAlong(float distance)
    {
        Quaternion worldPos = RotationAlong(distance);
        return worldPos.Subtract(origin);
    }

    public Quaternion Direction(float multiplier = 1f)
    {
        if (IsComplexPath())
            return Quaternion.AngleAxis(multiplier, path[1].Subtract(path[0]).Axis());
        else return Quaternion.AngleAxis(multiplier, difAxis);
    }

    public Quaternion SmoothDamp(ref Quaternion currentVelocity, float smoothTime, float maxSpeed, float deltaTime, bool alongPath = true)
    {
        //Limit smoothTime to avoid division by 0f;
        smoothTime = Mathf.Max(0.0001F, smoothTime);

        //Calculate omega and exponent
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
        Quaternion target = origin.Add(Displacement(change, alongPath));
        RotationPath subPath = alongPath ? new RotationPath(target, origin, mode)/*SubPath(change, 0, true)*/ :
            new RotationPath(target, origin, mode);

        float tangle;
        Vector3 taxis;
        currentVelocity.ToAngleAxis(out tangle, out taxis);

        Quaternion temp = Quaternion.AngleAxis(tangle * deltaTime * exp, taxis).Add(
            subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude) * deltaTime) * exp, alongPath));

        Quaternion output = target.Add(temp);

        //Avoid overshoot
        float disp;
        ClosestInPath(output, out disp);
        if (disp > magnitude) output = this.target;

        output.Subtract(origin).ToAngleAxis(out tangle, out taxis);
        currentVelocity = Quaternion.AngleAxis(tangle / deltaTime, taxis);

        return output;
    }

    public Quaternion Displacement(float distance, bool alongPath = true)
    {
        return alongPath ? DisplacementAlong(distance) : Direction(distance);
    }

    public Quaternion Displacement(float distance, float offset, bool alongPath = true)
    {
        return SubPath(magnitude - offset, offset).Displacement(distance, alongPath);
    }

    //TO DO
    public Quaternion ClosestInPath(Quaternion point)
    {
        float d;
        return ClosestInPath(point, out d);
    }

    //TO DO
    public Quaternion ClosestInPath(Quaternion point, out float disp)
    {
        disp = 0f;
        return origin;
    }

    RotationPath SubPath(float distance, float offset, bool invert = false)
    {
        if (IsComplexPath())
        {
            float tangle;
            Vector3 taxis;

            List<Quaternion> corners = new List<Quaternion>();

            Quaternion previousCorner = path[0];
            Quaternion currentCorner = previousCorner;
            float wholeDist = 0f;
            int i = 1;
            while (i < path.Length)
            {
                currentCorner = path[i];
                float dist = currentCorner.AngleDistance(previousCorner);
                wholeDist += dist;
                if (wholeDist > offset)
                {
                    wholeDist = wholeDist - offset;
                    break;
                }
                previousCorner = currentCorner;
                i++;
            }

            previousCorner.Subtract(currentCorner).ToAngleAxis(out tangle, out taxis);

            corners.Add(currentCorner.Add(Quaternion.AngleAxis(wholeDist, taxis)));

            if (wholeDist > distance)
                corners.Add(previousCorner.Add(Quaternion.AngleAxis(wholeDist + distance, taxis)));
            else
            {
                corners.Add(currentCorner);
                distance -= wholeDist;
                previousCorner = currentCorner;
                wholeDist = 0f;
                i++;
                while (i < path.Length)
                {
                    currentCorner = path[i];
                    float dist = currentCorner.AngleDistance(previousCorner);
                    wholeDist += dist;
                    if (wholeDist > distance)
                    {
                        wholeDist = wholeDist - offset;
                        break;
                    }
                    corners.Add(currentCorner);
                    previousCorner = currentCorner;
                    i++;
                }

                currentCorner.Subtract(previousCorner).ToAngleAxis(out tangle, out taxis);

                corners.Add(previousCorner.Add(Quaternion.AngleAxis(wholeDist, taxis)));
            }

            if (invert)
            {
                List<Quaternion> inverted = new List<Quaternion>();
                for (int j = corners.Count - 1; j >= 0; j--)
                    inverted.Add(corners[j]);
                corners.Clear();
                corners = inverted;
            }

            return new RotationPath(corners.ToArray(), mode);
        }
        else
        {
            return invert ?
                new RotationPath(origin.Add(Quaternion.AngleAxis(offset + distance, difAxis)),
                    origin.Add(Quaternion.AngleAxis(offset, difAxis)), mode) :
                new RotationPath(origin.Add(Quaternion.AngleAxis(offset, difAxis)),
                    origin.Add(Quaternion.AngleAxis(offset + distance, difAxis)), mode);
        }
    }

    bool IsComplexPath()
    {
        return (path != null) && (path.Length > 2);
    }
}

public struct MovementPath
{
    public Vector3 origin;
    public Vector3 target;
    public Vector3 dif;
    public Vector3[] path;
    public float magnitude;

    public MovementPath(Vector3 origin, Vector3 target, bool useNavMesh = false, int navMeshAreaMask = -1)
    {
        this.origin = origin;
        this.target = target;
        dif = target - origin;
        path = new Vector3[] { origin, target };
        magnitude = 0f;

        if (useNavMesh)
        {
            NavMeshPath nav = new NavMeshPath();
            NavMesh.CalculatePath(origin, target, navMeshAreaMask, nav);
            path = nav.corners;
        }

        CalculateMagnitude();
    }

    public MovementPath(Vector3[] points)
    {
        origin = points[0];
        target = points[points.Length - 1];
        path = points;
        dif = target - origin;
        magnitude = 0f;
        CalculateMagnitude();
    }

    public void ProjectOnPlane(Vector3 normal)
    {
        origin = Vector3.ProjectOnPlane(origin, normal);
        target = Vector3.ProjectOnPlane(target, normal);
        dif = Vector3.ProjectOnPlane(dif, normal);
        if (IsComplexPath())
            for (int i = 0; i < path.Length; i++)
                path[i] = Vector3.ProjectOnPlane(path[i], normal);
    }

    public void CalculateMagnitude()
    {
        if (IsComplexPath())
        {
            Vector3 previousCorner = path[0];
            float lengthSoFar = 0f;
            int i = 1;
            while (i < path.Length)
            {
                Vector3 currentCorner = path[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
            magnitude = lengthSoFar;
        }
        else magnitude = dif.magnitude;
    }

    public Vector3 PositionAlong(float distance)
    {
        if (IsComplexPath())
        {
            Vector3 previousCorner = path[0];
            float wholeDist = 0f;
            int i = 1;
            while (i < path.Length)
            {
                Vector3 currentCorner = path[i];
                float dist = Vector3.Distance(previousCorner, currentCorner);
                if ((wholeDist + dist) > distance)
                    return previousCorner + ((currentCorner - previousCorner).normalized * (distance - wholeDist));
                wholeDist += dist;
                previousCorner = currentCorner;
                i++;
            }
            return previousCorner + (previousCorner - path[path.Length - 2]).normalized * (distance - wholeDist);
        }
        else return origin + (dif.normalized * distance);
    }

    public Vector3 DisplacementAlong(float distance)
    {
        Vector3 worldPos = PositionAlong(distance);
        return worldPos - origin;
    }

    public Vector3 Direction(float multiplier = 1f)
    {
        if (IsComplexPath())
            return (path[1] - path[0]).normalized * multiplier;
        else return dif.normalized * multiplier;
    }

    public Vector3 SmoothDamp(ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime, bool alongPath = true)
    {
        //Limit smoothTime to avoid division by 0f;
        smoothTime = Mathf.Max(0.0001F, smoothTime);

        //Calculate omega and exponent
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
        Vector3 target = origin + Displacement(change, alongPath);
        //TO DO: This doesn't seem to work because it overshoots the path and so the current method calculates a different one?
        MovementPath subPath = alongPath ? new MovementPath(target, origin, true) /*SubPath(change, 0, true)*/ : new MovementPath(target, origin, false);

        Vector3 temp = (currentVelocity * deltaTime * exp) +
            subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude) * deltaTime) * exp, alongPath);

        Vector3 output = target + temp;

        //Avoid overshoot
        float disp;
        ClosestInPath(output, out disp);
        if (disp > magnitude) output = this.target;

        currentVelocity = (output - origin) / deltaTime;

        return output;
    }

    public Vector3 Displacement(float distance, bool alongPath = true)
    {
        return alongPath ? DisplacementAlong(distance) : Direction(distance);
    }

    public Vector3 Displacement(float distance, float offset, bool alongPath = true)
    {
        return SubPath(magnitude - offset, offset).Displacement(distance, alongPath);
    }

    public Vector3 ClosestInPath(Vector3 point)
    {
        float d;
        return ClosestInPath(point, out d);
    }

    public Vector3 ClosestInPath(Vector3 point, out float disp)
    {
        if (IsComplexPath())
        {
            float dist = Mathf.Infinity;
            float dispAdd = 0f;
            int n = 0;
            Vector3 clos = path[0];
            for (int i = 0; i < path.Length - 1; i++)
            {
                float dispAdd_c;
                Vector3 c = ClosestPointOnSegment(point, path[i], path[i + 1], out dispAdd_c);
                float d = Vector3.Distance(point, c);
                if (d < dist)
                {
                    dispAdd = dispAdd_c;
                    dist = d;
                    clos = c;
                    n = i;
                }
            }

            disp = dispAdd * Vector3.Distance(path[n], path[n + 1]);
            for (int i = 0; i < n; i++)
                disp += Vector3.Distance(path[i], path[i + 1]);

            return clos;
        }
        else return ClosestPointOnSegment(point, origin, target, out disp);
    }

    Vector3 ClosestPointOnLine(Vector3 p, Vector3 l0, Vector3 l1, out float dist)
    {
        Vector3 normal = (l1 - l0).normalized;

        dist = Vector3.Dot(p - l0, normal) / Vector3.Dot(normal, normal);
        return l0 + (normal * dist);
    }

    Vector3 ClosestPointOnSegment(Vector3 p, Vector3 l0, Vector3 l1, out float d)
    {
        Vector3 dir = l1 - l0;
        Vector3 clos = ClosestPointOnLine(p, l0, l1, out d);
        if (d < 0f)
        {
            d = 0f;
            clos = l0;
        }
        else if ((d * d) > dir.sqrMagnitude)
        {
            d = 1f;
            clos = l1;
        }
        return clos;
    }

    MovementPath SubPath(float distance, float offset, bool invert = false)
    {
        if (IsComplexPath())
        {
            List<Vector3> corners = new List<Vector3>();

            Vector3 previousCorner = path[0];
            Vector3 currentCorner = previousCorner;
            float wholeDist = 0f;
            int i = 1;
            while (i < path.Length)
            {
                currentCorner = path[i];
                float dist = Vector3.Distance(previousCorner, currentCorner);
                wholeDist += dist;
                if (wholeDist > offset)
                {
                    wholeDist = wholeDist - offset;
                    break;
                }
                previousCorner = currentCorner;
                i++;
            }
            corners.Add(currentCorner + ((previousCorner - currentCorner).normalized * wholeDist));

            if (wholeDist > distance)
                corners.Add(previousCorner + ((currentCorner - previousCorner).normalized * (wholeDist + distance)));
            else
            {
                corners.Add(currentCorner);
                distance -= wholeDist;
                previousCorner = currentCorner;
                wholeDist = 0f;
                i++;
                while (i < path.Length)
                {
                    currentCorner = path[i];
                    float dist = Vector3.Distance(previousCorner, currentCorner);
                    wholeDist += dist;
                    if (wholeDist > distance)
                    {
                        wholeDist = wholeDist - offset;
                        break;
                    }
                    corners.Add(currentCorner);
                    previousCorner = currentCorner;
                    i++;
                }
                corners.Add(previousCorner + ((currentCorner - previousCorner).normalized * wholeDist));
            }

            if (invert)
            {
                List<Vector3> inverted = new List<Vector3>();
                for (int j = corners.Count - 1; j >= 0; j--)
                    inverted.Add(corners[j]);
                corners.Clear();
                corners = inverted;
            }

            return new MovementPath(corners.ToArray());
        }
        else
        {
            return invert ?
                new MovementPath(origin + (dif.normalized * (offset + distance)), origin + (dif.normalized * offset)) :
                new MovementPath(origin + (dif.normalized * offset), origin + (dif.normalized * (offset + distance)));
        }
    }

    public void Draw()
    {
        for (int i = 0; i < path.Length - 1; i++)
            Debug.DrawLine(path[i], path[i + 1], Color.orange);
    }

    bool IsComplexPath()
    {
        return (path != null) && (path.Length > 2);
    }
}
