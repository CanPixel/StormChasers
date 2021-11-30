using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableOptimizer))]
    public class ScriptableOptimizerEditor : Optimizer_BaseEditor
    {
        protected override string TargetName() { return " Scriptable Optimizer"; }
        protected override string TargetTooltip() { return "Optimizer component which can be extended for custom Behaviours / Components LOD support.\n\nIt's recommended to use 'Shared Settings' because Unity have troubles with handling LOD files stored within prefab file."; }

        private ScriptableOptimizer DGet { get { if (_dGet == null) _dGet = target as ScriptableOptimizer; return _dGet; } }
        private ScriptableOptimizer _dGet;

        protected SerializedProperty sp_Shared;


        protected override void OnEnable()
        {
            base.OnEnable();
            sp_Shared = serializedObject.FindProperty("SaveSetFilesInPrefab");
        }


        protected override void PreLODGUI()
        {
            EditorGUIUtility.labelWidth = 222;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp_Shared, new GUIContent(sp_Shared.boolValue ? "Save Set Files In Prefab (Not Safe)" : "Save Sets In Prefab (Current:Shared)", sp_Shared.tooltip));


            bool display = false;
            if (!DGet.SaveSetFilesInPrefab)
            {
                for (int i = 0; i < DGet.ToOptimize.Count; i++)
                {
                    if (DGet.ToOptimize[i].GetSharedSet() == null) display = true;
                    if (display) break;
                }

                if (display)
                    EditorGUILayout.HelpBox("Some LOD settings aren't saved!", MessageType.None);
            }
            else
            {
                Object pf = Optimizers_LODTransport.GetPrefab(DGet.gameObject);
                if (pf == null)
                    EditorGUILayout.HelpBox("Not prefabed - LOD settings saved in scene file", MessageType.None);
            }


            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;
        }


        protected override void FillToOptimizeList()
        {
            if (DGet.IsPrefabed())
            {
                EditorGUILayout.HelpBox("Try to not remove/add components in 'To Optimize' list when you're in prefabed object! Do it in prefab file instead or just apply after editing.", MessageType.Warning);
                GUILayout.Space(4);
            }

            for (int i = DGet.ToOptimize.Count - 1; i >= 0; i--)
            {
                if (DGet.ToOptimize[i].Component == null) DGet.ToOptimize.RemoveAt(i);
            }

            if (DGet.ToOptimize.Count == 1)
            {
                if (DGet.ToOptimize[0].Component is Renderer)
                {
                    EditorGUILayout.HelpBox("Using optimizer on just one mesh renderer is not recommended, try using it on more complex objects.", MessageType.Warning);
                    GUI.color = new Color(1f, 0.9f, 0.5f);
                    GUILayout.Space(4);
                }
            }

            if (DGet.ToOptimize.Count > 0) GUILayout.Space(3f);

            base.FillToOptimizeList();
        }


        protected override void OnJustCreated()
        {
#if UNITY_2019_3_OR_NEWER
            // I assume here that with unity 2019.3 user is working with assets pipeline v2

            if (DGet.GetToOptimizeCount() > 0)
                if (DGet.ToOptimize[0].LODSet == null)
                {
                    Debug.Log("<b><color=red>[OPTIMIZERS EDITOR]</color></b> <b>Using Asset Pipeline V2 is having bug of creating prefabs with optimizers or it's because nested prefabs</b> in it by dragging them from scene. Please remove and add again optimizer component on prefab or move 'Lod Levels' slider. (Waiting for bugfix in next versions of asset pipeline v2)");
                }
#else
                        UnityEngine.Object prefab = Optimizers_LODTransport.GetPrefab(DGet.gameObject);

                        if (prefab)
                            if (prefab is GameObject)
                                Optimizers_LODTransport.ClearPrefabFromUnusedOptimizersSubAssets((GameObject)prefab);
#endif
        }


        protected override void PreOptionsStack()
        {
            if (selectedLOD == 0)
                if (!DGet.SaveSetFilesInPrefab)
                {
                    if (DGet.GetToOptimizeCount() > 1)
                    {
                        int notShared = 0;
                        for (int i = 0; i < DGet.ToOptimize.Count; i++)
                            if (!DGet.ToOptimize[i].UsingShared) notShared++;

                        if (notShared > 1)
                            if (GUILayout.Button(new GUIContent("Save all LOD Sets to be Shared (" + DGet.GetToOptimizeCount() + ")", "Generate new LOD set files basing on current settings in optimizer component."), EditorStyles.miniButton))
                                SaveAllLODSets(DGet);
                    }
                }
        }


        protected override void DrawHideProperties()
        {
            for (int i = 0; i < DGet.GetToOptimizeCount(); i++)
            {
                DGet.ToOptimize[i].GUI_HideProperties(true);
            }
        }


        protected override void OnStartGenerateProperties()
        {
            if (DGet.ToOptimize != null)
            {
                bool generated = false;
                for (int i = 0; i < DGet.GetToOptimizeCount(); i++)
                {
                    if (DGet.ToOptimize[i].LODSet == null)
                    {
                        DGet.ToOptimize[i].GenerateLODParameters();
                        generated = true;
                    }
                }

                if (generated)
                {
                    Debug.LogWarning("[OPTIMIZERS EDITOR] LOD Settings generated from scratch for " + DGet.name + ". Did you copy and paste objects through scenes? Unity is not able to remember LOD settings for not prefabed objects and to objects without shared settings between scenes like that :/");
                }
            }
        }


        protected override void DrawLODOptionsFor(int lodID)
        {

            for (int i = 0; i < DGet.ToOptimize.Count; i++)
            {
                if (DGet.ToOptimize[i] == null)
                {
                    DGet.ToOptimize.RemoveAt(i);
                    return;
                }
                else if (DGet.ToOptimize[i].Component == null)
                {
                    DGet.ToOptimize.RemoveAt(i);
                    return;
                }

                DGet.ToOptimize[i].Editor_DrawValues(lodID, DGet.ToOptimize[i].ToOptimizeIndex);
            }
        }


        /// <summary> Scriptable related </summary>
        protected override void LODSettingsStackStart()
        {

            if (DGet.DrawGeneratedPrefabInfo)
            {
                if (DGet.gameObject.scene.rootCount == 0)
                    EditorGUILayout.HelpBox("Creating prefab erased previous settings. Now settings will be stored inside prefab asset but consider using shared settings (Save LOD Set Files).", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox("Optimizer lost LOD settings references. You probably break link to prefab or something, there are generated new reseted settings.", MessageType.Warning);

                EditorGUILayout.BeginHorizontal();

                if (DGet.gameObject.scene.rootCount == 0)
                {
                    if (GUILayout.Button(new GUIContent("I prefer storing settings inside prefab asset", "LOD Levels parameters will be store inside prefab asset")))
                        DGet.DrawGeneratedPrefabInfo = false;

                    if (GUILayout.Button(new GUIContent("Save LOD Sets (" + DGet.ToOptimize.Count + ")", "Save LOD Levels Parameters file in project directory, you can share it with different prefabs"), new GUILayoutOption[2] { GUILayout.Width(135), GUILayout.Height(18) }))
                    {
                        DGet.SaveSetFilesInPrefab = false;
                        DGet.DrawGeneratedPrefabInfo = false;
                        SaveAllLODSets(DGet);
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Ok I know, hide this message")))
                        DGet.DrawGeneratedPrefabInfo = false;
                }

                EditorGUILayout.EndHorizontal();
            }

        }

    }
}

