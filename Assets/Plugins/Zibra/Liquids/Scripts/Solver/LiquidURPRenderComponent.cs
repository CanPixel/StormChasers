#if UNITY_PIPELINE_URP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using com.zibra.liquid.Solver;

namespace com.zibra.liquid
{
    public class LiquidURPRenderComponent : ScriptableRendererFeature
    {
        [System.Serializable]
        public class LiquidURPRenderSettings
        {
            // we're free to put whatever we want here, public fields will be exposed in the inspector
            public bool IsEnabled = true;
        }
        // Must be called exactly "settings" so Unity shows this as render feature settings in editor
        public LiquidURPRenderSettings settings = new LiquidURPRenderSettings();

        public class LiquidURPRenderPass : ScriptableRenderPass
        {
            private int depth;
            RenderTargetIdentifier cameraColorTexture;

            public void Setup(ScriptableRenderer renderer, ref RenderingData renderingData)
            {
                Camera camera = renderingData.cameraData.camera;
                depth = Shader.PropertyToID("_CameraDepthTexture");
                CommandBuffer cmd = CommandBufferPool.Get("ZibraLiquid.Render");
                cmd.GetTemporaryRT(depth, camera.pixelWidth, camera.pixelHeight, 32, FilterMode.Point,
                                   RenderTextureFormat.RFloat);
                cameraColorTexture = renderer.cameraColorTarget;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Camera camera = renderingData.cameraData.camera;
                camera.depthTextureMode = DepthTextureMode.Depth;
                CommandBuffer cmd = CommandBufferPool.Get("ZibraLiquid.Render");

                foreach (var liquid in ZibraLiquid.AllFluids)
                {
                    if (liquid != null && liquid.initialized)
                    {
                        liquid.RenderCallBack(camera);

                        liquid.UpdateCamera(camera);
                        // set initial parameters in the native plugin
                        ZibraLiquidBridge.SetCameraParameters(liquid.CurrentInstanceID, liquid.camNativeParams[camera]);
                        cmd.IssuePluginEventAndData(ZibraLiquidBridge.GetCameraUpdateFunction(),
                                                    ZibraLiquidBridge.EventAndInstanceID(ZibraLiquidBridge.EventID.None,
                                                                                         liquid.CurrentInstanceID),
                                                    liquid.camNativeParams[camera]);
                        Blit(cmd, cameraColorTexture, liquid.cameraResources[camera].background);
                        // blit depth to temp RT
                        liquid.RenderParticelsNative(cmd);
                        cmd.SetRenderTarget(cameraColorTexture);
                        // bind temp depth RT
                        liquid.RenderFluid(cmd, camera);
                    }
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                context.Submit();
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(depth);
            }
        }

        public LiquidURPRenderPass fluidPass;

        public override void Create()
        {
            fluidPass = new LiquidURPRenderPass { renderPassEvent = RenderPassEvent.AfterRenderingTransparents };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.IsEnabled)
            {
                return;
            }

            if (renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            Camera camera = renderingData.cameraData.camera;
            camera.depthTextureMode = DepthTextureMode.Depth;

            fluidPass.Setup(renderer, ref renderingData);
            renderer.EnqueuePass(fluidPass);
        }
    }
}

#endif // UNITY_PIPELINE_HDRP