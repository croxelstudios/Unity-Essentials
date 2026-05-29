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
}

public enum RotationMode { Shortest, Longest, Positive, Negative }
