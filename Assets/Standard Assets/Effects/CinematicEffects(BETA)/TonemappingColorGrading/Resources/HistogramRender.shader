// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/TonemappingColorGradingHistogram"
{
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Fog { Mode off }

        CGINCLUDE

            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 5.0
            #include "UnityCG.cginc"

            struct v_data
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            StructuredBuffer<uint4> _Histogram;
            float2 _Size;
            uint _Channel;
            float4 _ColorR;
            float4 _ColorG;
            float4 _ColorB;
            float4 _ColorL;

            v_data vert(appdata_img v)
            {
                v_data o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 frag_channel(v_data i) : SV_Target
            {
                const float4 COLORS[4] = { _ColorR, _ColorG, _ColorB, _ColorL };

                float remapI = i.uv.x * 255.0;
                uint index = floor(remapI);
                float delta = frac(remapI);
                float v1 = _Histogram[index][_Channel];
                float v2 = _Histogram[min(index + 1, 255)][_Channel];
                float h = v1 * (1.0 - delta) + v2 * delta;
                uint y = (uint)round(i.uv.y * _Size.y);

                float4 color = float4(0.0, 0.0, 0.0, 0.0);
                float fill = step(y, h);
                color = lerp(color, COLORS[_Channel], fill);
                return color;
            }

            float4 frag_rgb(v_data i) : SV_Target
            {
                const float4 COLORS[3] = { _ColorR, _ColorG, _ColorB };

                float4 targetColor = float4(0.0, 0.0, 0.0, 0.0);
                float4 emptyColor = float4(0.0, 0.0, 0.0, 0.0);
                float fill = 0;

                for (int j = 0; j < 3; j++)
                {
                    float remapI = i.uv.x * 255.0;
                    uint index = floor(remapI);
                    float delta = frac(remapI);
                    float v1 = _Histogram[index][j];
                    float v2 = _Histogram[min(index + 1, 255)][j];
                    float h = v1 * (1.0 - delta) + v2 * delta;
                    uint y = (uint)round(i.uv.y * _Size.y);
                    float fill = step(y, h);
                    float4 color = lerp(emptyColor, COLORS[j], fill);
                    targetColor += color;
                }

                return saturate(targetColor);
            }

        ENDCG

        // (0) Channel
        Pass
        {
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag_channel

            ENDCG
        }

        // (1) RGB
        Pass
        {
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag_rgb

            ENDCG
        }
    }
    FallBack off
}
