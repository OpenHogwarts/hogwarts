// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Opaque"
{
	Properties
	{
		[HideInInspector] _ZTest ("", Float) = 8		// UnityEngine.Rendering.CompareFunction. 4 = LessEqual, 8 = Always
		[HideInInspector] _StencilRef ("", Float) = 1
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
			
			struct appdata_vert
			{
				float4 vertex : POSITION;
			};
			
			float4 vert(appdata_vert v) : POSITION
			{
				return UnityObjectToClipPos(v.vertex);
			}
			
			fixed4 frag() : COLOR
			{
				return _Outline;
			}
			ENDCG
		}
	}
	Fallback Off
}