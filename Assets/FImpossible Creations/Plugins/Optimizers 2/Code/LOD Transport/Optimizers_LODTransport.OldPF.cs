#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class to solve many troubles with different unity versions to use not shared (unique) settings
    /// in all cases (isolated scene prefab mode / creating prefabs from scene etc.)
    /// Class with methods supporting versions before unity 2018.3
    /// </summary>
    public static partial class Optimizers_LODTransport
    {

#if UNITY_2018_3_OR_NEWER  ///////////////////////////////////////////////
#else //ELSE


        public static Object GetPrefab(GameObject target)
        {
            Object prefab = null;

            if (target.gameObject)
            {
                if (PrefabUtility.GetPrefabObject(target.gameObject)) // If it is project asset prefab
                    prefab = PrefabUtility.GetPrefabObject(target.gameObject);
                else // If it is instance of prefab
                {
                    prefab = PrefabUtility.GetPrefabParent(target.gameObject);
                }
            }
            else
                Debug.LogError("[OPTIMIZERS EDITOR] No Game Object inside lods controller!");

            if (prefab)
                if (!AssetDatabase.Contains(prefab))
                    return null; // It's not in assets database?

            return prefab;
        }


#endif  ///////////////////////////////////////////////

    }
}
#endif
