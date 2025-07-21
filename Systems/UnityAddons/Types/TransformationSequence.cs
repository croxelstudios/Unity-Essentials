using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface ITransformationSequence
{
    public float GetMagnitude();
    public void ProjectOnPlane(Vector3 normal);
    public float CalculateDistanceTo(int point);
    public void Draw();
    public void Draw(Color color);
}

public struct RotationPath : ITransformationSequence
{
    public RotationMode mode;
    public Quaternion[] path;
    public int count { get; private set; }
    public int last { get; private set; }
    public Quaternion origin { get; private set; }
    public Quaternion target { get; private set; }
    public Quaternion dif { get; private set; }
    public float difAngle { get; private set; }
    public Vector3 difAxis { get; private set; }
    public float magnitude { get; private set; }

    public RotationPath(Quaternion origin, Quaternion target, RotationMode mode = RotationMode.Shortest)
    {
        this.mode = mode;
        path = new Quaternion[] { origin, target };

        count = default;
        last = default;
        this.origin = default;
        this.target = default;
        dif = default;
        difAngle = default;
        difAxis = default;
        magnitude = default;
        CalculateData();
    }

    public RotationPath(Quaternion[] rotations, RotationMode mode = RotationMode.Shortest)
    {
        this.mode = mode;
        path = rotations;

        count = default;
        last = default;
        origin = default;
        target = default;
        dif = default;
        difAngle = default;
        difAxis = default;
        magnitude = default;
        CalculateData();
    }

    void CalculateData()
    {
        count = path.Length;
        last = count - 1;
        origin = path[0];
        target = path[last];
        dif = target.Subtract(origin);
        dif.ToAngleAxis(mode, out float difAngle, out Vector3 difAxis);
        this.difAngle = difAngle;
        this.difAxis = difAxis;
        if (IsComplexPath())
        {
            Quaternion previousCorner = path[0];
            float lengthSoFar = 0f;
            int i = 1;
            while (i < count)
            {
                Quaternion currentCorner = path[i];
                lengthSoFar += currentCorner.AngleDistance(previousCorner);
                previousCorner = currentCorner;
                i++;
            }
            magnitude = lengthSoFar;
        }
        else magnitude = dif.Angle(mode);
        if (magnitude == 360f)
            magnitude = 0f;
    }

    public void AddRotation(Quaternion rotation)
    {
        path = path.Resize(count + 1, rotation);
        CalculateData();
    }

    public void ReplaceRotation(int id, Quaternion rotation)
    {
        path[id] = rotation;
        CalculateData();
    }

    public float GetMagnitude()
    {
        return magnitude;
    }

    public void ProjectOnPlane(Vector3 normal)
    {
        for (int i = 0; i < count; i++)
            path[i] = Quaternion.AngleAxis(path[i].Angle(mode), normal);
        CalculateData();
    }

    public Quaternion PredictNext(float multiplier = 1f)
    {
        if (IsComplexPath())
            return path.PredictNext(multiplier);
        else return target.Add(dif);
    }

    public float CalculateDistanceTo(int point)
    {
        if (IsComplexPath())
        {
            Quaternion previousCorner = path[0];
            float lengthSoFar = 0f;
            int i = 1;
            while (i <= point)
            {
                Quaternion currentCorner = path[i];
                lengthSoFar += currentCorner.Angle(mode, previousCorner);
                previousCorner = currentCorner;
                i++;
            }
            return lengthSoFar;
        }
        else return (point > 0) ? magnitude : 0f;
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
            while (i < count)
            {
                Quaternion currentCorner = path[i];

                currentCorner.Subtract(previousCorner).ToAngleAxis(mode, out tangle, out taxis);

                if ((wholeDist + tangle) > distance)
                    return previousCorner.Add(Quaternion.AngleAxis(distance - wholeDist, taxis));
                wholeDist += tangle;
                previousCorner = currentCorner;
                i++;
            }

            path[count - 2].Subtract(previousCorner).ToAngleAxis(mode, out tangle, out taxis);

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
            return Quaternion.AngleAxis(multiplier, path[1].Subtract(path[0]).Axis(mode));
        else return Quaternion.AngleAxis(multiplier, difAxis);
    }

    public Vector3 DirectionAxis()
    {
        if (IsComplexPath())
            return path[1].Subtract(path[0]).Axis(mode);
        else return difAxis;
    }

    public Quaternion SmoothDamp(ref Vector3 angularVelocity, float smoothTime, float maxSpeed, float deltaTime, bool alongPath = true)
    {
        //Limit smoothTime to avoid division by 0f;
        smoothTime = Mathf.Max(0.0001f, smoothTime);

        //Calculate omega and exponent
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
        Quaternion target = origin.Add(Displacement(change, alongPath));
        RotationPath subPath = alongPath ? SubPath(change, magnitude - change, true) :
            new RotationPath(target, origin, mode);

        float tangle = angularVelocity.magnitude;
        Vector3 taxis = angularVelocity / tangle;

        Quaternion temp = Quaternion.AngleAxis(tangle * deltaTime * exp, taxis).Add(
            subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude) * deltaTime) * exp, alongPath));

        Quaternion output = target.Add(temp);

        //Avoid overshoot
        ClosestInPath(output, out float disp);
        if (disp > magnitude) output = this.target;

        output.Subtract(origin).ToAngleAxis(RotationMode.Shortest, out tangle, out taxis);
        angularVelocity = taxis * ((deltaTime > 0f) ? tangle / deltaTime : tangle);

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

    public Quaternion ClosestInPath(Quaternion point)
    {
        return ClosestInPath(point, out float d);
    }

    //TO DO
    public Quaternion ClosestInPath(Quaternion point, out float disp)
    {
        disp = 0f;
        return origin;
    }

    public RotationPath SubPath(float distance, float offset, bool invert = false)
    {
        if (IsComplexPath())
        {
            List<Quaternion> corners = new List<Quaternion>();

            Quaternion previousCorner = path[0];
            Quaternion currentCorner = previousCorner;
            float wholeDist = 0f;
            int i = 1;
            while (i < count)
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

            previousCorner.Subtract(currentCorner).ToAngleAxis(mode, out float tangle, out Vector3 taxis);

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
                while (i < count)
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

                currentCorner.Subtract(previousCorner).ToAngleAxis(mode, out tangle, out taxis);

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

    public void Draw()
    {
        Draw(Color.red);
    }

    public void Draw(Color color)
    {
        for (int i = 0; i < last; i++)
            Debug.DrawLine(path[i] * Vector3.forward, path[i + 1] * Vector3.forward, color);
    }

    bool IsComplexPath()
    {
        return (path != null) && (count > 2);
    }
}

public struct MovementPath : ITransformationSequence
{
    public Vector3[] path;
    public int count { get; private set; }
    public int last { get; private set; }
    public Vector3 origin { get; private set; }
    public Vector3 target { get; private set; }
    public Vector3 dif { get; private set; }
    public float magnitude { get; private set; }

    public enum SmoothMode { AlongPath, NavMesh, Direct }

    public MovementPath(Vector3 origin, Vector3 target, bool useNavMesh = false, int navMeshAreaMask = 0, int navMeshAgentType = 0)
    {
        path = new Vector3[] { origin, target };

        if (useNavMesh)
        {
            NavMeshPath nav = new NavMeshPath();
            NavMeshQueryFilter filter = new NavMeshQueryFilter();
            filter.agentTypeID = navMeshAgentType;
            filter.areaMask = navMeshAreaMask;
            NavMesh.CalculatePath(origin, target, filter, nav);
            path = nav.corners;
        }

        count = default;
        last = default;
        this.origin = default;
        this.target = default;
        dif = default;
        magnitude = default;

        CalculateData();
    }

    public MovementPath(Vector3[] points)
    {
        path = points;

        count = default;
        last = default;
        origin = default;
        target = default;
        dif = default;
        magnitude = default;

        CalculateData();
    }

    void CalculateData()
    {
        count = path.Length;
        last = count - 1;
        origin = path[0];
        target = path[last];
        dif = target - origin;
        if (IsComplexPath())
            magnitude = CalculateDistanceTo(last);
        else magnitude = dif.magnitude;
    }

    public void AddPoint(Vector3 point)
    {
        path = path.Resize(count + 1, point);
        CalculateData();
    }

    public void AddPointKillOld(Vector3 point, int maxPoints)
    {
        if (count < maxPoints)
            AddPoint(point);
        else AddPointKillOld(point);
    }

    public void AddPointKillOld(Vector3 point)
    {
        for (int i = 1; i < count; i++)
            path[i - 1] = path[i];
        path[last] = point;
        CalculateData();
    }

    public void ReplacePoint(int id, Vector3 point)
    {
        path[id] = point;
        CalculateData();
    }

    public float GetMagnitude()
    {
        return magnitude;
    }

    public void ProjectOnPlane(Vector3 normal)
    {
        for (int i = 0; i < count; i++)
            path[i] = Vector3.ProjectOnPlane(path[i], normal);
        CalculateData();
    }

    public Vector3 PredictNext(float multiplier = 1f)
    {
        if (IsComplexPath())
            return path.PredictNext(multiplier);
        else return target + dif;
    }

    public float CalculateDistanceTo(int point)
    {
        if (IsComplexPath())
        {
            Vector3 previousCorner = path[0];
            float lengthSoFar = 0f;
            int i = 1;
            while (i <= point)
            {
                Vector3 currentCorner = path[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
            return lengthSoFar;
        }
        else return (point > 0) ? magnitude : 0f;
    }

    public Vector3 PositionAlong(float distance)
    {
        if (IsComplexPath())
        {
            Vector3 previousCorner = path[0];
            float wholeDist = 0f;
            int i = 1;
            while (i < count)
            {
                Vector3 currentCorner = path[i];
                float dist = Vector3.Distance(previousCorner, currentCorner);
                if ((wholeDist + dist) > distance)
                    return previousCorner + ((currentCorner - previousCorner).normalized * (distance - wholeDist));
                wholeDist += dist;
                previousCorner = currentCorner;
                i++;
            }
            return previousCorner + (previousCorner - path[last - 1]).normalized * (distance - wholeDist);
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

    public Vector3 SmoothDamp(ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime, SmoothMode smoothMode = SmoothMode.AlongPath)
    {
        //Limit smoothTime to avoid division by 0f;
        smoothTime = Mathf.Max(0.0001F, smoothTime);

        //Calculate omega and exponent
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
        Vector3 target = origin + Displacement(change, smoothMode != SmoothMode.Direct);
        MovementPath subPath = (smoothMode != SmoothMode.Direct) ?
            ((smoothMode == SmoothMode.NavMesh) ? new MovementPath(target, origin, true) :
            SubPath(change, magnitude - change, true)) :
            new MovementPath(target, origin, false);

        Vector3 temp = (currentVelocity * deltaTime * exp) +
            subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude) * deltaTime) * exp,
            smoothMode != SmoothMode.Direct);

        Vector3 output = target + temp;

        //Avoid overshoot
        ClosestInPath(output, out float disp);
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
        return ClosestInPath(point, out float d);
    }

    public Vector3 ClosestInPath(Vector3 point, out float disp)
    {
        if (IsComplexPath())
        {
            float dist = Mathf.Infinity;
            float dispAdd = 0f;
            int n = 0;
            Vector3 clos = path[0];
            for (int i = 0; i < last; i++)
            {
                Vector3 c = point.ClosestPointOnSegment(path[i], path[i + 1], out float dispAdd_c);
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
        else return point.ClosestPointOnSegment(origin, target, out disp);
    }

    public MovementPath SubPath(float distance, float offset, bool invert = false)
    {
        if (IsComplexPath())
        {
            List<Vector3> corners = new List<Vector3>();

            Vector3 previousCorner = path[0];
            Vector3 currentCorner = previousCorner;
            float wholeDist = 0f;
            int i = 1;
            while (i < count)
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
                while (i < count)
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
        Draw(Color.red);
    }

    public void Draw(Color color)
    {
        for (int i = 0; i < last; i++)
            Debug.DrawLine(path[i], path[i + 1], color);
    }

    bool IsComplexPath()
    {
        return (path != null) && (count > 2);
    }
}
