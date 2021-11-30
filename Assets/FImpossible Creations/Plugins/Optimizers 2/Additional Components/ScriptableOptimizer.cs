using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Universal class for base type of optimization basing on 
    /// sphere visibility detection (Culling Groups api) and others.
    /// (All code is inside FOptimizer_Base class)
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Scriptable Optimizer", 1)]
    public partial class ScriptableOptimizer : Optimizer_Base, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        #region Hierarchy Icon


        public string EditorIconPath { get { if (PlayerPrefs.GetInt("OptH", 1) == 0) return ""; else return "FIMSpace/Optimizers 2/OptIconSmall"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        #endregion

        /// <summary> For editor safety in unity 2020 </summary>
        public override bool OptimizationListExists() { return ToOptimize != null; }

        [Tooltip("If scriptable files 'LOD sets' should be saved inside prefab file as sub-assets.\nThis is more comfortable but Unity have big trouble in good serving them this way so it's recommended to save scriptable files inside project directory to avoid any issues.\n\nWith this option disabled you will see 'Shared LOD Set' parameter when you unfold some component LOD settings and you can save LOD Set file with 'New' button.")]
        public bool SaveSetFilesInPrefab = true;

        /// <summary> 
        /// List of component's to optimize and their LOD Settings inside each instance of LODs Controller
        /// Scriptable LODs controller is using scriptable objects to support any type of component without changing Optimizers core code 
        /// </summary>
        public List<ScriptableLODsController> ToOptimize;

        protected override LODsControllerBase AddToOptimize(LODsControllerBase lod)
        {
#if UNITY_EDITOR
            ToOptimize.Add(lod as ScriptableLODsController);
            lod.GenerateLODParameters();
            lod.ToOptimizeIndex = ToOptimize.Count;
            return lod;
#else
            return null;
#endif
        }


        public override Component GetOptimizedComponent(int i)
        {
            if (i >= ToOptimize.Count) return null;
            return ToOptimize[i].Component;
        }

        internal override ILODInstance GetLODInstance(int i, int targetLODLevel)
        {
            if (i >= ToOptimize.Count) return null;
            if (targetLODLevel >= ToOptimize[i].LODSet.LevelOfDetailSets.Count) return null;
            return ToOptimize[i].LODSet.LevelOfDetailSets[targetLODLevel].GetLODInstance();
        }

        internal override void RemoveToOptimize(LODsControllerBase lODsControllerBase)
        {
            for (int i = ToOptimize.Count-1; i >= 0; i--)
            {
                LODsControllerBase lod = ToOptimize[i] as LODsControllerBase;
                if ( lod == null)
                {
                    ToOptimize.RemoveAt(i);
                    continue;
                }

                if ( lod == lODsControllerBase)
                {
                    ToOptimize.RemoveAt(i);
                    return;
                }
            }
        }
    }
}