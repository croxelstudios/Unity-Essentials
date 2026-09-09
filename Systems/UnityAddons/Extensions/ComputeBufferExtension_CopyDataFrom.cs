using System.Collections.Generic;
using UnityEngine;

public static class ComputeBufferExtension_CopyDataFrom
{
    const string copyCompute = "CopyBuffer";
    static ComputeShader compute;
    static Dictionary<int, string> dict;

    public static void CopyDataFrom(this ComputeBuffer target, ComputeBuffer source)
    {
        if (target.stride != source.stride)
        {
            Debug.LogError("ComputeBuffer.CopyDataFrom: Buffers stride is not the same");
            return;
        }

        if (compute == null)
            compute = (ComputeShader)Resources.Load(copyCompute);

        int count = Mathf.Min(target.count, source.count);
        compute.SetInt("count", count);

        int stride = target.stride;
        int ki = compute.FindKernel(GetName(stride, "CopyBuffer"));
        compute.SetBuffer(ki, GetName(stride, "source"), source);
        compute.SetBuffer(ki, GetName(stride, "target"), target);

        compute.Dispatch(ki, Mathf.CeilToInt(count / Computables.Numthreads_Small), 1, 1);
    }

    static string GetName(int stride, string start) //TO DO: There is probably a more generic way
    {
        if (dict == null)
        {
            dict = new()
            {
                { sizeof(float), "float" },
                { sizeof(float) * 2, "float2" },
                { sizeof(float) * 3, "float3" },
                { sizeof(float) * 4, "float4" }
            };
        }

        return start + "_" + dict[stride];
    }
}
