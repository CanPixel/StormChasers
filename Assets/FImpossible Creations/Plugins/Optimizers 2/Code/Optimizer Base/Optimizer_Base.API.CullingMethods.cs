using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FC: Scheme for getting LOD settings into use
    /// It depends on inherited class how it's done technically
    /// </summary>
    public abstract partial class Optimizer_Base
    {

        /// <summary>
        /// Initializing component with default variables values for correct starting operations
        /// </summary>
        protected virtual void StartVariablesRefresh()
        {
            manager = null;

            CurrentDynamicDistanceCategory = null;

            DynamicListIndex = 0;

            TransitionNextLOD = 0;
            TransitionPercent = -1f;

            ContainerGeneratedID = Optimizers_CullingContainer.GetId(GetDistanceMeasures());

            IsCulled = false;
            IsHidden = false;
        }



        /// <summary>
        /// Preparing initial data for culling
        /// </summary>
        protected virtual void InitBaseCullingVariables(Camera targetCamera)
        {
            OutOfDistance = true;
            OutOfCameraView = true;
            WasOutOfCameraView = false;
            IsHidden = false;
            WasHidden = false;
            CurrentLODLevel = 0;
            CurrentDistanceLODLevel = 0;

            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null)
            {
                if (FEditor_OneShotLog.CanDrawLog("optC", 16)) Debug.LogWarning("[OPTIMIZERS] There is no main camera on scene!");
            }
            else
                this.TargetCamera = targetCamera.transform;
        }


        /// <summary>
        /// Refreshing visibility state when something changed in optimizer system for this object
        /// </summary>
        protected void RefreshVisibilityState(int targetLODLevel)
        {
            if (enabled == false) return;

            bool shouldBeCulled = false;
            bool fastCull = false;
            bool hide = false;

            CurrentDistanceLODLevel = targetLODLevel;

            if (OutOfDistance)
            {
                shouldBeCulled = true;
            }
            else
            {
                // TODO MAYBE: Checking without else? Then transition will end when we go out of distance and look away
                if (CullIfNotSee) if (OutOfCameraView) fastCull = true;
                if (!fastCull) if (IsHidden) fastCull = true;

                if (fastCull) // Out of camera view or setted to hidden
                {
                    if (HiddenCullAt < 0)
                    {
                        shouldBeCulled = true;
                    }
                    else
                        if (targetLODLevel < HiddenCullAt + 1)
                    {
                        targetLODLevel = LODLevels + 1;
                        hide = true;
                    }
                    else
                    {
                        shouldBeCulled = true;
                    }
                }
                else // In camera range or unhidden and not out of distance
                {
                    if (WasOutOfCameraView) fastCull = true;
                }
            }


            if (!shouldBeCulled)
            {
                if (!IsHidden && WasHidden) fastCull = true;
            }

            if (fastCull)
            {
                if (TransitionPercent >= 0f)
                {
                    OptimizersManager.Instance.EndTransition(this);
                }
            }

            if (IsCulled && shouldBeCulled)
            { }
            else
            {
                // Executing culling procedures basing on computed behviour from culling event
                if (doFirstCull) // Instant setting LOD levels when object initializes
                {
                    if (shouldBeCulled)
                    {
                        ChangeLODLevelTo(LODLevels);
                    }
                    else
                        ChangeLODLevelTo(targetLODLevel);

                    doFirstCull = false;
                }
                else // Culling state changes during playmode
                {
                    if (CullIfNotSee) // If should check for deactivate when object is not visible in camera frustum
                    {
                        if (FadeViewVisibility) fastCull = false;

                        if (fastCull) // Instant change to LOD level when camera looking away or just looked on object by frustum
                        {
                            if (shouldBeCulled) // Culling when camera looking away
                            {
                                SetCulled(true);
                            }
                            else // Unculling when camera sees object (only frustum)
                            {
                                // If there is no transition
                                if (TransitionPercent < 0f || hide) ChangeLODLevelTo(targetLODLevel); // Hidden change of LOD level
                                if (!OutOfDistance) SetCulled(false, true); // If we aren't out of allowed distance we can uncull
                            }
                        }
                        else // Normal change or transitiong to LOD level
                        {
                            if (shouldBeCulled)
                            {
                                if (FadeDuration > 0f)
                                {
                                    if (!OutOfDistance)
                                        TransitionOrSetLODLevel(targetLODLevel);
                                    else
                                        TransitionOrSetLODLevel(LODLevels);
                                }
                                else
                                    TransitionOrSetLODLevel(LODLevels);
                            }
                            else
                            {
                                if (FadeDuration <= 0f)
                                {
                                    SetLODLevel(targetLODLevel);
                                    SetCulled(false);
                                }
                                else
                                {
                                    TransitionOrSetLODLevel(targetLODLevel);
                                    SetCulled(false, false);
                                }
                            }
                        }
                    }
                    else // If object should be culled only when is out of distance, no matter where camera is looking
                    {
                        if (shouldBeCulled)
                            TransitionOrSetLODLevel(LODLevels);
                        else
                        {
                            TransitionOrSetLODLevel(targetLODLevel);
                            SetCulled(false);
                        }
                    }
                }
            }

            WasOutOfCameraView = OutOfCameraView;
            WasHidden = IsHidden;
        }


        /// <summary>
        /// Setting new LOD level for optimized components or triggering transition if speed value is greater than 0
        /// </summary>
        protected virtual void TransitionOrSetLODLevel(int lodLevel)
        {
            if (FadeDuration <= 0f)
            {
                SetLODLevel(lodLevel); // No transitioning, just assigning parameters
            }
            else
            {
                if (lodLevel != CurrentLODLevel || IsCulled || TransitionPercent != -1)
                {
                    if (lodLevel > LODLevels) // Transition to culling
                    {
                        OptimizersManager.Instance.TransitionTo(this, LODLevels, FadeDuration);
                    }
                    else // Transition to other LOD level
                    {
                        OptimizersManager.Instance.TransitionTo(this, lodLevel, FadeDuration);
                    }
                }
            }
        }


        /// <summary>
        /// Setting object as hidden, applying hidden LOD settings or culled if range for culling is defined.
        /// Use this method for example when you detect through custom coding if object is behind the wall or so.
        /// </summary>
        public void SetHidden(bool hide)
        {
            if (hide != IsHidden)
            {
#if UNITY_EDITOR
                if (hide)
                    OptimizersManager.HiddenObjects++;
                else
                    OptimizersManager.HiddenObjects--;
#endif
                IsHidden = hide;
                RefreshVisibilityState(CurrentDistanceLODLevel);
            }
        }

        /// <summary>
        /// Culling object or unculling - applying current distance LOD level
        /// If it's distance based culling, not camera frustum dictated, then we do transition if enabled
        /// </summary>
        internal virtual void SetCulled(bool culled = true, bool apply = true)
        {
            if (culled) if (IsCulled == culled) return;

            IsCulled = culled;

            if (culled) // Culling object
            {
                AllLODComponents_ApplyCulledState();

                if (DeactivateObject)
                {
                    OnActivationChange(false);
                    gameObject.SetActive(false);
                }
            }
            else // Making object visible
            {
                if (DeactivateObject) if (!gameObject.activeInHierarchy)
                    {
                        OnActivationChange(true);
                        gameObject.SetActive(true);
                    }

                if (apply) AllLODComponents_ApplyCurrentState();
            }
        }


        protected abstract void AllLODComponents_ApplyCulledState();
        protected abstract void AllLODComponents_ApplyCurrentState();
        protected abstract void AllLODComponents_RefreshChoosedLODState(int lodLevel);
        protected abstract void AllLODComponents_ChangeChoosedLODState(int lodLevel);


        /// <summary>
        /// Applying LOD settings for optimized components
        /// </summary>
        internal virtual void SetLODLevel(int lodLevel)
        {
            if (lodLevel == LODLevels) // Culling UnityEngine.Object
            {
                SetCulled(true);
                CurrentLODLevel = lodLevel;
            }
            else // Setting defined LOD level to know which settings should be applied when object is visible
            {
                CurrentLODLevel = lodLevel;
                AllLODComponents_RefreshChoosedLODState(lodLevel);
            }
        }


        /// <summary>
        /// Applying LOD settings for optimized components
        /// </summary>
        internal virtual void ChangeLODLevelTo(int lodLevel)
        {
            CurrentLODLevel = Mathf.Min(lodLevel, LODLevels + 2);

            AllLODComponents_ChangeChoosedLODState(lodLevel);

            bool cullIt = false;

            if (lodLevel >= LODLevels)
            {
                if (lodLevel == LODLevels + 1) cullIt = false;
                else
                    cullIt = true;
            }

            if (cullIt)
                CullOrUncullObject(true);
            else
                CullOrUncullObject(false);
        }


        /// <summary>
        /// Culling object or unculling - applying current distance LOD level
        /// If it's distance based culling, not camera frustum dictated, then we do transition if enabled
        /// </summary>
        internal virtual void CullOrUncullObject(bool cull = true)
        {
            if (IsCulled == cull) return;
            IsCulled = cull;

            if (cull)
            {
                if (DeactivateObject) if (gameObject.activeInHierarchy)
                    {
                        OnActivationChange(false);
                        gameObject.SetActive(false);
                    }
            }
            else
            {
                if (DeactivateObject) if (!gameObject.activeInHierarchy)
                    {
                        OnActivationChange(true);
                        gameObject.SetActive(true);
                    }
            }
        }

    }
}
