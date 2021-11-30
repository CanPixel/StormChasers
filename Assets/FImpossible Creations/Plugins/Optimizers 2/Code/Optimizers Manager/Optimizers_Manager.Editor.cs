using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;

namespace FIMSpace.FOptimizing
{
    public partial class OptimizersManager
    {
        private DateTime[] ticksTimers;

        /// <summary> Drawing debugging info on screen in playmode </summary>
        private void OnGUI()
        {
            if (!Debugging) return;

            if (ticksTimers == null)
            {
                ticksTimers = new DateTime[GetDistanceTypesCount()];
                for (int i = 0; i < ticksTimers.Length; i++) ticksTimers[i] = DateTime.Now;
            }

            bool drawing = true;
            if (clocks == null || clocks.Length == 0) drawing = false;

            if (drawing)
            {
                GUI.Label(new Rect(10, 10 + (GetDistanceTypesCount() + 1) * 14, 800, 40), "Transitioning " + transitioning.Count + " lods");
                GUI.Label(new Rect(150, 10 + (GetDistanceTypesCount() + 1) * 14, 800, 40), "Total time consumption in this frame " + Math.Round( (double)totalTimeConsumption / 10000.0, 3) );
                GUI.Label(new Rect(404, 10 + (GetDistanceTypesCount() + 1) * 14, 800, 40), "ms for optimizing the rest of scene");
                GUI.Label(new Rect(10, 28 + (GetDistanceTypesCount() + 1) * 14, 800, 40), "Raycasts: " + RaycastsInThisFrame);
                GUI.Label(new Rect(120, 28 + (GetDistanceTypesCount() + 1) * 14, 800, 40), "Hidden: " + HiddenObjects);
                //GUI.Label(new Rect(205, 28 + (GetDistanceTypesCount() + 1) * 14, 800, 40), "Raycast Memory: " + detectedBounds.Count);

                for (int i = 0; i < clocks.Length; i++)
                {
                    if (Time.frameCount - clocks[i].LastTickFrame == 0)
                        ticksTimers[i] = DateTime.Now;

                    DateTime next = ticksTimers[i].AddSeconds(clocks[i].AdaptedDelay);
                    float endSecs = (float)next.Subtract(ticksTimers[i]).TotalSeconds;
                    float currentSecs = (float)next.Subtract(DateTime.Now).TotalSeconds;

                    float val = Mathf.Lerp(1f, 0.65f, Mathf.InverseLerp(0.2f, 3f, endSecs));

                    float lerp = Mathf.InverseLerp(0f, endSecs, currentSecs);
                    Color c = Color.Lerp(Color.white * val, Color.white, lerp);
                    GUI.color = c;

                    string id;
                    if (i != clocks.Length - 1)
                        id = " < " + (int)Distances[i] + ")";
                    else
                        id = " > " + (int)Distances[i - 1] + ")";

                    GUI.Label(new Rect(10, 10 + i * 14, 800, 40), "Clock " + i + " (" + (EOptimizingDistance)i + id);
                    GUI.Label(new Rect(162, 10 + i * 14, 800, 40), "Ticked " + (Time.frameCount - clocks[i].LastTickFrame));
                    GUI.Label(new Rect(230, 10 + i * 14, 800, 40), " frames ago  |");
                    GUI.Label(new Rect(320, 10 + i * 14, 800, 40), "have " + dynamicLists[i].Count + " objects to serve");
                    GUI.Label(new Rect(480, 10 + i * 14, 800, 40), "|  Took " + System.Math.Round((double)((double)clocks[i].LastTicksConsumption / 10000), 2) + " ms");
                    string s = ""; if (clocks[i].DelaysCount > 0) s = "s";
                    GUI.Label(new Rect(590, 10 + i * 14, 800, 40), "spread on " + (clocks[i].DelaysCount+1) + " frame" + s);
                    GUI.Label(new Rect(715, 10 + i * 14, 800, 40), clocks[i].GetAverage() + " avg");
                    GUI.Label(new Rect(785, 10 + i * 14, 800, 40), "Adapted Delay: " + Math.Round( clocks[i].AdaptedDelay, 3) + "s");
                }
            }
            else
            {
                GUI.Label(new Rect(10f, 10f, 800, 40), "Waiting for application to run");
            }
        }


        /// <summary>
        /// FM: Editor class component to enchance controll over component from inspector window
        /// </summary>
        //[CustomEditor(typeof(FOptimizers_Manager))]
        //private class FOptimizers_ManagerEditor : Editor
        //{
        //    public override void OnInspectorGUI()
        //    {
        //        FOptimizers_Manager targetScript = (FOptimizers_Manager)target;

        //        List<string> excluded = new List<string>();

        //        if (!targetScript.Advanced) { excluded.Add("Distances"); excluded.Add("MoveTreshold"); excluded.Add("Debugging"); } else excluded.Add("WorldScale");

        //        DrawPropertiesExcluding(serializedObject, excluded.ToArray());
        //        serializedObject.ApplyModifiedProperties();

        //        GUILayout.Space(4f);
        //    }
        //}

    }
}
#endif
