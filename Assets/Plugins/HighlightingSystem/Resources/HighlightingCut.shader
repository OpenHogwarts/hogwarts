Shader "Hidden/Highlighted/Cut"
{
	SubShader
	{
		Lighting Off
		Fog { Mode off }
		ZWrite Off
		ZTest Always
		Cull Back
		Blend Off
		
		Pass
		{
			Stencil
			{
				Ref 1
				Comp NotEqual
				Pass Keep
				ZFail Keep
			}
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};
			
			uniform sampler2D _HighlightingBlurred;
			uniform float2 _HighlightingBufferTexelSize;
			
			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = v.vertex;
				
				#if defined(UNITY_HALF_TEXEL_OFFSET)
				o.pos.xy += _HighlightingBufferTexelSize;
				#endif
				
				o.uv = v.texcoord.xy;
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				return tex2D(_HighlightingBlurred, i.uv);
			}
			ENDCG
		}
		
		Pass
		{
			Stencil
			{
				Ref 1
				Comp Equal
				Pass Keep
				ZFail Keep
			}
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			struct appdata_vert
			{
				float4 vertex : POSITION;
			};
			
			uniform float2 _HighlightingBufferTexelSize;
			
			float4 vert(appdata_vert v) : POSITION
			{
				float4 pos = v.vertex;
				
				#if defined(UNITY_HALF_TEXEL_OFFSET)
				pos.xy += _HighlightingBufferTexelSize;
				#endif
				
				return pos;
			}
			
			fixed4 frag() : COLOR
			{
				return 0;
			}
			ENDCG
		}
	}
	FallBack Off
}
