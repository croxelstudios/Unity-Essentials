using System;
using System.Collections.Generic;
using UnityEngine;

public static class BezierTools
{
    #region Data getters
    public static void QuadraticPointSmoothData<T, D>(T point, T prev, T next, float smooth,
        out T start, out T control, out T end)
    {
        CubicPointSmoothData<T, D>(point, prev, next, smooth, smooth,
            out start, out control, out T cB, out end);
    }

    public static void QuadraticPointSmoothData<T, D>(T point, T prev, T next,
        float smooth, float prevSmooth, float nextSmooth,
        out T start, out T control, out T end)
    {
        CubicPointSmoothData<T, D>(point, prev, next,
            smooth, prevSmooth, nextSmooth, smooth,
            out start, out control, out T cB, out end);
    }

    public static void CubicPointSmoothData<T, D>(T point, T prev, T next,
        float smooth, float prevSmooth, float nextSmooth, float modif,
        out T start, out T controlA, out T controlB, out T end)
    {
        if ((smooth == 0f) || (modif == 0f))
        {
            start = point;
            controlA = point;
            controlB = point;
            end = point;
            return;
        }

        T toPrev = Generics.Subtract(prev, point);
        T toNext = Generics.Subtract(next, point);
        Generics.DirectionMagnitude(toPrev, out D direction, out float prevMag);
        T curveP1 = Generics.Add(
            Generics.FromDirectionMagnitude<T, D>(direction, Mathf.Min(prevMag, smooth)), point);
        Generics.DirectionMagnitude(toNext, out direction, out float nextMag);
        T curveP2 = Generics.Add(
            Generics.FromDirectionMagnitude<T, D>(direction, Mathf.Min(nextMag, smooth)), point);

        start = curveP1;
        end = curveP2;

        D fromPrev = Generics.Direction<T, D>(Generics.Subtract(point, prev));
        T pOver = Generics.Add(Generics.FromDirectionMagnitude<T, D>(
            fromPrev, Mathf.Min(prevMag, prevSmooth)), prev);
        D fromNext = Generics.Direction<T, D>(Generics.Subtract(point, next));
        T nOver = Generics.Add(Generics.FromDirectionMagnitude<T, D>(
            fromNext, Mathf.Min(nextMag, nextSmooth)), next);

        float sDist = smooth;
        if (Generics.Distance(pOver, point) < Generics.Distance(start, point))
        {
            start = Generics.Lerp(pOver, start, 0.5f);
            sDist = Generics.Distance(start, point);
        }
        float eDist = smooth;
        if (Generics.Distance(nOver, point) < Generics.Distance(end, point))
        {
            end = Generics.Lerp(nOver, end, 0.5f);
            eDist = Generics.Distance(end, point);
        }

        controlA = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromPrev,
            modif * (sDist / smooth)), start);
        controlB = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromNext,
            modif * (eDist / smooth)), end);
    }

    public static void CubicPointSmoothData<T, D>(T point, T prev, T next, float smooth, float modif,
        out T start, out T controlA, out T controlB, out T end)
    {
        T toPrev = Generics.Subtract(prev, point);
        T toNext = Generics.Subtract(next, point);
        Generics.DirectionMagnitude(toPrev, out D direction, out float magnitude);
        T curveP1 = Generics.Add(
            Generics.FromDirectionMagnitude<T, D>(direction, Mathf.Min(magnitude, smooth)), point);
        Generics.DirectionMagnitude(toNext, out direction, out magnitude);
        T curveP2 = Generics.Add(
            Generics.FromDirectionMagnitude<T, D>(direction, Mathf.Min(magnitude, smooth)), point);

        start = curveP1;
        end = curveP2;

        D fromPrev = Generics.Direction<T, D>(Generics.Subtract(point, prev));
        controlA = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromPrev, modif), start);
        D fromNext = Generics.Direction<T, D>(Generics.Subtract(point, next));
        controlB = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromNext, modif), end);
    }
    #endregion

    public static T QuadraticBezier<T>(T start, T control, T end, float t)
    {
        T p0 = Generics.Lerp(start, control, t);
        T p1 = Generics.Lerp(control, end, t);
        return Generics.Lerp(p0, p1, t);
    }

    public static T CubicBezier<T>(T start, T controlA, T controlB, T end, float t)
    {
        T p0 = QuadraticBezier(start, controlA, controlB, t);
        T p1 = QuadraticBezier(controlA, controlB, end, t);
        return Generics.Lerp(p0, p1, t);
    }

    public static float QuadraticBezier_Length<T>(T start, T control, T end)
    {
        if (start.Equals(end))
            return 0f;

        T A = Generics.Subtract(Generics.Add(start, end), Generics.Scale(control, 2f));
        T B = Generics.Scale(Generics.Subtract(control, start), 2f);

        float a = 4f * Generics.Dot(A, A);
        float b = 4f * Generics.Dot(A, B);
        float c = Generics.Dot(B, B);

        if (a < 1e-8f)
            return Mathf.Sqrt(c);

        return Integral(1f, a, b, c) - Integral(0f, a, b, c);
    }

    #region QuadraticLength helpers
    static float Integral(float t, float a, float b, float c)
    {
        float sqrt_a = Mathf.Sqrt(a);

        float q = Mathf.Sqrt(a * t * t + b * t + c);

        float term1 =
            (2f * a * t + b) * q / (4f * a);

        float term2 =
            (4f * a * c - b * b) /
            (8f * a * sqrt_a);

        float logArg =
            2f * sqrt_a * q +
            2f * a * t + b;

        if (logArg < 1e-8f)
            return -Mathf.Infinity;

        return term1 + term2 * Mathf.Log(logArg);
    }
    #endregion

    public static float CubicBezier_Length<T>(
        T start, T controlA, T controlB, T end,
        float tolerance = 0.0001f) where T : IEquatable<T>
    {
        if (start.Equals(end))
            return 0f;

        Func<float, float> speed = t =>
        {
            T d = CubicDerivative(start, controlA, controlB, end, t);
            return Generics.Magnitude(d);
        };

        return AdaptiveSimpson(speed, 0f, 1f, tolerance, 12);
    }

    #region CubicLength helpers
    static bool IsVector(Type type)
    {
        return type == typeof(Vector2) ||
            type == typeof(Vector3) ||
            type == typeof(Vector4) ||
            type == typeof(Color);
    }

    static float AdaptiveSimpson(Func<float, float> f, float a, float b, float eps, int maxRecursion)
    {
        float c = (a + b) * 0.5f;
        float fa = f(a);
        float fb = f(b);
        float fc = f(c);

        float whole = Simpson(fa, fb, fc, a, b);
        return AdaptiveSimpsonRecursive(f, a, b, eps, whole, fa, fb, fc, maxRecursion);
    }

    static float AdaptiveSimpsonRecursive(
        Func<float, float> f,
        float a, float b,
        float eps, float whole,
        float fa, float fb, float fc,
        int rec)
    {
        float c = (a + b) * 0.5f;
        float d = (a + c) * 0.5f;
        float e = (c + b) * 0.5f;

        float fd = f(d);
        float fe = f(e);

        float left = Simpson(fa, fc, fd, a, c);
        float right = Simpson(fc, fb, fe, c, b);

        float delta = left + right - whole;

        if (rec <= 0 || Mathf.Abs(delta) <= 15f * eps)
            return left + right + delta / 15f;

        return AdaptiveSimpsonRecursive(f, a, c, eps * 0.5f, left, fa, fc, fd, rec - 1)
             + AdaptiveSimpsonRecursive(f, c, b, eps * 0.5f, right, fc, fb, fe, rec - 1);
    }

    static float Simpson(float fa, float fb, float fc, float a, float b)
    {
        return (b - a) * (fa + 4f * fc + fb) / 6f;
    }
    #endregion

    public static T QuadraticBezier_Closest<T>(T point, T start, T control, T end, out float t,
        bool fastMethod = false)
    {
        if (fastMethod)
        {
            //Initial approx
            const int SAMPLES = 16;

            t = 0f;
            float bestDist = float.MaxValue;
            for (int i = 0; i <= SAMPLES; i++)
            {
                float s = i / (float)SAMPLES;

                T p = QuadraticBezier(start, control, end, s);

                float dist = Generics.FastMagnitude(Generics.Subtract(point, p));

                if (dist < bestDist)
                {
                    bestDist = dist;
                    t = s;
                }
            }

            // Newton-Raphson refinement
            for (int i = 0; i < 4; i++)
            {
                T p = QuadraticBezier(start, control, end, t);

                T d1 = Generics.Add(
                    Generics.Scale(Generics.Subtract(control, start), 2f * (1f - t)),
                    Generics.Scale(Generics.Subtract(end, control), 2f * t));

                T d2 = Generics.Scale(
                    Generics.Subtract(end, Generics.Scale(Generics.Add(control, start), 2f)), 2f);

                T r = Generics.Subtract(p, point);

                float f =
                    Generics.Dot(r, d1);

                float df =
                    Generics.Dot(d1, d1) +
                    Generics.Dot(r, d2);

                if (Mathf.Abs(df) < 0.0001f)
                    break;

                t -= f / df;

                t = Mathf.Clamp01(t);
            }

            return QuadraticBezier(start, control, end, t);
        }
        else
        {
            // Quadratic bezier in polynomic form: B(t) = a t^2 + b t + c
            T a = Generics.Subtract(Generics.Add(start, end), Generics.Scale(control, 2f));
            T b = Generics.Scale(Generics.Subtract(control, start), 2f);
            T c = Generics.Subtract(start, point);

            // Squared distance derivative:
            // A t^3 + B t^2 + C t + D = 0
            float A = 2f * Generics.Dot(a, a);
            float B = 3f * Generics.Dot(a, b);
            float C = 2f * Generics.Dot(a, c) + Generics.Dot(b, b);
            float D = Generics.Dot(b, c);

            List<float> candidates = SolveRealCubic(A, B, C, D);

            // Extremes can also be the global minimum
            candidates.Add(0f);
            candidates.Add(1f);

            float bestT = 0f;
            float bestDist = float.PositiveInfinity;

            for (int i = 0; i < candidates.Count; i++)
            {
                float ti = candidates[i];

                if (ti < 0f || ti > 1f)
                    continue;

                T p = QuadraticBezier(start, control, end, ti);
                float dist = Generics.FastMagnitude(Generics.Subtract(p, point));

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = ti;
                }
            }

            t = bestT;
            return QuadraticBezier(start, control, end, bestT);
        }
    }

    #region QuadraticClosest helpers
    static List<float> SolveRealCubic(float a, float b, float c, float d)
    {
        const float eps = 1e-8f;
        List<float> roots = new List<float>(3);

        // Degenerate case: not really cubic.
        if (Mathf.Abs(a) < eps)
        {
            SolveRealQuadratic(b, c, d, roots);
            return roots;
        }

        // Normalization: x^3 + A x^2 + B x + C = 0
        float invA = 1f / a;
        float A = b * invA;
        float B = c * invA;
        float C = d * invA;

        // Sustitution: x = y - A/3
        float sqA = A * A;
        float p = B - sqA / 3f;
        float q = 2f * A * sqA / 27f - A * B / 3f + C;

        float halfQ = q * 0.5f;
        float thirdP = p / 3f;
        float discriminant = halfQ * halfQ + thirdP * thirdP * thirdP;

        float shift = A / 3f;

        if (discriminant > eps)
        {
            // Real root
            float sqrtDisc = Mathf.Sqrt(discriminant);
            float u = Cbrt(-halfQ + sqrtDisc);
            float v = Cbrt(-halfQ - sqrtDisc);
            roots.Add(u + v - shift);
        }
        else if (Mathf.Abs(discriminant) <= eps)
        {
            // Multiple roots
            if (Mathf.Abs(halfQ) < eps && Mathf.Abs(thirdP) < eps)
            {
                // Triple root
                roots.Add(-shift);
            }
            else
            {
                float u = Cbrt(-halfQ);
                roots.Add(2f * u - shift);
                roots.Add(-u - shift);
            }
        }
        else
        {
            // Three real roots
            float m = 2f * Mathf.Sqrt(-thirdP);
            float cosArg = -halfQ / Mathf.Sqrt(-(thirdP * thirdP * thirdP));
            cosArg = Mathf.Clamp(cosArg, -1f, 1f);

            float phi = Mathf.Acos(cosArg);

            roots.Add(m * Mathf.Cos(phi / 3f) - shift);
            roots.Add(m * Mathf.Cos((phi + 2f * Mathf.PI) / 3f) - shift);
            roots.Add(m * Mathf.Cos((phi + 4f * Mathf.PI) / 3f) - shift);
        }

        return roots;
    }

    static void SolveRealQuadratic(float a, float b, float c, List<float> roots)
    {
        const float eps = 1e-8f;

        if (Mathf.Abs(a) < eps)
        {
            if (Mathf.Abs(b) < eps)
                return;

            roots.Add(-c / b);
            return;
        }

        float disc = b * b - 4f * a * c;

        if (disc < -eps)
            return;

        if (Mathf.Abs(disc) <= eps)
        {
            roots.Add(-b / (2f * a));
            return;
        }

        float sqrtDisc = Mathf.Sqrt(disc);
        float inv2A = 1f / (2f * a);
        roots.Add((-b - sqrtDisc) * inv2A);
        roots.Add((-b + sqrtDisc) * inv2A);
    }

    static float Cbrt(float x)
    {
        return x >= 0f ? Mathf.Pow(x, 1f / 3f) : -Mathf.Pow(-x, 1f / 3f);
    }
    #endregion

    public static T CubicBezier_Closest<T>(T point, T start, T controlA, T controlB, T end, out float t,
        float tolerance = 0.0001f)
    {
        // Initial approx
        const int samples = 32;
        float bestT = 0f;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i <= samples; i++)
        {
            float s = i / (float)samples;
            T p = CubicBezier(start, controlA, controlB, end, s);
            float dist = Generics.FastMagnitude(Generics.Subtract(p, point));

            if (dist < bestDist)
            {
                bestDist = dist;
                bestT = s;
            }
        }

        // Newton-Raphson refinement
        float currentT = bestT;

        for (int i = 0; i < 8; i++)
        {
            T b = CubicBezier(start, controlA, controlB, end, currentT);
            T d1 = CubicDerivative(start, controlA, controlB, end, currentT);
            T d2 = CubicSecondDerivative(start, controlA, controlB, end, currentT);

            T r = Generics.Subtract(b, point);

            // f(t) = (B(t) - Q) · B'(t)
            float f = Generics.Dot(r, d1);

            // f'(t) = B'(t)·B'(t) + (B(t)-Q)·B''(t)
            float df = Generics.Dot(d1, d1) + Generics.Dot(r, d2);

            if (Mathf.Abs(df) < 1e-6f)
                break;

            currentT -= f / df;
            currentT = Mathf.Clamp01(currentT);
        }

        t = currentT;
        return CubicBezier(start, controlA, controlB, end, currentT);
    }

    #region CubicClosest helpers
    public static T CubicSecondDerivative<T>(
    T p0, T p1, T p2, T p3, float t)
    {
        return Generics.Add(
            Generics.Scale(Generics.Subtract(Generics.Add(p2, p0), Generics.Scale(p1, 2f)), 6f * (1f - t)),
            Generics.Scale(Generics.Subtract(Generics.Add(p3, p1), Generics.Scale(p2, 2f)), 6f * t));
    }
    #endregion

    #region Other helpers
    static T CubicDerivative<T>(
            T p0, T p1, T p2, T p3, float t)
    {
        float u = 1f - t;

        return Generics.Add(Generics.Add(
            Generics.Scale(Generics.Subtract(p1, p0), 3f * u * u),
            Generics.Scale(Generics.Subtract(p2, p1), 6f * u * t)),
            Generics.Scale(Generics.Subtract(p3, p2), 3f * t * t));
    }
    #endregion
}
