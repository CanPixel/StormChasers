using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal static class UnityInfo {
		public static bool UsingSRP => GraphicsSettings.renderPipelineAsset != null;
		public const int INSTANCES_MAX = 1023;

		#if UNITY_EDITOR
		internal static RenderPipeline GetCurrentRenderPipelineInUse() {
			RenderPipelineAsset rpa = GraphicsSettings.renderPipelineAsset;
			if( rpa != null ) {
				switch( rpa.GetType().Name ) {
					case "UniversalRenderPipelineAsset": return RenderPipeline.URP;
					case "HDRenderPipelineAsset":        return RenderPipeline.HDRP;
				}
			}

			return RenderPipeline.Legacy;
		}

		#if SHAPES_URP || SHAPES_HDRP
		public const string ON_PRE_RENDER_NAME = "RenderPipelineManager.beginCameraRendering";
		#else
		public const string ON_PRE_RENDER_NAME = "Camera.onPreRender";
		#endif
		
		#endif
	}

}