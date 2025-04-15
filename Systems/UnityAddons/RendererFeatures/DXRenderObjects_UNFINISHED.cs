using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Scripting.APIUpdating;
using System;
using System.Collections.Generic;

/// <summary>
/// The class for the render objects renderer feature.
/// </summary>
[ExcludeFromPreset]
[Tooltip("Render Objects simplifies the injection of additional render passes by exposing a selection of commonly used settings.")]
//[URPHelpURL("urp-renderer-feature", "#render-objects-renderer-featurea-namerender-objects-renderer-featurea")]
public class DXRenderObjects_UNFINISHED : ScriptableRendererFeature
{
    /// <summary>
    /// Settings class used for the render objects renderer feature.
    /// </summary>
    [System.Serializable]
    public class RenderObjectsSettings
    {
        /// <summary>
        /// The profiler tag used with the pass.
        /// </summary>
        [HideInInspector]
        public string passTag = "RenderObjectsFeature";

        /// <summary>
        /// Controls when the render pass executes.
        /// </summary>
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        /// <summary>
        /// The filter settings for the pass.
        /// </summary>
        [FoldoutGroup("Filters", true)]
        [HideLabel]
        [InlineProperty]
        public FilterSettings filterSettings = new FilterSettings();

        /// <summary>
        /// The override material to use.
        /// </summary>
        [FoldoutGroup("Overrides", true)]
        public Material overrideMaterial = null;

        /// <summary>
        /// The pass index to use with the override material.
        /// </summary>
        [Indent]
        [FoldoutGroup("Overrides", true)]
        [ShowIf("@overrideMaterial != null")]
        public int overrideMaterialPassIndex = 0;

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
        public CustomCameraSettings cameraSettings = new CustomCameraSettings();
    }

    /// <summary>
    /// The filter settings used.
    /// </summary>
    [System.Serializable]
    public class FilterSettings
    {
        // TODO: expose opaque, transparent, all ranges as drop down

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

    /// <summary>
    /// The settings for custom cameras values.
    /// </summary>
    [System.Serializable]
    public class CustomCameraSettings
    {
        /// <summary>
        /// Used to mark whether camera values should be changed or not.
        /// </summary>
        public bool overrideCamera = false;

        /// <summary>
        /// Changes the camera field of view.
        /// </summary>
        [LabelText("Field of View")]
        [Indent]
        [ShowIf("overrideCamera")]
        [Range(4f, 179f)]
        public float cameraFieldOfView = 60.0f;

        /// <summary>
        /// Changes the camera offset.
        /// </summary>
        [LabelText("Position Offset")]
        [Indent]
        [ShowIf("overrideCamera")]
        public Vector3 offset;

        /// <summary>
        /// Should the values be reverted after rendering the objects?
        /// </summary>
        [LabelText("Restore")]
        [Indent]
        [ShowIf("overrideCamera")]
        public bool restoreCamera = true;

        RenderObjects.CustomCameraSettings _roCamSettings;

        public RenderObjects.CustomCameraSettings roCamSettings
        {
            get
            {
                if (_roCamSettings == null)
                {
                    _roCamSettings = new RenderObjects.CustomCameraSettings();
                    _roCamSettings.overrideCamera = overrideCamera;
                    _roCamSettings.restoreCamera = restoreCamera;
                    _roCamSettings.offset = offset;
                    _roCamSettings.cameraFieldOfView = cameraFieldOfView;
                }
                return _roCamSettings;
            }
        }

        public static implicit operator RenderObjects.CustomCameraSettings(CustomCameraSettings cs) => cs.roCamSettings;
    }

    /// <summary>
    /// The settings used for the Render Objects renderer feature.
    /// </summary>
    [HideLabel]
    [InlineProperty]
    public RenderObjectsSettings settings = new RenderObjectsSettings();

    RenderObjectsPass renderObjectsPass;

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

        renderObjectsPass = new RenderObjectsPass(settings.passTag, settings.Event, filter.PassNames,
            filter.RenderQueueType, filter.LayerMask, settings.cameraSettings);

        if (settings.overrideMaterial != null)
        {
            renderObjectsPass.overrideMaterial = settings.overrideMaterial;
            renderObjectsPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;
            renderObjectsPass.overrideShader = null;
        }

        if (settings.overrideDepthState)
            renderObjectsPass.SetDepthState(settings.enableWrite, settings.depthCompareFunction);

        if (settings.stencilSettings.overrideStencilState)
            renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference,
                settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);
    }

    /// <inheritdoc/>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderObjectsPass);
    }
}

/// <summary>
/// The scriptable render pass used with the render objects renderer feature.
/// </summary>
[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal")]
public class DXRenderObjectsPass : ScriptableRenderPass
{
    RenderQueueType renderQueueType;
    FilteringSettings m_FilteringSettings;
    DXRenderObjects_UNFINISHED.CustomCameraSettings m_CameraSettings;

    /// <summary>
    /// The override material to use.
    /// </summary>
    public Material overrideMaterial { get; set; }

    /// <summary>
    /// The pass index to use with the override material.
    /// </summary>
    public int overrideMaterialPassIndex { get; set; }

    /// <summary>
    /// The override shader to use.
    /// </summary>
    public Shader overrideShader { get; set; }

    /// <summary>
    /// The pass index to use with the override shader.
    /// </summary>
    public int overrideShaderPassIndex { get; set; }

    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
    private PassData m_PassData;

    /// <summary>
    /// Sets the write and comparison function for depth.
    /// </summary>
    /// <param name="writeEnabled">Sets whether it should write to depth or not.</param>
    /// <param name="function">The depth comparison function to use.</param>
    public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
    {
        m_RenderStateBlock.mask |= RenderStateMask.Depth;
        m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
    }

    /// <summary>
    /// Sets up the stencil settings for the pass.
    /// </summary>
    /// <param name="reference">The stencil reference value.</param>
    /// <param name="compareFunction">The comparison function to use.</param>
    /// <param name="passOp">The stencil operation to use when the stencil test passes.</param>
    /// <param name="failOp">The stencil operation to use when the stencil test fails.</param>
    /// <param name="zFailOp">The stencil operation to use when the stencil test fails because of depth.</param>
    public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
    {
        StencilState stencilState = StencilState.defaultValue;
        stencilState.enabled = true;
        stencilState.SetCompareFunction(compareFunction);
        stencilState.SetPassOperation(passOp);
        stencilState.SetFailOperation(failOp);
        stencilState.SetZFailOperation(zFailOp);

        m_RenderStateBlock.mask |= RenderStateMask.Stencil;
        m_RenderStateBlock.stencilReference = reference;
        m_RenderStateBlock.stencilState = stencilState;
    }

    RenderStateBlock m_RenderStateBlock;

    /// <summary>
    /// The constructor for render objects pass.
    /// </summary>
    /// <param name="profilerTag">The profiler tag used with the pass.</param>
    /// <param name="renderPassEvent">Controls when the render pass executes.</param>
    /// <param name="shaderTags">List of shader tags to render with.</param>
    /// <param name="renderQueueType">The queue type for the objects to render.</param>
    /// <param name="layerMask">The layer mask to use for creating filtering settings that control what objects get rendered.</param>
    /// <param name="cameraSettings">The settings for custom cameras values.</param>
    public DXRenderObjectsPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, DXRenderObjects_UNFINISHED.CustomCameraSettings cameraSettings)
    {
        profilingSampler = new ProfilingSampler(profilerTag);
        Init(renderPassEvent, shaderTags, renderQueueType, layerMask, cameraSettings);
    }

    public void Init(RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, DXRenderObjects_UNFINISHED.CustomCameraSettings cameraSettings)
    {
        m_PassData = new PassData();

        this.renderPassEvent = renderPassEvent;
        this.renderQueueType = renderQueueType;
        this.overrideMaterial = null;
        this.overrideMaterialPassIndex = 0;
        this.overrideShader = null;
        this.overrideShaderPassIndex = 0;
        RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);

        if (shaderTags != null && shaderTags.Length > 0)
        {
            foreach (var tag in shaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(tag));
        }
        else
        {
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        }

        m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        m_CameraSettings = cameraSettings;
    }

    private static void ExecutePass(PassData passData, RasterCommandBuffer cmd, RendererList rendererList, bool isYFlipped)
    {
        Camera camera = passData.cameraData.camera;

        // In case of camera stacking we need to take the viewport rect from base camera
        Rect pixelRect = default;
        float cameraAspect = (float)pixelRect.width / (float)pixelRect.height;

        if (passData.cameraSettings.overrideCamera)
        {
            if (passData.cameraData.xr.enabled)
            {
                Debug.LogWarning("RenderObjects pass is configured to override camera matrices. While rendering in stereo camera matrices cannot be overridden.");
            }
            else
            {
                Matrix4x4 projectionMatrix = Matrix4x4.Perspective(passData.cameraSettings.cameraFieldOfView, cameraAspect,
                    camera.nearClipPlane, camera.farClipPlane);
                projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, isYFlipped);

                Matrix4x4 viewMatrix = passData.cameraData.GetViewMatrix();
                Vector4 cameraTranslation = viewMatrix.GetColumn(3);
                viewMatrix.SetColumn(3, cameraTranslation + passData.cameraSettings.offset);

                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix, projectionMatrix, false);
            }
        }
        cmd.DrawRendererList(rendererList);

        if (passData.cameraSettings.overrideCamera && passData.cameraSettings.restoreCamera && !passData.cameraData.xr.enabled)
        {
            RenderingUtils.SetViewAndProjectionMatrices(cmd, passData.cameraData.GetViewMatrix(), GL.GetGPUProjectionMatrix(passData.cameraData.GetProjectionMatrix(0), isYFlipped), false);
        }
    }

    private class PassData
    {
        public RenderObjects.CustomCameraSettings cameraSettings;
        public RenderPassEvent renderPassEvent;

        public TextureHandle color;
        public RendererListHandle rendererListHdl;

        public UniversalCameraData cameraData;

        // Required for code sharing purpose between RG and non-RG.
        public RendererList rendererList;
    }

    private void InitPassData(UniversalCameraData cameraData, ref PassData passData)
    {
        passData.cameraSettings = m_CameraSettings;
        passData.renderPassEvent = renderPassEvent;
        passData.cameraData = cameraData;
    }

    private void InitRendererLists(UniversalRenderingData renderingData, UniversalLightData lightData,
        ref PassData passData, ScriptableRenderContext context, RenderGraph renderGraph, bool useRenderGraph)
    {
        SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
            ? SortingCriteria.CommonTransparent
            : passData.cameraData.defaultOpaqueSortFlags;
        DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData,
            passData.cameraData, lightData, sortingCriteria);
        drawingSettings.overrideMaterial = overrideMaterial;
        drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;
        drawingSettings.overrideShader = overrideShader;
        drawingSettings.overrideShaderPassIndex = overrideShaderPassIndex;

        /*
        var activeDebugHandler = GetActiveDebugHandler(passData.cameraData);
        var filterSettings = m_FilteringSettings;
        if (useRenderGraph)
        {
            if (activeDebugHandler != null)
            {
                passData.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(renderGraph,
                    ref renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
            }
            //else
            //{
            //    RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawingSettings,
            //        m_FilteringSettings, m_RenderStateBlock, ref passData.rendererListHdl);
            //}
        }
        else
        {
            if (activeDebugHandler != null)
            {
                passData.debugRendererLists = activeDebugHandler.CreateRendererListsWithDebugRenderState(context, ref renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
            }
            //else
            //{
            //    RenderingUtils.CreateRendererListWithRenderStateBlock(context, ref renderingData.cullResults, drawingSettings, m_FilteringSettings, m_RenderStateBlock, ref passData.rendererList);
            //}
        }
        */
    }
    /*
    class DebugRendererLists
    {
        private readonly DebugHandler m_DebugHandler;
        readonly FilteringSettings m_FilteringSettings;
        List<DebugRenderSetup> m_DebugRenderSetups = new List<DebugRenderSetup>(2);
        List<RendererList> m_ActiveDebugRendererList = new List<RendererList>(2);
        List<RendererListHandle> m_ActiveDebugRendererListHdl = new List<RendererListHandle>(2);

        public DebugRendererLists(DebugHandler debugHandler,
            FilteringSettings filteringSettings)
        {
            m_DebugHandler = debugHandler;
            m_FilteringSettings = filteringSettings;
        }

        private void CreateDebugRenderSetups(FilteringSettings filteringSettings)
        {
            var sceneOverrideMode = m_DebugHandler.DebugDisplaySettings.renderingSettings.sceneOverrideMode;
            var numIterations = ((sceneOverrideMode == DebugSceneOverrideMode.SolidWireframe) || (sceneOverrideMode == DebugSceneOverrideMode.ShadedWireframe)) ? 2 : 1;
            for (var i = 0; i < numIterations; i++)
                m_DebugRenderSetups.Add(new DebugRenderSetup(m_DebugHandler, i, filteringSettings));
        }

        void DisposeDebugRenderLists()
        {
            foreach (var debugRenderSetup in m_DebugRenderSetups)
            {
                debugRenderSetup.Dispose();
            }
            m_DebugRenderSetups.Clear();
            m_ActiveDebugRendererList.Clear();
            m_ActiveDebugRendererListHdl.Clear();
        }

        internal void CreateRendererListsWithDebugRenderState(
             ScriptableRenderContext context,
             ref CullingResults cullResults,
             ref DrawingSettings drawingSettings,
             ref FilteringSettings filteringSettings,
             ref RenderStateBlock renderStateBlock)
        {
            CreateDebugRenderSetups(filteringSettings);
            foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
            {
                DrawingSettings debugDrawingSettings = debugRenderSetup.CreateDrawingSettings(drawingSettings);
                RenderStateBlock debugRenderStateBlock = debugRenderSetup.GetRenderStateBlock(renderStateBlock);
                RendererList rendererList = new RendererList();
                RenderingUtils.CreateRendererListWithRenderStateBlock(context, ref cullResults, debugDrawingSettings, filteringSettings, debugRenderStateBlock, ref rendererList);
                m_ActiveDebugRendererList.Add((rendererList));
            }
        }

        internal void CreateRendererListsWithDebugRenderState(
            RenderGraph renderGraph,
            ref CullingResults cullResults,
            ref DrawingSettings drawingSettings,
            ref FilteringSettings filteringSettings,
            ref RenderStateBlock renderStateBlock)
        {
            CreateDebugRenderSetups(filteringSettings);
            foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
            {
                DrawingSettings debugDrawingSettings = debugRenderSetup.CreateDrawingSettings(drawingSettings);
                RenderStateBlock debugRenderStateBlock = debugRenderSetup.GetRenderStateBlock(renderStateBlock);
                RendererListHandle rendererListHdl = new RendererListHandle();
                RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref cullResults, debugDrawingSettings, filteringSettings, debugRenderStateBlock, ref rendererListHdl);
                m_ActiveDebugRendererListHdl.Add((rendererListHdl));
            }
        }

        internal void PrepareRendererListForRasterPass(IRasterRenderGraphBuilder builder)
        {
            foreach (RendererListHandle rendererListHdl in m_ActiveDebugRendererListHdl)
            {
                builder.UseRendererList(rendererListHdl);
            }
        }

        internal void DrawWithRendererList(RasterCommandBuffer cmd)
        {
            foreach (DebugRenderSetup debugRenderSetup in m_DebugRenderSetups)
            {
                debugRenderSetup.Begin(cmd);
                RendererList rendererList = new RendererList();
                if (m_ActiveDebugRendererList.Count > 0)
                {
                    rendererList = m_ActiveDebugRendererList[debugRenderSetup.GetIndex()];
                }
                else if (m_ActiveDebugRendererListHdl.Count > 0)
                {
                    rendererList = m_ActiveDebugRendererListHdl[debugRenderSetup.GetIndex()];
                }

                debugRenderSetup.DrawWithRendererList(cmd, ref rendererList);
                debugRenderSetup.End(cmd);
            }

            DisposeDebugRenderLists();
        }
    }

    static DebugHandler GetActiveDebugHandler(UniversalCameraData cameraData)
    {
        var debugHandler = cameraData.renderer.DebugHandler;
        if ((debugHandler != null) && debugHandler.IsActiveForCamera(cameraData.isPreviewCamera))
            return debugHandler;
        return null;
    }
    */
    /// <inheritdoc />
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            InitPassData(cameraData, ref passData);

            passData.color = resourceData.activeColorTexture;
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            TextureHandle mainShadowsTexture = resourceData.mainShadowsTexture;
            TextureHandle additionalShadowsTexture = resourceData.additionalShadowsTexture;

            if (mainShadowsTexture.IsValid())
                builder.UseTexture(mainShadowsTexture, AccessFlags.Read);

            if (additionalShadowsTexture.IsValid())
                builder.UseTexture(additionalShadowsTexture, AccessFlags.Read);

            TextureHandle[] dBufferHandles = resourceData.dBuffer;
            for (int i = 0; i < dBufferHandles.Length; ++i)
            {
                TextureHandle dBuffer = dBufferHandles[i];
                if (dBuffer.IsValid())
                    builder.UseTexture(dBuffer, AccessFlags.Read);
            }

            TextureHandle ssaoTexture = resourceData.ssaoTexture;
            if (ssaoTexture.IsValid())
                builder.UseTexture(ssaoTexture, AccessFlags.Read);

            InitRendererLists(renderingData, lightData, ref passData, default(ScriptableRenderContext), renderGraph, true);

            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);
            if (cameraData.xr.enabled)
                builder.EnableFoveatedRasterization(cameraData.xr.supportsFoveatedRendering && (cameraData.xr as XRPassUniversal).canFoveateIntermediatePasses);

            builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
            {
                var isYFlipped = data.cameraData.IsRenderTargetProjectionMatrixFlipped(data.color);
                ExecutePass(data, rgContext.cmd, data.rendererListHdl, isYFlipped);
            });
        }
    }

    class XRPassUniversal : XRPass
    {
        public static XRPass Create(XRPassCreateInfo createInfo)
        {
            XRPassUniversal pass = GenericPool<XRPassUniversal>.Get();
            pass.InitBase(createInfo);

            // Initialize fields specific to Universal
            pass.isLateLatchEnabled = false;
            pass.canMarkLateLatch = false;
            pass.hasMarkedLateLatch = false;
            pass.canFoveateIntermediatePasses = true;

            return pass;
        }

        override public void Release()
        {
            GenericPool<XRPassUniversal>.Release(this);
        }

        /// If true, late latching mechanism is available for the frame.
        public bool isLateLatchEnabled { get; set; }

        /// Used by the render pipeline to control the granularity of late latching.
        public bool canMarkLateLatch { get; set; }

        /// Track the state of the late latching system.
        public bool hasMarkedLateLatch { get; set; }

        /// If false, foveated rendering should not be applied to intermediate render passes that are not the final pass.
        public bool canFoveateIntermediatePasses { get; set; }
    }
}
