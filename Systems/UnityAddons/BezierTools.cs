using System;
using UnityEngine;

public static class BezierTools
{
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
        T pOver = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromPrev, prevSmooth), prev);
        D fromNext = Generics.Direction<T, D>(Generics.Subtract(point, next));
        T nOver = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromNext, nextSmooth), next);

        if (Generics.Distance(pOver, point) < Generics.Distance(start, point))
            start = Generics.Lerp(pOver, start, 0.5f);
        if (Generics.Distance(nOver, point) < Generics.Distance(end, point))
            end = Generics.Lerp(nOver, end, 0.5f);

        controlA = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromPrev, modif), start);
        controlB = Generics.Add(Generics.FromDirectionMagnitude<T, D>(fromNext, modif), end);
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
        T A = Generics.Subtract(Generics.Add(start, end), Generics.Scale(control, 2f));
        T B = Generics.Scale(Generics.Subtract(control, start), 2f);

        float a = 4f * Generics.Dot(A, A);
        float b = 4f * Generics.Dot(A, B);
        float c = Generics.Dot(B, B);

        if (a < 1e-8f)
            return Mathf.Sqrt(c);

        return Integral(1f, a, b, c) - Integral(0f, a, b, c);
    }

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

        return term1 + term2 * Mathf.Log(logArg);
    }

    public static float CubicBezier_Length<T>(
        T start, T controlA, T controlB, T end,
        float tolerance = 0.0001f) where T : IEquatable<T>
    {
        if (controlA.Equals(controlB) && IsVector(typeof(T)))
            return QuadraticBezier_Length(start, controlA, end);
        else
        {
            Func<float, float> speed = t =>
            {
                T d = CubicDerivative(start, controlA, controlB, end, t);
                return Generics.Magnitude(d);
            };

            return AdaptiveSimpson(speed, 0f, 1f, tolerance, 12);
        }
    }

    static bool IsVector(Type type)
    {
        return type == typeof(Vector2) ||
            type == typeof(Vector3) ||
            type == typeof(Vector4) ||
            type == typeof(Color);
    }

    static T CubicDerivative<T>(
        T p0, T p1, T p2, T p3, float t)
    {
        float u = 1f - t;

        return Generics.Add(Generics.Add(
            Generics.Scale(Generics.Subtract(p1, p0), 3f * u * u),
            Generics.Scale(Generics.Subtract(p2, p1), 6f * u * t)),
            Generics.Scale(Generics.Subtract(p3, p2), 3f * t * t));
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
}
