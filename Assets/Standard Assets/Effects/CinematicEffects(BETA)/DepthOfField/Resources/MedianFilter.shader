// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/DepthOfField/MedianFilter"
{

Properties
{
    _MainTex ("-", 2D) = "black"
}

CGINCLUDE
#pragma target 3.0
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

sampler2D _MainTex;
uniform half4 _MainTex_TexelSize;
uniform half4 _MainTex_ST;
uniform float4 _Offsets;

///////////////////////////////////////////////////////////////////////////////
// Verter Shaders and declaration
///////////////////////////////////////////////////////////////////////////////

struct v2f
{
    half4 pos  : SV_POSITION;
    half2 uv   : TEXCOORD0;
};

v2f vert(appdata_img v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
    o.uv.y = 1-o.uv.y;
#endif
    return o;
}

///////////////////////////////////////////////////////////////////////////////

float4 fragCocMedian3 (v2f i) : SV_Target
{
    //TODO use med3 on GCN architecture.
    half4 A = tex2Dlod(_MainTex, half4(i.uv,0,0));
    half3 B = tex2Dlod(_MainTex, half4(i.uv + _Offsets.xy * _MainTex_TexelSize.xy,0,0)).rgb;
    half3 C = tex2Dlod(_MainTex, half4(i.uv - _Offsets.xy * _MainTex_TexelSize.xy,0,0)).rgb;

    half3 minAB = min(A.rgb,B);
    half3 maxAB = max(A.rgb,B);
    half3 temp   = min(C, maxAB);
    half3 median = max(minAB, temp);

    return half4(lerp(A.rgb, median, A.a*A.a), A.a);
}

/*/////////////////////////////////////////////////////////////////////////////
3x3 Median
Morgan McGuire and Kyle Whitson
http://graphics.cs.williams.edu

Copyright (c) Morgan McGuire and Williams College, 2006
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#define s2(a, b)                temp = a; a = min(a, b); b = max(temp, b);
#define mn3(a, b, c)            s2(a, b); s2(a, c);
#define mx3(a, b, c)            s2(b, c); s2(a, c);
#define mnmx3(a, b, c)          mx3(a, b, c); s2(a, b);
#define mnmx4(a, b, c, d)       s2(a, b); s2(c, d); s2(a, c); s2(b, d);
#define mnmx5(a, b, c, d, e)    s2(a, b); s2(c, d); mn3(a, c, e); mx3(b, d, e);
#define mnmx6(a, b, c, d, e, f) s2(a, d); s2(b, e); s2(c, f); mn3(a, b, c); mx3(d, e, f);

float4 fragCocMedian3x3 (v2f i) : SV_Target
{
    half4 center = tex2Dlod(_MainTex, half4(i.uv + half2(0, 0) * _MainTex_TexelSize.xy,0,0));

    half3 v[9];
    // Add the pixels which make up our window to the pixel array.
    v[0] = tex2Dlod(_MainTex, half4(i.uv + half2(-1, -1) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[1] = tex2Dlod(_MainTex, half4(i.uv + half2(-1,  0) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[2] = tex2Dlod(_MainTex, half4(i.uv + half2(-1,  1) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[3] = tex2Dlod(_MainTex, half4(i.uv + half2( 0, -1) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[4] = center.rgb;
    v[5] = tex2Dlod(_MainTex, half4(i.uv + half2( 0,  1) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[6] = tex2Dlod(_MainTex, half4(i.uv + half2( 1, -1) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[7] = tex2Dlod(_MainTex, half4(i.uv + half2( 1,  0) * _MainTex_TexelSize.xy,0,0)).rgb;
    v[8] = tex2Dlod(_MainTex, half4(i.uv + half2( 1,  1) * _MainTex_TexelSize.xy,0,0)).rgb;

    half3 temp;
    // TODO use med3 on GCN architecture.
    // Starting with a subset of size 6, remove the min and max each time
    mnmx6(v[0], v[1], v[2], v[3], v[4], v[5]);
    mnmx5(v[1], v[2], v[3], v[4], v[6]);
    mnmx4(v[2], v[3], v[4], v[7]);
    mnmx3(v[3], v[4], v[8]);

    return half4(lerp(center.rgb, v[4], center.a*center.a), center.a);
}
#undef s2
#undef mn3
#undef mx3
#undef mnmx3
#undef mnmx4
#undef mnmx5
#undef mnmx6

///////////////////////////////////////////////////////////////////////////////

ENDCG

SubShader
{

    ZTest Always Cull Off ZWrite Off Fog { Mode Off } Lighting Off Blend Off

    // 0
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCocMedian3
        ENDCG
    }

    // 15
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCocMedian3x3
        ENDCG
    }
}

FallBack Off
}
