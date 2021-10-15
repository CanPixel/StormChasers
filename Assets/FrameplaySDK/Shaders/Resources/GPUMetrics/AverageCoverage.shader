// Copyright 2019 Frameplay. All Rights Reserved.

Shader "Hidden/AverageCoverage"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _CoverageX("Texture", 2D) = "white" {}
        _CoverageW("Texture", 2D) = "white" {}
        _CoverageY("Texture", 2D) = "white" {}
        _CoverageZ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend One OneMinusSrcAlpha
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
			"DisableBatching" = "True"
		}
        Pass
        {
            CGPROGRAM
			#pragma vertex vertAC
            #pragma fragment fragAC
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vertCD
            #pragma fragment fragCD
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vertBO
            #pragma fragment fragBO
            #include "FrameplayImageComparison.cginc"
            ENDCG
        }
    }
}
