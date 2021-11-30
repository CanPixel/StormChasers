using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    internal static class Optimizers_LODGUI
    {

        #region Pre Defines

        public static readonly Color[] lODColors =
        {
            new Color(0.2231376f, 0.8011768f, 0.1619608f, 1.0f),
            new Color(0.2070592f, 0.6333336f, 0.7556864f, 1.0f),
            new Color(0.1592160f, 0.5578432f, 0.3435296f, 1.0f),
            new Color(0.1333336f, 0.400000f, 0.7982352f, 1.0f),
            new Color(0.3827448f, 0.2886272f, 0.5239216f, 1.0f),
            new Color(0.8000000f, 0.4423528f, 0.0000000f, 1.0f),
            new Color(0.4886272f, 0.1078432f, 0.801960f, 1.0f),
            new Color(0.7749016f, 0.6368624f, 0.0250984f, 1.0f)
        };

        public static readonly Color culledLODColor = new Color(.4f, 0f, 0f, 1f);
        public static readonly Color hiddenLODColor = new Color(.4f, .2f, .2f, .5f);

        public class GUIStyles
        {
            public readonly GUIStyle LODSliderBGStyle = "LODSliderBG";
            public readonly GUIStyle LODSliderRangeStyle = "LODSliderRange";
            //public readonly GUIStyle LODDistanceBarStyle = "LODDistanceBar";
            public readonly GUIStyle LODSliderRangeSelectedStyle = "LODSliderRangeSelected";
            public readonly GUIStyle LODSliderTextStyle = "LODSliderText";
            public readonly GUIStyle LODSliderRightTextStyle = new GUIStyle();
        }

        private static GUIStyles _styles;
        public static GUIStyles Styles { get { if (_styles == null) _styles = new GUIStyles(); return _styles; } }

        #endregion


        /// <summary>
        /// Assigning percent value for LOD info with clamping min max ranges
        /// </summary>
        public static void SetSelectedLODLevelPercentage(float newPercentage, int lod, List<Optimizers_LODInfo> lods)
        {
            if (lod < 0) return;

            // Find the lower detail lod, clamp value to stop overlapping slider
            float minimum = 0.0f;
            Optimizers_LODInfo lowerLOD = lods.FirstOrDefault(x => x.LODLevel == lods[lod].LODLevel - 1);
            if (lowerLOD != null) minimum = lowerLOD.LODPercentage;

            // Find the higher detail lod, clamp value to stop overlapping slider
            float maximum = 1.0f;
            Optimizers_LODInfo higherLOD = lods.FirstOrDefault(x => x.LODLevel == lods[lod].LODLevel + 1);
            if (higherLOD != null) maximum = higherLOD.LODPercentage;

            maximum = Mathf.Clamp01(maximum);
            minimum = Mathf.Clamp01(minimum);
            if (maximum * 0.98f <= minimum) maximum = Mathf.Lerp(minimum, maximum, 0.5f); else maximum *= 0.98f;

            // Set value
            lods[lod].LODPercentage = Mathf.Clamp(newPercentage, minimum, maximum);
        }


        /// <summary>
        /// Drawing slider for LOD level
        /// </summary>
        public static void DrawLODSlider(Optimizer_Base optimizer, Rect area, IList<Optimizers_LODInfo> lodInfos, int selectedLevel, float currentDistance = 0f, float maxDistance = 0f, bool hidden = false, int startCull = 0)
        {
            Rect areaA = area;

            if (hidden)
            {
                areaA = new Rect(area.x, area.y, area.width, area.height - 14);
            }

            // Drawing BG Rectangle
            Styles.LODSliderBGStyle.Draw(areaA, GUIContent.none, false, false, false, false);

            float[] measure = optimizer.GetDistanceMeasures();

            // Drawing LOD rectangle areas
            for (int i = 0; i < lodInfos.Count; i++)
            {
                Optimizers_LODInfo lodInfo = lodInfos[i];
                //DrawCameraButtonForRange(lodInfo, lodInfos.Count);

                float startPercent = 0.0f;
                if (i != 0) startPercent = lodInfos[i - 1].LODPercentage;

                DrawLODRange(lodInfo, startPercent, i == selectedLevel, measure[i], lodInfos.Count);
                if (i != lodInfos.Count - 1) DrawLODButton(lodInfo);
            }

            // Draw culled range as last one
            if (lodInfos == null) return;
            if (lodInfos.Count == 0) return;
            if (lodInfos[lodInfos.Count - 1] == null) return;

            float prePerc = lodInfos[lodInfos.Count - 1].LODPercentage;
            DrawCulledRange(areaA, lodInfos.Count > 0 ? prePerc : 1.0f, lodInfos[0].MaxDist, selectedLevel == lodInfos.Count);

            if (maxDistance > 0f)
                if (currentDistance > 0f)
                {
                    float perc = currentDistance / maxDistance;

                    float targetX = Mathf.Lerp(areaA.x, areaA.x + areaA.width - 42, perc);

                    Rect distanceBar = new Rect(targetX, areaA.y, 4, areaA.height);

                    Color preCol = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.3f);
                    Styles.LODSliderRangeStyle.Draw(distanceBar, GUIContent.none, false, false, false, false);
                    GUI.color = preCol;
                }

            // Draw looking away / hidden range on the bottom
            if (hidden)
            {
                float cullStartPercentage = 0f;
                if (startCull >= lodInfos.Count)
                {
                    startCull = -2;
                    cullStartPercentage = 1f;
                }
                else if (startCull >= 0)
                {
                    cullStartPercentage = lodInfos[startCull].LODPercentage;
                }

                DrawHiddenRange(area, cullStartPercentage, lodInfos[0].MaxDist, selectedLevel == lodInfos.Count + 1);
            }
        }

        private static Texture camIcon;

        public static void DrawCameraButtonForRange(Optimizers_LODInfo currentLOD, SceneView view, float distance)
        {
            Rect goRect = new Rect(currentLOD.RangeRect);
            goRect.y -= 14; goRect.x += goRect.width;
            goRect.height = 16; goRect.width = 16;

            if (camIcon == null) camIcon = EditorGUIUtility.IconContent("Camera Icon").image;

            if (GUI.Button(goRect, new GUIContent(camIcon, "Go with scene view camera to start of LOD"+(currentLOD.LODLevel+1) + " so in front of end range for LOD" + currentLOD.LODLevel), EditorStyles.label))
            {
                Camera cam = view.camera;
                if (cam)
                {
                    Optimizer_Base opt = currentLOD.optimizerRef;
                    if (opt)
                    {
                        TerrainOptimizer terr = opt as TerrainOptimizer;

                        if (terr)
                        {
                            var target = view.camera;
                            Vector3 toOpt = -terr.transform.forward + terr.transform.up - terr.transform.right;
                            target.transform.position = terr.transform.position - ( (terr.transform.position - toOpt) - terr.transform.position ).normalized * (distance + terr.DetectionRadius);
                            target.transform.rotation = Quaternion.LookRotation(-toOpt);
                            view.AlignViewToObject(target.transform);
                        }
                        else
                        {
                            Vector3 focPoint = opt.transform.position;
                            focPoint += opt.transform.TransformVector(opt.DetectionOffset);

                            if (opt.OptimizingMethod == EOptimizingMethod.Effective || opt.OptimizingMethod == EOptimizingMethod.Static)
                                distance += opt.DetectionRadius * opt.transform.lossyScale.x;

                            Vector3 toOpt = focPoint - view.camera.transform.position;

                            var target = view.camera;
                            target.transform.position = focPoint - toOpt.normalized * distance;
                            target.transform.rotation = Quaternion.LookRotation(toOpt);
                            view.AlignViewToObject(target.transform);
                        }
                    }
                }

            }
        }


        /// <summary>
        /// Calculating rect for LOD button
        /// </summary>
        public static Rect CalcLODButton(Rect totalRect, float percentage, bool hidden)
        {
            float rectW = totalRect.width - 43;
            float startX = Mathf.Round(rectW * percentage);

            return new Rect(totalRect.x + startX - 5, totalRect.y, 10, totalRect.height);
        }

        /// <summary>
        /// Calculating rect for drwing slot range box
        /// </summary>
        public static Rect CalcLODRange(Rect totalRect, float startPercent, float endPercent, bool culled = false, bool hidden = false)
        {
            float rectW = totalRect.width;
            float startX;

            if (!culled)
            {
                rectW = totalRect.width - 43;
                startX = Mathf.Round(rectW * startPercent);
            }
            else
            {
                rectW = totalRect.width;
                startX = Mathf.Round((rectW - 43) * startPercent);
            }

            float endX = Mathf.Round(rectW * endPercent);

            return new Rect(totalRect.x + startX, totalRect.y, endX - startX, totalRect.height - (hidden ? 14 : 0));
        }


        /// <summary>
        /// Drawing clickable box for selecting
        /// </summary>
        private static void DrawLODButton(Optimizers_LODInfo currentLOD)
        {
            // Make the lod button areas a horizonal resizer
            EditorGUIUtility.AddCursorRect(currentLOD.ButtonRect, MouseCursor.ResizeHorizontal);
        }


        /// <summary>
        /// Drawing LOD slider box
        /// </summary>
        private static void DrawLODRange(Optimizers_LODInfo currentLOD, float previousLODPercentage, bool isSelected, float nextUnits, int lodLevels)
        {
            Color tempColor = GUI.backgroundColor;
            string startPercentageString = currentLOD.LODName + "\n";

            //if (units == 0f) units = Mathf.Lerp(currentLOD.MinMax.x, currentLOD.MinMax.y, previousLODPercentage);
            //units = (float)Math.Round(units, 1);
            float preUnits = 0f;
            if (preUnits == 0f) preUnits = Mathf.Lerp(0f, currentLOD.MaxDist, previousLODPercentage);
            preUnits = (float)Math.Round(preUnits, 1);

            nextUnits = (float)Math.Round(nextUnits, 1);

            startPercentageString += /*"<" +*/ preUnits.ToString();
            GUIContent content = new GUIContent(startPercentageString, "Level of detail settings applied when object is about " + preUnits + " units away from the camera");

            if (isSelected)
            {
                Rect foreground = currentLOD.RangeRect;
                foreground.width -= 3 * 2;
                foreground.height -= 3 * 2;
                foreground.center += new Vector2(3, 3);

                Styles.LODSliderRangeSelectedStyle.Draw(currentLOD.RangeRect, GUIContent.none, false, false, false, false);
                GUI.backgroundColor = lODColors[currentLOD.LODLevel];

                if (foreground.width > 0) Styles.LODSliderRangeStyle.Draw(foreground, GUIContent.none, false, false, false, false);
                Styles.LODSliderTextStyle.Draw(currentLOD.RangeRect, content, false, false, false, false);

                if (currentLOD.LODLevel == lodLevels - 1)
                    if (currentLOD.RangeRect.width > 66)
                    {
                        Styles.LODSliderRightTextStyle.alignment = TextAnchor.UpperRight;
                        Styles.LODSliderRightTextStyle.normal.textColor = Styles.LODSliderTextStyle.normal.textColor;

                        Styles.LODSliderRightTextStyle.Draw(currentLOD.RangeRect, new GUIContent("\n" + nextUnits + " "), false, false, false, false);
                    }

            }
            else
            {
                GUI.backgroundColor = lODColors[currentLOD.LODLevel];
                GUI.backgroundColor *= 0.6f;
                Styles.LODSliderRangeStyle.Draw(currentLOD.RangeRect, GUIContent.none, false, false, false, false);
                Styles.LODSliderTextStyle.Draw(currentLOD.RangeRect, content, false, false, false, false);

                if (currentLOD.LODLevel == lodLevels - 1)
                    if (currentLOD.RangeRect.width > 66)
                    {
                        Styles.LODSliderRightTextStyle.alignment = TextAnchor.UpperRight;
                        Styles.LODSliderRightTextStyle.normal.textColor = Styles.LODSliderTextStyle.normal.textColor;

                        Styles.LODSliderRightTextStyle.Draw(currentLOD.RangeRect, new GUIContent("\n" + nextUnits + " "), false, false, false, false);
                    }
            }



            GUI.backgroundColor = tempColor;
        }


        /// <summary>
        /// Drawing red cull range LOD slot
        /// </summary>
        private static void DrawCulledRange(Rect totalRect, float previousLODPercentage, float dist, bool isSelected = false)
        {
            if (Mathf.Approximately(previousLODPercentage, 0.0f)) return;

            Rect rect = GetCulledBox(totalRect, previousLODPercentage);
            // Draw the range of a lod level on the slider
            Color preColor = GUI.color;
            Color preBGColor = GUI.backgroundColor;

            if (isSelected)
            {
                Rect foreground = rect;
                foreground.width -= 6;
                foreground.height -= 6;
                foreground.center += new Vector2(3f, 3f);

                Styles.LODSliderRangeSelectedStyle.Draw(rect, GUIContent.none, false, false, false, false);
                GUI.backgroundColor = culledLODColor;

                if (foreground.width > 0) Styles.LODSliderRangeStyle.Draw(foreground, GUIContent.none, false, false, false, false);
            }
            else
            {
                GUI.color = culledLODColor;
                Styles.LODSliderRangeStyle.Draw(rect, GUIContent.none, false, false, false, false);
            }

            GUI.color = preColor;
            GUI.backgroundColor = preBGColor;

            // Draw some details for the current marker
            string startPercentageString = "Culled\n>" + Mathf.Round(dist);
            Styles.LODSliderTextStyle.Draw(rect, new GUIContent(startPercentageString, "Culled means deactivated, lowest possible level of detail for your components, mostly disabling"), false, false, false, false);
        }


        private static void DrawHiddenRange(Rect totalRect, float cullStartPerc, float maxDist, bool isSelected = false)
        {
            Rect rect = GetHiddenBox(totalRect);
            // Draw the range of a lod level on the slider
            Color preColor = GUI.color;
            Color preBGColor = GUI.backgroundColor;

            if (isSelected)
            {
                Rect foreground = rect;
                foreground.width -= 6;
                foreground.height -= 6;
                foreground.center += new Vector2(3f, 3f);

                Styles.LODSliderRangeSelectedStyle.Draw(rect, GUIContent.none, false, false, false, false);

                if (cullStartPerc > 0f)
                    GUI.backgroundColor = hiddenLODColor;
                else
                    GUI.backgroundColor = culledLODColor;

                if (foreground.width > 0) Styles.LODSliderRangeStyle.Draw(foreground, GUIContent.none, false, false, false, false);
            }
            else
            {
                if (cullStartPerc > 0f)
                    GUI.color = hiddenLODColor;
                else
                    GUI.color = culledLODColor;

                Styles.LODSliderRangeStyle.Draw(rect, GUIContent.none, false, false, false, false);
            }

            if (cullStartPerc > 0f)
            {
                GUI.backgroundColor = culledLODColor * new Color(1.5f, 1.5f, 1.5f, 0.84f);
                GUI.color = culledLODColor * new Color(1.5f, 1.5f, 1.5f, 0.84f);
                Styles.LODSliderRangeStyle.Draw(GetHiddenBox(totalRect, cullStartPerc), GUIContent.none, false, false, false, false);
            }

            GUI.color = preColor;
            GUI.backgroundColor = preBGColor;

            // Draw some details for the current marker
            string startPercentageString = "Looking Away / Hidden";
            if (cullStartPerc <= 0f) startPercentageString = "Looking Away / Hidden : Same as Culled";
            GUIStyle style = new GUIStyle(Styles.LODSliderTextStyle);
            style.alignment = TextAnchor.UpperLeft;
            style.padding = new RectOffset(4, 0, 1, 0);
            style.fontSize = 9;
            style.Draw(rect, new GUIContent(startPercentageString, "When you enable 'Cull if not see' or 'Hideable' (You can do custom coding and use Optimizer.SetHidden(true); for example when you detect that object is not visible through wall)  you can optimize different parts of your components when object will be invisible in certain LOD ranges."), false, false, false, false);
        }


        /// <summary>
        /// Calculating box position and size for culled range
        /// </summary>
        public static Rect GetCulledBox(Rect totalRect, float previousLODPercentage)
        {
            Rect rect = CalcLODRange(totalRect, previousLODPercentage, 1.0f, true);
            rect.height -= 2;
            rect.width -= 1;
            rect.center += new Vector2(0f, 1.0f);
            return rect;
        }

        public static Rect GetHiddenBox(Rect totalRect)
        {
            Rect rect = new Rect(totalRect.x, totalRect.y + totalRect.height - 14, totalRect.width, 14);
            return rect;
        }

        public static Rect GetHiddenBox(Rect totalRect, float cullFrom)
        {
            float rectW = totalRect.width;
            float startX;

            rectW = totalRect.width;
            startX = Mathf.Round((rectW - 43) * cullFrom);

            float endX = Mathf.Round(rectW);

            return new Rect(totalRect.x + startX, totalRect.y + totalRect.height - 14, endX - startX, 14);
        }

        /// <summary>
        /// Returns value for LOD slider position
        /// </summary>
        public static float GetLODSliderPercent(Vector2 position, Rect sliderRect)
        {
            float percentage = Mathf.Clamp((position.x - sliderRect.x) / (sliderRect.width - 43), 0.01f, 1.0f);
            return percentage;
        }


        /// <summary>
        /// FM: Simple helper class to help drawing foptimizers LODs
        /// </summary>
        public sealed class Optimizers_LODInfo
        {
            public Optimizer_Base optimizerRef;

            public Rect ButtonRect;
            public Rect RangeRect;

            public Optimizers_LODInfo(int lodLevel, string name, float percentage, Optimizer_Base optim)
            {
                LODLevel = lodLevel;
                LODName = name;
                LODPercentage = percentage;
                optimizerRef = optim;
            }

            public int LODLevel { get; private set; }
            public string LODName { get; private set; }
            public float LODPercentage { get; set; }

            public float MaxDist { get; internal set; }
        }

    }
}