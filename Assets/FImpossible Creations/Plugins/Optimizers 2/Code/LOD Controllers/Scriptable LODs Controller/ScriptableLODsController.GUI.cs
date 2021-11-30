#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;

namespace FIMSpace.FOptimizing
{
    public partial class ScriptableLODsController
    {
        public void CheckForMultiAssigning()
        {
            bool can = false;
            Type type = Component.GetType();

            for (int i = 0; i < sOptimizer.ToOptimize.Count; i++)
            {
                if (sOptimizer.ToOptimize[i] == this) continue;
                if (sOptimizer.ToOptimize[i].Component.GetType() == type)
                {
                    if (sOptimizer.ToOptimize[i].GetSharedSet() != sharedLODSet)
                    {
                        can = true;
                        break;
                    }
                }
            }

            if (can)
                if (GUILayout.Button(new GUIContent("Set All", "Setting this LOD settings to all components of the same type"), new GUILayoutOption[2] { GUILayout.Width(50), GUILayout.Height(15) }))
                {
                    for (int i = 0; i < sOptimizer.ToOptimize.Count; i++)
                    {
                        if (sOptimizer.ToOptimize[i] == this) continue;
                        if (sOptimizer.ToOptimize[i].Component.GetType() == type)
                        {
                            if (sOptimizer.ToOptimize[i].GetSharedSet() != sharedLODSet)
                                sOptimizer.ToOptimize[i].SetSharedLODSettings(sharedLODSet);
                        }
                    }
                }
        }


        protected override void GUI_LODSettingHeader(ILODInstance iflod, int selectedLOD)
        {
            if (!sOptimizer.SaveSetFilesInPrefab)
            {
                var serializedOptimizer = new SerializedObject(optimizer);

                EditorGUILayout.BeginHorizontal();
                ScrOptimizer_LODSettings pre = sharedLODSet;
                ScrOptimizer_LODSettings tempShared = (ScrOptimizer_LODSettings)EditorGUILayout.ObjectField("Shared LOD Set", sharedLODSet, typeof(ScrOptimizer_LODSettings), false);

                bool button = false;

                if (!sharedLODSet)
                {
                    if (GUILayout.Button(new GUIContent("New", "Generate new LOD set file basing on current settings in optimizer component."), new GUILayoutOption[2] { GUILayout.Width(40), GUILayout.Height(15) }))
                    {
                        sharedLODSet = SaveLODSet();
                        button = true;
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("X", "Remove shared LOD so settings for this component will be unique"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(15) })) { sharedLODSet = null; button = true; }
                    CheckForMultiAssigning();
                }

                // If used button for "New" or "X"
                if (button)
                {
                    if (sharedLODSet != null)
                    {
                        if (LODSet != sharedLODSet)
                        {
                            if (CheckLODSetCorrectness(sharedLODSet, ReferenceLOD))
                                SetSharedLODSettings(sharedLODSet);
                            else
                                SetSharedLODSettings(null);
                        }
                    }
                    else
                        if (sharedLODSet == null)
                        if (uniqueLODSet == null)
                            SetSharedLODSettings(null);
                }
                else // when used object field
                {
                    if (tempShared != null)
                    {
                        if (pre != tempShared)
                        {
                            if (CheckLODSetCorrectness(tempShared, ReferenceLOD))
                            {
                                optimizer.LODLevels = tempShared.LevelOfDetailSets.Count - 2;
                                sharedLODSet = tempShared;
                                SetSharedLODSettings(sharedLODSet);
                                serializedOptimizer.ApplyModifiedProperties();
                                new SerializedObject(sharedLODSet).ApplyModifiedProperties();
                            }
                            else
                            {
                                tempShared = pre; // No change
                            }
                        }
                    }
                    else
                    {
                        if (sharedLODSet == null)
                            if (uniqueLODSet == null)
                            {
                                SetSharedLODSettings(null);
                            }
                    }
                }

                EditorGUILayout.EndHorizontal();

                serializedOptimizer.ApplyModifiedProperties();
            }
        }


        internal SerializedObject GetReferenceLODContainer()
        {
            return new SerializedObject(LODSet.LevelOfDetailSets[0]);
        }

        protected override SerializedObject GetSerializedObject()
        {
            return new SerializedObject(sOptimizer);
        }

        protected override SerializedProperty GetSerializedLODPropertyFor(int lod)
        {
            return new SerializedObject(LODSet.LevelOfDetailSets[lod]).FindProperty("settings");
        }

    }
}
#endif
