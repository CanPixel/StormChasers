using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor : Editor
    {
        [CanEditMultipleObjects]
        [CustomEditor(typeof(EssentialOptimizer))]
        public class EssentialOptimizerEditor : Optimizer_BaseEditor
        {
            protected override string TargetName() { return " Essential Optimizer"; }
            protected override string TargetTooltip() { return "Optimizer Component which is recommended to use with prefabs especially with nested prefabs.\n\nIt's limited to few optimization components but can be expanded by inheriting but it needs some extra coding and knowledge."; }
            protected override Texture2D GetSmallIcon() { if (__texOptimizersIcon != null) return __texOptimizersIcon; __texOptimizersIcon = Resources.Load<Texture2D>("FIMSpace/Optimizers 2/Optimizers Essential Icon Small"); return __texOptimizersIcon; }

            protected override void OnEnable()
            {
                base.OnEnable();
            }

            private EssentialOptimizer DGet { get { if (_dGet == null) _dGet = target as EssentialOptimizer; return _dGet; } }
            private EssentialOptimizer _dGet;


            protected override void GUI_ToOptimizeHeader()
            {
                GUILayout.Space(3);
                EditorGUILayout.HelpBox("Essential Optimizer supports only optimization for renderers / particle systems / lights / audio sources / nav mesh agents. Use Scriptable Optimizer if you need custom components support.", MessageType.None);
            }


            protected override void FillToOptimizeList()
            {
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
                        if (DGet.ToOptimize[i].GetLODSettingsCount() == 0)
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


        }

    }
}

