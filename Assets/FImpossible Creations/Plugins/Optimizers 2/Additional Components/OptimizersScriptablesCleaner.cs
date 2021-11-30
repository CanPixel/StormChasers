using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    [AddComponentMenu("FImpossible Creations/Optimizers 2/Optimizers Scriptables Cleaner")]
    public class OptimizersScriptablesCleaner : MonoBehaviour
    {
        public GameObject PrefabWithOptimizers;

        private void Reset()
        {
            PrefabWithOptimizers = gameObject;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Checking if some LOD sets lost references and was left inside prefab file
        /// </summary>
        public static List<Object> CheckForLeftovers(GameObject prefab, bool log = true)
        {
            if (!prefab)
            {
                Debug.LogError("[OPTIMIZERS EDITOR] No Prefab Object!");
                return null;
            }

            if (prefab)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefab);

                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogError("[OPTIMIZERS EDITOR] No Prefab path!");
                    return null;
                }

                // Getting all assets attached to prefab
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(prefabPath);

                if (assets.Length <= 1)
                {
                    Debug.LogError("[OPTIMIZERS EDITOR] No assets in prefab!");
                    return null;
                }

                // Collecting all optimizers related assests inside prefab
                List<Object> allOptimizersFiles = new List<Object>();

                for (int a = 0; a < assets.Length; a++)
                {
                    if (assets[a] is ScrOptimizer_LODSettings || assets[a] is ScrLOD_Base)
                        if (!allOptimizersFiles.Contains(assets[a])) allOptimizersFiles.Add(assets[a]);
                }

                if (log) Debug.Log("[OPTIMIZERS EDITOR] There are " + allOptimizersFiles.Count + " optimizer related files inside " + prefab.name);

                // Collecting all optimizers from component
                List<ScriptableOptimizer> optimizers = new List<ScriptableOptimizer>();
                foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
                {
                    ScriptableOptimizer opt = t.GetComponent<ScriptableOptimizer>();
                    if (opt) optimizers.Add(opt);
                }

                if (optimizers.Count > 0)
                {
                    List<Object> usedAssetFiles = new List<Object>();

                    // Going through all optimizers from prefab
                    for (int i = 0; i < optimizers.Count; i++)
                    {
                        // Going through all optimized components
                        for (int l = 0; l < optimizers[i].ToOptimize.Count; l++)
                        {
                            if (optimizers[i].ToOptimize[l].UsingShared) continue;

                            if (optimizers[i].ToOptimize[l].LODSet != null)
                            {
                                if (optimizers[i].ToOptimize[l].LODSet.LevelOfDetailSets != null)
                                {
                                    // Collecting used LOD bases
                                    for (int s = 0; s < optimizers[i].ToOptimize[l].LODSet.LevelOfDetailSets.Count; s++)
                                    {
                                        ScrLOD_Base lod = optimizers[i].ToOptimize[l].LODSet.LevelOfDetailSets[s];

                                        if (!usedAssetFiles.Contains(lod)) usedAssetFiles.Add(lod);

                                        // Checking if LOD Levels are added to prefab
                                        //for (int a = 0; a < assets.Length; a++)
                                        //    if (assets[a] == optimizers[i].ToOptimize[l].LODSet.LevelOfDetailSets[s])
                                        //    {
                                        //        if (!usedAssetFiles.Contains(assets[a])) usedAssetFiles.Add(assets[a]);
                                        //    }
                                    }
                                }

                                // Checking if LOD Set is added to prefab
                                if (AssetDatabase.Contains(optimizers[i].ToOptimize[l].LODSet))
                                {
                                    ScrOptimizer_LODSettings sets = optimizers[i].ToOptimize[l].LODSet;

                                    if (!usedAssetFiles.Contains(sets)) usedAssetFiles.Add(sets);

                                    //for (int a = 0; a < assets.Length; a++)
                                    //if (assets[a] == optimizers[i].ToOptimize[l].LODSet)
                                    //{
                                    //    if (!usedAssetFiles.Contains(assets[a]))
                                    //    {
                                    //        usedAssetFiles.Add(assets[a]);
                                    //        Debug.Log("Adding to used: " + assets[a].name);
                                    //    }

                                    //    break;
                                    //}
                                }
                            }
                        }
                    }

                    if (log) Debug.Log("[OPTIMIZERS EDITOR] There are " + usedAssetFiles.Count + " used optimizers files inside " + prefab.name);

                    // Removing files from prefab asset which are not used by any optimizer added to this prefab
                    // Creating list of not used files then removing it
                    List<Object> notUsedAssetFiles = new List<Object>();
                    for (int i = 0; i < allOptimizersFiles.Count; i++)
                    {
                        // If optimizer related file is not used by any optimizer in prefab
                        if (usedAssetFiles.Contains(allOptimizersFiles[i]) == false)
                            notUsedAssetFiles.Add(allOptimizersFiles[i]); // We remove it
                    }

                    if (log) Debug.Log("[OPTIMIZERS EDITOR] There are " + notUsedAssetFiles.Count + " NOT used optimizers files inside " + prefab.name);

                    return notUsedAssetFiles;
                }
                else
                {
                    Debug.LogWarning("[OPTIMIZERS EDITOR] There are no optimizers inside " + prefab.name);
                    return allOptimizersFiles;
                }
            }
            else
            {
                Debug.LogError("[OPTIMIZERS EDITOR] No Prefab Object!");
            }

            return null;
        }

#endif

    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(OptimizersScriptablesCleaner))]
    public class Optimizer_CleanerEditor : UnityEditor.Editor
    {
        public int leftovers = 0;
        public override void OnInspectorGUI()
        {
            OptimizersScriptablesCleaner targetScript = (OptimizersScriptablesCleaner)target;
            DrawDefaultInspector();


            if (targetScript.PrefabWithOptimizers != null)
                if (targetScript.gameObject.scene.rootCount != 0)
                {
                    EditorGUILayout.HelpBox("This component must be viewed on prefab through project browser! NO Prefab Isolated Mode AND NO prefab placed on scene.", MessageType.Error);
                }

            EditorGUILayout.HelpBox("This component is for debugging if some sub-assets wasn't removed from prefab file", MessageType.Info);

            GUILayout.Space(4f);

            if (targetScript.PrefabWithOptimizers == null) return;

            if (GUILayout.Button("Check for Leftovers")) if (!Application.isPlaying)
                {
                    List<Object> toRemove = OptimizersScriptablesCleaner.CheckForLeftovers(targetScript.PrefabWithOptimizers);

                    if (toRemove == null)
                    {
                        leftovers = 0;
                        return;
                    }

                    leftovers = toRemove.Count;

                    if (toRemove.Count > 0)
                        Debug.Log("There are " + toRemove.Count + " leftovers inside " + targetScript.PrefabWithOptimizers.name);
                    else
                        Debug.Log("No Leftovers inside " + targetScript.PrefabWithOptimizers.name);
                }

            if (leftovers > 0)
            {
                if (GUILayout.Button("Clear Leftovers (" + leftovers + ")")) if (!Application.isPlaying)
                    {
                        List<Object> toRemove = OptimizersScriptablesCleaner.CheckForLeftovers(targetScript.PrefabWithOptimizers, false);
                        string prefabPath = AssetDatabase.GetAssetPath(targetScript.PrefabWithOptimizers);

                        if (!string.IsNullOrEmpty(prefabPath))
                        {
                            if (toRemove == null)
                            {
                                leftovers = 0;
                                return;
                            }

#if UNITY_2019_1_OR_NEWER
                            for (int i = 0; i < toRemove.Count; i++)
                            {
                                if (toRemove[i])
                                {
                                    AssetDatabase.RemoveObjectFromAsset(toRemove[i]);
                                }
                            }
#endif

                            for (int i = 0; i < toRemove.Count; i++)
                            {
                                if (toRemove[i] != null)
                                {
                                    DestroyImmediate(toRemove[i], true);
                                }
                            }

                            AssetDatabase.SaveAssets();
                            AssetDatabase.ImportAsset(prefabPath);
                            AssetDatabase.Refresh();

                            toRemove = OptimizersScriptablesCleaner.CheckForLeftovers(targetScript.PrefabWithOptimizers, false);
                            if (toRemove == null) { leftovers = 0; return; }
                            leftovers = toRemove.Count;
                        }
                    }
            }


        }
    }
#endif

}