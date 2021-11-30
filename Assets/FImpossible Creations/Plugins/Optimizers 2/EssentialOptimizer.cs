using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Essential Optimizer is holding optimization data within serialized MonoBehaviour 
    /// instead of ScriptableObjects giving better flexibility but not allowing for custom
    /// optimization types implementation, unless it can be done through inheriting new monobehaviour from this class
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Essential Optimizer", 0)]
    public partial class EssentialOptimizer : Optimizer_Base, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        #region Hierarchy Icon

        public string EditorIconPath { get { if (PlayerPrefs.GetInt("OptH", 1) == 0) return ""; else return "FIMSpace/Optimizers 2/OptEsIconSmall"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        #endregion

        /// <summary> 
        /// List of component's to optimize and their LOD Settings inside each instance of LODs Controller 
        /// </summary>
        public List<EssentialLODsController> ToOptimize;


        protected override LODsControllerBase AddToOptimize(LODsControllerBase lod)
        {
#if UNITY_EDITOR
            ToOptimize.Add(lod as EssentialLODsController);
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
            if (targetLODLevel >= ToOptimize[i].GetLODSettingsCount()) return null;
            return ToOptimize[i].GetLODSetting(targetLODLevel);
        }

        internal override void RemoveToOptimize(LODsControllerBase lODsControllerBase)
        {
            for (int i = ToOptimize.Count - 1; i >= 0; i--)
            {
                LODsControllerBase lod = ToOptimize[i] as LODsControllerBase;
                if (lod == null)
                {
                    ToOptimize.RemoveAt(i);
                    continue;
                }

                if (lod == lODsControllerBase)
                {
                    ToOptimize.RemoveAt(i);
                    return;
                }
            }
        }

        public override bool OptimizationListExists()
        {
            return ToOptimize != null;
        }
    }
}