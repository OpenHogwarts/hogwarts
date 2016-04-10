Shader "Hidden/DepthOfField/DepthOfField"
{

Properties
{
    _MainTex ("-", 2D) = "black"
    _SecondTex ("-", 2D) = "black"
    _ThirdTex ("-", 2D) = "black"
}

CGINCLUDE
#pragma target 3.0
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _SecondTex;
sampler2D _ThirdTex;
sampler2D _CameraDepthTexture;
uniform half4 _MainTex_TexelSize;
uniform half4 _BlurCoe;
uniform half4 _BlurParams;
uniform half4 _BoostParams;
uniform half4 _Convolved_TexelSize;
uniform float4 _Offsets;

uniform half4 _MainTex_ST;
uniform half4 _SecondTex_ST;
uniform half4 _ThirdTex_ST;

///////////////////////////////////////////////////////////////////////////////
// Verter Shaders and declaration
///////////////////////////////////////////////////////////////////////////////

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
};

struct v2fDepth
{
    half4 pos  : SV_POSITION;
    half2 uv   : TEXCOORD0;
};

struct v2fBlur
{
    float4 pos  : SV_POSITION;
    float2 uv   : TEXCOORD0;
    float4 uv01 : TEXCOORD1;
    float4 uv23 : TEXCOORD2;
    float4 uv45 : TEXCOORD3;
    float4 uv67 : TEXCOORD4;
    float4 uv89 : TEXCOORD5;
};

v2fDepth vert(appdata_img v)
{
    v2fDepth o;
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    o.uv = v.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
    o.uv.y = 1-o.uv.y;
#endif
    return o;
}

v2fDepth vertNoFlip(appdata_img v)
{
    v2fDepth o;
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    o.uv = v.texcoord.xy;
    return o;
}

v2f vert_d( appdata_img v )
{
    v2f o;
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.uv1.xy = v.texcoord.xy;
    o.uv.xy = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
    o.uv.y = 1-o.uv.y;
#endif

    return o;
}

v2f vertFlip( appdata_img v )
{
    v2f o;
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.uv1.xy = v.texcoord.xy;
    o.uv.xy = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
    if (_MainTex_TexelSize.y < 0)
    o.uv.y = 1-o.uv.y;
    if (_MainTex_TexelSize.y < 0)
    o.uv1.y = 1-o.uv1.y;
#endif

    return o;
}

v2fBlur vertBlurPlusMinus (appdata_img v)
{
    v2fBlur o;
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    o.uv.xy = v.texcoord.xy;
    o.uv01 =  v.texcoord.xyxy + _Offsets.xyxy * float4(1,1, -1,-1) * _MainTex_TexelSize.xyxy / 6.0;
    o.uv23 =  v.texcoord.xyxy + _Offsets.xyxy * float4(2,2, -2,-2) * _MainTex_TexelSize.xyxy / 6.0;
    o.uv45 =  v.texcoord.xyxy + _Offsets.xyxy * float4(3,3, -3,-3) * _MainTex_TexelSize.xyxy / 6.0;
    o.uv67 =  v.texcoord.xyxy + _Offsets.xyxy * float4(4,4, -4,-4) * _MainTex_TexelSize.xyxy / 6.0;
    o.uv89 =  v.texcoord.xyxy + _Offsets.xyxy * float4(5,5, -5,-5) * _MainTex_TexelSize.xyxy / 6.0;
    return o;
}

///////////////////////////////////////////////////////////////////////////////
// Helpers
///////////////////////////////////////////////////////////////////////////////

inline half3 getBoostAmount(half4 colorAndCoc)
{
    half boost = colorAndCoc.a * (colorAndCoc.a < 0.0f ?_BoostParams.x:_BoostParams.y);
    half luma = dot(colorAndCoc.rgb, half3(0.3h, 0.59h, 0.11h));
    return luma < _BoostParams.z ? half3(0.0h, 0.0h, 0.0h):colorAndCoc.rgb * boost.rrr;
}

inline half GetCocFromDepth(half2 uv, bool useExplicit)
{
    half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
    d = Linear01Depth (d);

    if (useExplicit)
    {
        half coc = d < _BlurCoe.z ? clamp((_BlurParams.x * d + _BlurParams.y), -1.0f, 0.0f):clamp((_BlurParams.z * d + _BlurParams.w), 0.0f, 1.0f);
        return coc;
    }
    else
    {
        half aperture = _BlurParams.x;
        half focalLength = _BlurParams.y;
        half focusDistance01 = _BlurParams.z;
        half focusRange01 = _BlurParams.w;
        half coc = aperture * abs(d - focusDistance01) / (d + 1e-5f) - focusRange01;
        coc = (d < focusDistance01 ? -1.0h:1.0h) * clamp(coc, 0.0f, 1.0f);
        return coc;
    }
}

///////////////////////////////////////////////////////////////////////////////
// Directional (hexagonal/octogonal) bokeh
///////////////////////////////////////////////////////////////////////////////

#define SAMPLE_NUM_L    6
#define SAMPLE_NUM_M    11
#define SAMPLE_NUM_H    16

half4 shapeDirectionalBlur(half2 uv, bool mergePass, int numSample, bool sampleDilatedFG)
{
    half4 centerTap = tex2Dlod (_MainTex, float4(uv,0,0));
    half  fgCoc  = centerTap.a;
    half  fgBlendFromPreviousPass = centerTap.a * _Offsets.z;
    if (sampleDilatedFG)
    {
        half2 cocs = tex2Dlod(_SecondTex, half4(uv,0,0)).rg;
        fgCoc  = min(cocs.r, cocs.g);
        centerTap.a = cocs.g;
    }

    half  bgRadius = smoothstep(0.0h, 0.85h, centerTap.a)  * _BlurCoe.y;
    half  fgRadius = smoothstep(0.0h, 0.85h, -fgCoc) * _BlurCoe.x;
    half  radius = max(bgRadius, fgRadius);
    if (radius < 1e-2f )
    {
        return half4(centerTap.rgb, (sampleDilatedFG||mergePass)?fgBlendFromPreviousPass:centerTap.a);
    }

    half radOtherFgRad = radius/(fgRadius + 1e-2h);
    half radOtherBgRad = radius/(bgRadius + 1e-2h);
    half2 range = radius * _MainTex_TexelSize.xy;

    half fgWeight = 0.001h;
    half bgWeight = 0.001h;
    half3 fgSum = half3(0,0,0);
    half3 bgSum = half3(0,0,0);

    for (int k = 0; k < numSample; k++)
    {
        half t = (half)k / half(numSample-1);
        half2 kVal = lerp(_Offsets.xy, -_Offsets.xy, t);
        half2 offset = kVal * range;
        half2 texCoord = uv + offset;
        half4 sample0 = tex2Dlod(_MainTex, half4(texCoord,0,0));
        if (sampleDilatedFG)
        {
            sample0.a = tex2Dlod(_SecondTex, half4(texCoord,0,0)).g;
        }

        half dist = abs(2.0h * t - 1);
        half distanceFactor = saturate(-0.5f * abs(sample0.a - centerTap.a) * dist + 1.0f);
        half isNear = max(0.0h, -sample0.a);
        half isFar  = max(0.0h, sample0.a)  * distanceFactor;
        isNear *= 1- smoothstep(1.0h, 2.0h, dist * radOtherFgRad);
        isFar  *= 1- smoothstep(1.0h, 2.0h, dist * radOtherBgRad);

        fgWeight += isNear;
        fgSum += sample0.rgb * isNear;
        bgWeight += isFar;
        bgSum += sample0.rgb * isFar;
    }

    half3 fgColor = fgSum / (fgWeight + 0.0001h);
    half3 bgColor = bgSum / (bgWeight + 0.0001h);
    half bgBlend = saturate (2.0h * bgWeight / numSample);
    half fgBlend = saturate (2.0h * fgWeight / numSample);

    half3 finalBg = lerp(centerTap.rgb, bgColor, bgBlend);
    half3 finalColor = lerp(finalBg, fgColor, max(max(0.0h , -centerTap.a) , fgBlend));

    if (mergePass)
    {
        finalColor = min(finalColor, tex2Dlod(_ThirdTex, half4(uv,0,0)).rgb);
    }
    
    finalColor = lerp(centerTap.rgb, finalColor, saturate(bgBlend+fgBlend));
    fgBlend = max(fgBlendFromPreviousPass, fgBlend);
    return half4(finalColor, (sampleDilatedFG||mergePass)?fgBlend:centerTap.a);
}

half4 fragShapeLowQuality(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, false, SAMPLE_NUM_L, false);
}

half4 fragShapeLowQualityDilateFg(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, false, SAMPLE_NUM_L, true);
}

half4 fragShapeLowQualityMerge(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, true, SAMPLE_NUM_L, false);
}

half4 fragShapeLowQualityMergeDilateFg(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, true, SAMPLE_NUM_L, true);
}

half4 fragShapeMediumQuality(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, false, SAMPLE_NUM_M, false);
}

half4 fragShapeMediumQualityDilateFg(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, false, SAMPLE_NUM_M, true);
}

half4 fragShapeMediumQualityMerge(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, true, SAMPLE_NUM_M, false);
}

half4 fragShapeMediumQualityMergeDilateFg(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, true, SAMPLE_NUM_M, true);
}

half4 fragShapeHighQuality(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, false, SAMPLE_NUM_H, false);
}

half4 fragShapeHighQualityDilateFg(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, false, SAMPLE_NUM_H, true);
}

half4 fragShapeHighQualityMerge(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, true, SAMPLE_NUM_H, false);
}

half4 fragShapeHighQualityMergeDilateFg(v2fDepth i) : COLOR
{
    return shapeDirectionalBlur(i.uv, true, SAMPLE_NUM_H, true);
}

///////////////////////////////////////////////////////////////////////////////
// Disk Bokeh
///////////////////////////////////////////////////////////////////////////////

static const half3 DiscBokeh48[48] =
{
    //48 tap regularly spaced circular kernel (x,y, length)
    //fill free to change the shape to try other bokehs style :)
    half3( 0.99144h, 0.13053h, 1.0h),
    half3( 0.92388h, 0.38268h, 1.0h),
    half3( 0.79335h, 0.60876h, 1.0h),
    half3( 0.60876h, 0.79335h, 1.0h),
    half3( 0.38268h, 0.92388h, 1.0h),
    half3( 0.13053h, 0.99144h, 1.0h),
    half3(-0.13053h, 0.99144h, 1.0h),
    half3(-0.38268h, 0.92388h, 1.0h),
    half3(-0.60876h, 0.79335h, 1.0h),
    half3(-0.79335h, 0.60876h, 1.0h),
    half3(-0.92388h, 0.38268h, 1.0h),
    half3(-0.99144h, 0.13053h, 1.0h),
    half3(-0.99144h,-0.13053h, 1.0h),
    half3(-0.92388h,-0.38268h, 1.0h),
    half3(-0.79335h,-0.60876h, 1.0h),
    half3(-0.60876h,-0.79335h, 1.0h),
    half3(-0.38268h,-0.92388h, 1.0h),
    half3(-0.13053h,-0.99144h, 1.0h),
    half3( 0.13053h,-0.99144h, 1.0h),
    half3( 0.38268h,-0.92388h, 1.0h),
    half3( 0.60876h,-0.79335h, 1.0h),
    half3( 0.79335h,-0.60876h, 1.0h),
    half3( 0.92388h,-0.38268h, 1.0h),
    half3( 0.99144h,-0.13053h, 1.0h),
    half3( 0.64732h, 0.12876h, 0.66h),
    half3( 0.54877h, 0.36668h, 0.66h),
    half3( 0.36668h, 0.54877h, 0.66h),
    half3( 0.12876h, 0.64732h, 0.66h),
    half3(-0.12876h, 0.64732h, 0.66h),
    half3(-0.36668h, 0.54877h, 0.66h),
    half3(-0.54877h, 0.36668h, 0.66h),
    half3(-0.64732h, 0.12876h, 0.66h),
    half3(-0.64732h,-0.12876h, 0.66h),
    half3(-0.54877h,-0.36668h, 0.66h),
    half3(-0.36668h,-0.54877h, 0.66h),
    half3(-0.12876h,-0.64732h, 0.66h),
    half3( 0.12876h,-0.64732h, 0.66h),
    half3( 0.36668h,-0.54877h, 0.66h),
    half3( 0.54877h,-0.36668h, 0.66h),
    half3( 0.64732h,-0.12876h, 0.66h),
    half3( 0.30488h, 0.12629h, 0.33h),
    half3( 0.12629h, 0.30488h, 0.33h),
    half3(-0.12629h, 0.30488h, 0.33h),
    half3(-0.30488h, 0.12629h, 0.33h),
    half3(-0.30488h,-0.12629h, 0.33h),
    half3(-0.12629h,-0.30488h, 0.33h),
    half3( 0.12629h,-0.30488h, 0.33h),
    half3( 0.30488h,-0.12629h, 0.33h)
};

inline float4 circleCocBokeh(float2 uv, bool sampleDilatedFG, int increment)
{
    half4 centerTap = tex2Dlod(_MainTex, half4(uv,0,0));
    half  fgCoc  = centerTap.a;
    if (sampleDilatedFG)
    {
        fgCoc  = min(tex2Dlod(_SecondTex, half4(uv,0,0)).r, fgCoc);
    }

    half  bgRadius = 0.5h * smoothstep(0.0h, 0.85h, centerTap.a)  * _BlurCoe.y;
    half  fgRadius = 0.5h * smoothstep(0.0h, 0.85h, -fgCoc) * _BlurCoe.x;
    half radius = max(bgRadius, fgRadius);
    if (radius < 1e-2f )
    {
        return half4(centerTap.rgb, 0);
    }

    half2 poissonScale = radius * _MainTex_TexelSize.xy;
    half fgWeight = max(0,-centerTap.a);
    half bgWeight = max(0, centerTap.a);
    half3 fgSum = centerTap.rgb * fgWeight;
    half3 bgSum = centerTap.rgb * bgWeight;

    half radOtherFgRad = radius/(fgRadius + 1e-2h);
    half radOtherBgRad = radius/(bgRadius + 1e-2h);

    for (int l = 0; l < 48; l+= increment)
    {
        half2 sampleUV = uv + DiscBokeh48[l].xy * poissonScale.xy;
        half4 sample0  = tex2Dlod(_MainTex, half4(sampleUV,0,0));

        half isNear = max(0.0h, -sample0.a);
        half distanceFactor = saturate(-0.5f * abs(sample0.a - centerTap.a) * DiscBokeh48[l].z + 1.0f);
        half isFar  = max(0.0h, sample0.a)  * distanceFactor;
        isNear *= 1- smoothstep(1.0h, 2.0h, DiscBokeh48[l].z * radOtherFgRad);
        isFar  *= 1- smoothstep(1.0h, 2.0h, DiscBokeh48[l].z * radOtherBgRad);

        fgWeight += isNear;
        fgSum += sample0.rgb * isNear;
        bgWeight += isFar;
        bgSum += sample0.rgb * isFar;
    }

    half3 fgColor = fgSum / (fgWeight + 0.0001h);
    half3 bgColor = bgSum / (bgWeight + 0.0001h);
    half bgBlend = saturate (2.0h * bgWeight / 49.0h);
    half fgBlend = saturate (2.0h * fgWeight / 49.0h);

    half3 finalBg = lerp(centerTap.rgb, bgColor, bgBlend);
    half3 finalColor = lerp(finalBg, fgColor, max(max(0.0h , -centerTap.a) , fgBlend));
    half4 returnValue = half4(finalColor, fgBlend );

    return returnValue;
}

float4 fragCircleBlurWithDilatedFg (v2fDepth i) : SV_Target
{
    return circleCocBokeh(i.uv, true, 1);
}

float4 fragCircleBlur (v2fDepth i) : SV_Target
{
    return circleCocBokeh(i.uv, false, 1);
}

float4 fragCircleBlurWithDilatedFgLowQuality (v2fDepth i) : SV_Target
{
    return circleCocBokeh(i.uv, true, 2);
}

float4 fragCircleBlurLowQuality (v2fDepth i) : SV_Target
{
    return circleCocBokeh(i.uv, false, 2);
}

///////////////////////////////////////////////////////////////////////////////
// Prefilter blur
///////////////////////////////////////////////////////////////////////////////

#define DISC_PREFILTER_SAMPLE   9
static const half2 DiscPrefilter[DISC_PREFILTER_SAMPLE] =
{
    half2(0.01288369f, 0.5416069f),
    half2(-0.9192798f, -0.09529364f),
    half2(0.7596578f, 0.1922738f),
    half2(-0.14132f, -0.2880242f),
    half2(-0.5249333f, 0.7777638f),
    half2(-0.5871695f, -0.7403569f),
    half2(0.3202196f, -0.6442268f),
    half2(0.8553214f, -0.3920982f),
    half2(0.5827708f, 0.7599297f)
};

float4 fragCocPrefilter (v2fDepth i) : SV_Target
{
    half4 centerTap = tex2Dlod(_MainTex, half4(i.uv.xy,0,0));
    half  radius = 0.33h * 0.5h * (centerTap.a < 0.0f ? -(centerTap.a * _BlurCoe.x):(centerTap.a * _BlurCoe.y));
    half2 poissonScale = radius * _MainTex_TexelSize.xy;

    if (radius < 0.01h )
    {
        return centerTap;
    }

    half  sampleCount = 1;
    half3 sum = centerTap.rgb * 1;
    for (int l = 0; l < DISC_PREFILTER_SAMPLE; l++)
    {
        half2 sampleUV = i.uv + DiscPrefilter[l].xy * poissonScale.xy;
        half4 sample0 = tex2Dlod(_MainTex, half4(sampleUV.xy,0,0));
        half weight = max(sample0.a * centerTap.a,0.0h);
        sum += sample0.rgb * weight;
        sampleCount += weight;
    }

    half4 returnValue = half4(sum / sampleCount, centerTap.a);
    return returnValue;
}

///////////////////////////////////////////////////////////////////////////////
// Final merge and upsample
///////////////////////////////////////////////////////////////////////////////

float4 upSampleConvolved(half2 uv, bool useBicubic)
{
    if (useBicubic)
    {
        //bicubic upsampling (B-spline)
        half2 convolvedTexelPos    = uv * _Convolved_TexelSize.xy;
        half2 convolvedTexelCenter = floor( convolvedTexelPos - 0.5h ) + 0.5h;
        half2 f  = convolvedTexelPos - convolvedTexelCenter;
        half2 f2 = f * f;
        half2 f3 = f * f2;

        half2 w0 = -0.166h * f3 + 0.5h * f2 - 0.5h * f + 0.166h;
        half2 w1 =  0.5h   * f3 - f2 + 0.666h;
        half2 w3 =  0.166h * f3;
        half2 w2 =  1.0h - w0 - w1 - w3;

        half2 s0 = w0 + w1;
        half2 s1 = w2 + w3;
        half2 f0 = w1 / s0;
        half2 f1 = w3 / s1;

        half2 t0 = _Convolved_TexelSize.zw * (convolvedTexelCenter - 1.0h + f0);
        half2 t1 = _Convolved_TexelSize.zw * (convolvedTexelCenter + 1.0h + f1);

        return tex2Dlod(_SecondTex, half4(t0.x, t0.y, 0, 0)) * s0.x * s0.y +
               tex2Dlod(_SecondTex, half4(t1.x, t0.y, 0, 0)) * s1.x * s0.y +
               tex2Dlod(_SecondTex, half4(t0.x, t1.y, 0, 0)) * s0.x * s1.y +
               tex2Dlod(_SecondTex, half4(t1.x, t1.y, 0, 0)) * s1.x * s1.y;
    }
    else
    {
        return tex2Dlod(_SecondTex, half4(uv,0,0));
    }
}

float4 circleCocMerge (half2 uv, bool useExplicit, bool useBicubic)
{
    half4 convolvedTap = upSampleConvolved(uv, useBicubic);
    half4 sourceTap    = tex2Dlod(_MainTex, half4(uv,0,0));
    half  coc          = GetCocFromDepth(uv, useExplicit);

    sourceTap.rgb += getBoostAmount(half4(sourceTap.rgb, coc));

    coc = (coc * _BlurCoe.y > 1.0h )?coc:0.0h;
    half  blendValue = smoothstep(0.0, 0.33h, max(coc, convolvedTap.a));
    half3 returnValue = lerp(sourceTap.rgb, convolvedTap.rgb, blendValue);
    return (blendValue < 1e-2f) ? sourceTap : half4(returnValue.rgb, sourceTap.a);
}

float4 fragMergeBicubic (v2fDepth i) : SV_Target
{
    return circleCocMerge(i.uv, false, true);
}

float4 fragMergeExplicitBicubic (v2fDepth i) : SV_Target
{
    return circleCocMerge(i.uv, true, true);
}

float4 fragMerge (v2fDepth i) : SV_Target
{
    return circleCocMerge(i.uv, false, false);
}

float4 fragMergeExplicit (v2fDepth i) : SV_Target
{
    return circleCocMerge(i.uv, true, false);
}

///////////////////////////////////////////////////////////////////////////////
// Downsampling and COC computation
///////////////////////////////////////////////////////////////////////////////

half4 captureCoc(half2 uvColor, half2 uvDepth, bool useExplicit)
{
    half4 color = tex2Dlod (_MainTex, half4(uvColor, 0, 0 ));

    //TODO should use gather4 on supported platform!
    //TODO do only 1 tap on high resolution mode
    half cocA = GetCocFromDepth(uvDepth + _MainTex_TexelSize.xy * half2(+0.25f,+0.25f), useExplicit);
    half cocB = GetCocFromDepth(uvDepth + _MainTex_TexelSize.xy * half2(+0.25f,-0.25f), useExplicit);
    half cocC = GetCocFromDepth(uvDepth + _MainTex_TexelSize.xy * half2(-0.25f,+0.25f), useExplicit);
    half cocD = GetCocFromDepth(uvDepth + _MainTex_TexelSize.xy * half2(-0.25f,-0.25f), useExplicit);

    half cocAB = (abs(cocA)<abs(cocB))?cocA:cocB;
    half cocCD = (abs(cocC)<abs(cocD))?cocC:cocD;
    color.a    = (abs(cocAB)<abs(cocCD))?cocAB:cocCD;

    color.rgb += getBoostAmount(color);
    return color;
}

half4 fragCaptureCoc (v2f i) : SV_Target
{
    return captureCoc(i.uv, i.uv1, false);
}

half4 fragCaptureCocExplicit (v2f i) : SV_Target
{
    return captureCoc(i.uv, i.uv1, true);
}

///////////////////////////////////////////////////////////////////////////////
// Coc visualisation
///////////////////////////////////////////////////////////////////////////////

inline float4 visualizeCoc(half2 uv, bool useExplicit)
{
    half coc = GetCocFromDepth(uv, useExplicit);
    return (coc < 0)? half4(-coc, -coc, 0, 1.0) : half4(0, coc, coc, 1.0);
}

float4 fragVisualizeCoc(v2fDepth i) : SV_Target
{
    return visualizeCoc(i.uv, false);
}

float4 fragVisualizeCocExplicit(v2fDepth i) : SV_Target
{
    return visualizeCoc(i.uv, true);
}

///////////////////////////////////////////////////////////////////////////////
// Foreground blur dilatation
///////////////////////////////////////////////////////////////////////////////

inline half2 fgCocSourceChannel(half2 uv, bool fromAlpha)
{
    if (fromAlpha)
        return tex2Dlod(_MainTex, half4(uv,0,0)).aa;
    else
        return tex2Dlod(_MainTex, half4(uv,0,0)).rg;
}

inline half2 weigthedFGCocBlur(v2fBlur i, bool fromAlpha)
{
    half2 fgCoc00 = fgCocSourceChannel(i.uv.xy  , fromAlpha);
    half2 fgCoc01 = fgCocSourceChannel(i.uv01.zw, fromAlpha) * 1.0h;
    half2 fgCoc02 = fgCocSourceChannel(i.uv01.xy, fromAlpha) * 1.0h;
    half2 fgCoc03 = fgCocSourceChannel(i.uv23.xy, fromAlpha) * 0.8h;
    half2 fgCoc04 = fgCocSourceChannel(i.uv23.zw, fromAlpha) * 0.8h;
    half2 fgCoc05 = fgCocSourceChannel(i.uv45.xy, fromAlpha) * 0.6h;
    half2 fgCoc06 = fgCocSourceChannel(i.uv45.zw, fromAlpha) * 0.6h;
    half2 fgCoc07 = fgCocSourceChannel(i.uv67.xy, fromAlpha) * 0.40h;
    half2 fgCoc08 = fgCocSourceChannel(i.uv67.zw, fromAlpha) * 0.40f;
    half2 fgCoc09 = fgCocSourceChannel(i.uv89.xy, fromAlpha) * 0.25f;
    half2 fgCoc10 = fgCocSourceChannel(i.uv89.zw, fromAlpha) * 0.25f;

    half fgCoc;
    fgCoc = min( 0.0h, fgCoc00.r);
    fgCoc = min(fgCoc, fgCoc01.r);
    fgCoc = min(fgCoc, fgCoc02.r);
    fgCoc = min(fgCoc, fgCoc03.r);
    fgCoc = min(fgCoc, fgCoc04.r);
    fgCoc = min(fgCoc, fgCoc05.r);
    fgCoc = min(fgCoc, fgCoc06.r);
    fgCoc = min(fgCoc, fgCoc07.r);
    fgCoc = min(fgCoc, fgCoc08.r);
    fgCoc = min(fgCoc, fgCoc09.r);
    fgCoc = min(fgCoc, fgCoc10.r);

    return half2(fgCoc,fgCoc00.g);
}

float4 fragDilateFgCocFromColor (v2fBlur i) : SV_Target
{
    return weigthedFGCocBlur(i,true).xyxy;
}

float4 fragDilateFgCoc (v2fBlur i) : SV_Target
{
    return weigthedFGCocBlur(i,false).xyxy;
}

///////////////////////////////////////////////////////////////////////////////
// Texture Bokeh related
///////////////////////////////////////////////////////////////////////////////

float4 fragBlurAlphaWeighted (v2fBlur i) : SV_Target
{
    const float ALPHA_WEIGHT = 2.0f;
    float4 sum = float4 (0,0,0,0);
    float w = 0;
    float weights = 0;
    const float G_WEIGHTS[6] = {1.0, 0.8, 0.675, 0.5, 0.2, 0.075};

    float4 sampleA = tex2D(_MainTex, i.uv.xy);

    float4 sampleB = tex2D(_MainTex, i.uv01.xy);
    float4 sampleC = tex2D(_MainTex, i.uv01.zw);
    float4 sampleD = tex2D(_MainTex, i.uv23.xy);
    float4 sampleE = tex2D(_MainTex, i.uv23.zw);
    float4 sampleF = tex2D(_MainTex, i.uv45.xy);
    float4 sampleG = tex2D(_MainTex, i.uv45.zw);
    float4 sampleH = tex2D(_MainTex, i.uv67.xy);
    float4 sampleI = tex2D(_MainTex, i.uv67.zw);
    float4 sampleJ = tex2D(_MainTex, i.uv89.xy);
    float4 sampleK = tex2D(_MainTex, i.uv89.zw);

    w = sampleA.a * G_WEIGHTS[0]; sum += sampleA * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleB.a) * G_WEIGHTS[1]; sum += sampleB * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleC.a) * G_WEIGHTS[1]; sum += sampleC * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleD.a) * G_WEIGHTS[2]; sum += sampleD * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleE.a) * G_WEIGHTS[2]; sum += sampleE * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleF.a) * G_WEIGHTS[3]; sum += sampleF * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleG.a) * G_WEIGHTS[3]; sum += sampleG * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleH.a) * G_WEIGHTS[4]; sum += sampleH * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleI.a) * G_WEIGHTS[4]; sum += sampleI * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleJ.a) * G_WEIGHTS[5]; sum += sampleJ * w; weights += w;
    w = saturate(ALPHA_WEIGHT*sampleK.a) * G_WEIGHTS[5]; sum += sampleK * w; weights += w;

    sum /= weights + 1e-4f;

    sum.a = sampleA.a;
    if(sampleA.a<1e-2f) sum.rgb = sampleA.rgb;

    return sum;
}

float4 fragBoxBlur (v2f i) : SV_Target
{
    float4 returnValue = tex2D(_MainTex, i.uv1.xy + 0.75*_MainTex_TexelSize.xy);
    returnValue += tex2D(_MainTex, i.uv1.xy - 0.75*_MainTex_TexelSize.xy);
    returnValue += tex2D(_MainTex, i.uv1.xy + 0.75*_MainTex_TexelSize.xy * float2(1,-1));
    returnValue += tex2D(_MainTex, i.uv1.xy - 0.75*_MainTex_TexelSize.xy * float2(1,-1));
    return returnValue/4;
}

ENDCG

///////////////////////////////////////////////////////////////////////////////

SubShader
{

    ZTest Always Cull Off ZWrite Off Fog { Mode Off } Lighting Off Blend Off

    // 1
    Pass
    {
        CGPROGRAM
        #pragma vertex vertBlurPlusMinus
        #pragma fragment fragBlurAlphaWeighted
        ENDCG
    }

    // 2
    Pass
    {
        CGPROGRAM
        #pragma vertex vert_d
        #pragma fragment fragBoxBlur
        ENDCG
    }

    // 3
    Pass
    {
      CGPROGRAM
      #pragma vertex vertBlurPlusMinus
      #pragma fragment fragDilateFgCocFromColor
      ENDCG
    }

    // 4
    Pass
    {
      CGPROGRAM
      #pragma vertex vertBlurPlusMinus
      #pragma fragment fragDilateFgCoc
      ENDCG
    }

    // 5
    Pass
    {
        CGPROGRAM
        #pragma vertex vert_d
        #pragma fragment fragCaptureCoc
        ENDCG
    }

    // 6
    Pass
    {
        CGPROGRAM
        #pragma vertex vert_d
        #pragma fragment fragCaptureCocExplicit
        ENDCG
    }

    // 7
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragVisualizeCoc
        ENDCG
    }

    // 8
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragVisualizeCocExplicit
        ENDCG
    }

    // 9
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCocPrefilter
        ENDCG
    }

    // 10
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCircleBlur
        ENDCG
    }

    // 11
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCircleBlurWithDilatedFg
        ENDCG
    }

    // 12
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCircleBlurLowQuality
        ENDCG
    }

    // 13
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragCircleBlurWithDilatedFgLowQuality
        ENDCG
    }

    // 14
    Pass
    {
        CGPROGRAM
        #pragma vertex vertNoFlip
        #pragma fragment fragMerge
        ENDCG
    }

    // 15
    Pass
    {
        CGPROGRAM
        #pragma vertex vertNoFlip
        #pragma fragment fragMergeExplicit
        ENDCG
    }

    // 16
    Pass
    {
        CGPROGRAM
        #pragma vertex vertNoFlip
        #pragma fragment fragMergeBicubic
        ENDCG
    }

    // 17
    Pass
    {
        CGPROGRAM
        #pragma vertex vertNoFlip
        #pragma fragment fragMergeExplicitBicubic
        ENDCG
    }

    // 18
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeLowQuality
        ENDCG
    }

    // 19
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeLowQualityDilateFg
        ENDCG
    }

    // 20
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeLowQualityMerge
        ENDCG
    }

    // 21
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeLowQualityMergeDilateFg
        ENDCG
    }

    // 22
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeMediumQuality
        ENDCG
    }

    // 23
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeMediumQualityDilateFg
        ENDCG
    }

    // 24
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeMediumQualityMerge
        ENDCG
    }

    // 25
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeMediumQualityMergeDilateFg
        ENDCG
    }

    // 26
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeHighQuality
        ENDCG
    }

    // 27
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeHighQualityDilateFg
        ENDCG
    }

    // 28
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeHighQualityMerge
        ENDCG
    }

    // 29
    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment fragShapeHighQualityMergeDilateFg
        ENDCG
    }
}

FallBack Off
}
