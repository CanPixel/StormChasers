using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class OptimizersManager
    {
#if UNITY_EDITOR
        [Range(0f, 1f)]
        public float GizmosAlpha = 0.75f;

        public static readonly string[] RangeInfos = new string[5]
            {
                "When objects are very near to camera (this green range) objects will be checked for optimization every frame, in extremal situations with very small delay (if in this range would be a lot of dynamic optimized objects)",
                "When objects are near to camera (this yellow range) objects will be checked for optimization every frame, or with some delay (up to 1 second if in this range would be a lot of dynamic optimized objects)",
                "When objects are mid far from camera (this navy color range) objects will be checked for optimization every frame or with some delay (up to 2 seconds if in this range would be a lot of dynamic optimized objects)",
                "When objects are far from camera (this blue range) objects will be checked for optimization every frame, or with some delay (up to 3 seconds if in this range would be a lot of dynamic optimized objects)",
                "When objects are very far from camera (just further than blue range) objects will be checked for optimization every frame, or with delay (up to 6 seconds if in this range would be a lot of dynamic optimized objects)"
            };

        private void OnDrawGizmosSelected()
        {
            if (GizmosAlpha <= 0f) return;
            if (!MainCamera) return;

            int clocksCount = System.Enum.GetValues(typeof(EOptimizingDistance)).Length;
            RefreshDistances();

            GUIStyle label = new GUIStyle();
            label.alignment = TextAnchor.MiddleCenter;
            label.normal.textColor = Color.white;

            for (int i = 0; i < clocksCount; i++)
            {
                float dist;
                if (i == Distances.Length)
                    dist = Distances[i - 1] * 1.5f;
                else
                    dist = Distances[i];

                float startDist = 0f;
                if (i > 0) startDist = Distances[i - 1];

                Vector3 startPos = MainCamera.transform.position + Vector3.forward * startDist;
                Vector3 endPos = MainCamera.transform.position + Vector3.forward * (dist);
                Vector3 midPos = Vector3.Lerp(startPos, endPos, 0.5f);

                string info = "";
                string type = "";
                switch (i)
                {
                    case 0: type = "Highest"; break;
                    case 1: type = "High"; break;
                    case 2: type = "Medium"; break;
                    case 3: type = "Low"; break;
                    case 4: type = "Very Low"; break;
                }

                info = RangeInfos[i];

                if (i == 0) Gizmos.color = new Color(0f, 1f, 0f);
                else
                    Gizmos.color = Color.HSVToRGB((float)i / (float)clocksCount, 0.9f, 0.9f);

                Gizmos.color *= new Color(1f, 1f, 1f, GizmosAlpha);

                label.normal.textColor = Gizmos.color;
                UnityEditor.Handles.Label(midPos, new GUIContent("[" + type + " Priority]", info), label);

                if (GizmosAlpha > 0.7f)
                    if (i == 2)
                    {
                        float al = Mathf.Lerp(0.1f, 1f, Mathf.InverseLerp(0.7f, 1f, GizmosAlpha));
                        label.normal.textColor = Color.white * new Color(1f, 1f, 1f, al);
                        label.fontStyle = FontStyle.Bold;
                        UnityEditor.Handles.Label(midPos + Vector3.up * Distances[1] / 2f, new GUIContent("[This markings are ONLY for DYNAMIC and Effective optimization methods]", "If you using optimizers only on static objects, don't look at this gizmos or disable it by 'GizmosAlpha' variable"), label);
                        label.fontStyle = FontStyle.Normal;
                    }

                Gizmos.color *= new Color(1f, 1f, 1f, 0.4f * GizmosAlpha);
                Gizmos.DrawLine(startPos - Vector3.up * Distances[1] / 2f, startPos + Vector3.up * Distances[1] / 2f);

                if (i != Distances.Length)
                {
                    Gizmos.DrawLine(startPos, endPos);
                    Gizmos.DrawLine(endPos - Vector3.up * Distances[1] / 2f, endPos + Vector3.up * Distances[1] / 2f);
                    Gizmos.DrawLine(midPos, midPos + Vector3.up * Distances[1] / 4f);
                }
                else
                {
                    Gizmos.DrawCube(endPos + Vector3.forward * dist / 4f, new Vector3(0.01f, Distances[1] / 10f, dist / 2));
                    Gizmos.DrawCube(endPos + Vector3.forward * dist * 0.75f, new Vector3(0.01f, Distances[1] / 24f, dist / 2));
                }

                Gizmos.DrawCube(midPos, new Vector3(0.01f, Distances[1] / 5f, (startPos - endPos).magnitude));

                if (GizmosAlpha == 1f)
                    Gizmos.color *= new Color(1f, 1f, 1f, 0.25f * GizmosAlpha);
                else
                    Gizmos.color *= new Color(1f, 1f, 1f, 0.11f * GizmosAlpha);

                Gizmos.DrawWireSphere(MainCamera.transform.position, dist);
            }

            if (_editorToDrawContainer != null)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.7f);
                for (int i = 0; i < _editorToDrawContainer.CullingSpheres.Length; i++)
                {
                    if (_editorToDrawContainer.CullingSpheres[i].radius > 0f)
                        Gizmos.DrawWireSphere(_editorToDrawContainer.CullingSpheres[i].position, _editorToDrawContainer.CullingSpheres[i].radius);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private Texture HumanScaleGuide;

        /// <summary>
        /// Drawing world scale guide (approximated character height) for easy setup
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!MainCamera) return;

            if (GizmosAlpha > 0f)
            {
                if (DrawHumanSizeRefIcon)
                {
                    if (!HumanScaleGuide) HumanScaleGuide = Resources.Load("TEX_HumanScaleRef", typeof(Texture2D)) as Texture2D;

                    if (!Advanced)
                        if (HumanScaleGuide)
                        {
                            UnityEditor.Handles.BeginGUI();
                            Vector3 pos = MainCamera.transform.position;
                            Vector2 pos2D = UnityEditor.HandleUtility.WorldToGUIPoint(pos);
                            float scale = UnityEditor.HandleUtility.GetHandleSize(pos);
                            GUI.Label(new Rect(pos2D.x, pos2D.y - (82 * WorldScale) / scale, (82 * WorldScale) / scale, (82 * WorldScale) / scale), HumanScaleGuide);
                            UnityEditor.Handles.EndGUI();
                        }
                }
            }

            int clocksCount = GetDistanceTypesCount();
            for (int i = 0; i < clocksCount; i++)
            {
                float dist;

                if (Distances != null)
                    if (Distances.Length > 0)
                    {

                        if (i == Distances.Length)
                            dist = Distances[i - 1] * 1.5f;
                        else
                            dist = Distances[i];

                        float startDist = 0f;
                        if (i > 0) startDist = Distances[i - 1];

                        Vector3 startPos = MainCamera.transform.position + Vector3.forward * startDist;
                        Vector3 endPos = MainCamera.transform.position + Vector3.forward * (dist);
                        Vector3 midPos = Vector3.Lerp(startPos, endPos, 0.5f);

                        if (i == 0) Gizmos.color = new Color(0f, 1f, 0f);
                        else
                            Gizmos.color = Color.HSVToRGB((float)i / (float)clocksCount, 0.9f, 0.9f);

                        Gizmos.color *= new Color(1f, 1f, 1f, 0.1f * GizmosAlpha);

                        if (i != Distances.Length)
                        {
                            Gizmos.DrawLine(startPos, endPos);
                            Gizmos.DrawLine(endPos - Vector3.up * Distances[1] / 2f, endPos + Vector3.up * Distances[1] / 2f);
                            Gizmos.DrawLine(midPos, midPos + Vector3.up * Distances[1] / 4f);
                        }
                        else
                        {
                        }

                        Gizmos.DrawLine(startPos - Vector3.up * Distances[1] / 2f, startPos + Vector3.up * Distances[1] / 2f);
                    }
            }
        }
#endif

    }
}
