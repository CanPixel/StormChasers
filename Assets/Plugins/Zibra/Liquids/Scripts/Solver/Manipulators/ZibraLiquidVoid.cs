#if ZIBRA_LIQUID_PAID_VERSION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.zibra.liquid.Manipulators
{
    [AddComponentMenu("Zibra/Zibra Liquid Void")]
    public class ZibraLiquidVoid : Manipulator
    {
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }

        void OnDrawGizmos()
        {
            OnDrawGizmosSelected();
        }

        ZibraLiquidVoid()
        {
            // DataAmount = 4;
            TYPE = ManipulatorType.Void;
        }
    }
}

#endif
