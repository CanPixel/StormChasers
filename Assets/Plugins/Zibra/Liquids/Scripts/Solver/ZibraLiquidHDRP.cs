#if UNITY_PIPELINE_HDRP

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using com.zibra.liquid.Solver;

namespace com.zibra.liquid
{
    public class FluidHDRPRenderComponent : CustomPassVolume
    {
        public class FluidHDRPRender : CustomPass
        {
            public ZibraLiquid liquid;
            RTHandle Depth;

            protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
            {
                Depth = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                                        colorFormat: GraphicsFormat.R32_SFloat,
                                        // We don't need alpha for this effect
                                        useDynamicScale: true, name: "Depth buffer");
            }

            protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera,
                                            CullingResults cullingResult)
            {
                if (liquid && liquid.initialized && liquid.simulationInternalFrame > 1)
                {
                    if ((hdCamera.camera.cullingMask & (1 << liquid.gameObject.layer)) ==
                        0) // fluid gameobject layer is not in the culling mask of the camera
                        return;

                    liquid.RenderCallBack(hdCamera.camera);

                    RTHandle cameraColor, cameraDepth;
                    GetCameraBuffers(out cameraColor, out cameraDepth);

                    var depth = Shader.PropertyToID("_CameraDepthTexture");
                    cmd.GetTemporaryRT(depth, hdCamera.camera.pixelWidth, hdCamera.camera.pixelHeight, 32,
                                       FilterMode.Point, RenderTextureFormat.RFloat);

                    // copy screen to background
                    var scale = RTHandles.rtHandleProperties.rtHandleScale;
                    cmd.Blit(cameraColor, liquid.cameraResources[hdCamera.camera].background,
                             new Vector2(scale.x, scale.y), Vector2.zero, 0, 0);
                    // blit depth to temp RT
                    HDUtils.BlitCameraTexture(cmd, cameraDepth, Depth);
                    cmd.Blit(Depth, depth, new Vector2(scale.x, scale.y), Vector2.zero, 1, 0);

                    liquid.RenderParticelsNative(cmd);
                    CoreUtils.SetRenderTarget(cmd, cameraColor, cameraDepth, ClearFlag.None);
                    // bind temp depth RT
                    cmd.SetGlobalTexture("_CameraDepthTexture", depth);
                    liquid.RenderFluid(cmd, hdCamera.camera);
                    cmd.ReleaseTemporaryRT(depth);
                }
            }

            protected override void Cleanup()
            {
                RTHandles.Release(Depth);
            }
        }

        public FluidHDRPRender fluidPass;
    }
}

#endif // UNITY_PIPELINE_HDRP