using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DXRenderObjects : ScriptableRendererFeature
{
    /// <summary>
    /// Settings class used for the render objects renderer feature.
    /// </summary>
    [Serializable]
    public class DXRenderObjectsSettings
    {
        /// <summary>
        /// Controls when the render pass executes.
        /// </summary>
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        /// <summary>
        /// The texture target settings to use.
        /// </summary>
        [FoldoutGroup("Texture", true)]
        [HideLabel]
        [InlineProperty]
        public TextureTargetSettings textureTargetSettings = new TextureTargetSettings();

        /// <summary>
        /// The filter settings for the pass.
        /// </summary>
        [FoldoutGroup("Filters", true)]
        [HideLabel]
        [InlineProperty]
        public FilterSettings filterSettings = new FilterSettings();

        [HideLabel]
        [InlineProperty]
        [FoldoutGroup("Overrides", true)]
        public MaterialOverrideBehaviour materialOverride =
            new MaterialOverrideBehaviour(MaterialOverrideBehaviour.OverrideMaterialMode.Shader);

        [FoldoutGroup("Overrides", true)]
        [Tooltip("Global shader keywords that will be activated or deactivated during rendering of this feature")]
        public ShaderKeyword[] shaderKeywords = null;

        [FoldoutGroup("Overrides", true)]
        public float shaderTimeScale = 1f;
        [Indent]
        [FoldoutGroup("Overrides", true)]
        public bool useUnscaledTime = false;

        /// <summary>
        /// Sets whether it should override depth or not.
        /// </summary>
        [LabelText("Depth")]
        [FoldoutGroup("Overrides", true)]
        public bool overrideDepthState = false;

        /// <summary>
        /// Sets whether it should write to depth or not.
        /// </summary>
        [LabelText("Write Depth")]
        [Indent]
        [ShowIf("overrideDepthState")]
        [FoldoutGroup("Overrides", true)]
        public bool enableWrite = true;

        /// <summary>
        /// The depth comparison function to use.
        /// </summary>
        [LabelText("Depth Test")]
        [Indent]
        [ShowIf("overrideDepthState")]
        [FoldoutGroup("Overrides", true)]
        public CompareFunction depthCompareFunction = CompareFunction.LessEqual;

        /// <summary>
        /// The stencil settings to use.
        /// </summary>
        [FoldoutGroup("Overrides", true)]
        public StencilStateData stencilSettings = new StencilStateData();

        /// <summary>
        /// The camera settings to use.
        /// </summary>
        [HideLabel]
        [InlineProperty]
        [FoldoutGroup("Overrides", true)]
        public RenderGraphTools.CameraSettings cameraSettings = new RenderGraphTools.CameraSettings();
    }

    /// <summary>
    /// The texture target settings used.
    /// </summary>
    [Serializable]
    public class TextureTargetSettings
    {
        public TextureTarget textureTarget = TextureTarget.Active;

        [ShowIf("textureTarget", TextureTarget.GlobalTexture)]
        public string textureName = "_CameraDepthTexture";

        [ShowIf("textureTarget", TextureTarget.GlobalTexture)]
        public TextureSettings settings =
            new TextureSettings(GraphicsFormat.R32G32B32A32_SFloat, FilterMode.Point, TextureWrapMode.Clamp,
                MSAASamples.None, DepthBits.None);

        [ShowIf("textureTarget", TextureTarget.RenderTexture)]
        public RenderTexture targetTexture = null;

        [HideIf("textureTarget", TextureTarget.Active)]
        public bool clearColor = true;

        [ShowIf("@(textureTarget != TextureTarget.Active) && clearColor")]
        public Color color = Color.black;

        [HideIf("textureTarget", TextureTarget.Active)]
        public bool clearDepth = true;

        [ShowIf("@(textureTarget != TextureTarget.Active) && clearDepth")]
        public float depth = 1.0f;

        public Material blitMaterial = null;

        [ShowIf("@blitMaterial != null")]
        public bool fetchColorTexture = true;

        public bool FetchColor()
        {
            return (blitMaterial != null) && fetchColorTexture;
        }
    }

    public enum TextureTarget { Active, GlobalTexture, RenderTexture, DepthTexture, NormalsTexture, OpaqueTexture }

    /// <summary>
    /// The filter settings used.
    /// </summary>
    [Serializable]
    public class FilterSettings
    {
        /// <summary>
        /// The queue type for the objects to render.
        /// </summary>
        [LabelText("Queue")]
        public RenderQueueType RenderQueueType;

        /// <summary>
        /// The layer mask to use.
        /// </summary>
        public LayerMask LayerMask;

        /// <summary>
        /// The passes to render.
        /// </summary>
        [LabelText("LightMode Tags")]
        public string[] PassNames;

        /// <summary>
        /// The constructor for the filter settings.
        /// </summary>
        public FilterSettings()
        {
            RenderQueueType = RenderQueueType.Opaque;
            LayerMask = 0;
        }
    }

    [Serializable]
    public struct ShaderKeyword
    {
        [HorizontalGroup]
        public string keyword;
        [HorizontalGroup]
        public bool enable;

        public ShaderKeyword(string keyword, bool enable)
        {
            this.keyword = keyword;
            this.enable = enable;
        }
    }

    /// <summary>
    /// The settings used for the Render Objects renderer feature.
    /// </summary>
    [HideLabel]
    [InlineProperty]
    public DXRenderObjectsSettings settings = new DXRenderObjectsSettings();

    RenderPass pass;
    DepthBlitPass depthPass;
    public static TextureHandle colorDepth;

    /// <inheritdoc/>
    public override void Create()
    {
        FilterSettings filter = settings.filterSettings;

        // Render Objects pass doesn't support events before rendering prepasses.
        // The camera is not setup before this point and all rendering is monoscopic.
        // Events before BeforeRenderingPrepasses should be used for input texture passes (shadow map, LUT, etc) that doesn't depend on the camera.
        // These events are filtering in the UI, but we still should prevent users from changing it from code or
        // by changing the serialized data.
        if (settings.Event < RenderPassEvent.BeforeRenderingPrePasses)
            settings.Event = RenderPassEvent.BeforeRenderingPrePasses;

        pass = new RenderPass(name, settings.Event, filter.PassNames,
            filter.RenderQueueType, filter.LayerMask, settings.textureTargetSettings,
            settings.cameraSettings, settings.materialOverride, settings.shaderKeywords,
            settings.shaderTimeScale, settings.useUnscaledTime);

        pass.InitKeywords();

        if (settings.overrideDepthState)
            pass.SetDepthState(settings.enableWrite, settings.depthCompareFunction);

        if (settings.stencilSettings.overrideStencilState)
            pass.SetStencilState(settings.stencilSettings.stencilReference,
                settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);

        //if (settings.textureTargetSettings.textureTarget == TextureTargetSettings.TextureTarget.DepthTexture)
        //    depthPass = new DepthBlitPass(settings.Event, new Material(Shader.Find("Hidden/RToDepth")), true);
        //else
        depthPass = null;
    }

    /// <inheritdoc/>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Preview
            || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
            return;

        switch (settings.textureTargetSettings.textureTarget)
        {
            case TextureTarget.DepthTexture:
                pass.ConfigureInput(ScriptableRenderPassInput.Depth);
                break;
            case TextureTarget.NormalsTexture:
                pass.ConfigureInput(ScriptableRenderPassInput.Normal);
                break;
            case TextureTarget.OpaqueTexture:
                pass.ConfigureInput(ScriptableRenderPassInput.Depth);
                break;
        }

        renderer.EnqueuePass(pass);
        if (depthPass != null)
            renderer.EnqueuePass(depthPass);
    }

    /// <summary>
    /// The scriptable render pass used with the DX render objects renderer feature.
    /// </summary>
    public class RenderPass : ScriptableRenderPass
    {
        RenderQueueType renderQueueType;
        TextureTargetSettings textureSettings;
        RenderGraphTools.CameraSettings cameraSettings;
        MaterialOverrideBehaviour materialOverride;
        FilteringSettings filteringSettings;
        RenderStateBlock renderStateBlock;
        ShaderKeyword[] overrideKeywords;
        float timeScale;
        bool useUnscaledTime;

        RTHandle rt;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

        class PassData
        {
            public RenderGraphTools.CameraSettings cameraSettings;
            public RenderPassEvent renderPassEvent;

            public TextureHandle color;
            public TextureHandle depth;
            public RendererListHandle rendererListHdl;

            public UniversalCameraData cameraData;

            public bool clearColor;
            public Color clColor;
            public bool clearDepth;
            public float clDepth;
            public ShaderKeyword[] overrideKeywords;

            public List<RTHandle> toRelease;

            public float timeScale;
            public bool useUnscaledTime;

            public void SetKeywords(RasterCommandBuffer cmd)
            {
                if (!overrideKeywords.IsNullOrEmpty())
                    for (int i = 0; i < overrideKeywords.Length; i++)
                    {
                        ShaderKeyword kw = overrideKeywords[i];
                        if (kw.enable)
                            cmd.EnableShaderKeyword(kw.keyword);
                        else
                            cmd.DisableShaderKeyword(kw.keyword);
                        kw.enable = !kw.enable;
                        overrideKeywords[i] = kw;
                    }
            }
        }

        class CopyPassData
        {
            public RTHandle src;
            public TextureHandle dst;
        }

        class ExtraPassData
        {
            public TextureHandle srcTex;
            public TextureHandle color;
            public Material blitMaterial;
        }

        public void InitKeywords()
        {
            if (!overrideKeywords.IsNullOrEmpty())
                for (int i = 0; i < overrideKeywords.Length; i++)
                    GlobalKeyword.Create(overrideKeywords[i].keyword);
        }

        /// <summary>
        /// Sets the write and comparison function for depth.
        /// </summary>
        /// <param name="writeEnabled">Sets whether it should write to depth or not.</param>
        /// <param name="function">The depth comparison function to use.</param>
        public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
        {
            renderStateBlock.mask |= RenderStateMask.Depth;
            renderStateBlock.depthState = new DepthState(writeEnabled, function);
        }

        /// <summary>
        /// Sets up the stencil settings for the pass.
        /// </summary>
        /// <param name="reference">The stencil reference value.</param>
        /// <param name="compareFunction">The comparison function to use.</param>
        /// <param name="passOp">The stencil operation to use when the stencil test passes.</param>
        /// <param name="failOp">The stencil operation to use when the stencil test fails.</param>
        /// <param name="zFailOp">The stencil operation to use when the stencil test fails because of depth.</param>
        public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp,
            StencilOp failOp, StencilOp zFailOp)
        {
            StencilState stencilState = StencilState.defaultValue;
            stencilState.enabled = true;
            stencilState.SetCompareFunction(compareFunction);
            stencilState.SetPassOperation(passOp);
            stencilState.SetFailOperation(failOp);
            stencilState.SetZFailOperation(zFailOp);

            renderStateBlock.mask |= RenderStateMask.Stencil;
            renderStateBlock.stencilReference = reference;
            renderStateBlock.stencilState = stencilState;
        }

        /// <summary>
        /// The constructor for render objects pass.
        /// </summary>
        /// <param name="profilerTag">The profiler tag used with the pass.</param>
        /// <param name="renderPassEvent">Controls when the render pass executes.</param>
        /// <param name="shaderTags">List of shader tags to render with.</param>
        /// <param name="renderQueueType">The queue type for the objects to render.</param>
        /// <param name="layerMask">The layer mask to use for creating filtering settings that control what objects get rendered.</param>
        /// <param name="cameraSettings">The settings for custom cameras values.</param>
        public RenderPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags,
            RenderQueueType renderQueueType, int layerMask, TextureTargetSettings textureSettings,
            RenderGraphTools.CameraSettings cameraSettings, MaterialOverrideBehaviour materialOverride,
            ShaderKeyword[] overrideKeywords, float timeScale, bool useUnscaledTime)
        {
            profilingSampler = new ProfilingSampler(profilerTag);
            this.textureSettings = textureSettings;
            this.overrideKeywords = overrideKeywords;
            this.renderPassEvent = renderPassEvent;
            this.renderQueueType = renderQueueType;
            RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

            if (shaderTags != null && shaderTags.Length > 0)
            {
                foreach (string tag in shaderTags)
                    m_ShaderTagIdList.Add(new ShaderTagId(tag));
            }
            else
            {
                m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            }

            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            this.cameraSettings = cameraSettings;
            this.materialOverride = materialOverride;
            this.timeScale = timeScale;
            this.useUnscaledTime = useUnscaledTime;
        }

        void InitPassData(UniversalCameraData cameraData, ref PassData passData)
        {
            passData.cameraSettings = cameraSettings;
            passData.renderPassEvent = renderPassEvent;
            passData.cameraData = cameraData;
            passData.overrideKeywords = overrideKeywords;
        }

        /// <inheritdoc />
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Data for all passes
            UniversalData universalData = new UniversalData(frameData);
            AccessFlags colorAccess = AccessFlags.Write;
            AccessFlags depthAccess = AccessFlags.Write;
            string texName = textureSettings.textureName;
            TextureTarget texMode = textureSettings.textureTarget;
            BuilderResources resources = GetResources(renderGraph, universalData, texMode, ref texName);

            using (IRasterRenderGraphBuilder builder =
                renderGraph.AddRasterRenderPass(passName, out PassData passData, profilingSampler))
            {
                // Initialize pass data
                InitPassData(universalData.cameraData, ref passData);
                passData.clearColor = (texMode == TextureTarget.Active) ? false : textureSettings.clearColor;
                passData.clColor = textureSettings.color;
                passData.clearDepth = (texMode == TextureTarget.Active) ? false : textureSettings.clearDepth;
                passData.clDepth = textureSettings.depth;
                passData.timeScale = timeScale;
                passData.useUnscaledTime = useUnscaledTime;
                //   -   Add resources
                passData.color = resources.color;
                passData.depth = resources.depth;
                passData.toRelease = passData.toRelease.CreateAddRange(resources.toRelease);
                //

                builder.SetAttachments(frameData, passData.color, passData.depth, colorAccess, depthAccess);

                if (texMode == TextureTarget.GlobalTexture)
                    builder.SetGlobalTextureAfterPass(passData.color, texName);

                builder.SetupRenderingObjects(renderGraph, frameData, renderQueueType,
                    m_ShaderTagIdList, filteringSettings, renderStateBlock,
                    ref passData.rendererListHdl, materialOverride);

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    RasterCommandBuffer cmd = rgContext.cmd;
                    if (data.clearColor || data.clearDepth)
                        cmd.ClearRenderTarget(
                            data.clearDepth, data.clearColor, data.clColor, data.clDepth);
                    // Pre
                    if (data.useUnscaledTime)
                        cmd.ShaderTime_Unscaled(data.timeScale);
                    else cmd.ShaderTime_Scaled(data.timeScale);
                    data.SetKeywords(cmd);
                    //

                    // Main
                    cmd.ExecuteRenderObjects(universalData.cameraData, data.rendererListHdl,
                        universalData.cameraData.IsYFlipped(data.color), data.cameraSettings);
                    //

                    // Post
                    data.SetKeywords(cmd);
                    cmd.ShaderTime_Scaled();
                    //

                    if (!data.toRelease.IsNullOrEmpty())
                    {
                        foreach (RTHandle toRelease in data.toRelease)
                            RTHandles.Release(toRelease);
                        data.toRelease.Clear();
                    }
                });
            }

            //if (textureSettings.blitMaterial != null)
            //{
            //    // Fetch color mechanism
            //    bool fetchColor = textureSettings.FetchColor();
            //    TextureHandle copiedTexture;

            //    if (fetchColor)
            //    {
            //        TextureDesc targetDesc = resources.GetTextureDesc(renderGraph);
            //        targetDesc.name = "_DXRenderObjects_CopiedTexture";
            //        targetDesc.clearBuffer = false;

            //        copiedTexture = renderGraph.CreateTexture(targetDesc);

            //        RTHandle srcHandle = resources.GetColorRT();
            //        TextureHandle dstHandle = copiedTexture;

            //        using (var builder = renderGraph.AddRasterRenderPass(
            //            "DXRenderObjects Copy Pass", out CopyPassData passData, profilingSampler))
            //        {
            //            passData.src = srcHandle;
            //            //builder.UseTexture(passData.src, AccessFlags.Read);

            //            builder.SetRenderAttachment(dstHandle, 0, AccessFlags.Write);

            //            builder.SetRenderFunc((CopyPassData data, RasterGraphContext rgContext) =>
            //            {
            //                Blitter.BlitTexture(rgContext.cmd, data.src, new Vector4(1, 1, 0, 0), 0.0f, false);
            //            });
            //        }
            //    }
            //    else copiedTexture = TextureHandle.nullHandle;

            //    using (IRasterRenderGraphBuilder builder =
            //            renderGraph.AddRasterRenderPass("DXRenderObjects Blit Pass",
            //            out ExtraPassData passData, profilingSampler))
            //    {
            //        passData.srcTex = copiedTexture;
            //        if (passData.srcTex.IsValid())
            //            builder.UseTexture(passData.srcTex, AccessFlags.Read);
            //        passData.color = resources.color;
            //        passData.blitMaterial = textureSettings.blitMaterial;
            //        builder.SetRenderAttachment(passData.color, 0, colorAccess);
            //        builder.SetRenderFunc((ExtraPassData data, RasterGraphContext rgContext) =>
            //        {
            //            RasterCommandBuffer cmd = rgContext.cmd;
            //            RTHandle tex = copiedTexture;
            //            Material material = data.blitMaterial;

            //            if (cmd == null || tex == null || material == null)
            //                return;

            //            sharedPropertyBlock.Clear();
            //            Texture srcTexture = tex.rt;
            //            if (material.HasProperty(ShaderPropertyId.MainTex))
            //                sharedPropertyBlock.SetTexture(ShaderPropertyId.MainTex, srcTexture);
            //            else sharedPropertyBlock.SetTexture(ShaderPropertyId.BlitTexture, srcTexture);

            //            sharedPropertyBlock.SetVector(ShaderPropertyId.BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));

            //            cmd.DrawProcedural(Matrix4x4.identity, material, 0,
            //                MeshTopology.Triangles, 3, 1, sharedPropertyBlock);
            //        });
            //    }
            //}
        }

        struct BuilderResources
        {
            public TextureHandle color;
            public RenderTexture colorRT;
            public TextureHandle depth;
            public List<RTHandle> toRelease;

            public RTHandle GetColorRT()
            {
                if (colorRT != null)
                    return RTHandles.Alloc(colorRT);
                else return color;
            }

            public TextureDesc GetTextureDesc(RenderGraph renderGraph)
            {
                if (colorRT != null)
                    return new TextureDesc(colorRT);
                else return renderGraph.GetTextureDesc(color);
            }
        }

        BuilderResources GetResources(RenderGraph renderGraph, UniversalData data,
            TextureTarget texMode, ref string texName)
        {
            BuilderResources resources = new BuilderResources();
            resources.colorRT = TextureHandle.nullHandle;
            switch (texMode)
            {
                case TextureTarget.GlobalTexture:
                    resources.color = renderGraph.GetTexture(data.frameData,
                        texName, data.cameraData.cameraTargetDescriptor,
                        textureSettings.settings);
                    break;

                case TextureTarget.RenderTexture:
                    RenderTexture tex = textureSettings.targetTexture;
                    RenderTargetIdentifier rti = new RenderTargetIdentifier(tex.colorBuffer);
                    rt = RTHandles.Alloc(rti, "RenderTexture_Color");

                    RenderTargetInfo info = new RenderTargetInfo();
                    info.volumeDepth = tex.volumeDepth;
                    info.height = tex.height;
                    info.width = tex.width;
                    info.bindMS = tex.bindTextureMS;
                    info.msaaSamples = tex.antiAliasing;
                    info.format = tex.graphicsFormat;

                    texName = tex.name;
                    TextureHandle intermediate = renderGraph.ImportTexture(rt, info);

                    resources.color = intermediate;

                    if (tex.depthStencilFormat != GraphicsFormat.None)
                    {
                        rti = new RenderTargetIdentifier(tex.depthBuffer);
                        rt = RTHandles.Alloc(rti, "RenderTexture_Depth");
                        info.format = tex.depthStencilFormat;
                        intermediate = renderGraph.ImportTexture(rt, info);

                        resources.depth = intermediate;
                    }

                    resources.toRelease = resources.toRelease.CreateAdd(rt);
                    resources.colorRT = tex;
                    break;

                case TextureTarget.DepthTexture:
                    texName = "tempDepth";
                    colorDepth = renderGraph.CreateCameraDepthTexture(data.cameraData, texName);

                    resources.color = colorDepth;
                    break;

                case TextureTarget.NormalsTexture:
                    texName = "_CameraNormalsTexture";

                    //WARNING: This is a failed attempt at creating the texture here to avoid redraw and not require the input.
                    //  cameraNormalsTexture seems to always be valid even when it hasn't been created.
                    //  Redraw seems to be unavoidable for now. Same issue occurs in the opaque Texture.
                    //if (!resourceData.cameraNormalsTexture.IsValid()) 
                    //{
                    //    passData.color = renderGraph.CreateCameraNormalsTexture(cameraData, texName);
                    //    texMode = TextureTargetSettings.TextureTarget.GlobalTexture;
                    //}
                    //else
                    resources.color = data.resourceData.cameraNormalsTexture;
                    break;

                case TextureTarget.OpaqueTexture:
                    texName = "_CameraOpaqueTexture";

                    resources.color = renderGraph.CreateCameraTexture(data.cameraData, texName);
                    texMode = TextureTarget.GlobalTexture;
                    break;

                default: //Active
                    texName = "_CameraTargetAttachmentA";
                    resources.color = data.resourceData.activeColorTexture;
                    resources.depth = data.resourceData.activeDepthTexture;
                    break;
            }
            return resources;
        }
    }

    class DepthBlitPass : ScriptableRenderPass
    {
        class PassData
        {
            public TextureHandle srcRHalf;
            public TextureHandle depthTex;
        }

        Material depthBlitMat;
        bool blitOnCameraDepth;

        public DepthBlitPass(RenderPassEvent renderPassEvent, Material mat, bool blitOnCameraDepth = false)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitOnCameraDepth = blitOnCameraDepth;
            depthBlitMat = mat;
            depthBlitMat.SetTexture("_MainTex", colorDepth);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            if (colorDepth.CanBeUsedAsRTHandle())
            {
                UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Depth Blit Pass", out var passData))
                {
                    passData.srcRHalf = colorDepth;
                    builder.UseTexture(passData.srcRHalf);

                    var resData = frameContext.Get<UniversalResourceData>();

                    //builder.SetRenderAttachment(resData.activeColorTexture, 0, AccessFlags.Write);
                    passData.depthTex = blitOnCameraDepth ? resData.cameraDepthTexture : resData.activeDepthTexture;
                    builder.SetRenderAttachmentDepth(passData.depthTex, AccessFlags.Write);

                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawProcedural(Matrix4x4.identity, depthBlitMat, 0, MeshTopology.Triangles, 3, 1);
                    });
                }
            }
        }
    }
}
