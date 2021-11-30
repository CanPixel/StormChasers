using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class ScriptableOptimizer
    {

        public override int GetToOptimizeCount()
        {
            if (ToOptimize == null) return 0; else return ToOptimize.Count;
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            if (ToOptimize.Count == 0) Debug.LogWarning("[Optimizers] There is no object to optimize! (" + name + ")");
#endif

            bool removed = false;
            for (int i = ToOptimize.Count - 1; i >= 0; i--)
                if (ToOptimize[i].Component == null)
                {
                    ToOptimize.RemoveAt(i);
                    removed = true;
                }

            if (removed) Debug.LogWarning("[OPTIMIZERS] Optimizer had saved objects to optimize which are not existing anymore!");


            base.Start();
        }


        public override void EditorUpdate()
        {
            base.EditorUpdate();

            if (ToOptimize != null)
            {
                bool generated = false;
                for (int i = 0; i < ToOptimize.Count; i++)
                {
                    if (ToOptimize[i].LODSet == null)
                    {
                        ToOptimize[i].GenerateLODParameters();
                        generated = true;
                    }
                }

                if (generated)
                {
                    Debug.LogWarning("[OPTIMIZERS EDITOR] LOD Settings generated from scratch for " + name + ". Did you copy and paste objects through scenes? Unity is not able to remember LOD settings for not prefabed objects and to objects without shared settings between scenes like that :/ \n(without prefabing or saving shared settings this settings are scene assets, no object assets)");
                }
            }
        }


        protected override void AllLODComponents_ApplyCulledState()
        {
            for (int i = 0; i < ToOptimize.Count; i++)
            {
                ToOptimize[i].ApplyLODLevelSettings(ToOptimize[i].GetCullingLOD());
            }
        }

        protected override void AllLODComponents_ApplyCurrentState()
        {
            if (ToOptimize.Count == 0) return;
            if (ToOptimize[0].CurrentLODLevel < 0) return;

            for (int i = 0; i < ToOptimize.Count; i++)
            {
                ToOptimize[i].ApplyLODLevelSettings(ToOptimize[i].GetCurrentLOD());
            }
        }

        protected override void AllLODComponents_RefreshChoosedLODState(int lodLevel)
        {
            for (int i = 0; i < ToOptimize.Count; i++)
            {
                ToOptimize[i].SetCurrentLODLevel(lodLevel);
            }
        }

        protected override void AllLODComponents_ChangeChoosedLODState(int lodLevel)
        {
            for (int i = 0; i < ToOptimize.Count; i++)
            {
                ToOptimize[i].SetCurrentLODLevel(CurrentLODLevel);
                ToOptimize[i].ApplyLODLevelSettings(ToOptimize[i].GetCurrentLOD());
            }
        }


        internal override Optimizers_LODTransition GetLodTransitionFor(int i, int targetLODLevel)
        {
            return new Optimizers_LODTransition(ToOptimize[i].Component, ToOptimize[i].GetLODSetting(CurrentLODLevel), ToOptimize[i].GetLODSetting(targetLODLevel), ToOptimize[i].InitialSettings);
        }
    }
}