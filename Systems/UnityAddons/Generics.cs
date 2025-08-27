using System;
using System.Collections.Generic;
using UnityEngine;

public static class Generics
{
    public delegate T SmoothDampImpl<T>(T current, T target, ref T currentVelocity,
        float smoothTime, float maxSpeed, float deltaTime) where T : unmanaged;
    public delegate bool IsZeroImpl<T>(T value) where T : unmanaged;
    public delegate T AddImpl<T>(T A, T B) where T : unmanaged;
    public delegate T SubtractImpl<T>(T A, T B) where T : unmanaged;
    public delegate T ScaleImpl<T>(T value, float scale) where T : unmanaged;
    public delegate bool HasMagnitudeImpl<T>(T value, float epsilon) where T : unmanaged;
    public delegate float MagnitudeImpl<T>(T value) where T : unmanaged;
    public delegate T NegateImpl<T>(T value) where T : unmanaged;

    private static readonly Dictionary<Type, object> table_SmoothDamp;
    private static readonly Dictionary<Type, object> table_IsZero;
    private static readonly Dictionary<Type, object> table_Add;
    private static readonly Dictionary<Type, object> table_Subtract;
    private static readonly Dictionary<Type, object> table_Scale;
    private static readonly Dictionary<Type, object> table_HasMagnitude;
    private static readonly Dictionary<Type, object> table_Magnitude;
    private static readonly Dictionary<Type, object> table_Negate;

    static Generics()
    {
        table_SmoothDamp = new Dictionary<Type, object>(5);
        table_IsZero = new Dictionary<Type, object>(5);
        table_Add = new Dictionary<Type, object>(5);
        table_Subtract = new Dictionary<Type, object>(5);
        table_Scale= new Dictionary<Type, object>(5);
        table_HasMagnitude = new Dictionary<Type, object>(5);
        table_Magnitude = new Dictionary<Type, object>(5);
        table_Negate = new Dictionary<Type, object>(5);

        // SmoothDamp
        table_SmoothDamp[typeof(float)] = new SmoothDampImpl<float>(SmoothDamp_Float);
        table_SmoothDamp[typeof(Vector2)] = new SmoothDampImpl<Vector2>(SmoothDamp_Vector2);
        table_SmoothDamp[typeof(Vector3)] = new SmoothDampImpl<Vector3>(SmoothDamp_Vector3);
        table_SmoothDamp[typeof(Vector4)] = new SmoothDampImpl<Vector4>(SmoothDamp_Vector4);
        table_SmoothDamp[typeof(Quaternion)] = new SmoothDampImpl<Quaternion>(SmoothDamp_Quaternion);

        // IsZero
        table_IsZero[typeof(float)] = new IsZeroImpl<float>(IsZero_Float);
        table_IsZero[typeof(Vector2)] = new IsZeroImpl<Vector2>(IsZero_Vector2);
        table_IsZero[typeof(Vector3)] = new IsZeroImpl<Vector3>(IsZero_Vector3);
        table_IsZero[typeof(Vector4)] = new IsZeroImpl<Vector4>(IsZero_Vector4);
        table_IsZero[typeof(Quaternion)] = new IsZeroImpl<Quaternion>(IsZero_Quaternion);

        // Subtract
        table_Add[typeof(float)] = new AddImpl<float>(Add_Float);
        table_Add[typeof(Vector2)] = new AddImpl<Vector2>(Add_Vector2);
        table_Add[typeof(Vector3)] = new AddImpl<Vector3>(Add_Vector3);
        table_Add[typeof(Vector4)] = new AddImpl<Vector4>(Add_Vector4);
        table_Add[typeof(Quaternion)] = new AddImpl<Quaternion>(Add_Quaternion);

        // Subtract
        table_Scale[typeof(float)] = new ScaleImpl<float>(Scale_Float);
        table_Scale[typeof(Vector2)] = new ScaleImpl<Vector2>(Scale_Vector2);
        table_Scale[typeof(Vector3)] = new ScaleImpl<Vector3>(Scale_Vector3);
        table_Scale[typeof(Vector4)] = new ScaleImpl<Vector4>(Scale_Vector4);
        table_Scale[typeof(Quaternion)] = new ScaleImpl<Quaternion>(Scale_Quaternion);

        // Subtract
        table_Subtract[typeof(float)] = new SubtractImpl<float>(Subtract_Float);
        table_Subtract[typeof(Vector2)] = new SubtractImpl<Vector2>(Subtract_Vector2);
        table_Subtract[typeof(Vector3)] = new SubtractImpl<Vector3>(Subtract_Vector3);
        table_Subtract[typeof(Vector4)] = new SubtractImpl<Vector4>(Subtract_Vector4);
        table_Subtract[typeof(Quaternion)] = new SubtractImpl<Quaternion>(Subtract_Quaternion);

        // HasMagnitude
        table_HasMagnitude[typeof(float)] = new HasMagnitudeImpl<float>(HasMagnitude_Float);
        table_HasMagnitude[typeof(Vector2)] = new HasMagnitudeImpl<Vector2>(HasMagnitude_Vector2);
        table_HasMagnitude[typeof(Vector3)] = new HasMagnitudeImpl<Vector3>(HasMagnitude_Vector3);
        table_HasMagnitude[typeof(Vector4)] = new HasMagnitudeImpl<Vector4>(HasMagnitude_Vector4);
        table_HasMagnitude[typeof(Quaternion)] = new HasMagnitudeImpl<Quaternion>(HasMagnitude_Quaternion);

        // Magnitude
        table_Magnitude[typeof(float)] = new MagnitudeImpl<float>(Magnitude_Float);
        table_Magnitude[typeof(Vector2)] = new MagnitudeImpl<Vector2>(Magnitude_Vector2);
        table_Magnitude[typeof(Vector3)] = new MagnitudeImpl<Vector3>(Magnitude_Vector3);
        table_Magnitude[typeof(Vector4)] = new MagnitudeImpl<Vector4>(Magnitude_Vector4);
        table_Magnitude[typeof(Quaternion)] = new MagnitudeImpl<Quaternion>(Magnitude_Quaternion);

        // Negate
        table_Negate[typeof(float)] = new NegateImpl<float>(Negate_Float);
        table_Negate[typeof(Vector2)] = new NegateImpl<Vector2>(Negate_Vector2);
        table_Negate[typeof(Vector3)] = new NegateImpl<Vector3>(Negate_Vector3);
        table_Negate[typeof(Vector4)] = new NegateImpl<Vector4>(Negate_Vector4);
        table_Negate[typeof(Quaternion)] = new NegateImpl<Quaternion>(Negate_Quaternion);
    }

    #region Register functions
    public static void Register_SmoothDamp<T>(SmoothDampImpl<T> impl) where T : unmanaged
    {
        table_SmoothDamp[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_IsZero<T>(IsZeroImpl<T> impl) where T : unmanaged
    {
        table_IsZero[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Add<T>(AddImpl<T> impl) where T : unmanaged
    {
        table_Add[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Subtract<T>(SubtractImpl<T> impl) where T : unmanaged
    {
        table_Subtract[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Scale<T>(ScaleImpl<T> impl) where T : unmanaged
    {
        table_Scale[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_HasMagnitude<T>(HasMagnitudeImpl<T> impl) where T : unmanaged
    {
        table_HasMagnitude[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Magnitude<T>(MagnitudeImpl<T> impl) where T : unmanaged
    {
        table_Magnitude[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Negate<T>(NegateImpl<T> impl) where T : unmanaged
    {
        table_Negate[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }
    #endregion

    public static T SmoothDamp<T>(T current, T target, ref T currentVelocity,
        float smoothTime, float maxSpeed, float deltaTime) where T : unmanaged
    {
        if (deltaTime <= 0f) return current;
        if (smoothTime <= 0f) return target;

        if (!table_SmoothDamp.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.SmoothDamp(). Register an implementation using" +
                $"Generics.Register_SmoothDamp<{typeof(T).Name}>(...) if necessary.");

        SmoothDampImpl<T> impl = (SmoothDampImpl<T>)o;
        return impl(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static bool IsZero<T>(T value) where T : unmanaged
    {
        if (!table_IsZero.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.IsZero(). Register an implementation using" +
                $"Generics.Register_IsZero<{typeof(T).Name}>(...) if necessary.");

        IsZeroImpl<T> impl = (IsZeroImpl<T>)o;
        return impl(value);
    }

    public static T Add<T>(T A, T B) where T : unmanaged
    {
        if (!table_Add.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Add(). Register an implementation using" +
                $"Generics.Register_Add<{typeof(T).Name}>(...) if necessary.");

        AddImpl<T> impl = (AddImpl<T>)o;
        return impl(A, B);
    }

    public static T Subtract<T>(T A, T B) where T : unmanaged
    {
        if (!table_Subtract.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Subtract(). Register an implementation using" +
                $"Generics.Register_Subtract<{typeof(T).Name}>(...) if necessary.");

        SubtractImpl<T> impl = (SubtractImpl<T>)o;
        return impl(A, B);
    }

    public static T Scale<T>(T value, float scale) where T : unmanaged
    {
        if (!table_Scale.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Scale(). Register an implementation using" +
                $"Generics.Register_Scale<{typeof(T).Name}>(...) if necessary.");

        ScaleImpl<T> impl = (ScaleImpl<T>)o;
        return impl(value, scale);
    }

    public static bool HasMagnitude<T>(T value, float epsilon = 0f) where T : unmanaged
    {
        if (!table_HasMagnitude.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.HasMagnitude(). Register an implementation using" +
                $"Generics.Register_HasMagnitude<{typeof(T).Name}>(...) if necessary.");

        HasMagnitudeImpl<T> impl = (HasMagnitudeImpl<T>)o;
        return impl(value, epsilon);
    }

    public static float Magnitude<T>(T value) where T : unmanaged
    {
        if (!table_Magnitude.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Magnitude(). Register an implementation using" +
                $"Generics.Register_Magnitude<{typeof(T).Name}>(...) if necessary.");

        MagnitudeImpl<T> impl = (MagnitudeImpl<T>)o;
        return impl(value);
    }

    public static T Negate<T>(T value) where T : unmanaged
    {
        if (!table_Negate.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Negate(). Register an implementation using" +
                $"Generics.Register_Negate<{typeof(T).Name}>(...) if necessary.");

        NegateImpl<T> impl = (NegateImpl<T>)o;
        return impl(value);
    }

    #region Implementations
    #region SmoothDamp
    private static float SmoothDamp_Float(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    private static Vector2 SmoothDamp_Vector2(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        return Vector2.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    private static Vector3 SmoothDamp_Vector3(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        return Vector3.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    private static Vector4 SmoothDamp_Vector4(Vector4 current, Vector4 target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        float vx = currentVelocity.x, vy = currentVelocity.y, vz = currentVelocity.z, vw = currentVelocity.w;
        float x = Mathf.SmoothDamp(current.x, target.x, ref vx, smoothTime, maxSpeed, deltaTime);
        float y = Mathf.SmoothDamp(current.y, target.y, ref vy, smoothTime, maxSpeed, deltaTime);
        float z = Mathf.SmoothDamp(current.z, target.z, ref vz, smoothTime, maxSpeed, deltaTime);
        float w = Mathf.SmoothDamp(current.w, target.w, ref vw, smoothTime, maxSpeed, deltaTime);
        currentVelocity = new Vector4(vx, vy, vz, vw);
        return new Vector4(x, y, z, w);
    }

    private static Quaternion SmoothDamp_Quaternion(Quaternion current, Quaternion target, ref Quaternion currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        Vector3 v = new Vector3(currentVelocity.x, currentVelocity.y, currentVelocity.z);
        Quaternion r = current.SmoothDamp(target, ref v, smoothTime, maxSpeed, deltaTime);
        currentVelocity = new Quaternion(v.x, v.y, v.z, 1f);
        return r;
    }
    #endregion

    #region IsZero
    public static bool IsZero_Float(float value)
    {
        return value == 0f;
    }

    public static bool IsZero_Vector2(Vector2 value)
    {
        return value == Vector2.zero;
    }

    public static bool IsZero_Vector3(Vector3 value)
    {
        return value == Vector3.zero;
    }

    public static bool IsZero_Vector4(Vector4 value)
    {
        return value == Vector4.zero;
    }

    public static bool IsZero_Quaternion(Quaternion value)
    {
        return value == Quaternion.identity;
    }
    #endregion

    #region Add
    public static float Add_Float(float A, float B)
    {
        return A + B;
    }

    public static Vector2 Add_Vector2(Vector2 A, Vector2 B)
    {
        return A + B;
    }

    public static Vector3 Add_Vector3(Vector3 A, Vector3 B)
    {
        return A + B;
    }

    public static Vector4 Add_Vector4(Vector4 A, Vector4 B)
    {
        return A + B;
    }

    public static Quaternion Add_Quaternion(Quaternion A, Quaternion B)
    {
        return A.Add(B);
    }
    #endregion

    #region Subtract
    public static float Subtract_Float(float A, float B)
    {
        return A - B;
    }

    public static Vector2 Subtract_Vector2(Vector2 A, Vector2 B)
    {
        return A - B;
    }

    public static Vector3 Subtract_Vector3(Vector3 A, Vector3 B)
    {
        return A - B;
    }

    public static Vector4 Subtract_Vector4(Vector4 A, Vector4 B)
    {
        return A - B;
    }

    public static Quaternion Subtract_Quaternion(Quaternion A, Quaternion B)
    {
        return A.Subtract(B);
    }
    #endregion

    #region Scale
    public static float Scale_Float(float value, float scale)
    {
        return value * scale;
    }

    public static Vector2 Scale_Vector2(Vector2 value, float scale)
    {
        return value * scale;
    }

    public static Vector3 Scale_Vector3(Vector3 value, float scale)
    {
        return value * scale;
    }

    public static Vector4 Scale_Vector4(Vector4 value, float scale)
    {
        return value * scale;
    }

    public static Quaternion Scale_Quaternion(Quaternion value, float scale)
    {
        return value.Scale(scale);
    }
    #endregion

    #region HasMagnitude
    public static bool HasMagnitude_Float(float value, float epsilon)
    {
        return Mathf.Abs(value) > epsilon;
    }

    public static bool HasMagnitude_Vector2(Vector2 value, float epsilon)
    {
        return value.sqrMagnitude > epsilon;
    }

    public static bool HasMagnitude_Vector3(Vector3 value, float epsilon)
    {
        return value.sqrMagnitude > epsilon;
    }

    public static bool HasMagnitude_Vector4(Vector4 value, float epsilon)
    {
        return value.sqrMagnitude > epsilon;
    }

    public static bool HasMagnitude_Quaternion(Quaternion value, float epsilon)
    {
        return value.Angle() > epsilon;
    }
    #endregion

    #region Magnitude
    public static float Magnitude_Float(float value)
    {
        return value;
    }

    public static float Magnitude_Vector2(Vector2 value)
    {
        return value.magnitude;
    }

    public static float Magnitude_Vector3(Vector3 value)
    {
        return value.magnitude;
    }

    public static float Magnitude_Vector4(Vector4 value)
    {
        return value.magnitude;
    }

    public static float Magnitude_Quaternion(Quaternion value)
    {
        return value.Angle();
    }
    #endregion

    #region Negate
    public static float Negate_Float(float value)
    {
        return -value;
    }

    public static Vector2 Negate_Vector2(Vector2 value)
    {
        return -value;
    }

    public static Vector3 Negate_Vector3(Vector3 value)
    {
        return -value;
    }

    public static Vector4 Negate_Vector4(Vector4 value)
    {
        return -value;
    }

    public static Quaternion Negate_Quaternion(Quaternion value)
    {
        return Quaternion.Inverse(value);
    }
    #endregion
    #endregion
}
