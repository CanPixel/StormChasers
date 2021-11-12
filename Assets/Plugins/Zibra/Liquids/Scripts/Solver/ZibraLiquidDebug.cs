using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.zibra.liquid.Solver
{
    public static class ZibraLiquidDebug
    {

        [RuntimeInitializeOnLoadMethod]
        static void InitializeDebug()
        {
            SetDebugLogWrapperPointer(DebugLogCallback);
        }

        delegate void debugLogCallback(IntPtr message);
        [MonoPInvokeCallback(typeof(debugLogCallback))]
        static void DebugLogCallback(IntPtr request)
        {
            Debug.Log(Marshal.PtrToStringAnsi(request));
        }

#if (UNITY_IOS || UNITY_TVOS || UNITY_WEBGL) && !UNITY_EDITOR
        [DllImport("__Internal")]
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("ZibraFluidNative_Mac_x64")]
#elif UNITY_64 || UNITY_EDITOR_64
        [DllImport("ZibraFluidNative_Win_x64")]
#else
        [DllImport("ZibraFluidNative_Win_x86")]
#endif
        static extern void SetDebugLogWrapperPointer(debugLogCallback callback);
    }
}