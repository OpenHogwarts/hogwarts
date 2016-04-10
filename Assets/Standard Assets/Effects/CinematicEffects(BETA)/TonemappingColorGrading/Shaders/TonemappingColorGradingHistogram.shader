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
            half2 _Size;
            uint _Channel;
            half4 _ColorR;
            half4 _ColorG;
            half4 _ColorB;
            half4 _ColorL;

            v_data vert(appdata_img v)
            {
                v_data o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag_channel(v_data i) : COLOR
            {
                const half4 COLORS[4] = { _ColorR, _ColorG, _ColorB, _ColorL };

                half remapI = i.uv.x * 255.0;
                uint index = floor(remapI);
                half delta = frac(remapI);
                half v1 = _Histogram[index][_Channel];
                half v2 = _Histogram[min(index + 1, 255)][_Channel];
                half h = v1 * (1.0 - delta) + v2 * delta;
                uint y = (uint)round(i.uv.y * _Size.y);

                half4 color = half4(0.0, 0.0, 0.0, 0.0);
                half fill = step(y, h);
                color = lerp(color, COLORS[_Channel], fill);
                return color;
            }

            half4 frag_rgb(v_data i) : COLOR
            {
                const half4 COLORS[3] = { _ColorR, _ColorG, _ColorB };

                half4 targetColor = half4(0.0, 0.0, 0.0, 0.0);
                half4 emptyColor = half4(0.0, 0.0, 0.0, 0.0);
                half fill = 0;

                for (int j = 0; j < 3; j++)
                {
                    half remapI = i.uv.x * 255.0;
                    uint index = floor(remapI);
                    half delta = frac(remapI);
                    half v1 = _Histogram[index][j];
                    half v2 = _Histogram[min(index + 1, 255)][j];
                    half h = v1 * (1.0 - delta) + v2 * delta;
                    uint y = (uint)round(i.uv.y * _Size.y);
                    half fill = step(y, h);
                    half4 color = lerp(emptyColor, COLORS[j], fill);
                    targetColor += color;
                }

                return saturate(targetColor);
            }

        ENDCG

        // 0 - Channel
        Pass
        {
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag_channel

            ENDCG
        }

        // 1 - RGB
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
