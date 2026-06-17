using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface ITransformationSequence
{
    public float magnitude { get; }

    public float CalculateDistanceTo(int point);

    public void Draw();

    public void Draw(Color color);
}

public class BGenericPath<T, D> : ITransformationSequence where T : IEquatable<T>
{
    public T[] path;
    public int count { get; protected set; }
    public int last { get; protected set; }
    public T origin { get; protected set; }
    public T target { get; protected set; }
    public T dif { get; protected set; }
    public float difLength { get; protected set; }
    public D difDirection { get; protected set; }
    public float magnitude { get; protected set; }
    protected bool isComplexPath;
    bool usingBezier_L;
    bool usingBezier_R;
    bool usingBezier_Q;
    bool usingBeziers { get { return usingBezier_L || usingBezier_R || usingBezier_Q; } }

    Dictionary<int, T> lBezier;
    Dictionary<int, T> rBezier;
    Dictionary<int, T> qBezier;

    Dictionary<int, Dictionary<string, object>> pointsData;
    Dictionary<(int, int), FrameLength> frameLengths;
    struct FrameLength : IEquatable<FrameLength>
    {
        public int frame;
        public float length;

        public FrameLength(int frame, float length)
        {
            this.frame = frame;
            this.length = length;
        }

        public override bool Equals(object other)
        {
            if (!(other is FrameLength)) return false;
            return Equals((FrameLength)other);
        }

        public bool Equals(FrameLength other)
        {
            return (frame == other.frame)
                && (length == other.length);
        }

        public override int GetHashCode()
        {
            return HashMaker.Elements(frame, length);
        }

        public static bool operator ==(FrameLength o1, FrameLength o2)
        {
            return o1.Equals(o2);
        }

        public static bool operator !=(FrameLength o1, FrameLength o2)
        {
            return !o1.Equals(o2);
        }
    }

    #region Init
    public BGenericPath()
    {
        path = new T[2];
        CalculateData();
    }

    public BGenericPath(T origin, T target)
    {
        path = new T[] { origin, target };
        CalculateData();
    }

    public BGenericPath(T[] values)
    {
        path = values;
        CalculateData();
    }

    public virtual void Set(T origin, T target)
    {
        ClearDictionaries();
        path = new T[] { origin, target };
        CalculateData();
    }

    public virtual void Set(T[] values)
    {
        ClearDictionaries();
        path = values;
        CalculateData();
    }

    protected virtual void CalculateData()
    {
        count = path.Length;
        last = count - 1;
        origin = path[0];
        target = path[last];
        dif = Generics.Subtract(target, origin);
        DirectionMagnitude(dif, out D difDirection, out float difLength);
        this.difLength = difLength;
        this.difDirection = difDirection;
        isComplexPath = (path != null) && (count > 2);
        if (isComplexPath)
            magnitude = CalculateDistanceTo(last);
        else magnitude = difLength;
    }
    #endregion

    #region Path points management
    public void AddPoint(T point)
    {
        path = path.Resize(count + 1, point);
        CalculateData();
    }

    public void AddPointKillOld(T point, int maxPoints)
    {
        if (count < maxPoints)
            AddPoint(point);
        else AddPointKillOld(point);
    }

    public void AddPointKillOld(T point)
    {
        for (int i = 1; i < count; i++)
            path[i - 1] = path[i];
        path[last] = point;

        if (lBezier != null)
            for (int i = 1; i < count; i++)
                if (lBezier.TryGetValue(i, out T bezier))
                {
                    lBezier.Set(i - 1, bezier);
                    lBezier.Remove(i);
                }

        if (rBezier != null)
            for (int i = 1; i < count; i++)
                if (rBezier.TryGetValue(i, out T bezier))
                {
                    rBezier.Set(i - 1, bezier);
                    rBezier.Remove(i);
                }

        if (pointsData != null)
            for (int i = 1; i < count; i++)
                if (pointsData.TryGetValue(i, out Dictionary<string, object> data))
                {
                    pointsData.Set(i - 1, data);
                    pointsData.Remove(i);
                }

        CalculateData();
    }

    public void ReplacePoint(int id, T point)
    {
        path[id] = point;
        CalculateData();
    }
    #endregion

    #region Bezier data
    public bool GetBezier_L(int id, out T bezier)
    {
        if (lBezier.SmartGetValue(id, out bezier))
            return true;
        else return false;
    }

    public bool GetBezier_R(int id, out T bezier)
    {
        if (rBezier.SmartGetValue(id, out bezier))
            return true;
        else return false;
    }

    public bool GetBezier_Q(int id, out T bezier)
    {
        if (qBezier.SmartGetValue(id, out bezier))
            return true;
        else return false;
    }

    public void SetBezier_L(int id, T bezier)
    {
        lBezier = lBezier.CreateAdd(id, bezier);
        usingBezier_L = true;
    }

    public void SetBezier_R(int id, T bezier)
    {
        rBezier = rBezier.CreateAdd(id, bezier);
        usingBezier_R = true;
    }

    public void SetBezier(int id, T bezier)
    {
        qBezier = qBezier.CreateAdd(id, bezier);
        usingBezier_Q = true;
    }

    public void SetBezier(int id, T lBezier, T rBezier)
    {
        SetBezier_L(id, lBezier);
        SetBezier_R(id, rBezier);
    }

    public void ClearBezier_L(int id)
    {
        lBezier.SmartRemove(id);
        if (lBezier.IsNullOrEmpty()) usingBezier_L = false;
    }

    public void ClearBezier_R(int id)
    {
        rBezier.SmartRemove(id);
        if (rBezier.IsNullOrEmpty()) usingBezier_R = false;
    }

    public void ClearBezier(int id)
    {
        ClearBezier_L(id);
        ClearBezier_R(id);

        qBezier.SmartRemove(id);
        if (qBezier.IsNullOrEmpty()) usingBezier_Q = false;
    }

    public void SetBeziers_L(Dictionary<int, T> lBeziers)
    {
        lBezier = lBezier.CreateAddRange(lBeziers);
        usingBezier_L = !lBezier.IsNullOrEmpty();
    }

    public void SetBeziers_R(Dictionary<int, T> rBeziers)
    {
        rBezier = rBezier.CreateAddRange(rBeziers);
        usingBezier_R = !rBezier.IsNullOrEmpty();
    }

    public void SetBeziers(Dictionary<int, T> qBeziers)
    {
        qBezier = qBezier.CreateAddRange(qBeziers);
        usingBezier_Q = !qBezier.IsNullOrEmpty();
    }

    public void SetBeziers(Dictionary<int, T> lBeziers, Dictionary<int, T> rBeziers)
    {
        SetBeziers_L(lBeziers);
        SetBeziers_R(rBeziers);
    }

    public void ClearBeziers_L()
    {
        lBezier.Clear();
        lBezier = null;
        usingBezier_L = false;
    }

    public void ClearBeziers_R()
    {
        rBezier.Clear();
        rBezier = null;
        usingBezier_R = false;
    }

    public void ClearBeziers()
    {
        ClearBeziers_L();
        ClearBeziers_R();

        qBezier.Clear();
        qBezier = null;
        usingBezier_Q = false;
    }
    #endregion

    #region Points extra data system
    public DataType ExtractPointData<DataType>(int id, string identifier)
    {
        if (pointsData.SmartGetValue(id, identifier, out object data))
            return (DataType)data;
        else return default;
    }

    public void RegisterPointData<DataType>(int id, string identifier, DataType data)
    {
        pointsData = pointsData.CreateAdd(id, identifier, data);
    }

    public void RemovePointData(int id, string identifier)
    {
        pointsData.SmartRemove(id, identifier);
    }

    public Dictionary<string, object> ExtractPointData(int id)
    {
        if (pointsData.SmartGetValue(id, out Dictionary<string, object> data))
            return data;
        else return null;
    }

    public void RegisterPointData(int id, Dictionary<string, object> allData)
    {
        pointsData = pointsData.CreateAdd(id, allData);
    }

    public void RemovePointData(int id)
    {
        pointsData.SmartRemove(id);
    }

    public void RegisterPointsData(Dictionary<int, Dictionary<string, object>> allData)
    {
        pointsData = allData;
    }

    public void ClearPointsData()
    {
        pointsData.Clear();
        pointsData = null;
    }
    #endregion

    #region Features
    public float CalculateDistanceTo(int value)
    {
        if (isComplexPath)
        {
            int prev = 0;
            float lengthSoFar = 0f;
            int i = 1;
            while (i <= value)
            {
                lengthSoFar += IntervalLength(prev, i);
                prev = i;
                i++;
            }
            return lengthSoFar;
        }
        else return (value > 0) ? magnitude : 0f;
    }

    public T PredictNext(float multiplier = 1f)
    {
        if (isComplexPath)
            return ValueArrayExtension_PredictNext.PredictNext(path, multiplier);
        else return Generics.Add(target, dif);
    }

    public T Along(float distance, bool loop = false)
    {
        if (isComplexPath)
        {
            int prev = 0;
            float wholeDist = 0f;
            int i = 1;
            while (i < count)
            {
                float tlength = IntervalLength(prev, i);

                if ((wholeDist + tlength) > distance)
                    return AlongInterval(prev, i, distance - wholeDist);
                wholeDist += tlength;
                prev = i;
                i++;
                if (loop && (i >= count))
                    i -= count;
            }

            return AlongInterval(prev, last - 1, distance - wholeDist);
        }
        else return AlongInterval(0, 1, loop ? Mathf.PingPong(distance, magnitude) : distance);
    }

    public T AlongDelta(float distance, bool loop = false)
    {
        T worldPos = Along(distance, loop);
        return Generics.Subtract(worldPos, origin);
    }

    public D Direction()
    {
        if (isComplexPath)
            return IntervalDirection(0, 1);
        else return difDirection;
    }

    public T DirectionDisplacement(float multiplier = 1f)
    {
        return Generics.FromDirectionMagnitude<T, D>(Direction(), multiplier);
    }

    public T Displacement(float distance, bool alongPath = true)
    {
        return alongPath ? AlongDelta(distance) : DirectionDisplacement(distance);
    }

    public T Displacement(float distance, float offset, bool alongPath = true)
    {
        return SubPath(magnitude - offset, offset).Displacement(distance, alongPath);
    }

    public T SmoothDamp(ref D currentVelocity, float smoothTime, float maxSpeed, float deltaTime, bool alongPath = true)
    {
        //Limit smoothTime to avoid division by 0f;
        smoothTime = Mathf.Max(0.0001F, smoothTime);

        //Calculate omega and exponent
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float change = Mathf.Min(maxSpeed * smoothTime, magnitude);
        T target = Generics.Add(origin, Displacement(change, alongPath));
        BGenericPath<T, D> subPath = alongPath ? SmoothDamp_Subpath(change) : NewPath(target, origin);

        D tdir;
        float tlength;
        Generics.DirectionMagnitude(currentVelocity, out tdir, out tlength);

        T temp = Generics.Add(
            Generics.FromDirectionMagnitude<T, D>(tdir, tlength * deltaTime * exp),
            subPath.Displacement((subPath.magnitude + (omega * subPath.magnitude * deltaTime)) * exp, alongPath)
            );

        T output = Generics.Add(target, temp);

        //Avoid overshoot
        change = SmoothDamp_CorrectOvershoot(output, change);
        if (change > magnitude) output = this.target;

        Generics.DirectionMagnitude(Generics.Subtract(output, origin), out tdir, out tlength);
        currentVelocity = Generics.Scale(tdir, (deltaTime > 0f) ? (tlength / deltaTime) : tlength);

        return output;
    }

    public BGenericPath<T, D> SubPath(float distance, float offset, bool invert = false)
    {
        if (isComplexPath)
        {
            List<T> corners = new List<T>();

            int prev = 0;
            int curr = prev;
            float wholeDist = 0f;
            int i = 1;
            while (i < count)
            {
                curr = i;
                float dist = IntervalLength(prev, curr);
                wholeDist += dist;
                if (wholeDist > offset)
                {
                    wholeDist = wholeDist - offset;
                    break;
                }
                prev = curr;
                i++;
            }

            T currentCorner = path[curr];
            T previousCorner = path[prev];

            Dictionary<int, T> lBeziers = new Dictionary<int, T>();
            Dictionary<int, T> rBeziers = new Dictionary<int, T>();
            Dictionary<int, Dictionary<string, object>> subPointsData =
                new Dictionary<int, Dictionary<string, object>>();

            corners.Add(AlongInterval(curr, prev, wholeDist));
            //TO DO: Add interpolated beziers and point data when possible

            if (wholeDist > distance)
                corners.Add(AlongInterval(prev, curr, wholeDist + distance));
            //TO DO: Add interpolated beziers and point data when possible
            else
            {
                corners.Add(currentCorner);
                distance -= wholeDist;
                prev = curr;
                wholeDist = 0f;
                i++;
                while (i < count)
                {
                    curr = i;
                    float dist = IntervalLength(prev, curr);
                    wholeDist += dist;
                    if (wholeDist > distance)
                    {
                        wholeDist = wholeDist - offset;
                        break;
                    }

                    corners.Add(path[curr]);
                    if (lBezier.SmartGetValue(curr, out T bezier))
                        lBeziers.Add(corners.Count - 1, bezier);
                    if (rBezier.SmartGetValue(curr, out bezier))
                        rBeziers.Add(corners.Count - 1, bezier);
                    if (pointsData.SmartGetValue(curr, out Dictionary<string, object> data))
                        subPointsData.Add(corners.Count - 1, data);

                    prev = curr;
                    i++;
                }

                corners.Add(AlongInterval(prev, curr, wholeDist));
                //TO DO: Add interpolated beziers and point data when possible
            }

            if (invert)
            {
                List<T> inverted = new List<T>();
                for (int j = corners.Count - 1; j >= 0; j--)
                    inverted.Add(corners[j]);
                corners.Clear();
                corners = inverted;
            }

            BGenericPath<T, D> subPath = NewPath(corners.ToArray());
            if (lBeziers.Count > 0)
                subPath.SetBeziers_L(lBeziers);
            if (lBeziers.Count > 0)
                subPath.SetBeziers_R(rBeziers);
            if (subPointsData.Count > 0)
                subPath.RegisterPointsData(subPointsData);
            return subPath;
        }
        else
        {
            BGenericPath<T, D> subPath;
            if (invert)
            {
                subPath = NewPath(
                    AlongInterval(0, 1, offset + distance),
                    AlongInterval(0, 1, offset));
                //TO DO: Add interpolated beziers and point data when possible
            }
            else
            {
                subPath = NewPath(
                    AlongInterval(0, 1, offset),
                    AlongInterval(0, 1, offset + distance));
                //TO DO: Add interpolated beziers and point data when possible
            }
            return subPath;
        }
    }

    public T ClosestInPath(T point, int lastPoint = 0)
    {
        return ClosestInPath(point, out float d, lastPoint);
    }

    public T ClosestInPath(T point, out float disp, int lastPoint = 0)
    {
        return ClosestInPath(point, out disp, out int interval, lastPoint);
    }

    public T ClosestInPath(T point, out int interval, int lastPoint = 0)
    {
        return ClosestInPath(point, out float disp, out interval, lastPoint);
    }

    public T ClosestInPath(T point, out float disp, out int interval, int lastPoint = 0)
    {
        interval = 0;
        if (isComplexPath)
        {
            if (lastPoint <= 0) lastPoint = last;

            float dist = Mathf.Infinity;
            disp = 0f;
            T clos = path[0];
            for (int i = 0; i < (lastPoint - 1); i++)
            {
                T c = ClosestOnInterval(point, i, i + 1, out float dispAdd);
                float d = Distance(point, c);
                if (d < dist)
                {
                    disp = dispAdd;
                    dist = d;
                    clos = c;
                    interval = i;
                }
            }

            for (int i = 0; i < interval; i++)
                disp += IntervalLength(i, i + 1);

            return clos;
        }
        else return ClosestOnInterval(point, 0, last, out disp);
    }
    #endregion

    #region Debug
    public void Draw()
    {
        Draw(Color.red);
    }

    public virtual void Draw(Color color)
    {

    }
    #endregion

    #region Overridable
    protected virtual D IntervalDirection(int a, int b)
    {
        if (usingBeziers)
        {
            Dictionary<int, T> bez = (a > b) ? lBezier : rBezier;
            if (bez.SmartGetValue(a, out T bezier) || qBezier.SmartGetValue(a, out bezier))
                return Generics.Direction<T, D>(bezier);
        }

        return isComplexPath ?
            Generics.Direction<T, D>(Generics.Subtract(path[b], path[a])) : difDirection;
    }

    bool IntervalBezierData(int a, int b, out T start, out T controlA, out T controlB, out T end,
        out bool useQuadratic)
    {
        controlA = start = path[a];
        controlB = end = path[b];
        useQuadratic = false;
        if (!usingBeziers)
            return false;

        int first;
        int last;
        if (a < b) { first = a; last = b; }
        else { first = b; last = a; }

        bool inverted = a > b;

        bool hasL = false;
        bool hasR = useQuadratic = qBezier.SmartGetValue(first, out T rBez);
        T lBez = Default<T>.Value;
        if (!useQuadratic)
        {
            hasR = rBezier.SmartGetValue(first, out rBez);
            hasL = lBezier.SmartGetValue(last, out lBez);
        }

        if (hasR || hasL)
        {
            T firstP = inverted ? end : start;
            controlA = hasR ? Generics.Add(firstP, rBez) : firstP;
            T lastP = inverted ? start : end;
            controlB = useQuadratic ? controlA : (hasL ? Generics.Add(lastP, lBez) : lastP);

            if (inverted)
            {
                T temp = controlA;
                controlA = controlB;
                controlB = temp;
            }

            return true;
        }
        else return false;
    }

    float IntervalLength(int a, int b)
    {
        if (frameLengths.SmartGetValue((a, b), out FrameLength value) && (value.frame == Time.frameCount))
            return value.length;
        else
        {
            float length = CalculateIntervalLength(a, b);
            frameLengths = frameLengths.CreateAdd((a, b), new FrameLength(Time.frameCount, length));
            frameLengths.Add((b, a), new FrameLength(Time.frameCount, length));
            return length;
        }
    }

    protected virtual float CalculateIntervalLength(int a, int b)
    {
        if (IntervalBezierData(a, b, out T start, out T controlA, out T controlB, out T end, out bool useQuadratic))
            return useQuadratic ?
                BezierTools.QuadraticBezier_Length(start, controlA, end) :
                BezierTools.CubicBezier_Length(start, controlA, controlB, end);

        return isComplexPath ? Distance(path[a], path[b]) : difLength;
    }

    protected virtual T ClosestOnInterval(T point, int a, int b, out float disp)
    {
        if (IntervalBezierData(a, b, out T start, out T controlA, out T controlB, out T end, out bool useQuadratic))
        {
            T result = useQuadratic ?
                BezierTools.QuadraticBezier_Closest(point, start, controlA, end, out disp) :
                BezierTools.CubicBezier_Closest(point, start, controlA, controlB, end, out disp);
            disp *= IntervalLength(a, b);
            return result;
        }

        return VectorExtension_ClosestPointOnSegment.
            ClosestPointOnSegment(point, path[a], path[b], out disp);
    }

    protected virtual T AlongInterval(int a, int b, float distance)
    {
        if (IntervalBezierData(a, b, out T start, out T controlA, out T controlB, out T end, out bool useQuadratic))
            return useQuadratic ?
                BezierTools.QuadraticBezier(start, controlA, end, distance / IntervalLength(a, b)) :
                BezierTools.CubicBezier(start, controlA, controlB, end, distance / IntervalLength(a, b));

        D direction = IntervalDirection(a, b);
        return Generics.Add(path[a], Generics.FromDirectionMagnitude<T, D>(direction, distance));
    }

    protected virtual float Distance(T a, T b)
    {
        return Generics.Distance(a, b);
    }

    protected virtual void DirectionMagnitude(T value, out D direction, out float magnitude)
    {
        Generics.DirectionMagnitude(value, out direction, out magnitude);
    }

    protected virtual BGenericPath<T, D> NewPath(T origin, T target)
    {
        return new BGenericPath<T, D>(origin, target);
    }

    protected virtual BGenericPath<T, D> NewPath(T[] points)
    {
        return new BGenericPath<T, D>(points);
    }

    protected virtual BGenericPath<T, D> SmoothDamp_Subpath(float change)
    {
        return SubPath(change, magnitude - change, true);
    }

    protected virtual float SmoothDamp_CorrectOvershoot(T output, float change)
    {
        return change;
    }
    #endregion

    public void ClearDictionaries()
    {
        pointsData.SmartClear();
        frameLengths.SmartClear();
        lBezier.SmartClear();
        usingBezier_L = false;
        rBezier.SmartClear();
        usingBezier_R = false;
        qBezier.SmartClear();
        usingBezier_Q = false;
    }
}

public class ValuePath : BGenericPath<float, float>
{
    public ValuePath() : base() { }

    public ValuePath(float origin, float target) : base(origin, target) { }

    public ValuePath(float[] values) : base(values) { }

    protected override float ClosestOnInterval(float point, int a, int b, out float disp)
    {
        disp = Mathf.Clamp(path[a], path[b], point);
        return disp;
    }

    public override void Draw(Color color)
    {
        for (int i = 0; i < last; i++)
            Debug.DrawLine(new Vector2(0f, path[i]), new Vector2(0f, path[i + 1]), color);
    }
}

public class RotationPath : BGenericPath<Quaternion, Vector3>
{
    public RotationMode mode;

    public RotationPath() : base() { }

    public RotationPath(Quaternion origin, Quaternion target, RotationMode mode = RotationMode.Shortest)
    {
        this.mode = mode;
        path = new Quaternion[] { origin, target };

        CalculateData();
    }

    public RotationPath(Quaternion[] rotations, RotationMode mode = RotationMode.Shortest)
    {
        this.mode = mode;
        path = rotations;

        CalculateData();
    }

    public override void Set(Quaternion origin, Quaternion target)
    {
        mode = RotationMode.Shortest;
        base.Set(origin, target);
    }

    public override void Set(Quaternion[] values)
    {
        mode = RotationMode.Shortest;
        base.Set(values);
    }

    public void Set(Quaternion origin, Quaternion target, RotationMode mode)
    {
        this.mode = mode;
        base.Set(origin, target);
    }

    public void Set(Quaternion[] values, RotationMode mode)
    {
        this.mode = mode;
        base.Set(values);
    }

    protected override void CalculateData()
    {
        base.CalculateData();

        if (magnitude == 360f) //TO DO: Hmm, this is probably wrong
            magnitude = 0f;
    }

    public void ProjectOnPlane(Vector3 normal)
    {
        for (int i = 0; i < count; i++)
            path[i] = Quaternion.AngleAxis(path[i].Angle(mode), normal);
        CalculateData();
    }

    protected override float Distance(Quaternion a, Quaternion b)
    {
        return a.Angle(b, mode);
    }

    protected override void DirectionMagnitude(Quaternion direction, out Vector3 axis, out float angle)
    {
        direction.ToAngleAxis(mode, out angle, out axis);
    }

    protected override BGenericPath<Quaternion, Vector3> NewPath(Quaternion origin, Quaternion target)
    {
        return new RotationPath(origin, target, mode);
    }

    protected override BGenericPath<Quaternion, Vector3> NewPath(Quaternion[] points)
    {
        return new RotationPath(points, mode);
    }

    protected override float SmoothDamp_CorrectOvershoot(Quaternion output, float change)
    {
        ClosestInPath(output, out float disp);
        return disp;
    }

    //TO DO
    protected override Quaternion ClosestOnInterval(Quaternion point, int a, int b, out float disp)
    {
        disp = 0f;
        return origin;
    }

    public override void Draw(Color color)
    {
        for (int i = 0; i < last; i++)
            Debug.DrawLine(path[i] * Vector3.forward, path[i + 1] * Vector3.forward, color);
    }
}

public class MovementPath : BGenericPath<Vector3, Vector3>
{
    bool smoothDamp_useNavMesh;

    public enum SmoothMode { AlongPath, NavMesh, Direct }

    public MovementPath() : base() { }

    public MovementPath(Vector3 origin, Vector3 target,
        bool useNavMesh = false, int navMeshAreaMask = 0, int navMeshAgentType = 0)
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

        CalculateData();
    }

    public MovementPath(Vector3[] points) : base(points) { }

    public void Set(Vector3 origin, Vector3 target,
        bool useNavMesh = false, int navMeshAreaMask = 0, int navMeshAgentType = 0)
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

        CalculateData();
    }

    public void ProjectOnPlane(Vector3 normal)
    {
        for (int i = 0; i < count; i++)
            path[i] = Vector3.ProjectOnPlane(path[i], normal);
        CalculateData();
    }

    public Vector3 SmoothDamp(ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime, SmoothMode smoothMode = SmoothMode.AlongPath)
    {
        smoothDamp_useNavMesh = smoothMode == SmoothMode.NavMesh;
        return SmoothDamp(ref currentVelocity, smoothTime, maxSpeed, deltaTime, smoothMode != SmoothMode.Direct);
    }

    protected override float Distance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b); // TO DO: Bezier curve support
    }

    protected override void DirectionMagnitude(Vector3 value, out Vector3 direction, out float magnitude)
    {
        Generics.DirectionMagnitude(value, out direction, out magnitude); // TO DO: Bezier curve support
    }

    protected override BGenericPath<Vector3, Vector3> SmoothDamp_Subpath(float change)
    {
        return smoothDamp_useNavMesh ?
            new MovementPath(target, origin, true) :
            SubPath(change, magnitude - change, true);
    }

    protected override float SmoothDamp_CorrectOvershoot(Vector3 output, float change)
    {
        ClosestInPath(output, out float disp);
        return disp;
    }

    public override void Draw(Color color)
    {
        for (int i = 0; i < last; i++)
            Debug.DrawLine(path[i], path[i + 1], color);
    }
}

public class Movement4DPath : BGenericPath<Vector4, Vector4>
{
    public Movement4DPath() : base() { }

    public Movement4DPath(Vector4 origin, Vector4 target) : base(origin, target) { }

    public Movement4DPath(Vector4[] points) : base(points) { }

    //public void ProjectOnPlane(Vector4 normal)
    //{
    //    for (int i = 0; i < count; i++)
    //        path[i] = Vector4.ProjectOnPlane(path[i], normal);
    //    CalculateData();
    //}

    protected override float SmoothDamp_CorrectOvershoot(Vector4 output, float change)
    {
        ClosestInPath(output, out float disp);
        return disp;
    }

    public override void Draw(Color color)
    {
        for (int i = 0; i < last; i++)
            Debug.DrawLine(path[i], path[i + 1], color);
    }
}

public class ColorPath : BGenericPath<Color, Color> // TO DO
{
    public ColorPath() : base() { }

    public ColorPath(Color origin, Color target) : base(origin, target) { }

    public ColorPath(Color[] points) : base(points) { }

    public override void Draw(Color color)
    {
        //TO DO
    }
}

public static class PathStaticHelpers
{
    public static P SmartSetValues<P, T, D>(this P path, T origin, T target)
        where P : BGenericPath<T, D>, new()
        where T : IEquatable<T>
    {
        if (path == null)
            path = new P();
        path.Set(origin, target);
        return path;
    }

    public static P SmartSetValues<P, T, D>(this P path, T[] values)
        where P : BGenericPath<T, D>, new()
        where T : IEquatable<T>
    {
        if (path == null)
            path = new P();
        path.Set(values);
        return path;
    }

    public static RotationPath SmartSetValues(this RotationPath path,
        Quaternion origin, Quaternion target, RotationMode mode)
    {
        if (path == null)
            return new RotationPath(origin, target, mode);
        else
        {
            path.Set(origin, target, mode);
            return path;
        }
    }

    public static RotationPath SmartSetValues(this RotationPath path,
        Quaternion[] values, RotationMode mode)
    {
        if (path == null)
            return new RotationPath(values, mode);
        else
        {
            path.Set(values, mode);
            return path;
        }
    }

    public static MovementPath SmartSetValues(this MovementPath path,
        Vector3 origin, Vector3 target,
        bool useNavMesh = false, int navMeshAreaMask = 0, int navMeshAgentType = 0)
    {
        if (path == null)
            return new MovementPath(origin, target, useNavMesh, navMeshAreaMask, navMeshAgentType);
        else
        {
            path.Set(origin, target, useNavMesh, navMeshAreaMask, navMeshAgentType);
            return path;
        }
    }

    public static MovementPath SmartSetValues(this MovementPath path, Vector3[] values)
    {
        return SmartSetValues<MovementPath, Vector3, Vector3>(path, values);
    }
}
