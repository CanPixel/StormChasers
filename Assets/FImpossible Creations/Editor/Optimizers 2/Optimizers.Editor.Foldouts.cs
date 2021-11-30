using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor
    {
        bool drawObstaclesDetection = true;
        void Fold_DrawModuleObstacles()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseObstacleDetection, ref drawObstaclesDetection, Lang("Obstacles Detection"), null, _TexDetectionIcon, 22, "Tooltip", LangBig());

            if (drawObstaclesDetection && Get.UseObstacleDetection)
            {
                GUILayout.Space(3f);

                Obstacles_DrawObstaclesSettings();

                GUILayout.Space(5f);
            }
        }

        bool drawMultiShape = true;
        void Fold_DrawModuleMultiShape()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseMultiShape, ref drawMultiShape, Lang("Complex Detection Shape"), null, _TexShapeIcon, 22, "Tooltip", LangBig());

            if (drawMultiShape && Get.UseMultiShape)
            {
                if (Get.OptimizingMethod == EOptimizingMethod.Dynamic || Get.OptimizingMethod == EOptimizingMethod.TriggerBased) Get.OptimizingMethod = EOptimizingMethod.Effective;

                GUILayout.Space(3f);

                DefaultInspectorStackMultiShape();

                GUILayout.Space(5f);
            }
        }
    }

}

