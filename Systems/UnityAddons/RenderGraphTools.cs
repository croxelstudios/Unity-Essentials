using UnityEngine;
using Unity.Collections;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal.Internal;

public static class RenderGraphTools
{
    public static bool CanBeUsed(this TextureHandle tex)
    {
        if (!tex.IsValid())
            return false;

        try { RTHandle rt = (RTHandle)tex; }
        catch { return false; }

        return true;
    }

    public static TextureHandle CreateCameraDepthTexture(this RenderGraph renderGraph, UniversalCameraData cameraData,
        string name = "_CameraDepthTexture")
    {
        RenderTextureDescriptor depthDesc = cameraData.cameraTargetDescriptor;
        depthDesc.graphicsFormat = GraphicsFormat.R32_SFloat;
        depthDesc.depthBufferBits = 0;
        depthDesc.msaaSamples = 1;

        return UniversalRenderer.CreateRenderGraphTexture(renderGraph, depthDesc,
                name, true, FilterMode.Point, TextureWrapMode.Clamp);
    }

    public static TextureHandle CreateCameraTexture(this RenderGraph renderGraph, UniversalCameraData cameraData,
        string name = "_CameraNormalsTexture")
    {
        RenderTextureDescriptor normalsDesc = cameraData.cameraTargetDescriptor;
        normalsDesc.depthStencilFormat = GraphicsFormat.None;
        normalsDesc.msaaSamples = /*DepthPriming*/false ? cameraData.cameraTargetDescriptor.msaaSamples : 1;
        normalsDesc.depthBufferBits = 0;
        //if (false /*Is using deferred*/)
        //{
        //    m_DeferredLights.GetGBufferFormat(m_DeferredLights.GBufferNormalSmoothnessIndex);
        //}
        //else
        normalsDesc.graphicsFormat = DepthNormalOnlyPass.GetGraphicsFormat();

        return UniversalRenderer.CreateRenderGraphTexture(renderGraph, normalsDesc,
            name, true, FilterMode.Point, TextureWrapMode.Clamp);
    }

    public static TextureHandle CreateCameraOpaqueTexture(this RenderGraph renderGraph, UniversalCameraData cameraData,
        string name = "_CameraOpaqueTexture")
    {
        RenderTextureDescriptor normalsDesc = cameraData.cameraTargetDescriptor;
        normalsDesc.depthStencilFormat = GraphicsFormat.None;
        normalsDesc.msaaSamples = /*DepthPriming*/false ? cameraData.cameraTargetDescriptor.msaaSamples : 1;
        normalsDesc.depthBufferBits = 0;
        //if (false /*Is using deferred*/)
        //{
        //    m_DeferredLights.GetGBufferFormat(m_DeferredLights.GBufferNormalSmoothnessIndex);
        //}
        //else
        normalsDesc.graphicsFormat = DepthNormalOnlyPass.GetGraphicsFormat();

        return UniversalRenderer.CreateRenderGraphTexture(renderGraph, normalsDesc,
            name, true, FilterMode.Point, TextureWrapMode.Clamp);
    }

    public static void SetUpUsableTextures(UniversalResourceData resourceData, IRasterRenderGraphBuilder builder)
    {
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
    }

    public static void SetUpPassCulling(UniversalCameraData cameraData, IRasterRenderGraphBuilder builder)
    {
        builder.AllowPassCulling(false);
        builder.AllowGlobalStateModification(true);
        if (cameraData.xr.enabled)
            builder.EnableFoveatedRasterization(cameraData.xr.supportsFoveatedRendering
                /* && cameraData.xrUniversal.canFoveateIntermediatePasses*/);
    }

    public static void SetupRenderingObjects(this IRasterRenderGraphBuilder builder, RenderGraph graph,
        ContextContainer frameData, RenderQueueType renderQueueType, List<ShaderTagId> shaderTagIdList, FilteringSettings filteringSettings,
        RenderStateBlock rsb, ref RendererListHandle list, MaterialOverrideBehaviour materialOverride)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        SetUpUsableTextures(resourceData, builder);

        //Init rendererLists
        SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
            ? SortingCriteria.CommonTransparent : cameraData.defaultOpaqueSortFlags;
        DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(shaderTagIdList, renderingData,
            cameraData, lightData, sortingCriteria);
        materialOverride.Apply(ref drawingSettings);

        CreateRendererListWithRenderStateBlock(graph, ref renderingData.cullResults, drawingSettings,
            filteringSettings, rsb, ref list);
        //

        builder.UseRendererList(list);

        SetUpPassCulling(cameraData, builder);
    }

    public static void CreateRendererListWithRenderStateBlock(RenderGraph renderGraph,
        ref CullingResults cullResults, DrawingSettings ds, FilteringSettings fs,
        RenderStateBlock rsb, ref RendererListHandle renderersList)
    {
        NativeArray<ShaderTagId> tagValues =
            new NativeArray<ShaderTagId>(1, Allocator.Temp);
        NativeArray<RenderStateBlock> stateBlocks =
            new NativeArray<RenderStateBlock>(1, Allocator.Temp);

        tagValues[0] = ShaderTagId.none;
        stateBlocks[0] = rsb;

        var param = new RendererListParams(cullResults, ds, fs)
        {
            tagValues = tagValues,
            stateBlocks = stateBlocks,
            isPassTagName = false
        };

        renderersList = renderGraph.CreateRendererList(param);
    }

    public static void ExecuteRenderObjects(this RasterCommandBuffer cmd, UniversalCameraData cameraData,
        RendererListHandle renderersList, bool isYFlipped = false, CameraSettings cameraSettings = null)
    {

        Camera camera = cameraData.camera;

        // In case of camera stacking we need to take the viewport rect from base camera //Isn't it the same?
        Rect pixelRect = camera.pixelRect;
        float cameraAspect = (float)pixelRect.width / (float)pixelRect.height;

        if ((cameraSettings != null) && cameraSettings.overrideCamera)
        {
            if (cameraData.xr.enabled)
            {
                Debug.LogWarning("DXRenderObjects pass is configured to override camera matrices." +
                    "While rendering in stereo camera matrices cannot be overridden.");
            }
            else
            {
                Matrix4x4 projectionMatrix = Matrix4x4.Perspective(
                    cameraSettings.cameraFieldOfView, cameraAspect,
                    camera.nearClipPlane, camera.farClipPlane);
                projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, isYFlipped);

                Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
                Vector4 cameraTranslation = viewMatrix.GetColumn(3);
                viewMatrix.SetColumn(3, cameraTranslation + (Vector4)cameraSettings.offset);

                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix, projectionMatrix, false);
            }
        }

        cmd.DrawRendererList(renderersList);

        if ((cameraSettings != null) && cameraSettings.overrideCamera && cameraSettings.restoreCamera &&
            !cameraData.xr.enabled)
        {
            RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(),
                GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(0), isYFlipped), false);
        }
    }

    public static bool IsYFlipped(this UniversalCameraData cameraData, TextureHandle tex)
    {
        return tex.CanBeUsed() ? cameraData.IsRenderTargetProjectionMatrixFlipped(tex) : false;
    }

    public static void SetAttachments(this IRasterRenderGraphBuilder builder, TextureHandle color, TextureHandle depth,
        AccessFlags colorAccess, AccessFlags? depthAccess = null)
    {
        builder.SetRenderAttachment(color, 0, colorAccess);
        if (depth.CanBeUsed())
            builder.SetRenderAttachmentDepth(depth, depthAccess == null ? colorAccess : (AccessFlags)depthAccess);
    }

    public static void SetGlobalTextureAfterPass(this IRasterRenderGraphBuilder builder, TextureHandle texture, string name)
    {
        int globalID = Shader.PropertyToID(name);
        builder.SetGlobalTextureAfterPass(texture, globalID);
    }

    public static TextureHandle CreateTexture(this RenderGraph renderGraph, string name,
        RenderTextureDescriptor baseDescriptor, TextureSettings settings, bool clear = true, int depthBufferBits = 0)
    {
        RenderTextureDescriptor desc = baseDescriptor;
        desc.colorFormat = settings.colorFormat;
        desc.msaaSamples = settings.MSAA;
        desc.depthBufferBits = depthBufferBits;

        return UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc,
            name, clear, settings.filterMode, settings.wrapMode);
    }

    /// <summary>
    /// The settings for custom cameras values.
    /// </summary>
    [Serializable]
    public class CameraSettings
    {
        /// <summary>
        /// Used to mark whether camera values should be changed or not.
        /// </summary>
        [LabelText("Camera")]
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

        CameraSettings _roCamSettings;

        public CameraSettings roCamSettings
        {
            get
            {
                if (_roCamSettings == null)
                {
                    _roCamSettings = new CameraSettings();
                    _roCamSettings.overrideCamera = overrideCamera;
                    _roCamSettings.restoreCamera = restoreCamera;
                    _roCamSettings.offset = offset;
                    _roCamSettings.cameraFieldOfView = cameraFieldOfView;
                }
                return _roCamSettings;
            }
        }
    }
}

[Serializable]
public struct TextureSettings
{
    public RenderTextureFormat colorFormat;
    public FilterMode filterMode;
    public TextureWrapMode wrapMode;
    [Min(0)]
    public int MSAA;

    public TextureSettings(RenderTextureFormat colorFormat,
        FilterMode filterMode, TextureWrapMode wrapMode, int MSAA)
    {
        this.colorFormat = colorFormat;
        this.filterMode = filterMode;
        this.wrapMode = wrapMode;
        this.MSAA = MSAA;
    }
}

[Serializable]
public struct MaterialOverrideBehaviour
{
    /// <summary>
    /// Options to select which type of override mode should be used.
    /// </summary>
    public enum OverrideMaterialMode
    {
        /// <summary>
        /// Use this to not override.
        /// </summary>
        None,

        /// <summary>
        /// Use this to use an override material.
        /// </summary>
        Material,

        /// <summary>
        /// Use this to use an override shader.
        /// </summary>
        Shader
    };

    /// <summary>
    /// The selected override mode.
    /// </summary>
    public OverrideMaterialMode overrideMode;

    /// <summary>
    /// The override material to use.
    /// </summary>
    [ShowIf("@overrideMode == OverrideMaterialMode.Material")]
    public Material overrideMaterial;

    /// <summary>
    /// The pass index to use with the override material.
    /// </summary>
    [Indent]
    [ShowIf("@overrideMode == OverrideMaterialMode.Material")]
    public int overrideMaterialPassIndex;

    /// <summary>
    /// The override shader to use.
    /// </summary>
    [ShowIf("@overrideMode == OverrideMaterialMode.Shader")]
    public Shader overrideShader;

    /// <summary>
    /// The pass index to use with the override shader.
    /// </summary>
    [ShowIf("@overrideMode == OverrideMaterialMode.Shader")]
    public int overrideShaderPassIndex;

    public MaterialOverrideBehaviour(OverrideMaterialMode overrideMode)
    {
        this.overrideMode = overrideMode;
        overrideMaterial = null;
        overrideMaterialPassIndex = 0;
        overrideShader = null;
        overrideShaderPassIndex = 0;
    }

    public MaterialOverrideBehaviour(OverrideMaterialMode overrideMode, Material overrideMaterial,
        int overrideMaterialPassIndex, Shader overrideShader, int overrideShaderPassIndex)
    {
        this.overrideMode = overrideMode;
        this.overrideMaterial = overrideMaterial;
        this.overrideMaterialPassIndex = overrideMaterialPassIndex;
        this.overrideShader = overrideShader;
        this.overrideShaderPassIndex = overrideShaderPassIndex;
    }

    public void Apply(ref DrawingSettings dSettings)
    {
        dSettings.overrideMaterial = null;
        dSettings.overrideMaterialPassIndex = 0;
        dSettings.overrideShader = null;
        dSettings.overrideShaderPassIndex = 0;
        switch (overrideMode)
        {
            case OverrideMaterialMode.Material:
                dSettings.overrideMaterial = overrideMaterial;
                dSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;
                break;
            case OverrideMaterialMode.Shader:
                dSettings.overrideShader = overrideShader;
                dSettings.overrideShaderPassIndex = overrideShaderPassIndex;
                break;
        }    
    }
}
