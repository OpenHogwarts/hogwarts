Shader "Hidden/TonemappingColorGrading"
{
    Properties
    {
        _MainTex ("", 2D) = "black" {}
    }

    CGINCLUDE

        #pragma vertex vert
        #include "UnityCG.cginc"

        struct v2f {
            half4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
        };

        sampler2D _MainTex;

        sampler3D _LutTex;
        sampler2D _LutTex1D;

        half _LutA;
        half4 _LutExposureMult;

        half _Vibrance;

        v2f vert(appdata_img v)
        {
            v2f o;
            o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
            o.uv = v.texcoord.xy;
            return o;
        }

        half4 fragNonAdaptive1D(v2f i) : SV_Target
        {
            half4 color = tex2D (_MainTex, i.uv);
            half3 x = color.rgb;

            x *= _LutExposureMult.xyz;

            half pad1D = .5f/128.0f;
            half scale1D = 127.0f/128.0f;

            x = _LutA * (x/(1.0f + x));
            half3 padX = x*scale1D + pad1D;
            x.r = tex2D(_LutTex1D,half2(padX.r,.5f)).r;
            x.g = tex2D(_LutTex1D,half2(padX.g,.5f)).g;
            x.b = tex2D(_LutTex1D,half2(padX.b,.5f)).b;

            half3 lum = dot(x, half3(0.2125, 0.7154, 0.0721));
            color.rgb = lum + (x - lum) * _Vibrance;

            return color;
        }

        half4 fragDebug1D(v2f i) : SV_Target
        {
            half4 color = fragNonAdaptive1D(i);
            half highVal = max(color.x,max(color.y, color.z));
            if (highVal >= 254.0f / 255.0f)
                color = half4(1.0, .25, 1.0, 1.0);

            return color;
        }

        half4 fragNonAdaptive(v2f i) : SV_Target
        {
            half4 color = tex2D (_MainTex, i.uv);
            half3 x = color.rgb;

            x *= _LutExposureMult.xyz;

            // offset and scale
            half pad = .5f/32.0f;
            half scale = 31.0f/(32.0f);

            x = _LutA * (x/(1.0f + x));
            x = x*scale + pad;
            x = tex3D(_LutTex,x).xyz;

            color.rgb = x;

            return color;
        }

        half4 fragDebug(v2f i) : SV_Target
        {
            half4 color = fragNonAdaptive(i);
            half highVal = max(color.x,max(color.y, color.z));
            if (highVal >= 254.0f / 255.0f)
                color = half4(1.0, .25, 1.0, 1.0);

            return color;
        }

    ENDCG

    Subshader
    {
        // 1 - Non-adaptive
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
                #pragma fragment fragNonAdaptive
            ENDCG
        }

        // 2 - Non-adaptive 1D
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
                #pragma fragment fragNonAdaptive1D
            ENDCG
        }

        // 3 - Debugging
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
                #pragma fragment fragDebug
            ENDCG
        }

        // 4 - Debugging 1D
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
                #pragma fragment fragDebug1D
            ENDCG
        }
    }

    Fallback off
}
