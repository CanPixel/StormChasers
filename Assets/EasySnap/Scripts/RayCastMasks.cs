namespace EasySnap
{

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class RayCastMasks : MonoBehaviour
    {

        public static int defaultLayer = 0;
        public static int defaultMask = ~(1 << defaultLayer);           // Hit everything except the objects in defaultLayer and its children

        public static int ignoreLayer = 2;
        public static int ignoreMask = ~(1 << ignoreLayer);             // Hit everything except the objects in ignoreLayer and its children

        public static int uiLayer = 5;
        public static int uiMask = ~(1 << uiLayer);                     // Hit everything except the objects in uiLayer and its children

    }

#endif

}