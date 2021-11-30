using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// Helping transitioning one type of LODs
    /// </summary>
    public class Optimizers_LODTransition
    {
        /// <summary> When transition done it's full work </summary>
        public bool done = false;

        /// <summary> From what LOD level we starting transition </summary>
        public ILODInstance From;
        /// <summary> To which LOD level we will transition </summary>
        public ILODInstance To;

        /// <summary> Temporary LOD class in which we will store transitioned values of parameters </summary>
        private readonly ILODInstance tempLOD;

        /// <summary> Temporary LOD class in which we will store transition start variables if transition was interrupted </summary>
        private ILODInstance breakLOD;

        Component sceneComponent;
        ILODInstance initialLODSettings;


        public Optimizers_LODTransition(Component sceneComp, ILODInstance from, ILODInstance to, ILODInstance initialLODSettingsRef)
        {
            if (!initialLODSettingsRef.SupportingTransitions)
            {
                to.ApplySettingsToTheComponent(sceneComp, initialLODSettingsRef);
                To = null;
                done = true;
            }

            if ( from == null || to == null || initialLODSettingsRef == null)
            {
                Debug.Log("[Optimizers Transitions] Uknown transition data! [" + from + "," + to + "," + initialLODSettingsRef + "]");
                to.ApplySettingsToTheComponent(sceneComp, initialLODSettingsRef);
                To = null;
                done = true;
            }

            else
            {
                From = from;
                tempLOD = from.GetCopy();
                To = to;
                sceneComponent = sceneComp;
                initialLODSettings = initialLODSettingsRef;
            }
        }


        /// <summary>
        /// Breaking transition, saving current component's parameters and fading from them to new LOD
        /// </summary>
        public void BreakCurrentTransition(ILODInstance to)
        {
            done = false;

            if (tempLOD != null)
                breakLOD = tempLOD.GetCopy();
            else
                if (From != null) breakLOD = From.GetCopy();

            From = breakLOD;
            To = to;
        }


        public void Update(float progress, float secondsAfter = 0f)
        {
            if (To == null) return;

            tempLOD.InterpolateBetween(From, To, progress);
            tempLOD.ApplySettingsToTheComponent(sceneComponent, initialLODSettings);

            if (progress >= 1f)
                if (To.Disable)
                {
                    if (To.ToCullDelay <= 0f) done = true;
                    else
                    {
                        if (secondsAfter >= To.ToCullDelay)
                        {
                            done = true;
                        }
                    }
                }
                else
                {
                    done = true;
                }
        }

        public void Finish()
        {
            if (To == null) return;
            done = true;
            To.ApplySettingsToTheComponent(sceneComponent, initialLODSettings);
        }
    }
}
