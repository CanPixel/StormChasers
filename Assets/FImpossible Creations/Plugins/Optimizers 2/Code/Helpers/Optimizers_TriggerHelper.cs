using UnityEngine;

namespace FIMSpace.FOptimizing
{
    [AddComponentMenu("FImpossible Creations/Optimizers 2/Utilities/Optimizers Trigger Helper")]
    public class Optimizers_TriggerHelper : MonoBehaviour
    {
        public Optimizer_Base Optimizer;
        public int TriggerIndex = -1;

        public Optimizers_TriggerHelper Initialize(Optimizer_Base optimizer, int index)
        {
            Optimizer = optimizer;
            TriggerIndex = index;
            return this;
        }

        void OnTriggerEnter(Collider other)
        {
            if (Optimizer == null) { Destroy(gameObject); return; }

            if (other.transform != Optimizer.TargetCamera) return;

            Optimizer.OnTriggerChange(this, false);
        }


        void OnTriggerExit(Collider other)
        {
            if (Optimizer == null) { Destroy(gameObject); return; }
            if (other.transform != Optimizer.TargetCamera) return;

            Optimizer.OnTriggerChange(this, true);
        }

    }
}