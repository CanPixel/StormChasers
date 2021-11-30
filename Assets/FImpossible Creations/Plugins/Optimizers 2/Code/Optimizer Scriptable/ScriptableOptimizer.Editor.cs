using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class ScriptableOptimizer
    {

        /// <summary>
        /// Loading LOD Type reference from resources folder
        /// </summary>
        /// <param name="resourcesPath"> ex: Optimizers/FLOD_Mono Behaviour Reference</param>
        public ScrLOD_Base LoadLODReference(string resourcesPath)
        {
            ScrLOD_Base reference = Resources.Load<ScrLOD_Base>(resourcesPath);
            if (reference == null) Debug.LogError("[OPTIMIZERS CRITICAL ERROR] There are no references for base LOD Types, you removed them from resources folder???");
            return reference;
        }



        protected override void OptimizerReset()
        {
#if UNITY_EDITOR
            SetAutoDistance(0.23f);

            //AssignComponentsToOptimizeFrom(gameObject.transform);
            if (ToOptimize == null) ToOptimize = new List<ScriptableLODsController>();

            if (ToOptimize.Count == 0) AssignComponentsToBeOptimizedFromAllChildren(gameObject);
            if (ToOptimize.Count == 0) AssignComponentsToBeOptimizedFromAllChildren(gameObject, true);

            DrawDeactivateToggle = true;
#endif
        }


        public override void SyncWithReferences()
        {
            #region Prefab Syncing

            if (ToOptimize.Count > 0)
                if (ToOptimize[0].LODSet != null)
                    if (ToOptimize[0].LODSet.LevelOfDetailSets != null)
                        if (ToOptimize[0].LODSet.LevelOfDetailSets.Count > 0)
                            if (ToOptimize[0].LODSet.LevelOfDetailSets.Count - 2 != LODLevels)
                            {
                                LODLevels = ToOptimize[0].LODSet.LevelOfDetailSets.Count - 2;
                                preLODLevels = LODLevels;
                            }

            #endregion
        }


        protected override void OnValidateRefreshComponents()
        {
            if (ToOptimize != null)
                RefreshToOptimizeList();
            else
                AssignComponentsToOptimizeFrom(gameObject.transform);
        }


        protected override void OnValidateUpdateToOptimize()
        {
            if (preLODLevels != LODLevels) ResetLODs();

#if UNITY_EDITOR
            if (ToOptimize != null)
                for (int i = 0; i < ToOptimize.Count; i++)
                    if (ToOptimize[i].RootReference)
                        ToOptimize[i].OnValidate();
#endif


            preLODLevels = LODLevels;

#if UNITY_EDITOR
            if (Editor_WasSaving) Optimizers_LODTransport.OnValidateEnds(this);
#endif
        }


        public override void CheckForNullsToOptimize()
        {
            if (ToOptimize == null) return;
            for (int i = ToOptimize.Count - 1; i >= 0; i--)
            {
                if (ToOptimize[i] == null)
                    ToOptimize.RemoveAt(i);
                else
                {
                    if (ToOptimize[i].Component == null)
                        ToOptimize.RemoveAt(i);
                }
            }
        }


        public override void CleanAsset()
        {
#if UNITY_EDITOR
            for (int i = 0; i < ToOptimize.Count; i++)
                if (!ToOptimize[i].UsingShared)
                    Optimizers_LODTransport.RemoveLODControllerSubAssets(ToOptimize[i]);

            //FOptimizers_LODTransport.ClearPrefabFromUnusedOptimizersSubAssets(gameObject);

            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }


        protected override void ResetLODs()
        {
#if UNITY_EDITOR
            for (int i = 0; i < ToOptimize.Count; i++)
            {
                ToOptimize[i].GenerateLODParameters();
            }

            if (ToOptimize.Count > 0)
                if (ToOptimize[0].LODSet != null)
                    if (LODLevels != ToOptimize[0].LODSet.LevelOfDetailSets.Count - 2) HiddenCullAt = LODLevels;
#endif
        }



        public override void RemoveAllComponentsFromToOptimize()
        {
#if UNITY_EDITOR

            if (ToOptimize == null) return;

            for (int i = ToOptimize.Count - 1; i >= 0; i--)
            {
                if (!ToOptimize[i].UsingShared)
                {
                    if (ToOptimize[i].LODSet)
                        Optimizers_LODTransport.RemoveLODControllerSubAssets(ToOptimize[i]);
                }

                ToOptimize.RemoveAt(i);
            }
#endif
        }



        public override void RemoveFromToOptimizeAt(int i)
        {
#if UNITY_EDITOR
            if (i < ToOptimize.Count)
            {
                if (!ToOptimize[i].UsingShared)
                {
                    if (ToOptimize[i].LODSet)
                        Optimizers_LODTransport.RemoveLODControllerSubAssets(ToOptimize[i]);
                }

                ToOptimize.RemoveAt(i);
            }
#endif
        }


        protected override void RefreshInitialSettingsForOptimized()
        {
            RefreshDistances();

            for (int i = ToOptimize.Count - 1; i >= 0; i--)
            {
                if (ToOptimize == null) { ToOptimize.RemoveAt(i); continue; }
                ToOptimize[i].OnStart();
            }

            if (UseMultiShape) { AddToContainer = false; Debug.Log("Multi shape detection no container!"); }
        }



        public override void AssignComponentsToOptimizeFrom(Component target)
        {
#if UNITY_EDITOR
            if (ToOptimize == null) ToOptimize = new List<ScriptableLODsController>();

            // Checking if there is no other optimizer using this components for optimization
            List<Optimizer_Base> childOptimizers = FindComponentsInAllChildren<Optimizer_Base>(transform);

            manager = OptimizersManager.Instance; manager = null; // Casting Get() to generate optimizers manager

            TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Particle System Reference"), target, childOptimizers);
            TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Audio Source Reference"), target, childOptimizers);
            TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Nav Mesh Agent Reference"), target, childOptimizers);
            TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Renderer Reference"), target, childOptimizers);
            TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Light Reference"), target, childOptimizers);


            // Checking for extra unity components inside resources/optimizers/extra
            UnityEngine.Object[] custom = Resources.LoadAll("Optimizers/Extra/");
            ScriptableLODsController controller;
            List<Optimizer_Base> optimizers = FindComponentsInAllChildren<Optimizer_Base>(transform);
            for (int i = 0; i < custom.Length; i++)
            {
                ScrLOD_Base lodType = custom[i] as ScrLOD_Base;
                if (lodType != null)
                {
                    controller = lodType.GenerateLODController(target, this);

                    if (controller != null)
                    {
                        if (!CheckIfAlreadyInUse(controller, optimizers)) AddToOptimize(controller);
                    }
                }
            }

#endif
        }


        /// <summary>
        /// Generating LOD controller for target component type and adding to optimizer
        /// </summary>
        protected void TryAddLODControllerFor(ScrLOD_Base lod, Component target, List<Optimizer_Base> childOptims)
        {
#if UNITY_EDITOR
            if (lod == null) return;
            if (target == null) return;

            ScriptableLODsController controller = lod.GenerateLODController(target, this);
            if (controller != null)
            {
                if (!CheckIfAlreadyInUse(controller, childOptims)) AddToOptimize(controller);
            }
#endif
        }




        public override void AssignCustomComponentToOptimize(MonoBehaviour target)
        {

#if UNITY_EDITOR
            if (ToOptimize == null) ToOptimize = new List<ScriptableLODsController>();
            if (target == null) return;
            if (target is Optimizer_Base) return;
            if (target.GetType().IsSubclassOf(typeof(Optimizer_Base))) return;

            if (!ContainsComponent(target))
            {
                // Checking if there is no other optimizer using this components for optimization
                List<Optimizer_Base> optimizers = FindComponentsInAllChildren<Optimizer_Base>(transform);

                ScriptableLODsController controller;

                // Checking for custom components inside resources/optimizers/implementations
                UnityEngine.Object[] custom = Resources.LoadAll("Optimizers/Implementations/");
                for (int i = 0; i < custom.Length; i++)
                {
                    ScrLOD_Base lodType = custom[i] as ScrLOD_Base;
                    if (lodType != null)
                    {
                        controller = lodType.GenerateLODController(target, this);

                        if (controller != null)
                        {
                            if (!CheckIfAlreadyInUse(controller, optimizers)) AddToOptimize(controller);
                        }
                    }
                }

                // Checking for custom components inside resources/optimizers/custom
                custom = Resources.LoadAll("Optimizers/Custom/");
                for (int i = 0; i < custom.Length; i++)
                {
                    ScrLOD_Base lodType = custom[i] as ScrLOD_Base;
                    if (lodType != null)
                    {
                        controller = lodType.GenerateLODController(target, this);

                        if (controller != null)
                        {
                            if (!CheckIfAlreadyInUse(controller, optimizers)) AddToOptimize(controller);
                        }
                    }
                }

                controller = LoadLODReference("Optimizers/Base/FLOD_Mono Behaviour Reference").GenerateLODController(target, this);

                if (controller != null)
                {
                    if (!CheckIfAlreadyInUse(controller, optimizers)) AddToOptimize(controller);
                }
            }

#endif

        }



        public override bool ContainsComponent(Component component)
        {
            for (int i = ToOptimize.Count - 1; i >= 0; i--)
            {
                if (ToOptimize == null) { ToOptimize.RemoveAt(i); continue; }
                if (ToOptimize[i].Component == component) return true;
            }

            return false;
        }


        public override void RefreshToOptimizeList()
        {
            for (int i = ToOptimize.Count - 1; i >= 0; i--)
            {
                if (ToOptimize[i] == null) ToOptimize.RemoveAt(i);
            }
        }


    }
}