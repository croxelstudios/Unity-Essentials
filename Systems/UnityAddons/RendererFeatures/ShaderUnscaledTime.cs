using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// RenderGraph version of a feature that overrides Unity's time shader variables
// (_Time, _SinTime, _CosTime, unity_DeltaTime) with unscaled time.
// Requires URP 14+ (Unity 2022.2+) where URP RenderGraph API is available.
// Add this feature to your URP Renderer data asset in the "Renderer Features" list.
public class ShaderUnscaledTime : ScriptableRendererFeature
{
    class UnscaledTimePass : ScriptableRenderPass
    {
        static readonly int ID_Time = Shader.PropertyToID("_Time");
        static readonly int ID_SinTime = Shader.PropertyToID("_SinTime");
        static readonly int ID_CosTime = Shader.PropertyToID("_CosTime");
        static readonly int ID_TimeParameters = Shader.PropertyToID("_TimeParameters");
        static readonly int ID_DeltaTime = Shader.PropertyToID("unity_DeltaTime");

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("UnscaledTimeOverride", out var passData))
            {
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    var cmd = ctx.cmd;

                    float t = Time.unscaledTime;
                    float dt = Time.unscaledDeltaTime;

                    Vector4 timeVec = new Vector4(t / 20f, t, t * 2f, t * 3f);
                    Vector4 sinTime = new Vector4(
                        Mathf.Sin(t / 8f),
                        Mathf.Sin(t / 4f),
                        Mathf.Sin(t / 2f),
                        Mathf.Sin(t)
                    );
                    Vector4 cosTime = new Vector4(
                        Mathf.Cos(t / 8f),
                        Mathf.Cos(t / 4f),
                        Mathf.Cos(t / 2f),
                        Mathf.Cos(t)
                    );
                    Vector4 deltaTime = new Vector4(
                        dt,
                        dt > 0f ? 1f / dt : 0f,
                        Time.smoothDeltaTime,
                        Time.smoothDeltaTime > 0f ? 1f / Time.smoothDeltaTime : 0f
                    );

                    cmd.SetGlobalVector(ID_Time, timeVec);
                    cmd.SetGlobalVector(ID_SinTime, sinTime);
                    cmd.SetGlobalVector(ID_CosTime, cosTime);
                    cmd.SetGlobalVector(ID_TimeParameters, new Vector3(t, sinTime.w, cosTime.w));
                    cmd.SetGlobalVector(ID_DeltaTime, deltaTime);
                });
            }
        }
        class PassData { }
    }

    UnscaledTimePass pass;
    public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;

    public override void Create()
    {
        pass = new UnscaledTimePass
        {
            renderPassEvent = renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
