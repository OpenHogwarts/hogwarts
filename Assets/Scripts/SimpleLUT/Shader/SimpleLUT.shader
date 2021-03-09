// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SimpleLUT"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
		_Amount("Amount of Color Filter (0 - 1)", float) = 1.0
		_Tint("Tint (RGB)", Color) = (1, 1, 1, 1)
		_Hue("Hue (0 - 360)", float) = 0
		_Saturation("Saturation (0 - 2)", float) = 1.0
		_Brightness("Brightness (0 - 3)", float) = 1.0
		_Contrast("Contrast (0 - 2)", float) = 1.0
		_Sharpness("Sharpness (-4 - 4)", float) = 0
	}

	CGINCLUDE

#include "UnityCG.cginc"

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
	};

	uniform sampler2D _MainTex;
	uniform sampler3D _ClutTex;
	uniform float _Amount;
	uniform float4 _TintColor;
	uniform float _Hue;
	uniform float _Saturation;
	uniform float _Brightness;
	uniform float _Contrast;
	uniform float _Scale;
	uniform float _Offset;
	uniform float2 _ImageWidthFactor;
	uniform float2 _ImageHeightFactor;
	uniform float _SharpnessCenterMultiplier;
	uniform float _SharpnessEdgeMultiplier;

	inline float3 applyHue(float3 aColor)
	{
		float angle = radians(_Hue);
		float3 k = float3(0.57735, 0.57735, 0.57735);
		float cosAngle = cos(angle);

		return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
	}

	inline float3 applyHSBEffect(float3 c)
	{
		c.rgb = applyHue(c.rgb);
		c.rgb = ((c.rgb - 0.5f) * _Contrast) + 0.5f;
		c.rgb *= _Brightness;
		float3 intensity = dot(c.rgb, float3(0.299, 0.587, 0.114));
		c.rgb = lerp(intensity, c.rgb, _Saturation);

		return c;
	}

	inline float3 applySharpness(float3 c, float2 uv)
	{
		return (c * _SharpnessCenterMultiplier) -
		(
			(tex2D(_MainTex, uv - _ImageWidthFactor).rgb * _SharpnessEdgeMultiplier) +
			(tex2D(_MainTex, uv + _ImageWidthFactor).rgb * _SharpnessEdgeMultiplier) +
			(tex2D(_MainTex, uv + _ImageHeightFactor).rgb * _SharpnessEdgeMultiplier) +
			(tex2D(_MainTex, uv - _ImageHeightFactor).rgb * _SharpnessEdgeMultiplier)
		);
	}

	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;

		return o;
	}

	float4 frag(v2f i) : SV_Target
	{
		float4 c = tex2D(_MainTex, i.uv);
		c.rgb = applySharpness(c.rgb, i.uv);
		c.rgb = applyHSBEffect(c.rgb);
		float3 correctedColor = tex3D(_ClutTex, c.rgb * _Scale + _Offset).rgb;
		c.rgb = lerp(c.rgb, correctedColor, _Amount) * _TintColor;

        return c;
	}

	float4 fragLinear(v2f i) : SV_Target
	{
		float4 c = tex2D(_MainTex, i.uv);
		c.rgb = applySharpness(c.rgb, i.uv);
		c.rgb = sqrt(c.rgb);
		c.rgb = applyHSBEffect(c.rgb);
		float3 correctedColor = tex3D(_ClutTex, c.rgb * _Scale + _Offset).rgb;
		c.rgb = lerp(c.rgb, correctedColor, _Amount) * _TintColor;
		c.rgb *= c.rgb;

		return c;
	}

	ENDCG

	Subshader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vert
#pragma fragment fragLinear
#pragma target 3.0
			ENDCG
		}
	}

	Fallback off
}
