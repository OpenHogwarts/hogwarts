Shader "Hidden/SMAA" {
    Properties {
        _MainTex ("", 2D) = "black" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    ///////// from SMAA.fx
    float4 _PixelSize;

    #define SMAA_PIXEL_SIZE _PixelSize.xy
    #define SMAA_PRESET_HIGH 1

    // porting the DX9 version of functions
    #define SMAA_HLSL_3 1
    #define SMAA_ONLY_COMPILE_PS 1

    #include "SMAA.cginc"

    sampler2D _CameraDepthTexture;
    sampler2D_float _CameraMotionVectors;
    sampler2D _VelTex;

    sampler2D colorTex;
    sampler2D edgesTex;
    sampler2D blendTex;
    sampler2D areaTex;
    sampler2D searchTex;
    sampler2D accumTex;
    sampler2D smaaTex;

    int _JitterOffset;

    float4 _MainTex_TexelSize;
    float4x4 _ToPrevViewProjCombined; // combined

    float4 _PixelOffset;
    float _TemporalAccum;

        float _DepthThreshold;

    struct v2f {
        half4 pos : SV_POSITION;
        half2 uv : TEXCOORD0;
    };

    sampler2D _MainTex; // set implicitly in Graphics.Blit()
    v2f vert( appdata_img v )
    {
        v2f o;
        o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
        o.uv = v.texcoord.xy;
        return o;
    }

    half4 fragCopy(v2f i) : SV_Target
    {
        half4 color = tex2D (_MainTex, i.uv);
        return color;
    }

    half4 fragBlack(v2f i) : SV_Target
    {
        return float4(0,0,0,0);
    }

    half4 DX9_SMAALumaEdgeDetectionPS(v2f i) : SV_Target
    {
        float2 texcoord = i.uv;
        float4 offset[3];

        offset[0] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-1.0, 0.0, 0.0, -1.0);
        offset[1] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4( 1.0, 0.0, 0.0,  1.0);
        offset[2] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-2.0, 0.0, 0.0, -2.0);

        half4 color = SMAALumaEdgeDetectionPS(texcoord,offset,colorTex);

        return color;
    }


    half4 DX9_SMAAColorEdgeDetectionPS(v2f i) : SV_Target
    {
        float2 texcoord = i.uv;
        float4 offset[3];

        offset[0] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-1.0, 0.0, 0.0, -1.0);
        offset[1] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4( 1.0, 0.0, 0.0,  1.0);
        offset[2] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-2.0, 0.0, 0.0, -2.0);

        half4 color = SMAAColorEdgeDetectionPS(texcoord,offset,colorTex);

        return color;
    }

    half4 DX9_SMAADepthEdgeDetectionPS(v2f i) : SV_Target
    {
        float2 texcoord = i.uv;
        float4 offset[3];

        offset[0] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-1.0, 0.0, 0.0, -1.0);
        offset[1] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4( 1.0, 0.0, 0.0,  1.0);
        offset[2] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-2.0, 0.0, 0.0, -2.0);

        half4 color = SMAADepthEdgeDetectionPS(texcoord,offset,_CameraDepthTexture,_DepthThreshold);

        return color;
    }

    half4 DX9_SMAABlendingWeightCalculationPS(v2f i) : SV_Target
    {
        float2 texcoord = i.uv;

        float2 pixcoord;
        float4 offset[3];

        pixcoord = texcoord / SMAA_PIXEL_SIZE;

        // these are taken from SMAABlendingWeightCalculationVS

        // We will use these offsets for the searches later on (see @PSEUDO_GATHER4):
        offset[0] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-0.25, -0.125,  1.25, -0.125);
        offset[1] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-0.125, -0.25, -0.125,  1.25);

        // And these for the searches, they indicate the ends of the loops:
        offset[2] = float4(offset[0].xz, offset[1].yw) +
                    float4(-2.0, 2.0, -2.0, 2.0) *
                    SMAA_PIXEL_SIZE.xxyy * float(SMAA_MAX_SEARCH_STEPS);

        int4 jitterData = int4(_JitterOffset,_JitterOffset,_JitterOffset,0);

        half4 color = SMAABlendingWeightCalculationPS(texcoord, pixcoord, offset, edgesTex, areaTex, searchTex, jitterData);

        return color;
    }

    half4 DX9_SMAANeighborhoodBlendingPS(v2f i) : SV_Target
    {
        float2 texcoord = i.uv;

        float4 offset[2];
        offset[0] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4(-1.0, 0.0, 0.0, -1.0);
        offset[1] = texcoord.xyxy + SMAA_PIXEL_SIZE.xyxy * float4( 1.0, 0.0, 0.0,  1.0);

        half4 color = SMAANeighborhoodBlendingPS(texcoord, offset, colorTex, blendTex);
        return color;
    }

    float PixelsFromEdge(float2 uv)
    {
        float distX = min(uv.x,1.0-uv.x) / SMAA_PIXEL_SIZE.x;
        float distY = min(uv.y,1.0-uv.y) / SMAA_PIXEL_SIZE.y;

        float bestDist = min(distX,distY);
        return bestDist;
    }

    float K;

    float3 FetchPreviousUvWeight(float2 currUv)
    {
        float2 depth_uv = currUv;

        #if UNITY_UV_STARTS_AT_TOP
        //if (_MainTex_TexelSize.y < 0)
        {
            depth_uv.y = 1 - depth_uv.y;
        }
        #endif

        // read depth
        float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, depth_uv);

        // calculate position from pixel from depth
        float3 clipPos = float3(depth_uv.x*2.0-1.0, depth_uv.y*2.0-1.0, d);

        // only 1 matrix mul:
        float4 prevClipPos = mul(_ToPrevViewProjCombined, float4(clipPos, 1.0));
        prevClipPos.xyz /= prevClipPos.w;

        float2 prevUv = prevClipPos.xyz*.5f+.5f;
        #if UNITY_UV_STARTS_AT_TOP
        //if (_MainTex_TexelSize.y < 0)
        {
            prevUv.y = 1.0f - prevUv.y;
        }
        #endif

        float distFromEdge = PixelsFromEdge(prevUv);
        float weight = saturate(distFromEdge - .5f);

        float distSqr = dot(prevUv - currUv, prevUv - currUv);

        // 1% of the screen seems like a good number
        float rho = 0.01;
        float invRR = 1.0f/(rho*rho);
        float distW = exp(-invRR * distSqr);

        weight *= distW;

       // weight = 0.5 * max(0, 1 - K * sqrt(abs(length(currUv) - length(prevUv))));

        return float3(prevUv,weight);
    }

    // color is the current value, accum is the previous accumulation
    half4 ClampAccumulation(float2 texcoord, half4 accum, half4 color)
    {
        // get the corner pixels for max/max
        float3 pixelUL = tex2D(colorTex, texcoord + float2( 0.5f, 0.5f)*SMAA_PIXEL_SIZE.xy).rgb;
        float3 pixelUR = tex2D(colorTex, texcoord + float2(-0.5f, 0.5f)*SMAA_PIXEL_SIZE.xy).rgb;
        float3 pixelDL = tex2D(colorTex, texcoord + float2( 0.5f,-0.5f)*SMAA_PIXEL_SIZE.xy).rgb;
        float3 pixelDR = tex2D(colorTex, texcoord + float2(-0.5f,-0.5f)*SMAA_PIXEL_SIZE.xy).rgb;

        float3 colorMax = max(color,max(max(pixelUL,pixelUR),max(pixelDL,pixelDR)));
        float3 colorMin = min(color,min(min(pixelUL,pixelUR),min(pixelDL,pixelDR)));

        // since we are sampling half way to the pixels, we need to multiply by 2.0 on the intensity
        // which gives a rough guess as to min and max color in a 1 pixel radius
        float scale = 2.0f;
        colorMin = color + scale*(colorMin - color);
        colorMax = color + scale*(colorMax - color);

        accum.rgb = max(colorMin,min(colorMax,accum.rgb));
        return accum;
    }

    // same as above, but also blend with accumulation buffer
    half4 DX9_SMAANeighborhoodBlendingAccumPS(v2f i) : SV_Target
    {
        float2 uv = i.uv;

        #if UNITY_UV_STARTS_AT_TOP
            uv.y = 1 - uv.y;
        #endif

        float4 position = float4(2. * uv - 1., SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv), 1.);
        float4 previousPosition = mul(_ToPrevViewProjCombined, position);

        previousPosition.xyz /= previousPosition.w;

        uv = previousPosition.xy * .5 + .5;

        #if UNITY_UV_STARTS_AT_TOP
            uv.y = 1. - uv.y;
        #endif

        // Inverse velocity from the current position to the previous position
        float2 velocity = uv - i.uv;

        half4 current = SMAASample(colorTex, i.uv);
        half4 previous = SMAASample(colorTex, uv);

        float delta = abs(current.a * current.a - previous.a * previous.a) / 5.0;
        float weight = .5 * SMAASaturate(1. - (sqrt(delta) * K)); // K ~= SMAA_REPROJECTION_WEIGHT_SCALE

        //return half4(.5 * (uv - i.uv) + .5, 0., 1.);
        return lerp(current, previous, weight);
    }


    half4 DX9_SMAADebugDepthPS(v2f i) : SV_Target
    {
        return SMAASampleDepth_Unity(i.uv, _CameraDepthTexture, i.uv);
    }



    // same as above, but also blend with accumulation buffer
    half4 fragAccumulateOffset(v2f i) : SV_Target
    {
        float3 prevUvWeight = FetchPreviousUvWeight(i.uv);
        float  prevWeight = prevUvWeight.z;
        float2 prevUv = prevUvWeight.xy;

        half4 accum = tex2D (accumTex, prevUv);
        half4 color = tex2D (smaaTex, i.uv + _PixelOffset.xy);

        // get better accumulation
        {
            half4 clampAccum = ClampAccumulation(i.uv + _PixelOffset.xy,accum,color);

            half2 edgeData = tex2D (edgesTex, i.uv + SMAA_PIXEL_SIZE.xy*0.5f);
            float edgeVal = saturate(max(edgeData.x,edgeData.y));

            accum = lerp(clampAccum,accum,edgeVal);
        }

        float t = _TemporalAccum;
        t = t + (1.0f-t)*(1.0f-prevWeight);

        half4 res = accum * (1.0f-t) + color * t;

        return res;
    }

    ENDCG

Subshader {
 // 0 - just copy
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragCopy
      ENDCG
  }

  // 1 - luma detection
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAALumaEdgeDetectionPS
      ENDCG
  }

  // 2 - just clear to black, necessary becuase luma detection will discard
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBlack
      ENDCG
  }

  // 3 - weight calculation
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAABlendingWeightCalculationPS
      ENDCG
  }

  // 4 - apply weights and blend
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAANeighborhoodBlendingPS
      ENDCG
  }

  // 5 - apply weights and blend
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAANeighborhoodBlendingAccumPS
      ENDCG
  }

  // 6 - color detection
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAAColorEdgeDetectionPS
      ENDCG
  }

  // 7 - merge previous frame with current frame (with offset)
 Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragAccumulateOffset
      ENDCG
  }


  // 8 - depth detection
  Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAADepthEdgeDetectionPS
      ENDCG
  }

  // 9 - debug depth
  Pass {
      ZTest Always Cull Off ZWrite Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment DX9_SMAADebugDepthPS
      ENDCG
  }



}

Fallback off

} // shader
