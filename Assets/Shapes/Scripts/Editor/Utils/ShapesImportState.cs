#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
#if SHAPES_URP
using System;
using UnityEditor.Rendering.Universal;
#endif
#endif
using UnityEngine;
using UnityEngine.Rendering;

#if SHAPES_URP
using System.Reflection;
using UnityEngine.Rendering.Universal;

#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ShapesImportState : ScriptableObject {

		[Header( "Do not edit this~" )]
		[SerializeField]
		internal RenderPipeline currentShaderRP = RenderPipeline.Legacy;

		static ShapesImportState instance;
		public static ShapesImportState Instance => instance != null ? instance : instance = Resources.Load<ShapesImportState>( "Shapes Import State" );

		#if UNITY_EDITOR
		[DidReloadScripts( 1 )]
		public static void CheckRenderPipelineSoon() => EditorApplication.delayCall += CheckRenderPipeline;

		static void CheckRenderPipeline() {
			RenderPipeline rpInUnity = UnityInfo.GetCurrentRenderPipelineInUse();
			ShapesImportState inst = Instance;
			if( inst == null ) {
				Debug.LogWarning( "Failed to get import state - Shapes will retry on the next script recompile" );
				return; // I guess some weird import order shenan happened? :c
			}

			// set up preprocessor defines, this will also indirectly trigger a second pass of this whole method
			EnsurePreprocessorsAreDefined( rpInUnity );

			// makes sure all shaders are compiled to a specific render pipeline
			RenderPipeline rpShapesShaders = inst.currentShaderRP;
			if( rpInUnity != rpShapesShaders ) {
				string rpStr = rpInUnity.ToString();
				if( rpInUnity == RenderPipeline.Legacy )
					rpStr = "the built-in render pipeline";
				string desc = $"Looks like you're using {rpStr}!\nShapes will now regenerate all shaders, it might take a lil while~";
				EditorUtility.DisplayDialog( "Shapes", desc, "ok" );
				CodegenShaders.GenerateShadersAndMaterials();
			}

			// second pass check - make sure URP forward renderer has the custom Shapes pass in it
			#if SHAPES_URP
			EnsureShapesPassExistsInTheUrpRenderer();
			#endif

			// also on second pass
			MakeSureSampleMaterialsAreValid();
		}


		static void MakeSureSampleMaterialsAreValid() {
			#if SHAPES_URP || SHAPES_HDRP
			#if UNITY_2019_1_OR_NEWER
			Shader targetShader = GraphicsSettings.renderPipelineAsset.defaultShader;
			#else
			Shader targetShader = GraphicsSettings.renderPipelineAsset.GetDefaultShader();
			#endif
			#else
			Shader targetShader = UIAssets.Instance.birpDefaultShader;
			#endif

			bool changed = false;
			if( ShapesIO.TryMakeAssetsEditable( UIAssets.Instance.sampleMaterials ) ) { // ensures version control allows us to edit
				foreach( var mat in UIAssets.Instance.sampleMaterials ) {
					if( mat == null )
						continue; // samples were probably not imported into this project (or they were deleted) if this is null
					if( mat.shader != targetShader ) {
						Undo.RecordObject( mat, "Shapes update sample materials shaders" );
						Color color = GetMainColor( mat );
						mat.shader = targetShader;
						#if SHAPES_URP || SHAPES_HDRP
							mat.SetColor( ShapesMaterialUtils.propBaseColor, color );
						#else
							mat.SetColor( ShapesMaterialUtils.propColor, color );
						#endif
						changed = true;
					}
				}
			}

			if( changed )
				Debug.Log( "Shapes updated sample material shaders to match your current render pipeline" );
		}

		static Color GetMainColor( Material mat ) {
			if( mat.HasProperty( ShapesMaterialUtils.propColor ) ) return mat.GetColor( ShapesMaterialUtils.propColor );
			if( mat.HasProperty( ShapesMaterialUtils.propBaseColor ) ) return mat.GetColor( ShapesMaterialUtils.propBaseColor );
			return Color.white;
		}


		#if SHAPES_URP
		static class UrpRndFuncs {
			const BindingFlags bfs = BindingFlags.Instance | BindingFlags.NonPublic;
			public static readonly FieldInfo fRndDataList = typeof(UniversalRenderPipelineAsset).GetField( "m_RendererDataList", bfs );
			public static readonly MethodInfo fAddComponent = typeof(ScriptableRendererDataEditor).GetMethod( "AddComponent", bfs );
			public static readonly MethodInfo fOnEnable = typeof(ScriptableRendererDataEditor).GetMethod( "OnEnable", bfs );
			public static readonly bool successfullyLoaded = fRndDataList != null && fAddComponent != null && fOnEnable != null;

			public static readonly string failMessage = $"Unity's URP API seems to have changed. Failed to load: " +
														$"{( fRndDataList == null ? "UniversalRenderPipelineAsset.m_RendererDataList" : "" )} " +
														$"{( fAddComponent == null ? "ScriptableRendererDataEditor.AddComponent" : "" )} " +
														$"{( fOnEnable == null ? "ScriptableRendererDataEditor.OnEnable" : "" )}";
		}

		static void EnsureShapesPassExistsInTheUrpRenderer() {
			if( UrpRndFuncs.successfullyLoaded ) { // if our reflected members failed to load, we're kinda screwed :c
				if( GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset urpa ) { // find the URP asset
					ScriptableRendererData[] srd = (ScriptableRendererData[])UrpRndFuncs.fRndDataList.GetValue( urpa );
					foreach( var rndd in srd.Where( x => x is ForwardRendererData ) ) { // only add to forward renderer
						if( rndd.rendererFeatures.Any( x => x is ShapesRenderFeature ) == false ) { // does it have Shapes?
							// does not contain the Shapes render feature, so, oh boy, here we go~
							if( ShapesIO.TryMakeAssetsEditable( urpa ) ) {
								ForwardRendererDataEditor fwEditor = (ForwardRendererDataEditor)Editor.CreateEditor( rndd );
								UrpRndFuncs.fOnEnable.Invoke( fwEditor, null ); // you ever just call OnEnable manually
								UrpRndFuncs.fAddComponent.Invoke( fwEditor, new[] { (object)nameof(ShapesRenderFeature) } );
								DestroyImmediate( fwEditor ); // luv 2 create temporary editors
								Debug.Log( $"Added Shapes renderer feature to {rndd.name}", rndd );
							}
						}
					}
				} else
					Debug.LogWarning( $"Shapes failed to load the URP pipeline asset to add the renderer feature. " +
									  $"You might have to add {nameof(ShapesRenderFeature)} to your renderer asset manually" );
			} else
				Debug.LogError( UrpRndFuncs.failMessage );
		}

		#endif

		static void EnsurePreprocessorsAreDefined( RenderPipeline rpTarget ) {
			BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
			List<string> symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( buildTargetGroup ).Split( ';' ).ToList();

			bool changed = false;

			void CheckRpSymbol( RenderPipeline rp ) {
				bool on = rp == rpTarget;
				string ppName = rp.PreprocessorDefineName();
				if( on && symbols.Contains( ppName ) == false ) {
					symbols.Add( ppName );
					changed = true;
				} else if( on == false && symbols.Remove( ppName ) )
					changed = true;
			}

			CheckRpSymbol( RenderPipeline.URP );
			CheckRpSymbol( RenderPipeline.HDRP );

			if( changed && ShapesIO.TryMakeAssetsEditable( ShapesIO.projectSettingsPath ) ) {
				Debug.Log( $"Shapes updated your project scripting define symbols since you seem to be using {rpTarget.PrettyName()}, I hope that's okay~" );
				PlayerSettings.SetScriptingDefineSymbolsForGroup( buildTargetGroup, string.Join( ";", symbols ) );
			}
		}

		#endif

	}

}