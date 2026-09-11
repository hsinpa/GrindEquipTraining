using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Hsinpa.Outline
{
    /// <summary>
    /// URP Render Graph feature that draws a constant-width silhouette around renderers selected
    /// by MaskOutlineTarget. It does not use scene normals.
    /// </summary>
    public sealed class MaskOutlineRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public sealed class OutlineSettings
        {
            [Tooltip("Must match the Rendering Layer on MaskOutlineTarget components.")]
            [Range(0, 31)] public int renderingLayer = 31;

            [Min(1), Range(1, 12)] public int width = 3;
            public Color color = new Color(1f, 0.65f, 0f, 1f);
            public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
            public bool showInSceneView = true;
        }

        [SerializeField] private OutlineSettings settings = new OutlineSettings();
        [SerializeField, HideInInspector] private Shader maskShader;
        [SerializeField, HideInInspector] private Shader outlineShader;

        private Material maskMaterial;
        private Material outlineMaterial;
        private MaskOutlinePass outlinePass;

        public override void Create()
        {
            maskShader = maskShader != null
                ? maskShader
                : Resources.Load<Shader>("MaskOutlineMask");
            outlineShader = outlineShader != null
                ? outlineShader
                : Resources.Load<Shader>("MaskOutlineComposite");

            CoreUtils.Destroy(maskMaterial);
            CoreUtils.Destroy(outlineMaterial);

            if (maskShader != null)
                maskMaterial = CoreUtils.CreateEngineMaterial(maskShader);
            if (outlineShader != null)
                outlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);

            outlinePass = new MaskOutlinePass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (maskMaterial == null || outlineMaterial == null)
                return;

            CameraType cameraType = renderingData.cameraData.cameraType;
            if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
                return;
            if (!settings.showInSceneView && cameraType == CameraType.SceneView)
                return;

            outlinePass.renderPassEvent = settings.injectionPoint;
            outlinePass.Setup(maskMaterial, outlineMaterial, settings);
            renderer.EnqueuePass(outlinePass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(maskMaterial);
            CoreUtils.Destroy(outlineMaterial);
        }

        private sealed class MaskOutlinePass : ScriptableRenderPass
        {
            private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
            private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");

            private static readonly ShaderTagId[] ShaderTags =
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("LightweightForward")
            };

            private readonly List<ShaderTagId> shaderTagList = new List<ShaderTagId>(ShaderTags);

            private Material maskMaterial;
            private Material outlineMaterial;
            private OutlineSettings settings;

            private sealed class MaskPassData
            {
                public RendererListHandle rendererList;
            }

            public void Setup(Material newMaskMaterial, Material newOutlineMaterial, OutlineSettings newSettings)
            {
                maskMaterial = newMaskMaterial;
                outlineMaterial = newOutlineMaterial;
                settings = newSettings;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();

                TextureDesc maskDescriptor = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
                maskDescriptor.name = "Mask Outline - Object Mask";
                maskDescriptor.colorFormat = GraphicsFormat.R8_UNorm;
                maskDescriptor.depthBufferBits = DepthBits.None;
                maskDescriptor.msaaSamples = MSAASamples.None;
                maskDescriptor.clearBuffer = true;
                maskDescriptor.clearColor = Color.black;
                maskDescriptor.filterMode = FilterMode.Point;
                TextureHandle maskTexture = renderGraph.CreateTexture(maskDescriptor);

                TextureDesc expandedDescriptor = maskDescriptor;
                expandedDescriptor.name = "Mask Outline - Horizontal Expansion";
                expandedDescriptor.colorFormat = GraphicsFormat.R8G8_UNorm;
                expandedDescriptor.clearBuffer = false;
                TextureHandle expandedTexture = renderGraph.CreateTexture(expandedDescriptor);

                RecordMaskPass(
                    renderGraph,
                    renderingData,
                    cameraData,
                    lightData,
                    resourceData.activeDepthTexture,
                    maskTexture);

                MaterialPropertyBlock horizontalProperties = new MaterialPropertyBlock();
                horizontalProperties.SetFloat(OutlineWidthId, Mathf.Clamp(settings.width, 1, 12));
                var horizontalParameters = new RenderGraphUtils.BlitMaterialParameters(
                    maskTexture,
                    expandedTexture,
                    outlineMaterial,
                    0,
                    horizontalProperties,
                    RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle);
                renderGraph.AddBlitPass(horizontalParameters, "Mask Outline - Expand Horizontal");

                MaterialPropertyBlock compositeProperties = new MaterialPropertyBlock();
                compositeProperties.SetFloat(OutlineWidthId, Mathf.Clamp(settings.width, 1, 12));
                compositeProperties.SetColor(OutlineColorId, settings.color);
                var compositeParameters = new RenderGraphUtils.BlitMaterialParameters(
                    expandedTexture,
                    resourceData.activeColorTexture,
                    outlineMaterial,
                    1,
                    compositeProperties,
                    RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle);
                renderGraph.AddBlitPass(compositeParameters, "Mask Outline - Expand Vertical and Composite");
            }

            private void RecordMaskPass(
                RenderGraph renderGraph,
                UniversalRenderingData renderingData,
                UniversalCameraData cameraData,
                UniversalLightData lightData,
                TextureHandle depthTexture,
                TextureHandle maskTexture)
            {
                using (IRasterRenderGraphBuilder builder =
                       renderGraph.AddRasterRenderPass<MaskPassData>("Mask Outline - Render Selected Objects", out MaskPassData passData))
                {
                    FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all, -1)
                    {
                        renderingLayerMask = 1u << settings.renderingLayer
                    };

                    DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                        shaderTagList,
                        renderingData,
                        cameraData,
                        lightData,
                        cameraData.defaultOpaqueSortFlags);
                    drawingSettings.overrideMaterial = maskMaterial;
                    drawingSettings.overrideMaterialPassIndex = 0;

                    RendererListParams rendererListParams = new RendererListParams(
                        renderingData.cullResults,
                        drawingSettings,
                        filteringSettings);
                    passData.rendererList = renderGraph.CreateRendererList(rendererListParams);

                    builder.UseRendererList(passData.rendererList);
                    builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);
                    builder.SetRenderAttachmentDepth(depthTexture, AccessFlags.Read);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc(static (MaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black, 1f, 0);
                        context.cmd.DrawRendererList(data.rendererList);
                    });
                }
            }
        }
    }
}
