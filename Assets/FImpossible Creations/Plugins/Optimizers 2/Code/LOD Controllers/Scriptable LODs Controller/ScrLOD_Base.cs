using System;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Base class for LOD settings for single LOD Level
    /// </summary>
    public abstract class ScrLOD_Base : ScriptableObject
    {
        /// <summary>
        /// Returning instance with LOD settings
        /// </summary>
        public abstract ILODInstance GetLODInstance();

        /// <summary>
        /// Doing shallow copy of component's parameters as new instance
        /// </summary>
        public abstract ScrLOD_Base CreateNewScrCopy();// { return (ScrLOD_Base)MemberwiseClone(); }

        /// <summary>
        /// Generating new instance of LOD settings
        /// </summary>
        public abstract ScrLOD_Base GetScrLODInstance();


        /// <summary>
        /// Checking if target component is capable for this LOD class optimization
        /// then creating lod controller for target optimizer
        /// </summary>
        public virtual ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            #region Example code

            //Light light = target as Light;
            //if (!light) light = target.gameObject.GetComponentInChildren<Light>();
            //if (light) if (!optimizer.ContainsComponent(light))
            //    {
            //        return new FComponentLODsController(optimizer, light, "Light Properties", this);
            //    }

            //return null;

            #endregion

            return null;
        }


        #region Editor Stuff


#if UNITY_EDITOR
        /// <summary>
        /// Used only when replacing files when saving LOD set
        /// </summary>
        internal virtual bool IsTheSame(ScrLOD_Base lod)
        {
            bool same = true;

            Type t = GetType();
            if (t != lod.GetType()) same = false;

            if (same)
                foreach (var prop in t.GetProperties())
                {
                    same = prop.GetValue(this, null).Equals(prop.GetValue(lod, null));
                    if (!same) break;
                }

            return same;
        }
#endif

        #region Backup

        //        /// <summary>
        //        /// Drawing LOD settings properties to be changed inside inspector window
        //        /// </summary>
        //        public virtual void EditorWindow()
        //        {

        //#if UNITY_EDITOR
        //            // Inspector window variables fields
        //#endif

        //        }

        //        /// <summary>
        //        /// Drawing editor gui enabled in first LOD level window (like enabling changing some variables like intensity in light)
        //        /// </summary>
        //        public virtual void DrawTogglers(ScriptableLODsController lodsController)
        //        {
        //            //UnityEditor.EditorGUILayout.BeginVertical(FEditor.FEditor_StylesIn.Style(new Color(0.95f, 0.95f, 0.95f, 0.2f)));
        //            // Your GUI
        //            //UnityEditor.EditorGUILayout.EndVertical();
        //        }

        //        /// <summary>
        //        /// Assigning toggler variables at start
        //        /// </summary>
        //        public virtual void AssignToggler(ScrLOD_Base reference)
        //        {

        //        }
        #endregion

        #endregion

    }

}
