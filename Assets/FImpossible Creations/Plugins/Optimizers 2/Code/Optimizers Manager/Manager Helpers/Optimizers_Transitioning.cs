
namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Helper class to make smooth transitioning between optimizer's optimized types LOD levels for all optimized components by single optimizer
    /// </summary>
    public class Optimizers_Transitioning
    {
        public Optimizer_Base Optimizer;

        /// <summary> ID of transition helper in manager's dictionary </summary>
        public int Id { get; private set; }
        /// <summary> If transition finished it's job </summary>
        public bool Finished { get; private set; }

        /// <summary> Index inside transitioning list when class was added to list </summary>
        public int Index = -1;

        /// <summary> Time elapsed since transition started in seconds </summary>
        private float elapsed;

        /// <summary> Time of duration transition in seconds </summary>
        private float transitionDuration;

        /// <summary> Target LOD level for optimized types </summary>
        private int targetLODLevel;

        /// <summary> When all optimized types done fully transitioning </summary>
        private bool allDone = false;

        /// <summary> Transitioning all types optimized by optimizer object </summary>
        Optimizers_LODTransition[] lodTypes;
        //FOptimizers_LODTransition[] lodTypes;



        public Optimizers_Transitioning(int optimizerId, Optimizer_Base optimizer, int targetLODLevel, float duration, int index = -1)
        {
            Id = optimizerId;
            Finished = false;

            Optimizer = optimizer;
            this.targetLODLevel = targetLODLevel;
            transitionDuration = duration;
            elapsed = 0f;
            Index = index;

            InitTransitioning();
        }


        private void InitTransitioning()
        {
            lodTypes = new Optimizers_LODTransition[Optimizer.GetToOptimizeCount()];
            //lodTypes = new FOptimizers_LODTransition[Optimizer.GetToOptimizeCount()];

            for (int i = 0; i < lodTypes.Length; i++)
            {
                lodTypes[i] = Optimizer.GetLodTransitionFor(i, targetLODLevel);
            }

            Optimizer.TransitionNextLOD = targetLODLevel;
            Optimizer.TransitionPercent = 0f;
        }


        internal void BreakCurrentTransition(float newDuration, int targetLODLevel)
        {
            transitionDuration = newDuration;
            this.targetLODLevel = targetLODLevel;
            elapsed = 0f;

            BreakTransitioning();
        }

        private void BreakTransitioning()
        {
            for (int i = 0; i < lodTypes.Length; i++)
            {
                lodTypes[i].BreakCurrentTransition( Optimizer.GetLODInstance(i, targetLODLevel) );
            }

            Optimizer.TransitionNextLOD = targetLODLevel;
            Optimizer.TransitionPercent = -1f;
        }

        public void Finish()
        {
            Optimizer.SetLODLevel(targetLODLevel);

            for (int i = 0; i < lodTypes.Length; i++) lodTypes[i].Finish();

            Finished = true;

            Optimizer.TransitionNextLOD = 0;
            Optimizer.TransitionPercent = -1f;
        }

        /// <summary>
        /// Animating LODs' interpolations-transition
        /// </summary>
        public void Update(float deltaTime)
        {
            elapsed += deltaTime;

            if (allDone)
            {
                Finish();
            }
            else
            {
                float transitionProgress = elapsed / transitionDuration;
                Optimizer.TransitionPercent = transitionProgress;

                float after = 0f;
                if (elapsed > transitionDuration) after = elapsed - transitionDuration;

                if (!Optimizer.gameObject.activeInHierarchy) Optimizer.gameObject.SetActive(true);

                bool all = true;

                for (int i = 0; i < lodTypes.Length; i++)
                {
                    if (!lodTypes[i].done)
                    {
                        lodTypes[i].Update(transitionProgress, after);
                        all = false;
                    }
                }

                if (transitionProgress >= 1f) allDone = all; else allDone = false;
            }
        }
    }
}
