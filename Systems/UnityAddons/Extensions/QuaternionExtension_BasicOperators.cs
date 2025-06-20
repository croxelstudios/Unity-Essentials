using UnityEngine;

public static class QuaternionExtension_BasicOperators
{
    public static Quaternion Add(this Quaternion addee, Quaternion adder)
    {
        return adder * addee;
    }

    public static Quaternion Subtract(this Quaternion subtractee, Quaternion subtractor)
    {
        return subtractee * Quaternion.Inverse(subtractor);
    }

    public static float AngleDistance(this Quaternion subtractee, Quaternion subtractor, RotationMode mode = RotationMode.Shortest)
    {
        Quaternion dif = Subtract(subtractee, subtractor);
        return dif.Angle(mode);
    }
}

public enum RotationMode { Shortest, Longest, Positive, Negative }
