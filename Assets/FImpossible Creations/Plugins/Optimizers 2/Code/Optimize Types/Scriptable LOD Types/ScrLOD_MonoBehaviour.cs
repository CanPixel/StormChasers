using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scriptable container for IFLOD
    /// </summary>
    //[CreateAssetMenu(fileName = "MonoBehaviour Reference - Move it to Resources - Optimizers - Custom", menuName = "Custom Optimizers/FLOD_MonoBehaviour Reference")]
    public sealed class ScrLOD_MonoBehaviour : ScrLOD_Base
    {
        [SerializeField]
        private LODI_MonoBehaviour settings;
        public override ILODInstance GetLODInstance() { return settings; }
        public ScrLOD_MonoBehaviour() { settings = new LODI_MonoBehaviour(); }

        public override ScrLOD_Base GetScrLODInstance()
        {
            return CreateInstance<ScrLOD_MonoBehaviour>();
        }

        public override ScrLOD_Base CreateNewScrCopy()
        {
            ScrLOD_MonoBehaviour newMono = CreateInstance<ScrLOD_MonoBehaviour>();
            //newMono.CopyBase(this);
            //newMono.Parameters = new List<ParameterHelper>();

            //if ( Parameters != null)
            //for (int i = 0; i < Parameters.Count; i++)
            //{
            //    ParameterHelper helper = new ParameterHelper(Parameters[i].ParamName, Parameters[i].ParamType);
            //    helper.SetValue(Parameters[i].TypeID, Parameters[i].GetValue(Parameters[i].TypeID));
            //    helper.Change = Parameters[i].Change;
            //    newMono.Parameters.Add(helper);
            //}

            return newMono;
        }


        public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            MonoBehaviour m = target as MonoBehaviour;
            if (!m) m = target.GetComponentInChildren<MonoBehaviour>();
            if (m) if (!optimizer.ContainsComponent(m))
                {
                    return new ScriptableLODsController(optimizer, m, -1, "Custom Component", this);
                }

            return null;
        }

    }
}
