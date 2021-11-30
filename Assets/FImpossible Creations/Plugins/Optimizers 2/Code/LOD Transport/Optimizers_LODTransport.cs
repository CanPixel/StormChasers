#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class to solve many troubles with different unity versions to use not shared (unique) settings
    /// in all cases (isolated scene prefab mode / creating prefabs from scene etc.)
    /// Triggering methods for unity before 2018.3 or after
    /// </summary>
    public static partial class Optimizers_LODTransport
    {
        public static bool SomethingNeedToBeSaved(ScriptableOptimizer optimizer)
        {
            #region Initial null reference conditions and initial decalrations

            if (optimizer == null) return false; // Method called with wrong argument

            Object prefab = GetPrefab(optimizer.gameObject);

            if (prefab == null)
            {
                Debug.Log("No Prefab - Checking for need to save");
                return false; // If it's not a prefabed object we not saving anything, it will be remembered in scene data
            }

            #endregion

            // Checking if all not shared assets are saved in prefab file
            // If any of them is not saved we must perform saving
            List<ScriptableLODsController> haveNotSaved = CheckIfOptimizerHaveUnsavedLODs(optimizer, true);
            if (haveNotSaved != null) if (haveNotSaved.Count == 1) if (haveNotSaved[0] == null) return true;

            return false;
        }


        public static void SaveLackingLODsAsSubAssets(ScriptableOptimizer optimizer)
        {
            #region Initial null reference conditions and initial delcarations

            if (optimizer == null) return; // Method called with wrong argument

            Object prefab = GetPrefab(optimizer.gameObject);

            if (prefab == null)
            {
                Debug.Log("No Prefab - Checking for need to save");
                return; // If it's not a prefabed object we not saving anything, it will be remembered in scene data
            }

            #endregion  

            // Checking if all not shared assets are saved in prefab file
            // If any of them is not saved we must perform saving
            List<ScriptableLODsController> haveNotSaved = CheckIfOptimizerHaveUnsavedLODs(optimizer);

            if (haveNotSaved.Count > 0)
            {
                Object[] assets = GetAssetList(prefab);

                for (int i = 0; i < haveNotSaved.Count; i++)
                    SaveLODSetInAsset(haveNotSaved[i], prefab, assets);
            }
        }


        public static void RemoveLODControllerSubAssets(ScriptableLODsController controller, bool onlySubSettings = false)
        {
            #region Initial conditions

            if (Application.isPlaying)
            {
                Debug.LogWarning("[OPTIMIZERS] No allowed in playmode!");
                return;
            }

            #endregion

            ScrOptimizer_LODSettings lodSet = controller.LODSet;

            if (lodSet == null) return;

            for (int i = 0; i < lodSet.LevelOfDetailSets.Count; i++)
                if (lodSet.LevelOfDetailSets[i] != null)
                    if (AssetDatabase.Contains(lodSet.LevelOfDetailSets[i]))
                    {
                        Object.DestroyImmediate(lodSet.LevelOfDetailSets[i], true);
                    }

            lodSet.LevelOfDetailSets.Clear();

            if (!onlySubSettings)
            {
                if (AssetDatabase.Contains(lodSet))
                    Object.DestroyImmediate(lodSet, true);
            }
        }


        public static void ClearPrefabFromUnusedOptimizersSubAssets(GameObject prefab)
        {
            if (!prefab) return;

            List<Object> toRemove = OptimizersScriptablesCleaner.CheckForLeftovers(prefab, false);
            string prefabPath = AssetDatabase.GetAssetPath(prefab);

            if (!string.IsNullOrEmpty(prefabPath))
            {
                if (toRemove == null) return;

#if UNITY_2019_1_OR_NEWER
                for (int i = 0; i < toRemove.Count; i++) if (toRemove[i]) AssetDatabase.RemoveObjectFromAsset(toRemove[i]);
#endif

                for (int i = 0; i < toRemove.Count; i++)
                    if (toRemove[i] != null)
                        Object.DestroyImmediate(toRemove[i], true);

                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(prefabPath);
                AssetDatabase.Refresh();
            }
        }


        public static void OnValidateEnds(ScriptableOptimizer optimizer)
        {
            if (optimizer.Editor_WasSaving)
            {
                if (!optimizer.Editor_InIsolatedScene)
                {
#if UNITY_2019_3_OR_NEWER
                    // I assume here that with unity 2019.3 user is working with assets pipeline v2
                    //AssetDatabase.SaveAssets();
#endif

                    optimizer.Editor_WasSaving = false;
                    //Debug.Log("Save " + optimizer.gameObject.scene.rootCount);
                }
            }
        }
    }
}

#endif
