Shader "Hidden/SESSAO" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

CGINCLUDE
	#include "UnityCG.cginc"
	#pragma target 3.0
	#pragma glsl
	
	uniform half4 _MainTex_TexelSize;
	
	float4x4 ProjectionMatrixInverse;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;	
		
		#if UNITY_UV_STARTS_AT_TOP
		float4 uv2 : TEXCOORD1;
		#endif
	};
	
	v2f vert(appdata_img v)
	{
		v2f o;
		
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.uv = float4(v.texcoord.xy, 1, 1);		
		
		#if UNITY_UV_STARTS_AT_TOP
			o.uv2 = float4(v.texcoord.xy, 1, 1);				
			if (_MainTex_TexelSize.y < 0.0)
				o.uv.y = 1.0 - o.uv.y;
		#endif
	        	
		return o; 
	}
	
	float3 FLOAT3(float x)
	{
		return x.xxx;
	}
	
	sampler2D _CameraDepthNormalsTexture;
	sampler2D _CameraDepthTexture;
	sampler2D _DepthDownsampled;
	sampler2D _DitherTexture;
	sampler2D _MainTex;
	sampler2D _ColorDownsampled;
	
	float4x4 _WorldToCamera;
	float4x4 _CameraToWorld;
	
	float3 GetViewSpacePosition(float2 coord)
	{
		float depth = tex2Dlod(_CameraDepthTexture, float4(coord.x, coord.y, 0.0, 0.0)).x;
		
		float4 viewPosition = mul(ProjectionMatrixInverse, float4(coord.x * 2.0 - 1.0, coord.y * 2.0 - 1.0, 2.0 * depth - 1.0, 1.0));
			   viewPosition /= viewPosition.w;
		
		return viewPosition.xyz;
	}
	
	float3 GetViewSpacePosition2(float2 coord)
	{
		float depth = tex2D(_DepthDownsampled, coord).x;
		
		float4 viewPosition = mul(ProjectionMatrixInverse, float4(coord.x * 2.0 - 1.0, coord.y * 2.0 - 1.0, 2.0 * depth - 1.0, 1.0));
			   viewPosition /= viewPosition.w;
		
		return viewPosition.xyz;
	}
	
	float3 ProjectBack(float3 viewSpacePosition)
	{
		float depth = -viewSpacePosition.z;
		float x = viewSpacePosition.x / (_ScreenParams.x / _ScreenParams.y);
			  x /= depth;
			  x = x * 0.5 + 0.5;
			  
		float y = viewSpacePosition.y;
			  y /= depth;
			  y = y * 0.5 + 0.5;
			   
		return float3(x, y, depth);
	}
	
	float2 rand(float2 coord)
	{
		float width = 1.0;
		float height = 1.0;
		
//		coord = fmod(coord, _MainTex_TexelSize.xy * 5.0);
		
		float noiseX = saturate(frac(sin(dot(coord, float2(12.9898, 78.223))) * 43758.5453));
		float noiseY = saturate(frac(sin(dot(coord, float2(12.9898, 78.223)*2.0)) * 43758.5453));
		
		return float2(noiseX, noiseY);
	}

	float Radius;
	float Bias;
	float ZThickness;
	float Intensity;
	float SampleDistributionCurve;
	float DrawDistance;
	float DrawDistanceFadeSize;
	int Downsamp;
	int HalfSampling;
ENDCG


SubShader
{
	ZTest Off
	Cull Off
	ZWrite Off
	Fog { Mode off }
		
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			int PreserveDetails;
			int Orthographic;
			
			
			float4 frag(v2f input) : COLOR0
			{			
				float2 coord = input.uv.xy;
				
				float3 viewSpacePosition = GetViewSpacePosition(coord);
				float3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, coord));
				
				float2 noiseCoord = coord * float2(_MainTex_TexelSize.zw);
				       noiseCoord /= (5.0 - PreserveDetails * 2) + (float)Downsamp * (5.0 - PreserveDetails * 2);
				float dither = 0.5;
					  dither = tex2Dlod(_DitherTexture, float4(noiseCoord.xy, 0.0, 0.0)).a;
				
				float3 origin = viewSpacePosition.xyz;
				
				if (origin.z < -DrawDistance)
				return float4(1.0, 1.0, 1.0, 1.0);
				
				int numRaysPassed = 0;
				
				float ao = 0.0f;
				
				int numSamples = (8 + PreserveDetails * 4) - HalfSampling * (4 + PreserveDetails * 2);
				
				float weights = 0.0;

				float radius2 = Radius;
				float spread = radius2 / lerp((-origin.z + 0.0001), 10.0, Orthographic);
				
				const float sweeps = 1.0;
				
				float3 bleed = float3(0.0, 0.0, 0.0);
				
				float4 emphasisDir = mul(_WorldToCamera, float4(0.0, -1.0, 0.0, 0.0));
				emphasisDir.xyz = normalize(emphasisDir.xyz);
				emphasisDir.x *= -1.0f;
				emphasisDir.y *= -1.0f;
				
				for (int i = 0; i < numSamples; i++)
				{
					float2 kernel;
					float fi = (float)i / (float)numSamples;
					kernel.x = sin(fi * 3.14159265 * sweeps * 2.0 + dither * 6.0);
					kernel.y = cos(fi * 3.14159265 * sweeps * 2.0 + dither * 6.0);
					float2 kernelNormalized = kernel;
					kernel.y *= _ScreenParams.x / _ScreenParams.y;
					kernel *= pow(dither, SampleDistributionCurve);
					
					float2 finalKernel = kernel * spread + kernelNormalized * 0.001;
					
					float3 samplePosition = GetViewSpacePosition(clamp(coord.xy + finalKernel, float2(0.0, 0.0), float2(1.0, 1.0)));
					
					float3 sampleVector = normalize(samplePosition - origin);
					
					float angle = dot(sampleVector, normal.xyz);
					
					float weight = 1.0;
					
					float dist = length(samplePosition.z - origin.z);
					dist = max(0.0, dist - 0.8);
					weight = pow(saturate(1.0 - dist / (length(dither) * ZThickness * spread * 25.5)), 1.0);
					
					weights += weight;
					
					float3 colorSample = max(float3(0,0,0), tex2Dlod(_ColorDownsampled, float4(coord.xy + finalKernel * 1.0, 0.0, 0.0)).rgb);
					
					if (angle > Bias)
					{
						ao += saturate(angle * (1.0 + Bias) - Bias) * weight * (2.0 - 1.0 * dither);
						bleed += colorSample * weight * saturate(angle * (1.0 + Bias) - Bias);
					}
				}
				
				if (weights < 0.01)
				{
					weights = 1.0;
				}
				
				ao /= weights;
				ao = 1.0f - ao * 1.2;
				ao = saturate(ao * 1.0);
				ao = pow(ao, Intensity);
				
				ao = lerp(1.0, ao, saturate((DrawDistance / DrawDistanceFadeSize) + (origin.z / DrawDistanceFadeSize)));
				
				bleed /= weights;
				
				float4 res = float4(normalize(bleed.rgb + 0.0001), ao);
				
				return res;
			}
		
		ENDCG
	}
	
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			int PreserveDetails;
			int Orthographic;
			
			float4 frag(v2f input) : COLOR0
			{			
				float2 coord = input.uv.xy;
				
				float3 viewSpacePosition = GetViewSpacePosition(coord);
				float3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, coord));
				
				float2 noiseCoord = coord * float2(_MainTex_TexelSize.zw);
				       noiseCoord /= (5.0 - PreserveDetails * 2) + (float)Downsamp * (5.0 - PreserveDetails * 2);
				float dither = 0.5;
					  dither = tex2Dlod(_DitherTexture, float4(noiseCoord.xy, 0.0, 0.0)).a;
				
				float3 origin = viewSpacePosition.xyz;
				
				int numRaysPassed = 0;
				
				float ao = 0.0f;
				
				int numSamples = (8 + PreserveDetails * 4) - HalfSampling * (4 + PreserveDetails * 2);
				
				float weights = 0.0;

				float radius2 = Radius;
				float spread = radius2 / lerp((-origin.z + 0.0001), 10.0, Orthographic);
				
				const float sweeps = 31.0;
				
				float3 bleed = float3(0.0, 0.0, 0.0);
				
				float4 emphasisDir = mul(_WorldToCamera, float4(0.0, -1.0, 0.0, 0.0));
				emphasisDir.xyz = normalize(emphasisDir.xyz);
				emphasisDir.x *= -1.0f;
				emphasisDir.y *= -1.0f;
				
				for (int i = 0; i < numSamples; i++)
				{
					float2 kernel;
					float fi = (float)i / (float)numSamples;
					kernel.x = sin(fi * 3.14159265 * sweeps * 2.0 + dither * 6.0);
					kernel.y = cos(fi * 3.14159265 * sweeps * 2.0 + dither * 6.0);
					float2 kernelNormalized = kernel;
					kernel.y *= _ScreenParams.x / _ScreenParams.y;
					kernel *= pow(dither, SampleDistributionCurve);
					
					float2 finalKernel = kernel * spread + kernelNormalized * 0.001;
					
					float3 samplePosition = GetViewSpacePosition(clamp(coord.xy + finalKernel, float2(0.0, 0.0), float2(1.0, 1.0)));
					
					float3 sampleVector = normalize(samplePosition - origin);
					
					float angle = dot(sampleVector, normal.xyz);
					
					float weight = 1.0;
					
					float dist = length(samplePosition.z - origin.z);
					dist = max(0.0, dist - 0.8);
					weight = pow(saturate(1.0 - dist / (length(dither) * ZThickness * spread * 25.5)), 1.0);
					
					weights += weight;
					
					float emphasisWeight = (dot(sampleVector, emphasisDir.xyz)) * 4.0 + 1.0;
					
					if (angle > Bias)
					{
						ao += saturate(angle * (1.0 + Bias) - Bias) * weight * (2.0 - 1.0 * dither);
					}
				}
				
				if (weights < 0.01)
				{
					weights = 1.0;
				}
				
				ao /= weights;
				ao = 1.0f - ao * 1.2;
				ao = saturate(ao * 1.0);
				ao = pow(ao, Intensity);
				
				ao = lerp(1.0, ao, saturate((DrawDistance / DrawDistanceFadeSize) + (origin.z / DrawDistanceFadeSize)));
				
				float4 res = float4(normalize(float3(1.0, 1.0, 1.0)), ao);
				
				return res;
			}
		
		ENDCG
	}
	
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float2 Kernel;
			
			float DepthTolerance;
			
			int PreserveDetails;
					
			float4 frag(v2f input) : COLOR0
			{
				float2 coord = input.uv.xy;
				
//				return tex2D(_MainTex, coord);
			
				float4 blurred = float4(0.0, 0.0, 0.0, 0.0);
				float4 blurredDumb = float4(0.0, 0.0, 0.0, 0.0);
				float validWeights = 0.0;
				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, coord).x);
				half3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, coord));
				float thresh = DepthTolerance;
				
				thresh *= 1.0 + length(normal.xy) * 2.0;
				
				int numSamples = 5 - PreserveDetails * 2;
				for (int i = 0; i < numSamples; i++)
				{
					float2 offs = Kernel.xy * (i - 2 + PreserveDetails) * _MainTex_TexelSize.xy * 1.0;
					float sampleDepth = LinearEyeDepth(tex2Dlod(_CameraDepthTexture, float4(coord + offs.xy,0,0)).x);
					half3 sampleNormal = DecodeViewNormalStereo(tex2Dlod(_CameraDepthNormalsTexture, float4(coord + offs.xy,0,0)));
					
					float weight = saturate(1.0 - abs(depth - sampleDepth) / thresh);
					weight *= saturate(dot(sampleNormal, normal) * 5.0 - 4.0);
					
					float4 blurSample = tex2Dlod(_MainTex, float4(coord + offs.xy,0,0)).rgba;
					blurredDumb += blurSample;
					blurred += blurSample * weight;
					validWeights += weight;
				}
				
				blurredDumb /= 5.0;
				blurred /= validWeights;
				
				if (validWeights < 1.1)
				{
//					blurred = blurredDumb;
				}
				
				return blurred;
			}		
		
		ENDCG
	}		
	
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _SSAO;
			
			float ColorBleedAmount;
			float SelfBleedReduction;
			float BrightnessThreshold;
			
			float4 frag(v2f input) : COLOR0
			{
				#if UNITY_UV_STARTS_AT_TOP
					float2 coord = input.uv2.xy;
				#else
					float2 coord = input.uv.xy;
				#endif	
			
				float4 aotex = max(float4(0,0,0,0), tex2D(_SSAO, input.uv.xy).rgba);
				float ao = aotex.a;
				float3 bleed = aotex.rgb;
				
				float3 sceneColor = tex2D(_MainTex, coord).rgb;
				
				float3 aoCol = float3(0.0, 0.0, 0.0);
				float3 tint = bleed.rgb;
				float3 colorN = normalize(sceneColor.rgb);
				float dist = distance(colorN, tint);
				dist = saturate(dist * 3.0);
				dist = lerp(1.0, dist, SelfBleedReduction);
				tint = lerp(normalize(float3(1.0, 1.0, 1.0)), tint, float3(dist, dist, dist));

				float lum = length(sceneColor);
				float aoSub = saturate(lum * BrightnessThreshold);
				ao = lerp(ao, 1.0, aoSub);
				
				if (ao > 0.5)
				{
					aoCol = lerp(tint, float3(1.0, 1.0, 1.0), saturate(ao * 2.0 - 1.0));
				}
				else
				{
					aoCol = saturate(ao * 2.0) * tint;
				}
				
				float3 color = sceneColor * lerp(ao, aoCol, ColorBleedAmount);
				
				return float4(color, 1.0);
			}		
		
		ENDCG
	}	
	
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _SSAO;
			
			float4 frag(v2f input) : COLOR0
			{
				float2 coord = input.uv.xy;
				
				return float4(tex2D(_CameraDepthTexture, coord).xxx, 1.0);
			}		
		
		ENDCG
	}	
	
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _SSAO;
			float ColorBleedAmount;
			float SelfBleedReduction;
			
			float4 frag(v2f input) : COLOR0
			{
				float2 coord = input.uv.xy;
			
				float4 aotex = tex2D(_SSAO, coord).rgba;
				float ao = aotex.a;
				float3 bleed = aotex.rgb;
				
				float3 sceneColor = tex2D(_MainTex, coord).rgb;
				
				float3 aoCol = float3(0.0, 0.0, 0.0);
				float3 tint = bleed.rgb;
				float3 colorN = normalize(sceneColor.rgb);
				float dist = distance(colorN, tint);
				dist = saturate(dist * 3.0);
				dist = lerp(1.0, dist, SelfBleedReduction);
				tint = lerp(normalize(float3(1.0, 1.0, 1.0)), tint, float3(dist, dist, dist));
				
				if (ao > 0.5)
				{
					aoCol = lerp(tint, float3(1.0, 1.0, 1.0), saturate(ao * 2.0 - 1.0));
				}
				else
				{
					aoCol = saturate(ao * 2.0) * tint;
				}
				
				aoCol = lerp(float3(ao, ao, ao), aoCol, float3(ColorBleedAmount, ColorBleedAmount, ColorBleedAmount));
				
				return float4(aoCol, 1.0);
			}			
		
		ENDCG
	}	
	
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float2 Kernel;
			
			float DepthTolerance;
			
			float Near;
			float Far;
			
			int PreserveDetails;
			
			
			float OrthoLinearDepth(float depth)
			{		
				float z = ProjectionMatrixInverse._33 * (depth * 2.0 - 1.0) + ProjectionMatrixInverse._34 * 1.0;				
				return -z;
			}
					
			float4 frag(v2f input) : COLOR0
			{
//				return tex2D(_MainTex, input.uv.xy);
				
				float2 coord = input.uv.xy;
				
				float2 coord2 = input.uv.xy;
			
				float4 blurred = float4(0.0, 0.0, 0.0, 0.0);
				float4 blurredDumb = float4(0.0, 0.0, 0.0, 0.0);
				float validWeights = 0.0;
				float depth = OrthoLinearDepth(tex2D(_CameraDepthTexture, coord2).x);
				half3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, coord2));
				float thresh = DepthTolerance;
				
				thresh *= 1.0 + length(normal.xy) * 2.0;
				
				int numSamples = 5 - PreserveDetails * 2;
				for (int i = 0; i < numSamples; i++)
				{
					float2 offs = Kernel.xy * (i - 2 + PreserveDetails) * _MainTex_TexelSize.xy * 1.0;
					float sampleDepth = OrthoLinearDepth(tex2Dlod(_CameraDepthTexture, float4(coord2 + offs.xy,0,0)).x);
					half3 sampleNormal = DecodeViewNormalStereo(tex2Dlod(_CameraDepthNormalsTexture, float4(coord2 + offs.xy,0,0)));
					
					float weight = saturate(1.0 - abs(depth - sampleDepth) / thresh);
					weight *= saturate(dot(sampleNormal, normal) * 5.0 - 4.0);
					
					float4 blurSample = tex2Dlod(_MainTex, float4(coord + offs.xy,0,0)).rgba;
					blurredDumb += blurSample;
					blurred += blurSample * weight;
					validWeights += weight;
				}
				
				blurredDumb /= 5.0;
				blurred /= validWeights;
				
				if (validWeights < 1.1)
				{
					blurred = blurredDumb;
				}
				
				return blurred;
			}		
		
		ENDCG
	}		
}

Fallback off

}