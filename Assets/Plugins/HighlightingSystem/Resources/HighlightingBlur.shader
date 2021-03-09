// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Blur"
{
	Properties
	{
		[HideInInspector] _MainTex ("", 2D) = "" {}
		[HideInInspector] _Intensity ("", Range (0.25,0.5)) = 0.3
	}
	
	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Lighting Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			
			uniform half _HighlightingBlurOffset;
			uniform half _Intensity;
			
			struct v2f
			{
				float4 pos : POSITION;
				float2 uv[4] : TEXCOORD0;	// 8 to add straight directions
			};
			
			v2f vert (appdata_img v)
			{
				// Shader code optimized for the Unity shader compiler
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float2 offs = _HighlightingBlurOffset * _MainTex_TexelSize.xy;
				
				// Diagonal directions
				o.uv[0].x = v.texcoord.x - offs.x;
				o.uv[0].y = v.texcoord.y - offs.y;
				
				o.uv[1].x = v.texcoord.x + offs.x;
				o.uv[1].y = v.texcoord.y - offs.y;
				
				o.uv[2].x = v.texcoord.x + offs.x;
				o.uv[2].y = v.texcoord.y + offs.y;
				
				o.uv[3].x = v.texcoord.x - offs.x;
				o.uv[3].y = v.texcoord.y + offs.y;
				
				/*
				// Straight directions
				o.uv[4].x = v.texcoord.x - offs.x;
				o.uv[4].y = v.texcoord.y;
				
				o.uv[5].x = v.texcoord.x + offs.x;
				o.uv[5].y = v.texcoord.y;
				
				o.uv[6].x = v.texcoord.x;
				o.uv[6].y = v.texcoord.y - offs.y;
				
				o.uv[7].x = v.texcoord.x;
				o.uv[7].y = v.texcoord.y + offs.y;
				*/
				
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				int start = 0;
				int end = 4;
				
				half4 color1 = tex2D(_MainTex, i.uv[start]);
				fixed4 color2;
				for (int n = start + 1; n < end; n++)
				{
					color2 = tex2D(_MainTex, i.uv[n]);
					color1.rgb = max(color1.rgb, color2.rgb);
					color1.a += color2.a;
				}
				
				color1.a *= _Intensity;
				return color1;
			}
			ENDCG
		}
	}
	
	Fallback off
}