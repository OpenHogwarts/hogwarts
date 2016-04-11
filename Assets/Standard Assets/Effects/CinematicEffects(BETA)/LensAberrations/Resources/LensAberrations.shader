Shader "Hidden/LensAberrations"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Fog { Mode off }

        CGINCLUDE

            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile __ DISTORT UNDISTORT
            #pragma multi_compile __ CHROMATIC_SIMPLE CHROMATIC_ADVANCED
            #pragma multi_compile __ VIGNETTE_ADVANCED
            #include "UnityCG.cginc"
            #pragma target 3.0

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            half4 _DistCenterScale;
            half3 _DistAmount;
            half4 _ChromaticAberration;
            half4 _Vignette1;
            half3 _Vignette2;
            half4 _VignetteColor;

            sampler2D _BlurTex;
            half2 _BlurPass;

            struct v2f
            {
                half4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half4 uv1 : TEXCOORD1;
                half4 uv2 : TEXCOORD2;
            };

            v2f vert_blur_prepass(appdata_img v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;

            #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    o.uv.y = 1.0 - o.uv.y;
            #endif

                half2 d1 = 1.3846153846 * _BlurPass;
                half2 d2 = 3.2307692308 * _BlurPass;
                o.uv1 = half4(o.uv + d1, o.uv - d1);
                o.uv2 = half4(o.uv + d2, o.uv - d2);
                return o;
            }

            half4 frag_blur_prepass(v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);
                half3 c = color.rgb * 0.2270270270;
                c += tex2D(_MainTex, i.uv1.xy).rgb * 0.3162162162;
                c += tex2D(_MainTex, i.uv1.zw).rgb * 0.3162162162;
                c += tex2D(_MainTex, i.uv2.xy).rgb * 0.0702702703;
                c += tex2D(_MainTex, i.uv2.zw).rgb * 0.0702702703;
                return half4(c, color.a);
            }

            #define DISK_SAMPLE_NUM 9
            static const half2 SmallDiscKernel[DISK_SAMPLE_NUM] =
            {
                half2(-0.926212,-0.40581),
                half2(-0.695914, 0.457137),
                half2(-0.203345, 0.820716),
                half2( 0.96234, -0.194983),
                half2( 0.473434,-0.480026),
                half2( 0.519456, 0.767022),
                half2( 0.185461,-0.893124),
                half2( 0.89642,  0.412458),
                half2(-0.32194, -0.932615),
            };

            half4 chromaticAberration(half4 color, half2 uv)
            {
#if CHROMATIC_SIMPLE
                half2 coords = (uv - 0.5) * 2.0;
                half2 uvg = uv - _MainTex_TexelSize.xy * _ChromaticAberration.x * coords * dot(coords, coords);
                color.g = tex2D(_MainTex, uvg).g;
#elif CHROMATIC_ADVANCED
                half2 coords = (uv - 0.5) * 2.0;
                half tangentialStrength = _ChromaticAberration.x * dot(coords, coords);
                half uvg = -(_ChromaticAberration.y > abs(tangentialStrength) ? sign(tangentialStrength) * _ChromaticAberration.y : tangentialStrength);
                half2 offset = _MainTex_TexelSize.xy * uvg * coords;
                half3 blurredTap = color.rgb * 0.1;

                for (int l = 0; l < DISK_SAMPLE_NUM; l++)
                {
                    half2 sampleUV = uv + _MainTex_TexelSize.xy * SmallDiscKernel[l].xy + offset;
                    half3 tap = tex2D(_MainTex, sampleUV).rgb;
                    blurredTap += tap;
                }

                blurredTap /= (half)DISK_SAMPLE_NUM + 0.2;
                half contrast = saturate(_ChromaticAberration.z * Luminance(abs(blurredTap - color.rgb)));
                color.g = lerp(color.g, blurredTap.g, contrast);
#endif
                return color;
            }

            half2 distort(half2 uv)
            {
#if DISTORT
                uv = (uv - 0.5) * _DistAmount.z + 0.5;
                half2 ruv = _DistCenterScale.zw * (uv - 0.5 - _DistCenterScale.xy);
                half ru = length(ruv);
                half wu = ru * _DistAmount.x;
                ru = tan(wu) * (1.0 / (ru * _DistAmount.y));
                uv = uv + ruv * (ru - 1.0);
#elif UNDISTORT
                uv = (uv - 0.5) * _DistAmount.z + 0.5;
                half2 ruv = _DistCenterScale.zw * (uv - 0.5 - _DistCenterScale.xy);
                half ru = length(ruv);
                ru = (1.0 / ru) * _DistAmount.x * atan(ru * _DistAmount.y);
                uv = uv + ruv * (ru - 1.0);
#endif
                return uv;
            }

            half get_vignette_factor(half2 uv)
            {
#if VIGNETTE_ADVANCED
                uv = abs(uv - 0.5);
                half x = saturate((pow(pow(uv.x, _Vignette2.z) + pow(uv.y, _Vignette2.z), 1.0 / _Vignette2.z) - _Vignette1.x) * _Vignette1.y);
                half ix = 1.0 - x;
                half v = _Vignette2.x == 0.5 ? x : x <= _Vignette2.x ? x * x / ((2.0 - _Vignette2.y) * x + _Vignette2.x * (_Vignette2.y - 1.0)) : 1.0 + ix * ix / ((_Vignette2.y - 2.0) * ix + (_Vignette2.x - 1.0) * (_Vignette2.y - 1.0));
                return 1.0 - v;
#else
                half2 d = (uv - 0.5) * _Vignette1.x;
                return pow(saturate(1.0 - dot(d, d)), _Vignette1.y);
#endif
            }

            half4 vignette_simple(half4 color, half2 uv)
            {
                half v = get_vignette_factor(uv);
                color.rgb = lerp(_VignetteColor.rgb, color.rgb, lerp(1.0, v, _VignetteColor.a));
                return color;
            }

            half4 vignette_desat(half4 color, half2 uv)
            {
                half v = get_vignette_factor(uv);
                half lum = Luminance(color);
                color.rgb = lerp(lerp(lum.xxx, color.rgb, _Vignette1.w), color.rgb, v);
                color.rgb = lerp(_VignetteColor.rgb, color.rgb, lerp(1.0, v, _VignetteColor.a));
                return color;
            }

            half4 vignette_blur(half4 color, half2 uv)
            {
                half2 coords = (uv - 0.5) * 2.0;
                half v = get_vignette_factor(uv);
                half3 blur = tex2D(_BlurTex, uv);
                color.rgb = lerp(color.rgb, blur, saturate(_Vignette1.z * dot(coords, coords)));
                color.rgb = lerp(_VignetteColor.rgb, color.rgb, lerp(1.0, v, _VignetteColor.a));
                return color;
            }

            half4 vignette_blur_desat(half4 color, half2 uv)
            {
                half2 coords = (uv - 0.5) * 2.0;
                half v = get_vignette_factor(uv);
                half3 blur = tex2D(_BlurTex, uv);
                color.rgb = lerp(color.rgb, blur, saturate(_Vignette1.z * dot(coords, coords)));
                half lum = Luminance(color);
                color.rgb = lerp(lerp(lum.xxx, color.rgb, _Vignette1.w), color.rgb, v);
                color.rgb = lerp(_VignetteColor.rgb, color.rgb, lerp(1.0, v, _VignetteColor.a));
                return color;
            }

            half4 frag_simple(v2f_img i) : SV_Target
            {
                half2 uv = distort(i.uv);
                half4 color = tex2D(_MainTex, uv);
                color = chromaticAberration(color, uv);
                color = vignette_simple(color, uv);
                return color;
            }

            half4 frag_desat(v2f_img i) : SV_Target
            {
                half2 uv = distort(i.uv);
                half4 color = tex2D(_MainTex, uv);
                color = chromaticAberration(color, uv);
                color = vignette_desat(color, uv);
                return color;
            }

            half4 frag_blur(v2f_img i) : SV_Target
            {
                half2 uv = distort(i.uv);
                half4 color = tex2D(_MainTex, uv);
                color = chromaticAberration(color, uv);
                color = vignette_blur(color, uv);
                return color;
            }

            half4 frag_blur_desat(v2f_img i) : SV_Target
            {
                half2 uv = distort(i.uv);
                half4 color = tex2D(_MainTex, uv);
                color = chromaticAberration(color, uv);
                color = vignette_blur_desat(color, uv);
                return color;
            }

            half4 frag_chroma_only(v2f_img i) : SV_Target
            {
                half2 uv = distort(i.uv);
                half4 color = tex2D(_MainTex, uv);
                color = chromaticAberration(color, uv);
                return color;
            }

            half4 frag_distort_only(v2f_img i) : SV_Target
            {
                half2 uv = distort(i.uv);
                return tex2D(_MainTex, uv);
            }

        ENDCG

        // (0) Blur pre-pass
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_blur_prepass
                #pragma fragment frag_blur_prepass
            ENDCG
        }

        // (1) Vignette simple
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_simple
            ENDCG
        }

        // (2) Vignette desat
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_desat
            ENDCG
        }

        // (3) Vignette blur
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_blur
            ENDCG
        }

        // (4) Vignette blur desat
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_blur_desat
            ENDCG
        }

        // (5) Chromatic aberration only
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_chroma_only
            ENDCG
        }

        // (6) Distort only
        Pass
        {
            CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag_distort_only
            ENDCG
        }
    }
    FallBack off
}
