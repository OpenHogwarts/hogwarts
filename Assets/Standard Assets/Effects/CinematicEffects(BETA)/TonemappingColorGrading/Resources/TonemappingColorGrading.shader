Shader "Hidden/TonemappingColorGrading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE

        #pragma vertex vert_img
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma target 3.0

    ENDCG

    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Fog { Mode off }

        // Lut generator
        Pass
        {
            CGPROGRAM

                #pragma fragment frag_lut_gen
                #include "TonemappingColorGrading.cginc"

                sampler2D _UserLutTex;
                half4 _UserLutParams;

                half3 _WhiteBalance;
                half3 _Lift;
                half3 _Gamma;
                half3 _Gain;
                half3 _ContrastGainGamma;
                half _Vibrance;
                half3 _HSV;
                half3 _ChannelMixerRed;
                half3 _ChannelMixerGreen;
                half3 _ChannelMixerBlue;
                sampler2D _CurveTex;
                half _Contribution;

                static const half3x3 LIN_2_LMS_MAT = {
                    3.90405e-1, 5.49941e-1, 8.92632e-3,
                    7.08416e-2, 9.63172e-1, 1.35775e-3,
                    2.31082e-2, 1.28021e-1, 9.36245e-1
                };

                static const half3x3 LMS_2_LIN_MAT = {
                     2.85847e+0, -1.62879e+0, -2.48910e-2,
                    -2.10182e-1,  1.15820e+0,  3.24281e-4,
                    -4.18120e-2, -1.18169e-1,  1.06867e+0
                };

                half3 rgb_to_hsv(half3 c)
                {
                    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
                    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));
                    half d = q.x - min(q.w, q.y);
                    half e = 1.0e-4;
                    return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
                }

                half3 hsv_to_rgb(half3 c)
                {
                    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                    half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
                }

                // CG's fmod() is not the same as GLSL's mod() with negative values, we'll use our own
                inline half gmod(half x, half y)
                {
                    return x - y * floor(x / y);
                }

                half4 frag_lut_gen(v2f_img i) : SV_Target
                {
                    half3 neutral_lut = tex2D(_MainTex, i.uv).rgb;
                    half3 final_lut = neutral_lut;

                    // User lut + contrib
                    half3 user_luted = apply_lut(_UserLutTex, final_lut, _UserLutParams.xyz);
                    final_lut = lerp(final_lut, user_luted, _UserLutParams.w);

                    // White balance
                    half3 lms = mul(LIN_2_LMS_MAT, final_lut);
                    lms *= _WhiteBalance;
                    final_lut = mul(LMS_2_LIN_MAT, lms);

                    // Lift/gamma/gain
                    final_lut = _Gain * (_Lift * (1.0 - final_lut) + pow(final_lut, _Gamma));
                    final_lut = max(final_lut, 0.0);

                    // Hue/saturation/value
                    half3 hsv = rgb_to_hsv(final_lut);
                    hsv.x = gmod(hsv.x + _HSV.x, 1.0);
                    hsv.yz *= _HSV.yz;
                    final_lut = saturate(hsv_to_rgb(hsv));

                    // Vibrance
                    half sat = max(final_lut.r, max(final_lut.g, final_lut.b)) - min(final_lut.r, min(final_lut.g, final_lut.b));
                    final_lut = lerp(Luminance(final_lut).xxx, final_lut, (1.0 + (_Vibrance * (1.0 - (sign(_Vibrance) * sat)))));

                    // Contrast
                    final_lut = saturate((final_lut - 0.5) * _ContrastGainGamma.x + 0.5);

                    // Gain
                    half f = pow(2.0, _ContrastGainGamma.y) * 0.5;
                    final_lut = (final_lut < 0.5) ? pow(final_lut, _ContrastGainGamma.y) * f : 1.0 - pow(1.0 - final_lut, _ContrastGainGamma.y) * f;

                    // Gamma
                    final_lut = pow(final_lut, _ContrastGainGamma.z);

                    // Color mixer
                    final_lut = half3(
                        dot(final_lut, _ChannelMixerRed),
                        dot(final_lut, _ChannelMixerGreen),
                        dot(final_lut, _ChannelMixerBlue)
                    );

                    // Curves
                    half mr = tex2D(_CurveTex, half2(final_lut.r, 0.5)).a;
                    half mg = tex2D(_CurveTex, half2(final_lut.g, 0.5)).a;
                    half mb = tex2D(_CurveTex, half2(final_lut.b, 0.5)).a;
                    final_lut = half3(mr, mg, mb);
                    half r = tex2D(_CurveTex, half2(final_lut.r, 0.5)).r;
                    half g = tex2D(_CurveTex, half2(final_lut.g, 0.5)).g;
                    half b = tex2D(_CurveTex, half2(final_lut.b, 0.5)).b;
                    final_lut = half3(r, g, b);

                    return half4(final_lut, 1.0);
                }

            ENDCG
        }

        // The three following passes are used to get an average log luminance using a downsample pyramid
        Pass
        {
            CGPROGRAM
                #pragma fragment frag_log
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
                #pragma fragment frag_exp
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        Pass
        {
            Blend Off

            CGPROGRAM
                #pragma fragment frag_exp
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping off
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping (ACES)
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #define TONEMAPPING_ACES
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping (Curve)
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #define TONEMAPPING_CURVE
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping (Hable)
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #define TONEMAPPING_HABLE
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping (Hejl-Dawson)
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #define TONEMAPPING_HEJL_DAWSON
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping (Photographic)
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #define TONEMAPPING_PHOTOGRAPHIC
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Tonemapping (Reinhard)
        Pass
        {
            CGPROGRAM
                #pragma multi_compile __ GAMMA_COLORSPACE
                #pragma multi_compile __ ENABLE_COLOR_GRADING
                #pragma multi_compile __ ENABLE_EYE_ADAPTATION
                #pragma multi_compile __ ENABLE_DITHERING
                #pragma fragment frag_tcg
                #define TONEMAPPING_REINHARD
                #include "TonemappingColorGrading.cginc"
            ENDCG
        }

        // Eye adaptation debug slider
        Pass
        {
            CGPROGRAM
                #pragma fragment frag_debug
                #include "TonemappingColorGrading.cginc"

                half4 frag_debug(v2f_img i) : SV_Target
                {
                    half lum = tex2D(_MainTex, i.uv).r;
                    half grey = i.uv.x;

                    int lum_px = floor(256.0 * lum);
                    int g_px = floor(256.0 * grey);

                    half3 color = half3(grey, grey, grey);
                    color = lerp(color, half3(1.0, 0.0, 0.0), lum_px == g_px);

                    return half4(color, 1.0);
                }
            ENDCG
        }
    }
}
