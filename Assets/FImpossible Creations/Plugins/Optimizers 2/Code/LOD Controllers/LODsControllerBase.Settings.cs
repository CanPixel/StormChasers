using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    public partial class LODsControllerBase
    {
        /// <summary>
        /// Taking care of creating LOD Set, LOD Parameters with automatic settings and syncing with prefab if using.
        /// </summary>
        public void GenerateLODParameters()
        {
            if (NeedToReGenerate(optimizer.LODLevels)) // Generating new LOD params containers
                GenerateNewLODSettings();

            CheckAndGenerateLODParameters();

            CheckCoreRequirements();

            RefreshLODAutoParametersSettings();
        }


        /// <summary>
        /// Checking if LOD parameters need to be generated for needed LOD levels count.
        /// If count is invalid to needed one, new LOD parameters are generated.
        /// </summary>
        protected abstract void CheckAndGenerateLODParameters();


        /// <summary>
        /// Checking if LOD controller should generate new LOD Settings
        /// </summary>
        protected virtual bool NeedToReGenerate(int targetCount)
        {
            return GetLODSettingsCount() == 0 || GetLODSettingsCount() - 2 != targetCount;
        }


        /// <summary>
        /// Syncing optimizer component to LOD Set settings count
        /// </summary>
        protected void RefreshOptimizerLODCount()
        {
            if (GetLODSettingsCount() != 0) optimizer.LODLevels = GetLODSettingsCount() - 2;
        }

        /// <summary>
        /// Generating LOD Set file which will contain parameters for each LOD Level
        /// </summary>
        protected abstract void GenerateNewLODSettings();

        /// <summary>
        /// Checking if some references was lost or not (for scriptable optimizer)
        /// </summary>
        protected virtual bool CheckCoreRequirements(bool hard = false)
        {
            return true;
        }

        /// <summary>
        /// Getting universal list of LOD instances
        /// </summary>
        protected virtual List<ILODInstance> GetIFLODList()
        {
            return null;
        }


        /// <summary>
        /// Applying auto settings for all optimizer parameters inside LOD set
        /// </summary>
        public void RefreshLODAutoParametersSettings(float lowerer = 1f)
        {
            string nameShort = optimizer.name;
            nameShort = nameShort.Replace("PR_", "");
            nameShort = nameShort.Replace("PR.", "");
            nameShort = nameShort.Substring(0, UnityEngine.Mathf.Min(5, nameShort.Length)) + "[";

            string type = Component.GetType().ToString();
            type = type.Replace("FIMSpace.FOptimizing.", "");
            type = type.Replace("LOD_", "");
            type = type.Replace("FLOD_", "");

            type = type.Substring(0, UnityEngine.Mathf.Min(6, type.Length)) + "]";

            string prefix = nameShort + type;

            // Nearest LOD Params
            ILODInstance nearestLOD = GetIFLODList()[0];
            nearestLOD.DrawingVersion = Version;
            nearestLOD.AssignSettingsAsForNearest(Component);
            nearestLOD.QualityLowerer = lowerer;
            nearestLOD.Name = prefix + "Nearest";

            // All LODs between - NEAREST ...LODs... and CULLED, HIDDEN
            for (int i = 0; i < optimizer.LODLevels - 1; i++)
            {
                ILODInstance lod = GetIFLODList()[i + 1];
                lod.DrawingVersion = Version;
                lod.QualityLowerer = lowerer;
                lod.AssignAutoSettingsAsForLODLevel(i, optimizer.LODLevels, Component);
                lod.Name = prefix + "LOD" + (i + 1);
            }

            ILODInstance culledLOD = GetIFLODList()[GetIFLODList().Count - 2];
            culledLOD.DrawingVersion = Version;
            culledLOD.QualityLowerer = lowerer;
            culledLOD.AssignSettingsAsForCulled(Component);
            culledLOD.Name = prefix + "Culled";

            ILODInstance hiddenLOD = GetIFLODList()[GetIFLODList().Count - 1];
            hiddenLOD.DrawingVersion = Version;
            hiddenLOD.QualityLowerer = lowerer;
            hiddenLOD.AssignAutoSettingsAsForLODLevel(optimizer.LODLevels - 2, optimizer.LODLevels, Component);
            hiddenLOD.AssignSettingsAsForHidden(Component);
            hiddenLOD.Name = prefix + "Hidden";
        }


        /// <summary>
        /// Applying quality lowerer variable to LOD set parameters
        /// </summary>
        public void AutoQualityLowerer(float lowerer = 1f)
        {
            GetIFLODList()[0].QualityLowerer = lowerer;
            if (!CheckCoreRequirements()) return;

            ILODInstance lod;
            for (int i = 1; i < optimizer.LODLevels; i++)
            {
                lod = GetIFLODList()[i];
                lod.QualityLowerer = lowerer;
                lod.AssignAutoSettingsAsForLODLevel(i - 1, optimizer.LODLevels, Component);
            }

            lod = GetIFLODList()[GetIFLODList().Count - 1];
            lod.QualityLowerer = lowerer;
            lod.AssignSettingsAsForHidden(Component);
            lod.AssignAutoSettingsAsForLODLevel(optimizer.LODLevels - 1, optimizer.LODLevels + 1, Component);
        }

    }
}
