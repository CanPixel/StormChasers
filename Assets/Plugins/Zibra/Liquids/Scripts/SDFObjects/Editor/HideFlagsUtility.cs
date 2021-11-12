using UnityEditor;
using UnityEngine;

namespace com.zibra.liquid.Editor.SDFObjects
{
    public static class HideFlagsUtility
    {
        [MenuItem("Utilities/Remove Hide Flags")]
        public static void RemoveHideFlags()
        {
            var gameObject = Selection.activeObject as GameObject;

            if (gameObject != null)
            {
                RemoveHideFlagsRecursive(gameObject);
            }
        }

        private static void RemoveHideFlagsRecursive(GameObject target)
        {
            target.hideFlags = HideFlags.None;

            foreach (Transform child in target.transform)
            {
                RemoveHideFlagsRecursive(child.gameObject);
            }
        }
    }
}