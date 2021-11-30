using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class which is containing methods to help support additional stuff 
    /// in editor for optimizers classes management
    /// </summary>
    public static class Optimizers_EditorHelperMethods
    {

        /// <summary>
        /// Restrictions for drawing error messages by Optimizers to not spam editor with them
        /// </summary>
        public static bool CanDrawErrorMessage()
        {
#if UNITY_EDITOR
            string dateBinary = PlayerPrefs.GetString("FOPT_LastWrongAccess", new DateTime(1999, 1, 1).ToBinary().ToString());
            long dateBin = 0;
            int errorCounter = 0;

            if (long.TryParse(dateBinary, out dateBin))
            {
                DateTime lastAccessTime = DateTime.FromBinary(dateBin);

                if (DateTime.Now.Subtract(lastAccessTime).TotalHours > 10) // Last time was more than 10 hours so we are resetting error counter helper
                {
                    PlayerPrefs.SetString("FOPT_WrongCounter", "0");
                    return true;
                }
                else // Otherwise we limiting error messages display to 4 then every 2 errors display up to 10
                {
                    if (int.TryParse(PlayerPrefs.GetString("FOPT_WrongCounter", "0"), out errorCounter))
                    {
                        errorCounter++;
                        PlayerPrefs.SetString("FOPT_WrongCounter", errorCounter.ToString());

                        if (errorCounter < 4)
                        {
                            return true;
                        }
                        else if (errorCounter < 10)
                        {
                            if (errorCounter % 2 == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else return false;

            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Just #if unityeditor DisplayDialog for cleaner code in files
        /// </summary>
        public static void DisplayError(string title, string message, string buttonText)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(title, message, buttonText);
#endif
        }

        /// <summary>
        /// Predefined LOD levels colors
        /// </summary>
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


        /// <summary>
        /// Getting specific LOD color
        /// </summary>
        public static Color GetLODColor(int index, int count, float multiply = 1f, float saturation = 1f, float value = 1f, float multiplyHiddenCull = 1f)
        {
            Color col;

            if (index == count) col = culledLODColor;
            else if (index == count + 1) col = hiddenLODColor;
            else
                col = lODColors[index];

            if (multiply != 1f) col *= (Color.white * multiply);

            if (multiplyHiddenCull != 1f)
                if (index == count || index == count + 1) col *= (Color.white * multiplyHiddenCull);

            if (saturation != 1f || value != 1f)
            {
                float h, s, v;
                Color.RGBToHSV(col, out h, out s, out v);
                col = Color.HSVToRGB(h, s * saturation, v * value);
            }

            return col;
        }

        public static readonly Color culledLODColor = new Color(.38f, .43f, .25f, 1f);
        public static readonly Color hiddenLODColor = new Color(.38f, .43f, .25f, .8f);
    }
}
