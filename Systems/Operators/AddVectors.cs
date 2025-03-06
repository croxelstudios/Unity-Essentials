using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddVectors : MonoBehaviour
{
    [SerializeField]
    DXVectorEvent vectorEvent = null;

    Vector3 sum;

    void LateUpdate()
    {
        DoUpdate();
        sum = Vector3.zero;
    }

    void DoUpdate()
    {
        vectorEvent?.Invoke(sum);
    }

    public void Add(Vector3 vector)
    {
        sum += vector;
    }
}
