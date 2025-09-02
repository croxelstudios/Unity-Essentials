using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// RenderGraph version of a feature that overrides Unity's time shader variables
// (_Time, _SinTime, _CosTime, unity_DeltaTime) with unscaled time.
// Requires URP 14+ (Unity 2022.2+) where URP RenderGraph API is available.
// Add this feature to your URP Renderer data asset in the "Renderer Features" list.
public class ShaderTimeModifier : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {
        bool useUnscaledTime = true;
        float timeScale = 1f;

        public RenderPass(bool useUnscaledTime, float timeScale)
        {
            this.useUnscaledTime = useUnscaledTime;
            this.timeScale = timeScale;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddUnsafePass("UnscaledTimeOverride", out PassData passData))
            {
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
                {
                    UnsafeCommandBuffer cmd = ctx.cmd;
                    
                    if (useUnscaledTime)
                        cmd.ShaderTime_Unscaled(timeScale);
                    else
                        cmd.ShaderTime_Scaled(timeScale);
                });
            }
        }

        class PassData { }
    }

    RenderPass pass;

    [SerializeField]
    bool useUnscaledTime = true;
    [SerializeField]
    float timeScale = 1f;
    [SerializeField]
    RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;

    public override void Create()
    {
        pass = new RenderPass(useUnscaledTime, timeScale)
        { renderPassEvent = renderPassEvent };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
