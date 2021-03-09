// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Composite"
{
	Properties
	{
		[HideInInspector] _MainTex ("", 2D) = "" {}
	}
	
	SubShader
	{
		Pass
		{
			Lighting Off
			Fog { Mode off }
			ZWrite Off
			ZTest Always
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : POSITION;
				half2 uv0 : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
			};
			
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform sampler2D _HighlightingBuffer;
			
			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.texcoord.xy;
				o.uv1 = v.texcoord.xy;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
				{
					o.uv1.y = 1-o.uv1.y;
					#if defined(UNITY_HALF_TEXEL_OFFSET)
					o.uv1.y -= _MainTex_TexelSize.y;
					#endif
				}
				#endif
				
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 c1 = tex2D(_MainTex, i.uv0);
				fixed4 c2 = tex2D(_HighlightingBuffer, i.uv1);
				c1.rgb = lerp(c1.rgb, c2.rgb, c2.a);
				return c1;
			}
			ENDCG
		}
	}
	FallBack Off
}