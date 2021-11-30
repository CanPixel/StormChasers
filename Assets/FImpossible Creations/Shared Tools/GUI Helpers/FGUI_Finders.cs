#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    public static class FGUI_Finders
    {
        public static Component FoundAnimator;
        private static bool checkForAnim = true;
        private static int clicks = 0;

        /// <summary>
        /// Resetting static finders for new searching
        /// </summary>
        public static void ResetFinders(bool resetClicks = true)
        {
            checkForAnim = true;
            FoundAnimator = null;
            if ( resetClicks ) clicks = 0;
        }


        /// <summary>
        /// Searching for animator in given root object and it's parents
        /// If you want to search new aniamtor you have to call ResetFinders()
        /// </summary>
        /// <returns> Returns true if animator is found, enabled and have controller </returns>
        public static bool CheckForAnimator(GameObject root, bool needAnimatorBox = true, bool drawInactiveWarning = true, int clicksTohide = 1)
        {
            bool working = false;

            if (checkForAnim)
            {
                FoundAnimator = SearchForParentWithAnimator(root);
            }

            // Drawing animator specific dialogs
            if (FoundAnimator)
            {
                Animation legacy = FoundAnimator as Animation;
                Animator mec = FoundAnimator as Animator;

                if (legacy) if (legacy.enabled) working = true;

                if (mec) // Mecanim found but no controller assigned
                {
                    if (mec.enabled) working = true;

                    if (mec.runtimeAnimatorController == null)
                    {
                        EditorGUILayout.HelpBox("  No 'Animator Controller' inside Animator ("+FoundAnimator.transform.name+")", MessageType.Warning);
                        drawInactiveWarning = false;
                        working = false;
                    }
                }

                // Drawing dialogs for warnings
                if (needAnimatorBox)
                {
                    if (drawInactiveWarning)
                    {
                        if (!working)
                        {
                            GUILayout.Space(-4);
                            FGUI_Inspector.DrawWarning(" ANIMATOR IS DISABLED! ");
                            GUILayout.Space(2);
                        }
                    }
                }
            }
            else
            {
                if (needAnimatorBox)
                {
                    if (clicks < clicksTohide)
                    {
                        GUILayout.Space(-4);
                        if (FGUI_Inspector.DrawWarning(" ANIMATOR NOT FOUND! ")) clicks++;
                        GUILayout.Space(2);
                    }
                }
            }

            checkForAnim = false;
            return working;
        }



        /// <summary>
        /// Searching in first children for animation/animator components
        /// If not found then searching in parents
        /// </summary>
        /// <returns> Returns transform with component or null if not found </returns>
        public static Component SearchForParentWithAnimator(GameObject root)
        {
            Animation animation = root.GetComponentInChildren<Animation>();
            if (animation) return animation;
            Animator animator = root.GetComponentInChildren<Animator>();
            if (animator) return animator;

            if (root.transform.parent != null)
            {
                Transform pr = root.transform.parent;

                while (pr != null)
                {
                    animation = pr.GetComponent<Animation>();
                    if (animation) return animation;
                    animator = pr.GetComponent<Animator>();
                    if (animator) return animator;

                    pr = pr.parent;
                }
            }

            return null;
        }
    }
}

#endif
