using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class BlurEffectRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private BlurSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private BlurRenderPass blurRenderPass;

    public class BlurRenderPass : ScriptableRenderPass
    {
        private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
        private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");
        private const string k_BlurTextureName = "_BlurTexture";
        private const string k_VerticalPassName = "VerticalBlurRenderPass";
        private const string k_HorizontalPassName = "HorizontalBlurRenderPass";

        private BlurSettings defaultSettings;
        private Material material;

        private RenderTextureDescriptor blurTextureDescriptor;

        public BlurRenderPass(Material material, BlurSettings defaultSettings)
        {
            this.material = material;
            this.defaultSettings = defaultSettings;

            blurTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
                RenderTextureFormat.Default, 0);
        }

        private void UpdateBlurSettings()
        {
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            var volumeComponent =
                VolumeManager.instance.stack.GetComponent<BlurVolumeComponent>();
            float horizontalBlur = volumeComponent.horizontalBlur.overrideState ?
                volumeComponent.horizontalBlur.value : defaultSettings.horizontalBlur;
            float verticalBlur = volumeComponent.verticalBlur.overrideState ?
                volumeComponent.verticalBlur.value : defaultSettings.verticalBlur;
            material.SetFloat(horizontalBlurId, horizontalBlur);
            material.SetFloat(verticalBlurId, verticalBlur);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph,
        ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // The following line ensures that the render pass doesn't blit
            // from the back buffer.
            if (resourceData.isActiveTargetBackBuffer)
                return;

            // Set the blur texture size to be the same as the camera target size.
            blurTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
            blurTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
            blurTextureDescriptor.depthBufferBits = 0;

            TextureHandle srcCamColor = resourceData.activeColorTexture;
            TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
                blurTextureDescriptor, k_BlurTextureName, false);

            // Update the blur settings in the material
            UpdateBlurSettings();

            // This check is to avoid an error from the material preview in the scene
            if (!srcCamColor.IsValid() || !dst.IsValid())
                return;

            // The AddBlitPass method adds a vertical blur render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
            RenderGraphUtils.BlitMaterialParameters paraVertical = new(srcCamColor, dst, material, 0);
            renderGraph.AddBlitPass(paraVertical, k_VerticalPassName);

            // The AddBlitPass method adds a horizontal blur render graph pass that blits from the texture written by the vertical blur pass to the camera color texture. The method uses the second shader pass.
            RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(dst, srcCamColor, material, 1);
            renderGraph.AddBlitPass(paraHorizontal, k_HorizontalPassName);
        }
    }

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        blurRenderPass = new BlurRenderPass(material, settings);

        blurRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (blurRenderPass == null)
        {
            return;
        }
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(blurRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }
}

[Serializable]
public class BlurSettings
{
    [Range(0, 0.4f)] public float horizontalBlur;
    [Range(0, 0.4f)] public float verticalBlur;
}
