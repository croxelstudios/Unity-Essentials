using Sirenix.OdinInspector;
using UnityEngine;

public class AddTwoVectors : MonoBehaviour
{
    [ReadOnly]
    [ShowInInspector]
    Vector3 vector0 = Vector3.zero;
    [ReadOnly]
    [ShowInInspector]
    Vector3 vector1 = Vector3.zero;

    [SerializeField]
    DXVectorEvent result = null;

    public void SetVector0(Vector3 vector)
    {
        vector0 = vector;
        result?.Invoke(vector0 + vector1);
    }

    public void SetVector1(Vector3 vector)
    {
        vector1 = vector;
        result?.Invoke(vector0 + vector1);
    }

    public void SetVector0(Vector2 vector)
    {
        SetVector0((Vector3)vector);
    }

    public void SetVector1(Vector2 vector)
    {
        SetVector1((Vector3)vector);
    }

    public void ResetVector0()
    {
        vector0 = Vector3.zero;
        result?.Invoke(vector0 + vector1);
    }

    public void ResetVector1()
    {
        vector1 = Vector3.zero;
        result?.Invoke(vector0 + vector1);
    }
}
