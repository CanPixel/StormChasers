using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    public partial class ScriptableLODsController
    {

        protected override bool CheckCoreRequirements(bool hard = false)
        {
            if (!RootReference)
            {
                if (hard)
                {
#if UNITY_EDITOR
                    EditorGUILayout.HelpBox("Root Reference Lost!", MessageType.Warning);
#endif
                    if (GUILayout.Button(new GUIContent("Retry"), new GUILayoutOption[2] { GUILayout.Width(50), GUILayout.Height(15) }))
                    {
                        optimizer.RemoveAllComponentsFromToOptimize();
                        optimizer.AssignComponentsToBeOptimizedFromAllChildren(optimizer.gameObject);
                        if (sOptimizer.ToOptimize.Count == 0) optimizer.AssignComponentsToBeOptimizedFromAllChildren(optimizer.gameObject, true);
                    }
                }
                else
                {
                    Debug.LogError("[OPTIMIZERS] No Root Reference! Try adding Optimizers Manager again!");
                    return false;
                }
            }


            return true;
        }


        /// <summary>
        /// Generating new scriptable LOD Set container for component
        /// </summary>
        protected override void GenerateNewLODSettings()
        {
            ScrOptimizer_LODSettings lodSet = LODSet;

            if (LODSet != null)
            {
#if UNITY_EDITOR
                Optimizers_LODTransport.RemoveLODControllerSubAssets(this, true);
#endif
            }
            else
            {
                lodSet = ScriptableObject.CreateInstance<ScrOptimizer_LODSettings>();
            }

            if (UsingShared)
                SetSharedLODSettings(lodSet);
            else
                SetUniqueLODSettings(lodSet);
        }


        /// <summary>
        /// Checking if LOD controller should generate new LOD Settings
        /// </summary>
        protected override bool NeedToReGenerate(int targetCount)
        {
            bool need = false;

            if (LODSet == null)
            {
                if (uniqueLODSet == null && sharedLODSet == null)
                {
                    need = true;
                    GenerateNewLODSettings();
                }
                else
                {
                    if (uniqueLODSet != null) LODSet = uniqueLODSet; else LODSet = sharedLODSet;
                }
            }
            else
            {
                if (LODSet.LevelOfDetailSets == null)
                {
                    need = true;
                    LODSet.LevelOfDetailSets = new List<ScrLOD_Base>();
                }
                else
                if (LODSet.LevelOfDetailSets.Count == 0) need = true;
                else
                {
                    if (LODSet.LevelOfDetailSets[0] != null)
                    {/*Version = LODSet.LevelOfDetailSets[0].Version;*/}
                    else
                        need = true;

                    if (targetCount != LODSet.LevelOfDetailSets.Count + 2)
                    {
                        need = true;
                    }
                    else if (targetCount != 0)
                    {
                        bool nulls = false;
                        // Checking if there are null references inside LOD Set List
                        for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
                            if (LODSet.LevelOfDetailSets[i] == true)
                            {
                                nulls = true;
#if UNITY_EDITOR
                                Optimizers_LODTransport.RemoveLODControllerSubAssets(this, true);
#endif
                                break;
                            }

                        if (nulls)
                        {
                            need = true;
                        }
                    }
                }
            }

            return need;
        }



        #region Main Settings / Scriptables Related Stuff


        /// <summary>
        /// Setting shared LOD settings reference to be used for optimized component settings
        /// </summary>
        public void SetSharedLODSettings(ScrOptimizer_LODSettings lodSettings)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Debug.LogWarning("[OPTIMIZERS] No allowed in playmode!");
                return;
            }

            if (lodSettings == null) // lodSettings null means we remove shared - Creating new unique LOD set with the same settings
            {
                lodSettings = ScriptableObject.CreateInstance<ScrOptimizer_LODSettings>();

                for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
                    lodSettings.LevelOfDetailSets.Add(LODSet.LevelOfDetailSets[i].CreateNewScrCopy());

                bool shar = UsingShared;
                SetUniqueLODSettings(lodSettings);

                // (nulling shared settings)
                if (shar) Optimizers_LODTransport.SaveLODSetInAsset(this, Optimizers_LODTransport.GetPrefab(optimizer.gameObject));
            }
            else
            {
                if (uniqueLODSet != null)
                {
                    if (uniqueLODSet != lodSettings)
                        Optimizers_LODTransport.RemoveLODControllerSubAssets(this, false);
                }

                uniqueLODSet = null;
                sharedLODSet = lodSettings;
                LODSet = lodSettings;
                UsingShared = true;
            }
#endif
        }


        /// <summary>
        /// Checking if lod set is right Type
        /// </summary>
        public static bool CheckLODSetCorrectness(ScrOptimizer_LODSettings lodSet, ILODInstance referenceLOD)
        {
            if (lodSet.LevelOfDetailSets.Count == 0)
            {
                Debug.LogError("[OPTIMIZERS] LOD Set is empty");
                return false;
            }

            if (lodSet.LevelOfDetailSets[0] == null)
            {
                Debug.LogError("[OPTIMIZERS] LOD Set element is null");
                return false;
            }

            Type setType = lodSet.LevelOfDetailSets[0].GetLODInstance().GetType();

            if (setType == referenceLOD.GetType())
            {
                return true;
            }
            else
            {
                Debug.LogError("[OPTIMIZERS] Type of LODSet is uncorrect! (<color=red><b>" + setType.ToString() + "</b></color>) You need <color=blue><b>" + referenceLOD.GetType().ToString() + "</b></color> type");
                return false;
            }
        }


        /// <summary>
        /// Setting unique/individual LOD Settings reference to be used only for this one Game Object
        /// </summary>
        public void SetUniqueLODSettings(ScrOptimizer_LODSettings lodSettings)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[OPTIMIZERS] No allowed in playmode!");
                return;
            }

            if (lodSettings == null)
            {
                Debug.LogError("[OPTIMIZERS] Target lod settings cannot be null!");
                return;
            }

            sharedLODSet = null;
            uniqueLODSet = lodSettings;

            LODSet = lodSettings;
            LODSet.name = "LOD Set-" + optimizer.name;
            UsingShared = false;
        }


        #endregion



        protected override void CheckAndGenerateLODParameters()
        {
#if UNITY_EDITOR
            if (RootReference == null)
            {
                Debug.LogError("[OPTIMIZERS] CRITICAL ERROR: There is no root reference in Optimizer's LOD Controller!(" + optimizer + ") " + "Try adding Optimizers Manager again to the scene or import newest version from the Asset Store!");
            }

            if (LODSet != null)
            {
                if (LODSet.LevelOfDetailSets.Count == optimizer.LODLevels + 2)
                {
                    // Checking if there are null references inside LOD Set List
                    for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
                        if (LODSet.LevelOfDetailSets[i] == null)
                        {
                            Optimizers_LODTransport.RemoveLODControllerSubAssets(this, true);
                            break;
                        }
                }


                if (RootReference)
                {
                    // Checking again count in case if it was cleared in previous lines of code
                    if (LODSet.LevelOfDetailSets.Count != optimizer.LODLevels + 2)
                        RootReference.GetLODInstance().DrawingVersion = Version;
                    {
                        for (int i = 0; i < optimizer.LODLevels + 2; i++)
                        {
                            ScrLOD_Base newParam = RootReference.GetScrLODInstance();
                            newParam.GetLODInstance().DrawingVersion = Version;
                            LODSet.LevelOfDetailSets.Add(newParam);
                        }
                    }
                }


                if (UsingShared) // Saving sub LOD settings inside LODSet - shared settings asset
                    Optimizers_LODTransport.SaveLODSetInAsset(this, LODSet);
                else
                {
                    UnityEngine.Object prefab = Optimizers_LODTransport.GetPrefab(optimizer.gameObject);

                    if (prefab) // Saving sub LOD settings inside prefab
                    {
                        Optimizers_LODTransport.SaveLODSetInAsset(this, prefab);
                    }
                    else // Saving sub LOD settings inside LODSet - scene object without prefab
                    {
                        Optimizers_LODTransport.SaveLODSetInAsset(this, LODSet);
                    }
                }

                RefreshOptimizerLODCount();
            }
            else
            {
                Debug.LogWarning("[OPTIMIZERS] No LODSet!");
            }
#endif
        }



        public bool LostRequiredReferences()
        {
            if (RootReference == null) return true;
            if (uniqueLODSet == null && sharedLODSet == null) return true;
            return false;
        }


        protected override void Editor_ValuesChanged()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(LODSet);
            for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
                EditorUtility.SetDirty(LODSet.LevelOfDetailSets[i]);
#endif
        }


        #region Saving scriptable files



        #region Saving LOD Sets

        public static string pathTo = "";
        public ScrOptimizer_LODSettings SaveLODSet()
        {
            ScrOptimizer_LODSettings newLODSet = null;

            string type = "";
            string nameShort = "";

            if (RootReference != null)
            {
                if (Component == null)
                {
                    nameShort = optimizer.name;
                    type = RootReference.GetType().ToString();
                    type = type.Replace("FIMSpace.FOptimizing.", "");
                    type = type.Replace("LOD_", "");
                    type = type.Replace("FLOD_", "");
                    int dotIndex = type.LastIndexOf('.') + 1;
                    type = type.Substring(dotIndex, type.Length - dotIndex);
                }
                else
                {
                    nameShort = Component.name;
                    type = Component.GetType().ToString();
                    int dotIndex = type.LastIndexOf('.') + 1;
                    type = type.Substring(dotIndex, type.Length - dotIndex);
                }

                nameShort = nameShort.Replace("PR_", "");
                nameShort = nameShort.Replace("PR.", "");
                nameShort = nameShort.Substring(0, Mathf.Min(11, nameShort.Length));
            }


#if UNITY_EDITOR

            if (pathTo == "")
            {
                if (PlayerPrefs.HasKey("FOPT_LastLSDir"))
                {
                    pathTo = PlayerPrefs.GetString("FOPT_LastLSDir");
                    if (!System.IO.Directory.Exists(pathTo)) pathTo = Application.dataPath;
                }
                else pathTo = Application.dataPath;
            }

            if (!pathTo.Contains(Application.dataPath))
            {
                pathTo = Application.dataPath;
            }

            string path = EditorUtility.SaveFilePanelInProject("Generate LOD Type Settings File (Can be overwritten)", "LS_" + type + "-" + nameShort + " (" + LODLevelsCount + " LODs)", "asset", "Enter name of file which will contain settings for L.O.D. of different components", pathTo);

            if (!pathTo.Contains(Application.dataPath))
            {
                pathTo = Application.dataPath;
            }

            try
            {
                if (path != "")
                {
                    pathTo = Application.dataPath + "/" + System.IO.Path.GetDirectoryName(path).Replace("Assets/", "").Replace("Assets", "");
                    PlayerPrefs.SetString("FOPT_LastLSDir", pathTo);


                    if (File.Exists(path))
                    {
                        ScrOptimizer_LODSettings selected = (ScrOptimizer_LODSettings)AssetDatabase.LoadAssetAtPath(path, typeof(ScrOptimizer_LODSettings));
                        if (selected != null)
                        {
                            if (selected.IsTheSame(LODSet))
                            {
                                SetSharedLODSettings(selected);
                                return selected;
                            }
                        }
                    }

                    newLODSet = ScrOptimizer_LODSettings.CreateInstance<ScrOptimizer_LODSettings>();
                    AssetDatabase.CreateAsset(newLODSet, path);

                    if (newLODSet != null)
                    {
                        for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
                        {
                            ScrLOD_Base lodCopy = LODSet.LevelOfDetailSets[i].CreateNewScrCopy();
                            AssetDatabase.AddObjectToAsset(lodCopy, newLODSet);
                            newLODSet.LevelOfDetailSets.Add(lodCopy);
                        }

                        SetSharedLODSettings(newLODSet);
                    }
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogError("[OPTIMIZERS] Something went wrong when creating LOD Set in your project. That's probably because of permissions on your hard drive.\n" + exc.ToString());
            }
#endif
            return newLODSet;
        }


#if UNITY_EDITOR
        /// <summary>
        /// Checking if there is shared LOD set and if settings are updated
        /// </summary>
        internal void OnValidate()
        {
            if (!optimizer) return;
            if (!optimizer.enabled) return;
            if (Application.isPlaying)
            {
                return;
            }

            CheckAssetStructureCorrectness();

            if (sharedLODSet)
            {
                if (LODSet.LevelOfDetailSets != sharedLODSet.LevelOfDetailSets)
                    SetSharedLODSettings(sharedLODSet);

                if (optimizer.LODLevels != sharedLODSet.LevelOfDetailSets.Count - 2)
                    optimizer.LODLevels = sharedLODSet.LevelOfDetailSets.Count - 2;
            }

            if (LODSet != null)
                if (LODSet.LevelOfDetailSets != null)
                    if (LODSet.LevelOfDetailSets.Count > 1)
                        for (int i = 1; i < LODSet.LevelOfDetailSets.Count; i++)
                        {
                            if (LODSet.LevelOfDetailSets[i] == null) continue;
                            LODSet.LevelOfDetailSets[i].GetLODInstance().AssignToggler(LODSet.LevelOfDetailSets[0].GetLODInstance());
                        }
        }
#endif

#endregion


        public void CheckAssetStructureCorrectness()
        {
            if (!RootReference)
            {
                Debug.LogError("[OPTIMIZERS] CRITICAL ERROR: There is no root reference in Optimizer's LOD Controller! Try adding Optimizers Manager again to the scene or import newest version from the Asset Store!");
                return;
            }

#if UNITY_EDITOR


#if UNITY_2018_3_OR_NEWER
            if (optimizer.gameObject.scene.rootCount == 0)
            {
                // It's hilarious what I need to do to make Unity cooperate with this saving scriptable objects operations
                // I remembering path to asset inside internal memory so I can get it's reference inside prefab mode

                UnityEngine.Object prefab = Optimizers_LODTransport.GetPrefab(optimizer.gameObject);
                if (prefab)
                {
                    PlayerPrefs.SetString("optim_lastEditedPrefabPath", AssetDatabase.GetAssetPath(prefab));
                }
            }

            // Using remembered path to apply settings from prefab mode to target prefab
            if (optimizer.gameObject.scene.rootCount == 1)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefs.GetString("optim_lastEditedPrefabPath"));
                if (prefab)
                {
                    Transform myRoot = optimizer.transform;

                    Transform parentRoot = optimizer.transform.parent;
                    while (parentRoot != null)
                    {
                        myRoot = parentRoot;
                        parentRoot = myRoot.parent;
                    }

                    if (prefab.name == myRoot.name)
                    {
                        Optimizers_LODTransport.SaveLODSetInAsset(this, prefab);
                    }
                }
            }
#endif


            // When it's not editor mode or prefab mode - just scene object
            if (optimizer.gameObject.scene.rootCount > 1)
            {
                Optimizers_LODTransport.SyncSceneOptimizerWithPrefab(sOptimizer, this);
            }

            // If optimizer lost references to LOD Sets, we will generate new LOD settings
            // Or when we create prefab from scene object
            bool referencesLost = false;
            if (LODSet == null) if (LostRequiredReferences()) referencesLost = true;

            if (referencesLost)
            {
                // If it's optimizer inside prefab asset
                // That means there was sprobably just created prefab from scene object
                if (optimizer.gameObject.scene.rootCount == 0)
                {
                    Optimizers_LODTransport.SaveLODSettingsFromSceneOptimizer(this);
                }
            }
#endif
        }

        #endregion

    }
}
