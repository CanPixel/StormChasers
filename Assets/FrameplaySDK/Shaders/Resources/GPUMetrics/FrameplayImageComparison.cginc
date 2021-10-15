// Copyright 2019 Frameplay. All Rights Reserved.
#ifndef FRAMEPLAY_IMAGE_COMPARISON_CGINC
#define FRAMEPLAY_IMAGE_COMPARISON_CGINC

#define FRAMEPLAY_ROLLING_AVERAGE
float4 f(float4 v, float4 f, float4 i) { return(i - v) / (f - v); }float f(fixed3 f) { return dot(f, fixed3(.2126, .7152, .0722)); }fixed f(float4 f, float4 v) { return dot(1., max(sign(f - v), 0.)); }float4 f(fixed2 f, float3 v, float3 x, float3 i, float4x4 z) { float4 y = float4(v + f.x * x + f.y * i, 1.); return mul(z, y); }float4 t(float4 f) {
    float4 v = f * .5f; v.xy = float2(v.x, v.y) + v.w; v.zw = f.zw;
#if FRAMEPLAY_os
    v.z *= .5; v.z += .5;
#endif
    return v;
}fixed D(float4 v) { float4 x = abs(v * 2. - 1.); fixed y = f(float4(x.xyz, 0.), x.w); return y; }struct v2fRQ { float4 jp:TEXCOORD0; float4 xv:SV_POSITION; float4 bl:TEXCOORD1; float4 na:TEXCOORD2; float4 miX:TEXCOORD3; float4 miY:TEXCOORD4; }; struct appdataRQ { float4 xv:POSITION; float2 uv:TEXCOORD0; }; float4 D(appdataRQ v, float4x4 x, float4x4 i) { const float4 z = x._m00_m10_m20_m30, y = x._m01_m11_m21_m31, s = x._m02_m12_m22_m32; float4 a = f(v.uv.xy, z, y, s, i); return t(a); }struct yr { float3 go; float3 tc; }; yr D(float3 f, float3 v) { yr x; x.go = f; x.tc = v; return x; }yr t(float2 f, float4x4 v, float3 i) { float3 y = mul(v, float4(f, 0.f, 1.f)).xyz; return D(i, y); }yr D(float2 f, float3 v, float4x4 x, float4 i) { return D(v + mul((float3x3)x, float3(f, 0.)), i); }float3 f(yr f, float3 v, float3 i, float t) { float x = dot(i, f.tc), y = t / x; float3 z = f.go + y * f.tc; return z; }float3 v(yr f, float3 v, float3 i) { float x = dot(i, f.tc), y = dot(v - f.go, i) / x; float3 z = f.go + y * f.tc; return z; }struct ac { float3 na; float3 bl; float pd; float4x4 nd; float3 cp; float4 miX; float4 miY; }; float4x4 _F_V, _F_N, _F_CF[4]; float4 _F_IX[4], _F_IY[4], _F_S, _F_C, _F_F; sampler2D _AdTex0, _AdTex1, _AdTex2, _AdTex3; float4 _DerivativeMultipliers; half2 t(float2 i, ac x) {
    yr z; float3 y;
#if FRAMEPLAY_co
    z = D(i, x.cp, x.nd, _F_F); y = v(z, x.bl, x.na);
#else
    z = t(i, x.nd, x.cp); y = f(z, x.bl, x.na, x.pd);
#endif
    half4 d = half4(y, 1.); half2 w = half2(dot(x.miX, d), dot(x.miY, d)); return w;
}v2fRQ vertRQ(appdataRQ v) { int y = (int)(v.xv.z * 4.); float4x4 x = _F_CF[y]; v2fRQ r; r.xv = v.xv; float3 z = x._m00_m10_m20, s = x._m01_m11_m21, i = x._m02_m12_m22; float4 a = f(v.uv.xy, z, s, i, _F_V); r.jp = t(a); r.bl.xyz = z; r.bl.w = _DerivativeMultipliers[y]; r.na = x._m03_m13_m23_m33; r.jp.xy *= _F_S.xy; r.miX = _F_IX[y]; r.miY = _F_IY[y]; return r; }fixed4 fragRQ(v2fRQ v) :SV_Target{ fixed4 x = 0; float2 z = v.jp.xy / v.jp.w; z = floor(z) + .5; z = z * _F_S.zw - 1; ac r; r.cp = _F_C; r.nd = _F_N; r.miX = v.miX; r.miY = v.miY; r.na = v.na.xyz; r.bl = v.bl.xyz; r.pd = v.na.w; float2 i = t(z,r); const float2 y = float2(_F_S.z,0.),s = float2(0.,_F_S.w); half2 d = t(z + y,r),w = t(z + s,r),e = i.xy - d,c = i.xy - w; float a = v.bl.w; e *= a; c *= a; float l = 0,h = 1;
#if SHADER_API_GLES3||SHADER_API_GLES||SHADER_API_GLCORE
l = .5; h = .5;
#endif
bool u = v.xv.z == 0 * h + l,m = v.xv.z == 1 / 4. * h + l,n = v.xv.z == 2 / 4. * h + l,R = v.xv.z == 3 / 4. * h + l;
#ifdef SHADER_API_GLES
if (u)x = tex2D(_AdTex0,i); else if (m)x = tex2D(_AdTex1,i); else if (n)x = tex2D(_AdTex2,i); else if (R)x = tex2D(_AdTex3,i);
#else
if (u)x = tex2D(_AdTex0,i,e,c); else if (m)x = tex2D(_AdTex1,i,e,c); else if (n)x = tex2D(_AdTex2,i,e,c); else if (R)x = tex2D(_AdTex3,i,e,c);
#endif

#ifdef UNITY_COLORSPACE_GAMMA

#else
x.xyz = pow(x.xyz,2.2);
#endif
x.z = f(x.xyz); x.xy = 0; return x; }struct v2fSQ { float4 jp:TEXCOORD0; float2 uv:TEXCOORD1; float4 xv:SV_POSITION; }; v2fSQ vertSQ(appdataRQ v) { v2fSQ f; f.xv = v.xv; int x = (int)(v.xv.z * 4.); f.jp = D(v, _F_CF[x], _F_V); f.uv = v.uv; return f; }sampler2D _F_T; fixed4 fragSQ(v2fSQ v) :SV_Target{ fixed4 x = 0;
#if SHADER_API_GLES
x = tex2Dproj(_F_T,v.jp);
#else
v.jp.xy /= v.jp.w; v.jp.w = 0; x = tex2Dlod(_F_T,v.jp);
#endif
fixed i = D(v.jp / v.jp.w); x.y = f(x.xyz) * (1 - i); x.xz = 0; return x; }struct v2f1TB { float2 uv:TEXCOORD0; float4 xv:SV_POSITION; }; v2f1TB vert1TB(appdataRQ v) { v2f1TB f; f.xv = v.xv; f.uv = v.xv.xy * fixed2(.5, -.5) + .5; return f; }sampler2D _F_A; fixed4 fragEA(v2f1TB v) :SV_Target{ const float2 x = float2(1. / 256.,1. / 64.); const fixed3 f = fixed3(1.,-1.,0.); fixed2 z = tex2D(_F_A,f.zx * x + v.uv).yz,y = tex2D(_F_A,f.zy * x + v.uv).yz,t = tex2D(_F_A,f.xz * x + v.uv).yz,w = tex2D(_F_A,f.yz * x + v.uv).yz; half2 i = y - z,s = t - w; half4 a = fixed4(s,i); const fixed2 l = .5; half2 d = atan2(i * .5 + l,s); const float h = .31831; fixed2 m = fixed2(length(a.xz),length(a.yw)); fixed4 r = fixed4((d * h + 1.) * .5,m * 11.7); fixed2 e = r.yw; fixed u = r.x; return float4(e,u,1.); }sampler2D _F_E; fixed4 fragCAS(v2f1TB v) :SV_Target{ const float2 x = float2(1. / 256.,1. / 64.); fixed3 z = fixed3(1.,-1.,0.); fixed4 i = tex2D(_F_E,x * z.xx + v.uv),y = tex2D(_F_E,x * z.yy + v.uv),s = tex2D(_F_E,x * z.xy + v.uv),r = tex2D(_F_E,x * z.yx + v.uv),e = min(i,min(y,min(s,r))),t = max(i,max(y,max(s,r))); fixed a = t.y; const fixed w = .015; fixed d = t.x - e.x < w; const float l = .02; half2 u = frac(half2(v.uv.x * 4,v.uv.y)); fixed2 h = abs(u - .5) > .5 - l; h.x += h.y; a -= h.x; a -= d; const fixed m = .004; e = lerp(e,0,m); t = lerp(t,1,m); fixed4 n = f(e.x,t.x,fixed4(i.x,y.x,s.x,r.x)),c = f(e.z,t.z,fixed4(i.z,y.z,s.z,r.z)); fixed S = .8; fixed4 R = c,X = n; float U = dot(abs(R - X),1.); U = U < S; fixed F = U * a; return fixed4(a,F,a,a); }struct v2fAC { float2 uv:TEXCOORD0; float4 uv2:TEXCOORD1; float4 xv:SV_POSITION; }; sampler2D _F_PC; float4 _F_P, _F_Y, _F_R; float _F_X; sampler2D _F_O; v2fAC vertAC(appdataRQ v) {
    v2fAC f; f.xv = v.xv; int x = (int)(v.xv.z * 4.); float4 i = float4(x == 0, x == 1, x == 2, x == 3); f.uv2.xyw = 0; f.uv2.z = dot(_F_R, i); f.xv.x = _F_X; f.xv.y = _F_Y[x]; f.uv = f.xv;
#if UNITY_UV_STARTS_AT_TOP
    f.xv.y = 1 - f.xv.y;
#endif
    float2 z = v.xv.xy; f.xv.xy += z * _F_P.xy; const float4 y = float4(.125, .375, .125 + .25 * 2, .125 + .25 * 3);
#ifdef SHADER_API_GLES
    f.xv.xy += _F_P.xy * .99; f.uv2.xy = v.uv.xy;
#else
    f.uv2.xyw = float3(y[x], .5, 5); f.xv.xy += _F_P.xy * .9372;
#endif
    f.xv.xy = f.xv.xy * 2. - 1.; return f;
}fixed2 v(sampler2D f, float4 v) {
    fixed4 x = 0;
#ifdef SHADER_API_GLES
    x = tex2D(f, v.xy);
#else
    x = tex2Dlod(f, v);
#endif
    fixed z = x.x; fixed2 y = 0; if (z == 0)y.y = 1; else y.x = x.y / z; return y;
}fixed4 fragAC(v2fAC f) :SV_Target{ fixed2 x = v(_F_O,f.uv2); fixed i = x.x; fixed4 r;
#ifdef FRAMEPLAY_ROLLING_AVERAGE
float4 z = tex2D(_F_PC,f.uv); float y = f.uv2.z; float4 a = (z * (y - 1.) + i) / y; r = fixed4(a.x,a.x,x.y,1.);
#else
r = fixed4(i,i,x.y,1.);
#endif
return r; }sampler2D _MainTex; v2f1TB vertCD(appdataRQ v) { v2f1TB f; f.xv = UnityObjectToClipPos(v.xv); f.uv = v.uv; f.uv.y *= 2.; return f; }fixed4 fragCD(v2f1TB v) :SV_Target{ fixed4 f = tex2D(_MainTex,v.uv); f *= v.uv.y < 1.; return f; }v2f1TB vertBO(appdataRQ v) { v2f1TB f; f.xv = UnityObjectToClipPos(v.xv); f.uv = v.uv; return f; }fixed4 fragBO(v2f1TB v) : SV_Target{ fixed4 x = tex2D(_MainTex,v.uv); return x; }
#endif