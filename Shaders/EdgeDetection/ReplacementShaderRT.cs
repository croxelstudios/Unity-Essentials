// MIT License

// Copyright (c) 2021 NedMakesGames

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ReplacementShaderRT : ScriptableRendererFeature
{
    [SerializeField]
    string textureName = "_SceneViewSpaceNormals";
    [SerializeField]
    Shader shader = null;
    [SerializeField]
    RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    [SerializeField]
    TextureSettings textureSettings =
        new TextureSettings(RenderTextureFormat.Default, FilterMode.Point, 32, Color.black);

    [Serializable]
    struct TextureSettings
    {
        public RenderTextureFormat colorFormat;
        [HideInInspector]
        public int depthBufferBits;
        public FilterMode filterMode;
        public Color backgroundColor;

        public TextureSettings(RenderTextureFormat colorFormat, FilterMode filterMode, int depthBufferBits, Color backgroundColor)
        {
            this.colorFormat = colorFormat;
            this.filterMode = filterMode;
            this.backgroundColor = backgroundColor;
            this.depthBufferBits = depthBufferBits;
        }
    }

    class RenderPass : ScriptableRenderPass
    {
        TextureSettings textureSettings;
        RTHandle rt;
        List<ShaderTagId> shaderTagIdList;
        Shader shader;

        static int RTID = -1;

        public RenderPass(Shader shader, RenderPassEvent renderPassEvent, TextureSettings settings, string textureName) : base()
        {
            this.renderPassEvent = renderPassEvent;
            StaticRTHandler.Init();
            RTHandles.SetReferenceSize(Screen.width, Screen.height);
            rt = RTHandles.Alloc(textureName, name: textureName);
            RTID = Shader.PropertyToID(rt.name);
            shaderTagIdList = new List<ShaderTagId>
            {
                new ShaderTagId("DepthOnly"),
                //new ShaderTagId("SRPDefaultUnlit"),
                //new ShaderTagId("UniversalForward"),
                //new ShaderTagId("LightweightForward"),
            };
            this.shader = shader;
        }

        // Configure the pass by creating a temporary render texture and
        // readying it for rendering
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.colorFormat = textureSettings.colorFormat;
            //normalsTextureDescriptor.depthBufferBits = viewSpaceNormalsTextureSettings.depthBufferBits;

            cmd.GetTemporaryRT(Shader.PropertyToID(rt.name), normalsTextureDescriptor, textureSettings.filterMode);
            ConfigureTarget(rt);
            ConfigureClear(ClearFlag.All, textureSettings.backgroundColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ReplacementShaderRTCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                DrawingSettings drawSettings =
                    CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawSettings.overrideShader = shader;
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(RTID);
        }
    }

    RenderPass renderPass;

    public override void Create()
    {
        // We will use the built-in renderer's depth normals texture shader
        if (shader == null)
            shader = Shader.Find("Hidden/Internal-DepthNormalsTexture");
        this.renderPass = new RenderPass(shader, renderPassEvent, textureSettings, textureName);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }
}