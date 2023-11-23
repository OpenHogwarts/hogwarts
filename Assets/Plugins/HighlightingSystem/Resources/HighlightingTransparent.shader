// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Transparent"
{
	Properties
	{
		[HideInInspector] _MainTex ("", 2D) = "" {}
		[HideInInspector] _Cutoff ("", Float) = 0.5
		[HideInInspector] _ZTest ("", Float) = 8		// UnityEngine.Rendering.CompareFunction. 4 = LessEqual, 8 = Always
		[HideInInspector] _StencilRef ("", Float) = 1
		[HideInInspector] _Cull ("", Float) = 2			// UnityEngine.Rendering.CullMode.Back
	}
	
	SubShader
	{
		Pass
		{
			Lighting Off
			Fog { Mode Off }
			ZWrite [_HighlightingZWrite]
			ZTest [_ZTest]
			Offset [_HighlightingOffsetFactor], [_HighlightingOffsetUnits]
			Cull [_Cull]
			Stencil
			{
				Ref [_StencilRef]
				Comp Always
				Pass Replace
				ZFail Keep
			}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			uniform fixed4 _Outline;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed _Cutoff;

			struct appdata_vert_tex
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed alpha : TEXCOORD1;
			};

			v2f vert(appdata_vert_tex v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.alpha = v.color.a;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed a = tex2D(_MainTex, i.texcoord).a;
				clip(a - _Cutoff);
				fixed4 c = _Outline;
				c.a *= i.alpha;
				return c;
			}
			ENDCG
		}
	}
	
	Fallback Off
}