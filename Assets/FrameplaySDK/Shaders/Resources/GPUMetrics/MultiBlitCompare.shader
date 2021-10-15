// Copyright 2019 Frameplay. All Rights Reserved.
Shader "Hidden/MultiBlitCompare"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderType" = "Transparent" "DisableBatching" = "True" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vertRQ
            #pragma fragment fragRQ
            #pragma multi_compile __ FRAMEPLAY_co
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
        Pass
        {
            Blend One One
            CGPROGRAM
            #pragma vertex vertSQ
            #pragma fragment fragSQ
            #pragma multi_compile __ FRAMEPLAY_os
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert1TB
            #pragma fragment fragEA
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert1TB
            #pragma fragment fragCAS
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
    }
}