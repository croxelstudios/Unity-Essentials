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
    class TextureCollection : ContextItem
    {
        Dictionary<string, TextureHandleData> textures;
        Dictionary<int, TextureHandle> depthAttachments;

        public override void Reset()
        {
            textures.SmartClear();
            depthAttachments.SmartClear();
        }

        public void CleanTextures()
        {
            if (textures != null)
            {
                List<string> remove = new List<string>();
                foreach (KeyValuePair<string, TextureHandleData> pair in textures)
                {
                    if (!pair.Value.handle.IsValid())
                    {
                        depthAttachments.SmartRemove(pair.Value.handle.GetHashCode());
                        remove.Add(pair.Key);
                    }
                }
                foreach (string key in remove)
                    textures.Remove(key);
            }
        }

        public void RegisterTexture(string name, TextureHandle texture, TextureSettings settings)
        {
            textures = textures.CreateAdd(name, new TextureHandleData(texture, settings));
        }

        public bool GetTexture(string name, out TextureHandleData texture)
        {
            if (textures == null)
            {
                texture = new TextureHandleData();
                return false;
            }

            return textures.TryGetValue(name, out texture);
        }

        public void RemoveTexture(string name)
        {
            if (textures != null)
                textures.SmartRemove(name);
        }

        public TextureHandle GetDepth(TextureHandle color)
        {
            if (depthAttachments.SmartGetValue(color.GetHashCode(), out TextureHandle depth))
                return depth;
            else return new TextureHandle();
        }

        public void RegisterDepth(TextureHandle color, TextureHandle depth)
        {
            depthAttachments = depthAttachments.CreateAdd(color.GetHashCode(), depth);
        }
    }

    public static bool CanBeUsedAsRTHandle(this TextureHandle tex)
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
        return tex.IsValid() ? cameraData.IsRenderTargetProjectionMatrixFlipped(tex) : false;
    }

    public static void SetAttachments(this IRasterRenderGraphBuilder builder, ContextContainer context, TextureHandle color, TextureHandle depth,
        AccessFlags colorAccess, AccessFlags? depthAccess = null)
    {
        TextureCollection textures = GetSavedTextures(context);

        builder.SetRenderAttachment(color, 0, colorAccess);

        if (!depth.IsValid())
            depth = textures.GetDepth(color);

        if (depth.IsValid())
            builder.SetRenderAttachmentDepth(depth, depthAccess == null ? colorAccess : (AccessFlags)depthAccess);
    }

    static TextureCollection GetSavedTextures(ContextContainer context)
    {
        TextureCollection textures;
        if (context.Contains<TextureCollection>())
            textures = context.Get<TextureCollection>();
        else textures = context.Create<TextureCollection>();
        return textures;
    }

    public static void SetGlobalTextureAfterPass(this IRasterRenderGraphBuilder builder, TextureHandle texture, string name)
    {
        int globalID = Shader.PropertyToID(name);
        builder.SetGlobalTextureAfterPass(texture, globalID);
    }

    public static TextureHandle GetTexture(this RenderGraph renderGraph, ContextContainer context, string name,
        RenderTextureDescriptor baseDescriptor, TextureSettings settings, bool clear = false)
    {
        TextureCollection textures = GetSavedTextures(context);

        RenderTextureDescriptor desc = baseDescriptor;
        desc.graphicsFormat = settings.colorFormat;
        desc.msaaSamples = settings.MSAA.Int();
        desc.depthBufferBits = 0;

        TextureHandle texture = new TextureHandle();
        TextureHandleData textureData;
        if (textures.GetTexture(name, out textureData) && (textureData.settings == settings))
        {
            if (textureData.settings == settings)
                texture = textureData.handle;
            else textures.RemoveTexture(name);
        }

        if (!texture.IsValid())
        {
            texture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc,
                name, clear, settings.filterMode, settings.wrapMode);
            if (settings.depthBufferBits != DepthBits.None)
            {
                desc.graphicsFormat = GraphicsFormat.None;
                desc.depthBufferBits = settings.depthBufferBits.Int();
                textures.RegisterDepth(texture,
                    UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc,
                    name + "_Depth", clear, settings.filterMode, settings.wrapMode));
            }
            textures.RegisterTexture(name, texture, settings);
        }

        return texture;
    }

    public static int Int(this MSAASamples samples)
    {
        switch (samples)
        {
            case MSAASamples.MSAA2x:
                return 2;
            case MSAASamples.MSAA4x:
                return 4;
            case MSAASamples.MSAA8x:
                return 8;
            default:
                return 1;
        }
    }

    public static int Int(this DepthBits bits)
    {
        switch (bits)
        {
            case DepthBits.Depth8:
                return 8;
            case DepthBits.Depth16:
                return 16;
            case DepthBits.Depth24:
                return 24;
            case DepthBits.Depth32:
                return 32;
            default:
                return 0;
        }
    }

    static readonly int ID_Time = Shader.PropertyToID("_Time");
    static readonly int ID_SinTime = Shader.PropertyToID("_SinTime");
    static readonly int ID_CosTime = Shader.PropertyToID("_CosTime");
    static readonly int ID_TimeParameters = Shader.PropertyToID("_TimeParameters");
    static readonly int ID_DeltaTime = Shader.PropertyToID("unity_DeltaTime");

    static void GetShaderTimeValues(float time, out Vector4 timeVec, out Vector4 sinTime, out Vector4 cosTime)
    {
        timeVec = new Vector4(time / 20f, time, time * 2f, time * 3f);
        sinTime = new Vector4(
            Mathf.Sin(time / 8f),
            Mathf.Sin(time / 4f),
            Mathf.Sin(time / 2f),
            Mathf.Sin(time)
        );
        cosTime = new Vector4(
            Mathf.Cos(time / 8f),
            Mathf.Cos(time / 4f),
            Mathf.Cos(time / 2f),
            Mathf.Cos(time)
        );
    }

    public static void SetShaderTime(this RasterCommandBuffer cmd, float time)
    {
        GetShaderTimeValues(time, out Vector4 timeVec, out Vector4 sinTime, out Vector4 cosTime);

        cmd.SetGlobalVector(ID_Time, timeVec);
        cmd.SetGlobalVector(ID_SinTime, sinTime);
        cmd.SetGlobalVector(ID_CosTime, cosTime);
        cmd.SetGlobalVector(ID_TimeParameters, new Vector3(time, sinTime.w, cosTime.w));
    }

    public static void SetShaderTime(this UnsafeCommandBuffer cmd, float time)
    {
        GetShaderTimeValues(time, out Vector4 timeVec, out Vector4 sinTime, out Vector4 cosTime);

        cmd.SetGlobalVector(ID_Time, timeVec);
        cmd.SetGlobalVector(ID_SinTime, sinTime);
        cmd.SetGlobalVector(ID_CosTime, cosTime);
        cmd.SetGlobalVector(ID_TimeParameters, new Vector3(time, sinTime.w, cosTime.w));
    }

    static Vector4 GetDeltaTimeVector(float deltaTime)
    {
        return new Vector4(
            deltaTime,
            deltaTime > 0f ? 1f / deltaTime : 0f,
            Time.smoothDeltaTime,
            Time.smoothDeltaTime > 0f ? 1f / Time.smoothDeltaTime : 0f
        );
    }

    public static void SetShaderDeltaTime(this RasterCommandBuffer cmd, float deltaTime)
    {
        Vector4 deltaTimeVec = GetDeltaTimeVector(deltaTime);

        cmd.SetGlobalVector(ID_DeltaTime, deltaTimeVec);
    }

    public static void SetShaderDeltaTime(this UnsafeCommandBuffer cmd, float deltaTime)
    {
        Vector4 deltaTimeVec = GetDeltaTimeVector(deltaTime);

        cmd.SetGlobalVector(ID_DeltaTime, deltaTimeVec);
    }

    public static void ShaderTime_Unscaled(this RasterCommandBuffer cmd, float scale = 1f)
    {
        cmd.SetShaderTime(Time.unscaledTime * scale);
        cmd.SetShaderDeltaTime(Time.unscaledDeltaTime * scale);
    }

    public static void ShaderTime_Unscaled(this UnsafeCommandBuffer cmd, float scale = 1f)
    {
        cmd.SetShaderTime(Time.unscaledTime * scale);
        cmd.SetShaderDeltaTime(Time.unscaledDeltaTime * scale);
    }

    public static void ShaderTime_Scaled(this RasterCommandBuffer cmd, float scale = 1f)
    {
        cmd.SetShaderTime(Time.time * scale);
        cmd.SetShaderDeltaTime(Time.deltaTime * scale);
    }

    public static void ShaderTime_Scaled(this UnsafeCommandBuffer cmd, float scale = 1f)
    {
        cmd.SetShaderTime(Time.time * scale);
        cmd.SetShaderDeltaTime(Time.deltaTime * scale);
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

    public struct TextureHandleData
    {
        public TextureHandle handle;
        public TextureSettings settings;

        public TextureHandleData(TextureHandle handle, TextureSettings settings)
        {
            this.handle = handle;
            this.settings = settings;
        }
    }
}

[Serializable]
public struct TextureSettings : IEquatable<TextureSettings>
{
    public GraphicsFormat colorFormat;
    public FilterMode filterMode;
    public TextureWrapMode wrapMode;
    public MSAASamples MSAA;
    public DepthBits depthBufferBits;

    public TextureSettings(GraphicsFormat colorFormat,
        FilterMode filterMode, TextureWrapMode wrapMode, MSAASamples MSAA,
        DepthBits depthBufferBits)
    {
        this.colorFormat = colorFormat;
        this.filterMode = filterMode;
        this.wrapMode = wrapMode;
        this.MSAA = MSAA;
        this.depthBufferBits = depthBufferBits;
    }

    public override bool Equals(object other)
    {
        if (!(other is TextureSettings)) return false;
        return Equals((TextureSettings)other);
    }

    public bool Equals(TextureSettings other)
    {
        return (colorFormat == other.colorFormat) &&
            (filterMode == other.filterMode) &&
            (wrapMode == other.wrapMode) &&
            (MSAA == other.MSAA) &&
            (depthBufferBits == other.depthBufferBits);
    }

    public override int GetHashCode()
    {
        return (((colorFormat.GetHashCode() * 31 + filterMode.GetHashCode()) * 31 +
            wrapMode.GetHashCode()) * 31 + MSAA.GetHashCode()) * 31 + depthBufferBits.GetHashCode();
    }

    public static bool operator ==(TextureSettings o1, TextureSettings o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(TextureSettings o1, TextureSettings o2)
    {
        return !o1.Equals(o2);
    }
}

static class ShaderPropertyId
{
    public static readonly int BlitTexture = Shader.PropertyToID("_BlitTexture");
    public static readonly int MainTex = Shader.PropertyToID("_MainTex");
    public static readonly int BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
}

struct UniversalData
{
    public ContextContainer frameData;
    public UniversalCameraData cameraData;
    public UniversalRenderingData renderingData;
    public UniversalLightData lightData;
    public UniversalResourceData resourceData;

    public UniversalData(ContextContainer frameData)
    {
        this.frameData = frameData;
        cameraData = frameData.Get<UniversalCameraData>();
        renderingData = frameData.Get<UniversalRenderingData>();
        lightData = frameData.Get<UniversalLightData>();
        resourceData = frameData.Get<UniversalResourceData>();
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
    [ShowIf("overrideMode", OverrideMaterialMode.Material)]
    public Material overrideMaterial;

    /// <summary>
    /// The pass index to use with the override material.
    /// </summary>
    [Indent]
    [ShowIf("overrideMode", OverrideMaterialMode.Material)]
    public int overrideMaterialPassIndex;

    /// <summary>
    /// The override shader to use.
    /// </summary>
    [ShowIf("overrideMode", OverrideMaterialMode.Shader)]
    public Shader overrideShader;

    /// <summary>
    /// The pass index to use with the override shader.
    /// </summary>
    [ShowIf("overrideMode", OverrideMaterialMode.Shader)]
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
