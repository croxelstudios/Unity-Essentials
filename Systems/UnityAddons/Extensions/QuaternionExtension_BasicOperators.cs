using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtension_BasicOperators
{
    public static Quaternion Add(this Quaternion addee, Quaternion adder)
    {
        return Quaternion.Inverse(adder) * addee;
    }

    public static Quaternion Subtract(this Quaternion subtractee, Quaternion subtractor)
    {
        return subtractee * Quaternion.Inverse(subtractor);
    }
}
