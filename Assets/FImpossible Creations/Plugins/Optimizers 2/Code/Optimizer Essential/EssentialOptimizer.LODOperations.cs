using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class EssentialOptimizer
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
                for (int i = 0; i < ToOptimize.Count; i++)
                {
                    if (ToOptimize[i].GetLODSettingsCount() == 0)
                        ToOptimize[i].GenerateLODParameters();
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