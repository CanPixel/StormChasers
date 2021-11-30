using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor
    {
        protected virtual Texture2D GetSmallIcon() { if (__texOptimizersIcon != null) return __texOptimizersIcon; __texOptimizersIcon = Resources.Load<Texture2D>("FIMSpace/Optimizers 2/Optimizers Icon Small"); return __texOptimizersIcon; }
        protected Texture2D __texOptimizersIcon = null;

        public static Texture2D _TexShapeIcon { get { if (__texShapeIcon != null) return __texShapeIcon; __texShapeIcon = Resources.Load<Texture2D>("FIMSpace/Optimizers 2/ShapeIcon"); return __texShapeIcon; } }
        private static Texture2D __texShapeIcon = null;
        public static Texture2D _TexDetectionIcon { get { if (__texDetection != null) return __texDetection; __texDetection = Resources.Load<Texture2D>("FIMSpace/Optimizers 2/WallDetectionIcon"); return __texDetection; } }
        private static Texture2D __texDetection = null;

        private static UnityEngine.Object _manualFile;


    }

}

