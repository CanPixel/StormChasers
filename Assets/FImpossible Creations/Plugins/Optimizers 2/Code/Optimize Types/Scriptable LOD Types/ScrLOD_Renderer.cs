using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scriptable container for IFLOD
    /// </summary>
    //[CreateAssetMenu(menuName = "Custom Optimizers/FLOD_Renderer Reference")]
    public sealed class ScrLOD_Renderer : ScrLOD_Base
    {
        [SerializeField]
        private LODI_Renderer settings;
        public override ILODInstance GetLODInstance() { return settings; }
        public ScrLOD_Renderer() { settings = new LODI_Renderer(); }

        public override ScrLOD_Base GetScrLODInstance()
        {
            return CreateInstance<ScrLOD_Renderer>();
        }

        public override ScrLOD_Base CreateNewScrCopy()
        {
            ScrLOD_Renderer newR = CreateInstance<ScrLOD_Renderer>();
            //newR.CopyBase(this);
            //newR.UseShadows = UseShadows;
            //newR.ShadowsCast = ShadowsCast;
            //newR.ShadowsReceive = ShadowsReceive;
            //newR.MotionVectors = MotionVectors;
            //newR.SkinnedQuality = SkinnedQuality;
            return newR;
        }


        /// <summary>
        /// Assign this LOD type to FOptimizers_Manager
        /// </summary>
        public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
        {
            Renderer rend = target as Renderer;
            if (!rend) rend = target.GetComponent<Renderer>();
            if (rend) if (!optimizer.ContainsComponent(rend))
                {
                    SkinnedMeshRenderer skinned = rend as SkinnedMeshRenderer;
                    if (skinned)
                    {
                        if (optimizer.ToOptimize != null)
                        {
                            bool hadLight = false;
                            for (int i = 0; i < optimizer.ToOptimize.Count; i++)
                                if (optimizer.ToOptimize[i].Component is Light)
                                {
                                    hadLight = true;
                                    break;
                                }

                            if (!hadLight)
                            {
                                optimizer.DetectionRadius = skinned.bounds.extents.magnitude;
                                optimizer.DetectionBounds = skinned.bounds.size * 1.2f;

                                if (optimizer.DetectionOffset == Vector3.zero)
                                    optimizer.DetectionOffset = skinned.transform.InverseTransformPoint(skinned.bounds.center);
                            }
                        }

                        return new ScriptableLODsController(optimizer, skinned, -1, "Skinned Renderer", this);
                    }
                    else
                    {
                        MeshRenderer mesh = rend as MeshRenderer;
                        if (mesh)
                        {
                            if (optimizer.ToOptimize != null)
                            {
                                bool hadLight = false;
                                for (int i = 0; i < optimizer.ToOptimize.Count; i++)
                                    if (optimizer.ToOptimize[i].Component is Light)
                                    {
                                        hadLight = true;
                                        break;
                                    }

                                if (!hadLight)
                                {
                                    float scaler = Optimizer_Base.GetScaler(optimizer.transform); if (scaler == 0f) scaler = 1f;
                                    optimizer.DetectionRadius = mesh.bounds.extents.magnitude / scaler;
                                    optimizer.DetectionBounds = (mesh.bounds.size * 1.05f) / scaler;

                                    if (optimizer.DetectionOffset == Vector3.zero)
                                        optimizer.DetectionOffset = mesh.transform.InverseTransformPoint(mesh.bounds.center);
                                }
                            }

                            return new ScriptableLODsController(optimizer, mesh, -1, "MeshRenderer", this);
                        }
                    }
                }

            return null;
        }

    }
}
