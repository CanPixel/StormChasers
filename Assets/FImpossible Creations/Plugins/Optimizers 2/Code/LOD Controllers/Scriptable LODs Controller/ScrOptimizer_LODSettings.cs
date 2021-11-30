using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: LOD set for different types of components to optimize
    /// It contains list of LOD settings for one component
    /// *Edit mode, remembered settings -> FOptimizer_Base
    /// </summary>
    public class ScrOptimizer_LODSettings : ScriptableObject
    {
        /// <summary> All LODs </summary>
        public List<ScrLOD_Base> LevelOfDetailSets;

        public ScrOptimizer_LODSettings()
        {
            LevelOfDetailSets = new List<ScrLOD_Base>();
        }


        public ScrOptimizer_LODSettings CreateCopy()
        {
            ScrOptimizer_LODSettings setts = ScrOptimizer_LODSettings.CreateInstance<ScrOptimizer_LODSettings>();

            for (int i = 0; i < LevelOfDetailSets.Count; i++)
                setts.LevelOfDetailSets.Add(LevelOfDetailSets[i].CreateNewScrCopy());

            return setts;
        }


#if UNITY_EDITOR
        /// <summary>
        /// Checking if LOD Set is the same for file overrite when saving 
        /// </summary>
        internal bool IsTheSame(ScrOptimizer_LODSettings lODSet)
        {
            bool same = true;
            if (lODSet.LevelOfDetailSets.Count != LevelOfDetailSets.Count) same = false;
            if (same) for (int i = 0; i < LevelOfDetailSets.Count; i++) LevelOfDetailSets[i].IsTheSame(lODSet.LevelOfDetailSets[i]);

            return same;
        }
#endif

    }


#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [CustomEditor(typeof(ScrOptimizer_LODSettings))]
    public class Optimizers_LODSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ScrOptimizer_LODSettings targetScript = (ScrOptimizer_LODSettings)target;
            DrawDefaultInspector();

            GUILayout.Space(7f);

            int count = targetScript.LevelOfDetailSets.Count;
            string type = "";
            if (count > 0) type = targetScript.LevelOfDetailSets[0].GetType().ToString() +"\n";

            if ( type != "")
            {
                int dotIndex = type.LastIndexOf('.') + 1;
                type = type.Substring(dotIndex, type.Length - dotIndex);
                EditorGUILayout.LabelField(type, UnityEditor.EditorStyles.boldLabel);
            }

            EditorGUILayout.HelpBox((count - 2) + " LODs [0]Nearest + " + (count - 3) + " custom\n[" + (count - 2) + "]Culled and [" + (count - 1) + "]Hidden LOD coontainer", MessageType.Info);
        }
    }
#endif

}