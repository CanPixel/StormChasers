// We have two shader variants, one for the Frameplay Placeholder UVs,
// and one for the Ad UVs.
Shader "Frameplay/HighQualityAd"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [Toggle] FRAMEPLAY_PLACEHOLDER_UV("Use Placeholder UVs", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" } // batching causes the logo to only appear in the centre in world space, not object space
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert
        // 3 shader variants: placeholder ad, loading, and normal ad
        #pragma multi_compile __ FRAMEPLAY_PLACEHOLDER_UV_ON FRAMEPLAY_LOADING_ON
#if FRAMEPLAY_LOADING_ON
		#define FRAMEPLAY_PLACEHOLDER_UV_ON
#endif
            
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			half3 pos : TEXCOORD;
        };

        half _Glossiness;
        half _Metallic;
        half _FRAMEPLAY_AD_TEXTURE_BIAS;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.pos = v.vertex.xyz;
        }

        // returns 1 if in rectangle, otherwise 0
        fixed rect(half2 leftBottom, half2 rightTop, half2 uv) 
        {
            fixed2 borders = step(leftBottom, uv);
            // shenanigans to avoid compile errors on GLES2 with Unity 2018
            // originally: step(leftBottom, uv) * step(uv, rightTop);
            fixed2 b2 = step(uv, rightTop);
            return borders.x*borders.y * b2.x * b2.y;
        }

        half subtleVignette(half2 uv)
        {
            uv *=  1.0 - uv.yx;
            half vig = uv.x*uv.y * 5.0;
            vig = 1-vig;
            vig *= vig * vig;
            vig *= vig;
            vig*=0.4;
            vig = 1-vig;
            return vig;
        }

        float movingRing(half2 d, float midR, float thickR)
        { // from https://www.shadertoy.com/view/XdBXzd
            float r = length(d),
              theta = -atan2(d.y,d.x);
            theta = frac( 0.5*(1.0+theta/3.1415926) -_Time.y );
            // antialiasing
            theta -= max(theta - 1.0 + 1e-2, 0.0) * 1e2;
            return theta * smoothstep(2., 0., abs(r-midR)-thickR);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half2 uv;
            // switch how UVs are generated based on the shader variant (compile time)
#ifdef FRAMEPLAY_PLACEHOLDER_UV_ON
            const float meshInitialSize = 2.0; // 3 by default, smaller to have the logo not touch the edges
            uv = (IN.pos.xy+meshInitialSize*0.5)/meshInitialSize;
#else
            uv = IN.uv_MainTex;
#endif
            half4 col = tex2Dbias (_MainTex, float4(uv, 0, _FRAMEPLAY_AD_TEXTURE_BIAS));

#ifdef FRAMEPLAY_PLACEHOLDER_UV_ON
            // avoid streaks from lower mips in logo, or tiling
            col *= rect(0, 1, uv);
            // background
            const float4 frameplayBlue = float4(2,109,249,255)/255.0;
            const float logoOpacity = 0.44;
            col.rgb = lerp(subtleVignette(IN.uv_MainTex)*frameplayBlue, 1, col.a*logoOpacity);
            
#ifdef FRAMEPLAY_LOADING_ON
            col.rgb = max(col.rgb*0.7, movingRing((uv-0.5)*256, 50, 15));
#endif
#endif

//LinearSpace fix
#ifdef UNITY_COLORSPACE_GAMMA
//Do nothing
#else
col.rgb = GammaToLinearSpace(col.rgb);
#endif
            o.Albedo = col.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
