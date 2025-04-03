using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtension_BasicOperators
{
    public static Quaternion Add(this Quaternion addee, Quaternion adder)
    {
        return Quaternion.Inverse(adder) * addee;
    }

    public static Quaternion Subtract(this Quaternion subtractee, Quaternion subtractor, RotationMode mode = RotationMode.Shortest)
    {
        float dot = Quaternion.Dot(subtractor, subtractee);
        switch (mode)
        {
            case RotationMode.Longest:
                if (dot > 0f)
                    subtractee = new Quaternion(-subtractee.x, -subtractee.y, -subtractee.z, -subtractee.w);
                break;
            case RotationMode.Positive:
                if (subtractor.w < 0f)
                    subtractor = new Quaternion(-subtractor.x, -subtractor.y, -subtractor.z, -subtractor.w);
                if (subtractee.w < 0f)
                    subtractee = new Quaternion(-subtractee.x, -subtractee.y, -subtractee.z, -subtractee.w);
                break;
            case RotationMode.Negative:
                if (subtractor.w > 0f)
                    subtractor = new Quaternion(-subtractor.x, -subtractor.y, -subtractor.z, -subtractor.w);
                if (subtractee.w > 0f)
                    subtractee = new Quaternion(-subtractee.x, -subtractee.y, -subtractee.z, -subtractee.w);
                break;
            default:
                if (dot < 0f)
                    subtractee = new Quaternion(-subtractee.x, -subtractee.y, -subtractee.z, -subtractee.w);
                break;
        }

        return subtractee * Quaternion.Inverse(subtractor);
    }

    public static float AngleDistance(this Quaternion subtractee, Quaternion subtractor, RotationMode mode = RotationMode.Shortest)
    {
        Quaternion dif = Subtract(subtractee, subtractor, mode);
        return dif.Angle();
    }
}

public enum RotationMode { Shortest, Longest, Positive, Negative }
