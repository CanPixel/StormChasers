#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class to solve many troubles with different unity versions to use not shared (unique) settings
    /// in all cases (isolated scene prefab mode / creating prefabs from scene etc.)
    /// Utilities methods for LOD transport
    /// </summary>
    public static partial class Optimizers_LODTransport
    {
        /// <summary>
        /// Checking if optimizer have unsaved LOD scriptable files
        /// </summary>
        /// <param name="checkIfAny"> If any asset is missing to be saved, then will be immediately returned list with one element[0] == null </param>
        public static List<ScriptableLODsController> CheckIfOptimizerHaveUnsavedLODs(ScriptableOptimizer optimizer, bool checkIfAny = false)
        {
            #region Initial null reference conditions and initial delcarations

            Object prefab = GetPrefab(optimizer.gameObject);

            if (prefab == null) return null; // No prefab

            string path = AssetDatabase.GetAssetPath(prefab);

            if (string.IsNullOrEmpty(path)) return null; // No prefab

            #endregion

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            List<ScriptableLODsController> haveUnsaved = new List<ScriptableLODsController>();

            for (int o = 0; o < optimizer.ToOptimize.Count; o++)
            {
                // LODs controller for one component under optimization by optimizer
                ScriptableLODsController lodsController = optimizer.ToOptimize[0];
                ScrOptimizer_LODSettings lodSettings = lodsController.LODSet;
                List<ScrLOD_Base> lods = lodSettings.LevelOfDetailSets;

                // Checking only individual settings LODs
                if (lodsController.UsingShared) continue;

                if (!AssetDatabase.IsSubAsset(lodSettings))
                {
                    if (checkIfAny)
                    {
                        haveUnsaved.Add(null);
                        return haveUnsaved;
                    }
                    else
                    {
                        haveUnsaved.Add(lodsController);
                        continue;
                    }
                }

                for (int l = 0; l < lods.Count; l++)
                {
                    Object lod = lods[l];

                    // LOD instance can be added as sub-asset for LOD Set
                    // So we need to check if it's sub-asset and if it belongs to optimizer object

                    bool contains = false;
                    for (int a = 0; a < assets.Length; a++)
                        if (assets[a] == lod)
                        {
                            contains = true;
                            break;
                        }

                    // This LOD instance is not saved to optimizer's prefab file
                    if (!contains)
                    {
                        if (checkIfAny)
                        {
                            haveUnsaved.Add(null);
                            return haveUnsaved;
                        }
                        else
                        {
                            haveUnsaved.Add(lodsController);
                            break;
                        }
                    }
                }

            }

            return haveUnsaved;
        }


        /// <summary>
        /// Saving all optimizer unique scriptable data inside prefab as sub-assets
        /// </summary>
        public static void SaveOptimizerLODDataInPrefab(ScriptableOptimizer optimizer, Object prefab)
        {
            #region Initial conditions

            if (Application.isPlaying)
            {
                Debug.LogWarning("[OPTIMIZERS] No allowed in playmode!");
                return;
            }

            #endregion

            Object[] assets = GetAssetList(prefab);

            if (assets.Length != 0) // No Assets under path - No prefabed object?
                for (int i = 0; i < optimizer.ToOptimize.Count; i++)
                {
                    SaveLODSetInAsset(optimizer.ToOptimize[i], prefab, assets);
                }

            #region Unity Version 2018.3 Conditional

#if UNITY_2018_3_OR_NEWER
            //if (FEditor_OneShotLog.CanDrawLog("optSaveAssets", 1)) AssetDatabase.SaveAssets();
#endif
            #endregion
        }


        /// <summary>
        /// Saving LOD set settings in prefab file as sub-assets
        /// </summary>
        public static void SaveLODSetInAsset(ScriptableLODsController controller, Object asset, Object[] assetSubAssets)
        {
            #region Initial null reference conditions

            if (asset == null)
            {
                return;
            }

            if (assetSubAssets == null || assetSubAssets.Length == 0)
            {
                return;
            }

            if (controller == null)
            {
                return;
            }

            bool lodSetCheckable = true;

            if (controller.LODSet == null)
            {
                if (controller.GetUniqueSet() == null || controller.GetSharedSet() == null)
                {
                    lodSetCheckable = false;
                }
            }
            else
                if (controller.LODSet.LevelOfDetailSets == null)
            {
                lodSetCheckable = false;
            }


            #endregion

            if (lodSetCheckable)
            {
                // Checking if object is not already containing this assets
                for (int i = 0; i < controller.LODSet.LevelOfDetailSets.Count; i++)
                {
                    ScrLOD_Base lod = controller.LODSet.LevelOfDetailSets[i];
                    if (lod == null) continue;

                    // Checking this asset to be applied only when it's not project asset already
                    if (!AssetDatabase.Contains(lod))
                    {
                        bool contains = false;

                        // Checking if prefab have this lod settings instance as sub-asset already
                        for (int a = 0; a < assetSubAssets.Length; a++)
                            if (assetSubAssets[a] == lod)
                            {
                                contains = true;
                                break;
                            }

                        // If prefab don't have this lod settings inside already
                        if (!contains)
                        {
                            AssetDatabase.AddObjectToAsset(lod, asset);
                            controller.GetOptimizer.Editor_WasSaving = true;
                        }
                    }
                    else
                    {
                        //Debug.Log("Is already a asset");
                    }
                }

                // Checking if LOD Set asset is not already added to object
                bool containsSet = false;
                for (int a = 0; a < assetSubAssets.Length; a++)
                    if (assetSubAssets[a] == controller.LODSet)
                    {
                        containsSet = true;
                        break;
                    }

                if (!containsSet)
                {
                    AssetDatabase.AddObjectToAsset(controller.LODSet, asset);
                    controller.GetOptimizer.Editor_WasSaving = true;
                }
            }
            else
            {
                // Debug.Log("No checkable");
            }
        }


        /// <summary>
        /// When using shared settings or not prefabed object we only saving LOD sub settings to LOD Set
        /// </summary>
        public static void SaveLODSetInAsset(ScriptableLODsController controller, Object asset)
        {
            SaveLODSetInAsset(controller, asset, GetAssetList(asset));
        }


        /// <summary>
        /// Getting all object and sub-assets attached to target prefab
        /// </summary>
        public static Object[] GetAssetList(Object prefab)
        {
            return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(prefab));
        }


        /// <summary>
        /// Syncing LOD settings of scene object with optimizer with it's prefab
        /// </summary>
        public static void SyncSceneOptimizerWithPrefab(ScriptableOptimizer optimizer, ScriptableLODsController controller)
        {
            Object prefab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(GetPrefab(optimizer.gameObject)));

            if (prefab)
            {
                try
                {
                    GameObject o = (GameObject)prefab;

                    if (o)
                    {
                        ScriptableOptimizer prefabOptim = null;

                        // Checking all game objects inside optimizers prefab
                        // Searching for the same optimizer
                        foreach (Transform childInDepth in o.GetComponentsInChildren<Transform>())
                        {
                            // Getting all optimizers
                            var Opts = childInDepth.GetComponents<ScriptableOptimizer>();

                            // Checking rach optimizer
                            foreach (var childOpt in Opts)
                            {
                                // If it's the same type of optimizer
                                if (childOpt.GetType() == optimizer.GetType())
                                {
                                    // If they've the same optimized objects count
                                    if (childOpt.ToOptimize.Count == optimizer.ToOptimize.Count)
                                    {
                                        bool isSameOptimizerSetup = true;

                                        // If all optimized objects have the same types
                                        for (int k = 0; k < childOpt.ToOptimize.Count; k++)
                                        {
                                            if (childOpt.ToOptimize[k].Component.GetType() != optimizer.ToOptimize[k].Component.GetType())
                                            {
                                                isSameOptimizerSetup = false;
                                                break;
                                            }
                                        }

                                        if (isSameOptimizerSetup) prefabOptim = childOpt;
                                    }
                                }

                                if (prefabOptim) break;
                            }
                        }

                        // We found optimizer with the same setup
                        if (prefabOptim)
                        {
                            // We syncing optimizer from scene with one from prefab
                            for (int i = 0; i < prefabOptim.ToOptimize.Count; i++)
                            {
                                ScriptableLODsController lodC = prefabOptim.ToOptimize[i];

                                if (lodC.LODSet != null)
                                    if (lodC.RootReference == controller.RootReference)
                                        if (lodC.LODSet != controller.LODSet)
                                        {
                                            if (lodC.UsingShared)
                                                controller.SetSharedLODSettings(lodC.LODSet);
                                            else
                                                controller.SetUniqueLODSettings(lodC.LODSet);
                                        }
                            }
                        }
                    }
                }
                catch (System.Exception exc)
                {
                    Debug.LogWarning("[OPTIMZERS EDITOR] Something went wrong during process of syncing optimizer from scene with prefabed optimizer. " + exc);
                }
            }
        }


        /// <summary>
        /// Saving LOD settings of scene object with optimizer (searched through all optimizers on scene) 
        /// to created prefab
        /// </summary>
        public static void SaveLODSettingsFromSceneOptimizer(ScriptableLODsController controller)
        {
            ScriptableOptimizer optimizer = controller.GetOptimizer;

            bool success = false;

            // If it is prefabed object
            if (optimizer.gameObject.scene.rootCount == 0)
            {
                ScriptableOptimizer[] sceneOpts = GameObject.FindObjectsOfType<ScriptableOptimizer>();

                #region Unity Version Conditional
#if UNITY_2018_3_OR_NEWER

                // Avoiding Unity error occuring when adding optimizer just inside prefab mode...
                if (controller.LODSet == null)
                {
                    if (controller.GetUniqueSet() == null && controller.GetSharedSet() == null)
                    {
                        Debug.Log("[OPTIMIZERS EDITOR] <color=yellow>Please do not add optimizer inside prefab mode.</color> (or you're using nested prefabs? Not fully supported yet) <b>Exit prefab mode -> Change count of LOD levels -> After that you can freely edit prefab in prefab mode, if it doesn't help try: </b>  Add Optimizer through inspector window by selecting prefab inside project browser window   or add optimizer to object on scene and apply overrides settings. (To check if this work, exit prefab mode and click on any LOD level inside inspector window, if there will be warning message 'LOD Set Lost' that means there is something wrong)");
                    }
                }

                if (AssetDatabase.GetAssetPath(optimizer.gameObject) != "")
#endif
                    #endregion

                    // Searching for instance of optimizer's prefab inside scene
                    for (int i = 0; i < sceneOpts.Length; i++)
                    {
                        if (sceneOpts[i].ToOptimize != null)
                        {
                            bool isParent = false;

                            #region Unity Version Dependent
#if UNITY_2018_3_OR_NEWER
                            string pathA = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(sceneOpts[i].gameObject));
                            string pathB = AssetDatabase.GetAssetPath(optimizer.gameObject);
#else
                        string pathA = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(sceneOpts[i].gameObject));
                        string pathB = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabObject(optimizer.gameObject));
#endif
                            #endregion

                            isParent = string.Equals(pathA, pathB);
                            if (pathA == "") isParent = false;

                            // We found instance of prefab
                            if (isParent)
                            {
                                // Making sure this are the same optimizers
                                if (sceneOpts[i].ToOptimize.Count == optimizer.ToOptimize.Count)
                                    for (int s = 0; s < sceneOpts[i].ToOptimize.Count; s++)
                                    {
                                        // Making sure they've the same components to optimize
                                        if (sceneOpts[i].ToOptimize[s].RootReference.GetType() != optimizer.ToOptimize[s].RootReference.GetType())
                                        {
                                            // If components to optimize are different, we can't go forward
                                            success = false;
                                            break;
                                        }

                                        // Transfer LODsets from scene to asset
                                        if (sceneOpts[i].ToOptimize[s].UsingShared)
                                        {
                                            optimizer.ToOptimize[s].SetSharedLODSettings(sceneOpts[i].ToOptimize[s].LODSet);
                                            success = true;
                                        }
                                        else
                                        {
                                            if (sceneOpts[i].ToOptimize[s].LODSet == null)
                                            {
                                                Debug.LogError("[OPTIMIZERS EDITOR] There couldn't be found LOD settings to apply for prefab (Unity Asset System incompatibility? -> Even you see LOD settings inside inspector, Unity is just forgeting about their existence during saving frame :/ )");
                                            }
                                            else
                                            {
                                                SaveLODSetInAsset(sceneOpts[i].ToOptimize[s], GetPrefab(optimizer.gameObject));
                                                optimizer.ToOptimize[s].SetUniqueLODSettings(sceneOpts[i].ToOptimize[s].LODSet);
                                                success = true;
                                            }
                                        }
                                    }
                            }
                        }
                    }
            }

            if (!success)
            {
                // Unity 2018 is not syncing instances from scene immediately with prefab asset
                // We preventing from generating reseted LOD settings for previous code to execute
                // And previous code need recognition of prefab instance on the scene with prefab asset
                // If there is no recognition after 6 frames then we reset settings - that means this optimizer just lost references somehow
                #region Unity Version 2018.3 Conditional
#if UNITY_2018_3_OR_NEWER
                if (controller.nullTry > 6) // Limit of tries to generate settings from scene object from which we generated prefab
                {
#endif
                    Debug.LogWarning("[OPTIMIZERS] There was generated LOD settings again probably because object prefabing or lost references, it was neccesary. (" + optimizer.name + " - " + controller.Component + ")");
                    controller.GenerateLODParameters();
                    optimizer.DrawGeneratedPrefabInfo = true;
#if UNITY_2018_3_OR_NEWER
                    controller.nullTry = 0;
                }
                else
                {
                    if (!Application.isPlaying) EditorUtility.SetDirty(optimizer);
                }

                controller.nullTry++;
#endif
                #endregion
            }
            else
            {
                if (optimizer.ToOptimize.Count > 0)
                    if (optimizer.ToOptimize[0] != null)
                        if (optimizer.ToOptimize[0].LODSet != null)
                        {
                            //Debug.Log("Succesfully saved lod settings from scene optimizer");

                            #region Unity Version 2018.3 Conditional

#if UNITY_2018_3_OR_NEWER
                            //AssetDatabase.SaveAssets();
#endif
                            #endregion
                        }
            }

            return;
        }

    }
}

#endif
