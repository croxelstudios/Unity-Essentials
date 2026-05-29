using System;
using System.Collections.Generic;
using UnityEngine;

public static class Generics
{
    public delegate T SmoothDampImpl<T>(T current, T target, ref T currentVelocity,
        float smoothTime, float maxSpeed, float deltaTime);
    public delegate bool IsZeroImpl<T>(T value);
    public delegate T AddImpl<T>(T A, T B);
    public delegate T SubtractImpl<T>(T A, T B);
    public delegate T ScaleImpl<T>(T value, float scale);
    public delegate T MultiplyImpl<T>(T A, T B);
    public delegate bool HasMagnitudeImpl<T>(T value, float epsilon);
    public delegate float MagnitudeImpl<T>(T value);
    public delegate T NegateImpl<T>(T value);
    public delegate D DirectionImpl<T, D>(T value);
    public delegate void DirectionMagnitudeImpl<T, D>(T value, out D direction, out float magnitude);
    public delegate T FromDirectionMagnitudeImpl<T, D>(D direction, float magnitude);
    public delegate float DotImpl<T>(T a, T b);
    public delegate T LerpImpl<T>(T a, T b, float t);

    static readonly Dictionary<Type, object> table_SmoothDamp;
    static readonly Dictionary<Type, object> table_IsZero;
    static readonly Dictionary<Type, object> table_Add;
    static readonly Dictionary<Type, object> table_Subtract;
    static readonly Dictionary<Type, object> table_Scale;
    static readonly Dictionary<Type, object> table_Multiply;
    static readonly Dictionary<Type, object> table_HasMagnitude;
    static readonly Dictionary<Type, object> table_Magnitude;
    static readonly Dictionary<Type, object> table_Negate;
    static readonly Dictionary<(Type, Type), object> table_Direction;
    static readonly Dictionary<(Type, Type), object> table_DirectionMagnitude;
    static readonly Dictionary<(Type, Type), object> table_FromDirectionMagnitude;
    static readonly Dictionary<Type, object> table_Dot;
    static readonly Dictionary<Type, object> table_Lerp;

    static Generics()
    {
        const int MANY = 6;
        const int VECTORS = 4;

        table_SmoothDamp = new Dictionary<Type, object>(MANY);
        table_IsZero = new Dictionary<Type, object>(MANY);
        table_Add = new Dictionary<Type, object>(MANY);
        table_Subtract = new Dictionary<Type, object>(MANY);
        table_Scale= new Dictionary<Type, object>(MANY);
        table_Multiply = new Dictionary<Type, object>(MANY);
        table_HasMagnitude = new Dictionary<Type, object>(MANY);
        table_Magnitude = new Dictionary<Type, object>(MANY);
        table_Negate = new Dictionary<Type, object>(MANY);
        table_Dot = new Dictionary<Type, object>(VECTORS);
        table_Lerp = new Dictionary<Type, object>(MANY);
        table_Direction = new Dictionary<(Type, Type), object>(MANY);
        table_DirectionMagnitude = new Dictionary<(Type, Type), object>(MANY);
        table_FromDirectionMagnitude = new Dictionary<(Type, Type), object>(MANY);

        // SmoothDamp
        table_SmoothDamp[typeof(float)] = new SmoothDampImpl<float>(SmoothDamp_Float);
        table_SmoothDamp[typeof(Vector2)] = new SmoothDampImpl<Vector2>(SmoothDamp_Vector2);
        table_SmoothDamp[typeof(Vector3)] = new SmoothDampImpl<Vector3>(SmoothDamp_Vector3);
        table_SmoothDamp[typeof(Vector4)] = new SmoothDampImpl<Vector4>(SmoothDamp_Vector4);
        table_SmoothDamp[typeof(Color)] = new SmoothDampImpl<Color>(SmoothDamp_Color);
        table_SmoothDamp[typeof(Quaternion)] = new SmoothDampImpl<Quaternion>(SmoothDamp_Quaternion);

        // IsZero
        table_IsZero[typeof(float)] = new IsZeroImpl<float>(IsZero_Float);
        table_IsZero[typeof(Vector2)] = new IsZeroImpl<Vector2>(IsZero_Vector2);
        table_IsZero[typeof(Vector3)] = new IsZeroImpl<Vector3>(IsZero_Vector3);
        table_IsZero[typeof(Vector4)] = new IsZeroImpl<Vector4>(IsZero_Vector4);
        table_IsZero[typeof(Color)] = new IsZeroImpl<Color>(IsZero_Color);
        table_IsZero[typeof(Quaternion)] = new IsZeroImpl<Quaternion>(IsZero_Quaternion);

        // Add
        table_Add[typeof(float)] = new AddImpl<float>(Add_Float);
        table_Add[typeof(Vector2)] = new AddImpl<Vector2>(Add_Vector2);
        table_Add[typeof(Vector3)] = new AddImpl<Vector3>(Add_Vector3);
        table_Add[typeof(Vector4)] = new AddImpl<Vector4>(Add_Vector4);
        table_Add[typeof(Color)] = new AddImpl<Color>(Add_Color);
        table_Add[typeof(Quaternion)] = new AddImpl<Quaternion>(Add_Quaternion);

        // Scale
        table_Scale[typeof(float)] = new ScaleImpl<float>(Scale_Float);
        table_Scale[typeof(Vector2)] = new ScaleImpl<Vector2>(Scale_Vector2);
        table_Scale[typeof(Vector3)] = new ScaleImpl<Vector3>(Scale_Vector3);
        table_Scale[typeof(Vector4)] = new ScaleImpl<Vector4>(Scale_Vector4);
        table_Scale[typeof(Color)] = new ScaleImpl<Color>(Scale_Color);
        table_Scale[typeof(Quaternion)] = new ScaleImpl<Quaternion>(Scale_Quaternion);

        // Multiply
        table_Multiply[typeof(float)] = new MultiplyImpl<float>(Multiply_Float);
        table_Multiply[typeof(Vector2)] = new MultiplyImpl<Vector2>(Multiply_Vector2);
        table_Multiply[typeof(Vector3)] = new MultiplyImpl<Vector3>(Multiply_Vector3);
        table_Multiply[typeof(Vector4)] = new MultiplyImpl<Vector4>(Multiply_Vector4);
        table_Multiply[typeof(Color)] = new MultiplyImpl<Color>(Multiply_Color);
        table_Multiply[typeof(Quaternion)] = new MultiplyImpl<Quaternion>(Multiply_Quaternion);

        // Subtract
        table_Subtract[typeof(float)] = new SubtractImpl<float>(Subtract_Float);
        table_Subtract[typeof(Vector2)] = new SubtractImpl<Vector2>(Subtract_Vector2);
        table_Subtract[typeof(Vector3)] = new SubtractImpl<Vector3>(Subtract_Vector3);
        table_Subtract[typeof(Vector4)] = new SubtractImpl<Vector4>(Subtract_Vector4);
        table_Subtract[typeof(Color)] = new SubtractImpl<Color>(Subtract_Color);
        table_Subtract[typeof(Quaternion)] = new SubtractImpl<Quaternion>(Subtract_Quaternion);

        // HasMagnitude
        table_HasMagnitude[typeof(float)] = new HasMagnitudeImpl<float>(HasMagnitude_Float);
        table_HasMagnitude[typeof(Vector2)] = new HasMagnitudeImpl<Vector2>(HasMagnitude_Vector2);
        table_HasMagnitude[typeof(Vector3)] = new HasMagnitudeImpl<Vector3>(HasMagnitude_Vector3);
        table_HasMagnitude[typeof(Vector4)] = new HasMagnitudeImpl<Vector4>(HasMagnitude_Vector4);
        table_HasMagnitude[typeof(Color)] = new HasMagnitudeImpl<Color>(HasMagnitude_Color);
        table_HasMagnitude[typeof(Quaternion)] = new HasMagnitudeImpl<Quaternion>(HasMagnitude_Quaternion);

        // Magnitude
        table_Magnitude[typeof(float)] = new MagnitudeImpl<float>(Magnitude_Float);
        table_Magnitude[typeof(Vector2)] = new MagnitudeImpl<Vector2>(Magnitude_Vector2);
        table_Magnitude[typeof(Vector3)] = new MagnitudeImpl<Vector3>(Magnitude_Vector3);
        table_Magnitude[typeof(Vector4)] = new MagnitudeImpl<Vector4>(Magnitude_Vector4);
        table_Magnitude[typeof(Color)] = new MagnitudeImpl<Color>(Magnitude_Color);
        table_Magnitude[typeof(Quaternion)] = new MagnitudeImpl<Quaternion>(Magnitude_Quaternion);

        // Negate
        table_Negate[typeof(float)] = new NegateImpl<float>(Negate_Float);
        table_Negate[typeof(Vector2)] = new NegateImpl<Vector2>(Negate_Vector2);
        table_Negate[typeof(Vector3)] = new NegateImpl<Vector3>(Negate_Vector3);
        table_Negate[typeof(Vector4)] = new NegateImpl<Vector4>(Negate_Vector4);
        table_Negate[typeof(Color)] = new NegateImpl<Color>(Negate_Color);
        table_Negate[typeof(Quaternion)] = new NegateImpl<Quaternion>(Negate_Quaternion);

        // Dot
        table_Dot[typeof(Vector2)] = new DotImpl<Vector2>(Dot_Vector2);
        table_Dot[typeof(Vector3)] = new DotImpl<Vector3>(Dot_Vector3);
        table_Dot[typeof(Vector4)] = new DotImpl<Vector4>(Dot_Vector4);
        table_Dot[typeof(Color)] = new DotImpl<Color>(Dot_Color);

        // Lerp
        table_Lerp[typeof(float)] = new LerpImpl<float>(Lerp_Float);
        table_Lerp[typeof(Vector2)] = new LerpImpl<Vector2>(Lerp_Vector2);
        table_Lerp[typeof(Vector3)] = new LerpImpl<Vector3>(Lerp_Vector3);
        table_Lerp[typeof(Vector4)] = new LerpImpl<Vector4>(Lerp_Vector4);
        table_Lerp[typeof(Color)] = new LerpImpl<Color>(Lerp_Color);
        table_Lerp[typeof(Quaternion)] = new LerpImpl<Quaternion>(Lerp_Quaternion);

        // Direction
        table_Direction[(typeof(float), typeof(float))] = new DirectionImpl<float, float>(Direction_Float);
        table_Direction[(typeof(Vector2), typeof(Vector2))] = new DirectionImpl<Vector2, Vector2>(Direction_Vector2);
        table_Direction[(typeof(Vector3), typeof(Vector3))] = new DirectionImpl<Vector3, Vector3>(Direction_Vector3);
        table_Direction[(typeof(Vector4), typeof(Vector4))] = new DirectionImpl<Vector4, Vector4>(Direction_Vector4);
        table_Direction[(typeof(Quaternion), typeof(Vector3))] = new DirectionImpl<Quaternion, Vector3>(Direction_Quaternion);
        table_Direction[(typeof(Color), typeof(Color))] = new DirectionImpl<Color, Color>(Direction_Color);

        // DirectionMagnitude
        table_DirectionMagnitude[(typeof(float), typeof(float))] =
            new DirectionMagnitudeImpl<float, float>(DirectionMagnitude_Float);
        table_DirectionMagnitude[(typeof(Vector2), typeof(Vector2))] =
            new DirectionMagnitudeImpl<Vector2, Vector2>(DirectionMagnitude_Vector2);
        table_DirectionMagnitude[(typeof(Vector3), typeof(Vector3))] =
            new DirectionMagnitudeImpl<Vector3, Vector3>(DirectionMagnitude_Vector3);
        table_DirectionMagnitude[(typeof(Vector4), typeof(Vector4))] =
            new DirectionMagnitudeImpl<Vector4, Vector4>(DirectionMagnitude_Vector4);
        table_DirectionMagnitude[(typeof(Quaternion), typeof(Vector3))] =
            new DirectionMagnitudeImpl<Quaternion, Vector3>(DirectionMagnitude_Quaternion);
        table_DirectionMagnitude[(typeof(Color), typeof(Color))] =
            new DirectionMagnitudeImpl<Color, Color>(DirectionMagnitude_Color);

        // FromDirectionMagnitude
        table_FromDirectionMagnitude[(typeof(float), typeof(float))] =
            new FromDirectionMagnitudeImpl<float, float>(FromDirectionMagnitude_Float);
        table_FromDirectionMagnitude[(typeof(Vector2), typeof(Vector2))] =
            new FromDirectionMagnitudeImpl<Vector2, Vector2>(FromDirectionMagnitude_Vector2);
        table_FromDirectionMagnitude[(typeof(Vector3), typeof(Vector3))] =
            new FromDirectionMagnitudeImpl<Vector3, Vector3>(FromDirectionMagnitude_Vector3);
        table_FromDirectionMagnitude[(typeof(Vector4), typeof(Vector4))] =
            new FromDirectionMagnitudeImpl<Vector4, Vector4>(FromDirectionMagnitude_Vector4);
        table_FromDirectionMagnitude[(typeof(Quaternion), typeof(Vector3))] =
            new FromDirectionMagnitudeImpl<Quaternion, Vector3>(FromDirectionMagnitude_Quaternion);
        table_FromDirectionMagnitude[(typeof(Color), typeof(Color))] =
            new FromDirectionMagnitudeImpl<Color, Color>(FromDirectionMagnitude_Color);
    }

    #region Register functions
    public static void Register_SmoothDamp<T>(SmoothDampImpl<T> impl)
    {
        table_SmoothDamp[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_IsZero<T>(IsZeroImpl<T> impl)
    {
        table_IsZero[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Add<T>(AddImpl<T> impl)
    {
        table_Add[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Subtract<T>(SubtractImpl<T> impl)
    {
        table_Subtract[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Scale<T>(ScaleImpl<T> impl)
    {
        table_Scale[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_HasMagnitude<T>(HasMagnitudeImpl<T> impl)
    {
        table_HasMagnitude[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Magnitude<T>(MagnitudeImpl<T> impl)
    {
        table_Magnitude[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Negate<T>(NegateImpl<T> impl)
    {
        table_Negate[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Dot<T>(DotImpl<T> impl)
    {
        table_Dot[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Lerp<T>(LerpImpl<T> impl)
    {
        table_Lerp[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_Direction<T, D>(DirectionImpl<T, D> impl)
    {
        table_Direction[(typeof(T), typeof(D))] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_DirectionMagnitude<T, D>(DirectionMagnitudeImpl<T, D> impl)
    {
        table_DirectionMagnitude[(typeof(T), typeof(D))] = impl ?? throw new ArgumentNullException(nameof(impl));
    }

    public static void Register_FromDirectionMagnitude<T, D>(FromDirectionMagnitudeImpl<T, D> impl)
    {
        table_FromDirectionMagnitude[(typeof(T), typeof(D))] = impl ?? throw new ArgumentNullException(nameof(impl));
    }
    #endregion

    public static T SmoothDamp<T>(T current, T target, ref T currentVelocity,
        float smoothTime, float maxSpeed, float deltaTime)
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

    public static bool IsZero<T>(T value)
    {
        if (!table_IsZero.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.IsZero(). Register an implementation using" +
                $"Generics.Register_IsZero<{typeof(T).Name}>(...) if necessary.");

        IsZeroImpl<T> impl = (IsZeroImpl<T>)o;
        return impl(value);
    }

    public static T Add<T>(T A, T B)
    {
        if (!table_Add.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Add(). Register an implementation using" +
                $"Generics.Register_Add<{typeof(T).Name}>(...) if necessary.");

        AddImpl<T> impl = (AddImpl<T>)o;
        return impl(A, B);
    }

    public static T Subtract<T>(T A, T B)
    {
        if (!table_Subtract.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Subtract(). Register an implementation using" +
                $"Generics.Register_Subtract<{typeof(T).Name}>(...) if necessary.");

        SubtractImpl<T> impl = (SubtractImpl<T>)o;
        return impl(A, B);
    }

    public static T Scale<T>(T value, float scale)
    {
        if (!table_Scale.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Scale(). Register an implementation using" +
                $"Generics.Register_Scale<{typeof(T).Name}>(...) if necessary.");

        ScaleImpl<T> impl = (ScaleImpl<T>)o;
        return impl(value, scale);
    }

    public static T Multiply<T>(T A, T B)
    {
        if (!table_Multiply.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Scale(). Register an implementation using" +
                $"Generics.Register_Scale<{typeof(T).Name}>(...) if necessary.");

        MultiplyImpl<T> impl = (MultiplyImpl<T>)o;
        return impl(A, B);
    }

    public static bool HasMagnitude<T>(T value, float epsilon = 0f)
    {
        if (!table_HasMagnitude.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.HasMagnitude(). Register an implementation using" +
                $"Generics.Register_HasMagnitude<{typeof(T).Name}>(...) if necessary.");

        HasMagnitudeImpl<T> impl = (HasMagnitudeImpl<T>)o;
        return impl(value, epsilon);
    }

    public static float Magnitude<T>(T value)
    {
        if (!table_Magnitude.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Magnitude(). Register an implementation using" +
                $"Generics.Register_Magnitude<{typeof(T).Name}>(...) if necessary.");

        MagnitudeImpl<T> impl = (MagnitudeImpl<T>)o;
        return impl(value);
    }

    public static T Negate<T>(T value)
    {
        if (!table_Negate.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Negate(). Register an implementation using" +
                $"Generics.Register_Negate<{typeof(T).Name}>(...) if necessary.");

        NegateImpl<T> impl = (NegateImpl<T>)o;
        return impl(value);
    }

    public static float Dot<T>(T a, T b)
    {
        if (!table_Dot.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Dot(). Register an implementation using" +
                $"Generics.Register_Dot<{typeof(T).Name}>(...) if necessary.");

        DotImpl<T> impl = (DotImpl<T>)o;
        return impl(a, b);
    }

    public static T Lerp<T>(T a, T b, float t)
    {
        if (!table_Lerp.TryGetValue(typeof(T), out object o))
            throw new NotSupportedException($"Type {typeof(T)} not supported by" +
                $"Generics.Lerp(). Register an implementation using" +
                $"Generics.Register_Lerp<{typeof(T).Name}>(...) if necessary.");

        LerpImpl<T> impl = (LerpImpl<T>)o;
        return impl(a, b, t);
    }

    public static D Direction<T, D>(T value)
    {
        if (!table_Direction.TryGetValue((typeof(T), typeof(D)), out object o))
            throw new NotSupportedException($"Type {typeof(T)}-{typeof(D)} not supported by" +
                $"Generics.Direction(). Register an implementation using" +
                $"Generics.Register_Direction<{typeof(T).Name}, {typeof(D).Name}>(...) if necessary.");

        DirectionImpl<T, D> impl = (DirectionImpl<T, D>)o;
        return impl(value);
    }

    public static void DirectionMagnitude<T, D>(T value, out D direction, out float magnitude)
    {
        if (!table_DirectionMagnitude.TryGetValue((typeof(T), typeof(D)), out object o))
            throw new NotSupportedException($"Type {typeof(T)}-{typeof(D)} not supported by" +
                $"Generics.DirectionMagnitude(). Register an implementation using" +
                $"Generics.Register_DirectionMagnitude<{typeof(T).Name}, {typeof(D).Name}>(...) if necessary.");

        DirectionMagnitudeImpl<T, D> impl = (DirectionMagnitudeImpl<T, D>)o;
        impl(value, out direction, out magnitude);
    }

    public static T FromDirectionMagnitude<T, D>(D direction, float magnitude)
    {
        if (!table_FromDirectionMagnitude.TryGetValue((typeof(T), typeof(D)), out object o))
            throw new NotSupportedException($"Type {typeof(T)}-{typeof(D)} not supported by" +
                $"Generics.FromDirectionMagnitude(). Register an implementation using" +
                $"Generics.Register_FromDirectionMagnitude<{typeof(T).Name}, {typeof(D).Name}>(...) if necessary.");

        FromDirectionMagnitudeImpl<T, D> impl = (FromDirectionMagnitudeImpl<T, D>)o;
        return impl(direction, magnitude);
    }

    public static float Distance<T>(T a, T b)
    {
        return Magnitude(Subtract(a, b));
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
    
    private static Color SmoothDamp_Color(Color current, Color target, ref Color currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        float vx = currentVelocity.r, vy = currentVelocity.g, vz = currentVelocity.b, vw = currentVelocity.a;
        float x = Mathf.SmoothDamp(current.r, target.r, ref vx, smoothTime, maxSpeed, deltaTime);
        float y = Mathf.SmoothDamp(current.g, target.g, ref vy, smoothTime, maxSpeed, deltaTime);
        float z = Mathf.SmoothDamp(current.b, target.b, ref vz, smoothTime, maxSpeed, deltaTime);
        float w = Mathf.SmoothDamp(current.a, target.a, ref vw, smoothTime, maxSpeed, deltaTime);
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

    public static bool IsZero_Color(Color value)
    {
        return value == Color.clear;
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

    public static Color Add_Color(Color A, Color B)
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

    public static Color Subtract_Color(Color A, Color B)
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

    public static Color Scale_Color(Color value, float scale)
    {
        return value * scale;
    }

    public static Quaternion Scale_Quaternion(Quaternion value, float scale)
    {
        return value.Scale(scale);
    }
    #endregion

    #region Multiply
    public static float Multiply_Float(float A, float B)
    {
        return A * B;
    }

    public static Vector2 Multiply_Vector2(Vector2 A, Vector2 B)
    {
        return Vector2.Scale(A, B);
    }

    public static Vector3 Multiply_Vector3(Vector3 A, Vector3 B)
    {
        return Vector3.Scale(A, B);
    }

    public static Vector4 Multiply_Vector4(Vector4 A, Vector4 B)
    {
        return Vector4.Scale(A, B);
    }

    public static Color Multiply_Color(Color A, Color B)
    {
        return new Color(A.r * B.r, A.g * B.g, A.b * B.b, A.a * B.a);
    }

    public static Quaternion Multiply_Quaternion(Quaternion A, Quaternion B)
    {
        return new Quaternion(A.x * B.x, A.y * B.y, A.z * B.z, A.w * B.w);
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

    public static bool HasMagnitude_Color(Color value, float epsilon)
    {
        return value.grayscale > epsilon;
    }

    public static bool HasMagnitude_Quaternion(Quaternion value, float epsilon)
    {
        return value.Angle() > epsilon;
    }
    #endregion

    #region Magnitude
    public static float Magnitude_Float(float value)
    {
        return Mathf.Abs(value);
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

    public static float Magnitude_Color(Color value)
    {
        return value.grayscale;
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

    public static Color Negate_Color(Color value)
    {
        return Color.white - value;
    }

    public static Quaternion Negate_Quaternion(Quaternion value)
    {
        return Quaternion.Inverse(value);
    }
    #endregion

    #region Dot
    public static float Dot_Vector2(Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b);
    }

    public static float Dot_Vector3(Vector3 a, Vector3 b)
    {
        return Vector3.Dot(a, b);
    }

    public static float Dot_Vector4(Vector4 a, Vector4 b)
    {
        return Vector4.Dot(a, b);
    }

    public static float Dot_Color(Color a, Color b)
    {
        return Vector4.Dot(a, b);
    }
    #endregion

    #region Lerp
    public static float Lerp_Float(float a, float b, float t)
    {
        return Mathf.Lerp(a, b, t);
    }

    public static Vector2 Lerp_Vector2(Vector2 a, Vector2 b, float t)
    {
        return Vector2.Lerp(a, b, t);
    }

    public static Vector3 Lerp_Vector3(Vector3 a, Vector3 b, float t)
    {
        return Vector3.Lerp(a, b, t);
    }

    public static Vector4 Lerp_Vector4(Vector4 a, Vector4 b, float t)
    {
        return Vector4.Lerp(a, b, t);
    }

    public static Color Lerp_Color(Color a, Color b, float t)
    {
        return Color.Lerp(a, b, t);
    }

    public static Quaternion Lerp_Quaternion(Quaternion a, Quaternion b, float t)
    {
        return Quaternion.Slerp(a, b, t);
    }
    #endregion

    #region Direction
    public static float Direction_Float(float value)
    {
        return Mathf.Sign(value);
    }

    public static Vector2 Direction_Vector2(Vector2 value)
    {
        return value.normalized;
    }

    public static Vector3 Direction_Vector3(Vector3 value)
    {
        return value.normalized;
    }

    public static Vector4 Direction_Vector4(Vector4 value)
    {
        return value.normalized;
    }

    public static Vector3 Direction_Quaternion(Quaternion value)
    {
        return value.Axis();
    }

    public static Color Direction_Color(Color value)
    {
        return value / value.grayscale;
    }
    #endregion

    #region DirectionMagnitude
    public static void DirectionMagnitude_Float(float value, out float sign, out float abs)
    {
        if (value < 0f)
        {
            sign = -1f;
            abs = -value;
        }
        else
        {
            sign = 1f;
            abs = value;
        }
    }

    public static void DirectionMagnitude_Vector2(Vector2 value, out Vector2 direction, out float magnitude)
    {
        magnitude = value.magnitude;
        direction = (magnitude > 0f) ? (value / magnitude) : Vector2.zero;
    }

    public static void DirectionMagnitude_Vector3(Vector3 value, out Vector3 direction, out float magnitude)
    {
        magnitude = value.magnitude;
        direction = (magnitude > 0f) ? (value / magnitude) : Vector3.zero;
    }

    public static void DirectionMagnitude_Vector4(Vector4 value, out Vector4 direction, out float magnitude)
    {
        magnitude = value.magnitude;
        direction = (magnitude > 0f) ? (value / magnitude) : Vector4.zero;
    }

    public static void DirectionMagnitude_Quaternion(Quaternion value, out Vector3 axis, out float angle)
    {
        value.ToAngleAxis(out angle, out axis);
    }

    public static void DirectionMagnitude_Color(Color value, out Color hue, out float grayscale)
    {
        grayscale = value.grayscale;
        float alpha = value.a;
        hue = (grayscale > 0f) ? (value / grayscale) : Color.black;
        hue.a = alpha;
    }
    #endregion

    #region FromDirectionMagnitude
    public static float FromDirectionMagnitude_Float(float sign, float abs)
    {
        return Mathf.Sign(sign) * abs;
    }

    public static Vector2 FromDirectionMagnitude_Vector2(Vector2 direction, float magnitude)
    {
        return direction.normalized * magnitude;
    }

    public static Vector3 FromDirectionMagnitude_Vector3(Vector3 direction, float magnitude)
    {
        return direction.normalized * magnitude;
    }

    public static Vector4 FromDirectionMagnitude_Vector4(Vector4 direction, float magnitude)
    {
        return direction.normalized * magnitude;
    }

    public static Quaternion FromDirectionMagnitude_Quaternion(Vector3 axis, float angle)
    {
        return Quaternion.AngleAxis(angle, axis);
    }

    public static Color FromDirectionMagnitude_Color(Color hue, float grayscale)
    {
        float alpha = hue.a;
        hue *= grayscale;
        hue.a = alpha;
        return hue;
    }
    #endregion
    #endregion
}
