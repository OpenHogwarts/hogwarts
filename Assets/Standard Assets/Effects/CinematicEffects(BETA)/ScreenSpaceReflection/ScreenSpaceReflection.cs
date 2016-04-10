using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Rendering/Screen Space Reflection")]
    public class ScreenSpaceReflection : MonoBehaviour
    {
        public enum SSRDebugMode
        {
            None = 0,
            IncomingRadiance = 1,
            SSRResult = 2,
            FinalGlossyTerm = 3,
            SSRMask = 4,
            Roughness = 5,
            BaseColor = 6,
            SpecColor = 7,
            Reflectivity = 8,
            ReflectionProbeOnly = 9,
            ReflectionProbeMinusSSR = 10,
            SSRMinusReflectionProbe = 11,
            NoGlossy = 12,
            NegativeNoGlossy = 13,
            MipLevel = 14,
        }

        public enum SSRResolution
        {
            FullResolution = 0,
            HalfTraceFullResolve = 1,
            HalfResolution = 2,
        }

        [Serializable]
        public struct SSRSettings
        {
            [AttributeUsage(AttributeTargets.Field)]
            public class LayoutAttribute : PropertyAttribute
            {}

            [Layout]
            public BasicSettings basicSettings;

            [Layout]
            public ReflectionSettings reflectionSettings;

            [Layout]
            public AdvancedSettings advancedSettings;

            [Layout]
            public DebugSettings debugSettings;

            private static readonly SSRSettings s_Performance = new SSRSettings
            {
                basicSettings = new BasicSettings
                {
                    screenEdgeFading = 0,
                    maxDistance = 10.0f,
                    fadeDistance = 10.0f,
                    reflectionMultiplier = 1.0f,
                    enableHDR = false,
                    additiveReflection = false
                },
                reflectionSettings = new ReflectionSettings
                {
                    maxSteps = 64,
                    rayStepSize = 4,
                    widthModifier = 0.5f,
                    smoothFallbackThreshold = 0.4f,
                    distanceBlur = 1.0f,
                    fresnelFade = 0.2f,
                    fresnelFadePower = 2.0f,
                    smoothFallbackDistance = 0.05f,
                },
                advancedSettings = new AdvancedSettings
                {
                    useTemporalConfidence = false,
                    temporalFilterStrength = 0.0f,
                    treatBackfaceHitAsMiss = false,
                    allowBackwardsRays = false,
                    traceBehindObjects = true,
                    highQualitySharpReflections = false,
                    traceEverywhere = false,
                    resolution = SSRResolution.HalfResolution,
                    bilateralUpsample = false,
                    improveCorners = false,
                    reduceBanding = false,
                    highlightSuppression = false
                },
                debugSettings = new DebugSettings
                {
                    debugMode = SSRDebugMode.None
                }
            };

            private static readonly SSRSettings s_Default = new SSRSettings
            {
                basicSettings = new BasicSettings
                {
                    screenEdgeFading = 0.03f,
                    maxDistance = 100.0f,
                    fadeDistance = 100.0f,
                    reflectionMultiplier = 1.0f,
                    enableHDR = true,
                    additiveReflection = false,
                },
                reflectionSettings = new ReflectionSettings
                {
                    maxSteps = 128,
                    rayStepSize = 3,
                    widthModifier = 0.5f,
                    smoothFallbackThreshold = 0.2f,
                    distanceBlur = 1.0f,
                    fresnelFade = 0.2f,
                    fresnelFadePower = 2.0f,
                    smoothFallbackDistance = 0.05f,
                },
                advancedSettings = new AdvancedSettings
                {
                    useTemporalConfidence = true,
                    temporalFilterStrength = 0.7f,
                    treatBackfaceHitAsMiss = false,
                    allowBackwardsRays = false,
                    traceBehindObjects = true,
                    highQualitySharpReflections = true,
                    traceEverywhere = true,
                    resolution = SSRResolution.HalfTraceFullResolve,
                    bilateralUpsample = true,
                    improveCorners = true,
                    reduceBanding = true,
                    highlightSuppression = false
                },
                debugSettings = new DebugSettings
                {
                    debugMode = SSRDebugMode.None
                }
            };

            private static readonly SSRSettings s_HighQuality = new SSRSettings
            {
                basicSettings = new BasicSettings
                {
                    screenEdgeFading = 0.03f,
                    maxDistance = 100.0f,
                    fadeDistance = 100.0f,
                    reflectionMultiplier = 1.0f,
                    enableHDR = true,
                    additiveReflection = false,
                },
                reflectionSettings = new ReflectionSettings
                {
                    maxSteps = 512,
                    rayStepSize = 1,
                    widthModifier = 0.5f,
                    smoothFallbackThreshold = 0.2f,
                    distanceBlur = 1.0f,
                    fresnelFade = 0.2f,
                    fresnelFadePower = 2.0f,
                    smoothFallbackDistance = 0.05f,
                },
                advancedSettings = new AdvancedSettings
                {
                    useTemporalConfidence = true,
                    temporalFilterStrength = 0.7f,
                    treatBackfaceHitAsMiss = false,
                    allowBackwardsRays = false,
                    traceBehindObjects = true,
                    highQualitySharpReflections = true,
                    traceEverywhere = true,
                    resolution = SSRResolution.HalfTraceFullResolve,
                    bilateralUpsample = true,
                    improveCorners = true,
                    reduceBanding = true,
                    highlightSuppression = false
                },
                debugSettings = new DebugSettings
                {
                    debugMode = SSRDebugMode.None
                }
            };


            public static SSRSettings performanceSettings
            {
                get { return s_Performance; }
            }

            public static SSRSettings defaultSettings
            {
                get { return s_Default; }
            }

            public static SSRSettings highQualitySettings
            {
                get { return s_HighQuality; }
            }
        }

        [Serializable]
        public struct BasicSettings
        {
            /// BASIC SETTINGS
            [Tooltip("Nonphysical multiplier for the SSR reflections. 1.0 is physically based.")]
            [Range(0.0f, 2.0f)]
            public float reflectionMultiplier;

            [Tooltip("Maximum reflection distance in world units.")]
            [Range(0.5f, 1000.0f)]
            public float maxDistance;

            [Tooltip("How far away from the maxDistance to begin fading SSR.")]
            [Range(0.0f, 1000.0f)]
            public float fadeDistance;

            [Tooltip("Higher = fade out SSRR near the edge of the screen so that reflections don't pop under camera motion.")]
            [Range(0.0f, 1.0f)]
            public float screenEdgeFading;

            [Tooltip("Enable for better reflections of very bright objects at a performance cost")]
            public bool enableHDR;

            // When enabled, we just add our reflections on top of the existing ones. This is physically incorrect, but several
            // popular demos and games have taken this approach, and it does hide some artifacts.
            [Tooltip("Add reflections on top of existing ones. Not physically correct.")]
            public bool additiveReflection;
        }

        [Serializable]
        public struct ReflectionSettings
        {
            /// REFLECTIONS
            [Tooltip("Max raytracing length.")]
            [Range(16, 2048)]
            public int maxSteps;

            [Tooltip("Log base 2 of ray tracing coarse step size. Higher traces farther, lower gives better quality silhouettes.")]
            [Range(0, 4)]
            public int rayStepSize;

            [Tooltip("Typical thickness of columns, walls, furniture, and other objects that reflection rays might pass behind.")]
            [Range(0.01f, 10.0f)]
            public float widthModifier;

            [Tooltip("Increase if reflections flicker on very rough surfaces.")]
            [Range(0.0f, 1.0f)]
            public float smoothFallbackThreshold;

            [Tooltip("Start falling back to non-SSR value solution at smoothFallbackThreshold - smoothFallbackDistance, with full fallback occuring at smoothFallbackThreshold.")]
            [Range(0.0f, 0.2f)]
            public float smoothFallbackDistance;

            [Tooltip("Amplify Fresnel fade out. Increase if floor reflections look good close to the surface and bad farther 'under' the floor.")]
            [Range(0.0f, 1.0f)]
            public float fresnelFade;

            [Tooltip("Higher values correspond to a faster Fresnel fade as the reflection changes from the grazing angle.")]
            [Range(0.1f, 10.0f)]
            public float fresnelFadePower;

            [Tooltip("Controls how blurry reflections get as objects are further from the camera. 0 is constant blur no matter trace distance or distance from camera. 1 fully takes into account both factors.")]
            [Range(0.0f, 1.0f)]
            public float distanceBlur;
        }

        [Serializable]
        public struct AdvancedSettings
        {
            /// ADVANCED
            [Range(0.0f, 0.99f)]
            [Tooltip("Increase to decrease flicker in scenes; decrease to prevent ghosting (especially in dynamic scenes). 0 gives maximum performance.")]
            public float temporalFilterStrength;

            [Tooltip("Enable to limit ghosting from applying the temporal filter.")]
            public bool useTemporalConfidence;

            [Tooltip("Enable to allow rays to pass behind objects. This can lead to more screen-space reflections, but the reflections are more likely to be wrong.")]
            public bool traceBehindObjects;

            [Tooltip("Enable to increase quality of the sharpest reflections (through filtering), at a performance cost.")]
            public bool highQualitySharpReflections;

            [Tooltip("Improves quality in scenes with varying smoothness, at a potential performance cost.")]
            public bool traceEverywhere;

            [Tooltip("Enable to force more surfaces to use reflection probes if you see streaks on the sides of objects or bad reflections of their backs.")]
            public bool treatBackfaceHitAsMiss;

            [Tooltip("Enable for a performance gain in scenes where most glossy objects are horizontal, like floors, water, and tables. Leave on for scenes with glossy vertical objects.")]
            public bool allowBackwardsRays;

            [Tooltip("Improve visual fidelity of reflections on rough surfaces near corners in the scene, at the cost of a small amount of performance.")]
            public bool improveCorners;

            [Tooltip("Half resolution SSRR is much faster, but less accurate. Quality can be reclaimed for some performance by doing the resolve at full resolution.")]
            public SSRResolution resolution;

            [Tooltip("Drastically improves reflection reconstruction quality at the expense of some performance.")]
            public bool bilateralUpsample;

            [Tooltip("Improve visual fidelity of mirror reflections at the cost of a small amount of performance.")]
            public bool reduceBanding;

            [Tooltip("Enable to limit the effect a few bright pixels can have on rougher surfaces")]
            public bool highlightSuppression;
        }

        [Serializable]
        public struct DebugSettings
        {
            /// DEBUG
            [Tooltip("Various Debug Visualizations")]
            public SSRDebugMode debugMode;
        }


        [SerializeField]
        public SSRSettings settings = SSRSettings.defaultSettings;

        ///////////// Unexposed Variables //////////////////

        // Perf optimization we still need to test across platforms
        [Tooltip("Enable to try and bypass expensive bilateral upsampling away from edges. There is a slight performance hit for generating the edge buffers, but a potentially high performance savings from bypassing bilateral upsampling where it is unneeded. Test on your target platforms to see if performance improves.")]
        private bool useEdgeDetector = false;

        // Debug variable, useful for forcing all surfaces in a scene to reflection with arbitrary sharpness/roughness
        [Range(-4.0f, 4.0f)]
        private float mipBias = 0.0f;

        // Flag for whether to knock down the reflection term by occlusion stored in the gbuffer. Currently consistently gives
        // better results when true, so this flag is private for now.
        private bool useOcclusion = true;

        // When enabled, all filtering is performed at the highest resolution. This is extraordinarily slow, and should only be used during development.
        private bool fullResolutionFiltering = false;

        // Crude sky fallback, feature-gated until next revision
        private bool fallbackToSky = false;

        // For next release; will improve quality at the expense of performance
        private bool computeAverageRayDistance = false;

        // Internal values for temporal filtering
        private bool m_HasInformationFromPreviousFrame;
        private Matrix4x4 m_PreviousWorldToCameraMatrix;
        private RenderTexture m_PreviousDepthBuffer;
        private RenderTexture m_PreviousHitBuffer;
        private RenderTexture m_PreviousReflectionBuffer;

        public Shader ssrShader;
        private Material m_SSRMaterial;

        [NonSerialized]
        private RenderTexureUtility m_RTU = new RenderTexureUtility();

        public Material ssrMaterial
        {
            get
            {
                if (m_SSRMaterial == null)
                    m_SSRMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(ssrShader);

                return m_SSRMaterial;
            }
        }

        // Shader pass indices used by the effect
        private enum PassIndex
        {
            RayTraceStep1 = 0,
            RayTraceStep2 = 1,
            RayTraceStep4 = 2,
            RayTraceStep8 = 3,
            RayTraceStep16 = 4,
            CompositeFinal = 5,
            Blur = 6,
            CompositeSSR = 7,
            Blit = 8,
            EdgeGeneration = 9,
            MinMipGeneration = 10,
            HitPointToReflections = 11,
            BilateralKeyPack = 12,
            BlitDepthAsCSZ = 13,
            TemporalFilter = 14,
            AverageRayDistanceGeneration = 15,
            PoissonBlur = 16,
        }


        protected void OnEnable()
        {
            if (ssrShader == null)
                ssrShader = Shader.Find("Hidden/ScreenSpaceReflection");

            if (!ImageEffectHelper.IsSupported(ssrShader, true, true, this))
            {
                enabled = false;
                Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
                return;
            }

            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnDisable()
        {
            if (m_SSRMaterial)
                DestroyImmediate(m_SSRMaterial);
            if (m_PreviousDepthBuffer)
                DestroyImmediate(m_PreviousDepthBuffer);
            if (m_PreviousHitBuffer)
                DestroyImmediate(m_PreviousHitBuffer);
            if (m_PreviousReflectionBuffer)
                DestroyImmediate(m_PreviousReflectionBuffer);

            m_SSRMaterial = null;
            m_PreviousDepthBuffer = null;
            m_PreviousHitBuffer = null;
            m_PreviousReflectionBuffer = null;
        }

        private void PreparePreviousBuffers(int w, int h)
        {
            if (m_PreviousDepthBuffer != null)
            {
                if ((m_PreviousDepthBuffer.width != w) || (m_PreviousDepthBuffer.height != h))
                {
                    DestroyImmediate(m_PreviousDepthBuffer);
                    DestroyImmediate(m_PreviousHitBuffer);
                    DestroyImmediate(m_PreviousReflectionBuffer);
                    m_PreviousDepthBuffer = null;
                    m_PreviousHitBuffer = null;
                    m_PreviousReflectionBuffer = null;
                }
            }
            if (m_PreviousDepthBuffer == null)
            {
                m_PreviousDepthBuffer = new RenderTexture(w, h, 0, RenderTextureFormat.RFloat);
                m_PreviousHitBuffer = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf);
                m_PreviousReflectionBuffer = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf);
            }
        }

        [ImageEffectOpaque]
        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (ssrMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (m_HasInformationFromPreviousFrame)
            {
                m_HasInformationFromPreviousFrame = (m_PreviousDepthBuffer != null) &&
                    (source.width == m_PreviousDepthBuffer.width) &&
                    (source.height == m_PreviousDepthBuffer.height);
            }
            bool doTemporalFilterThisFrame = m_HasInformationFromPreviousFrame && settings.advancedSettings.temporalFilterStrength > 0.0;
            m_HasInformationFromPreviousFrame = false;

            // Not using deferred shading? Just blit source to destination.
            if (Camera.current.actualRenderingPath != RenderingPath.DeferredShading)
            {
                Graphics.Blit(source, destination);
                return;
            }

            var rtW = source.width;
            var rtH = source.height;

            // RGB: Normals, A: Roughness.
            // Has the nice benefit of allowing us to control the filtering mode as well.
            RenderTexture bilateralKeyTexture = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, RenderTextureFormat.ARGB32);
            bilateralKeyTexture.filterMode = FilterMode.Point;
            Graphics.Blit(source, bilateralKeyTexture, ssrMaterial, (int)PassIndex.BilateralKeyPack);
            ssrMaterial.SetTexture("_NormalAndRoughnessTexture", bilateralKeyTexture);

            float sWidth = source.width;
            float sHeight = source.height;

            Vector2 sourceToTempUV = new Vector2(sWidth / rtW, sHeight / rtH);

            int downsampleAmount = (settings.advancedSettings.resolution == SSRResolution.FullResolution) ? 1 : 2;

            rtW = rtW / downsampleAmount;
            rtH = rtH / downsampleAmount;

            ssrMaterial.SetVector("_SourceToTempUV", new Vector4(sourceToTempUV.x, sourceToTempUV.y, 1.0f / sourceToTempUV.x, 1.0f / sourceToTempUV.y));


            Matrix4x4 P = GetComponent<Camera>().projectionMatrix;
            Vector4 projInfo = new Vector4
                    ((-2.0f / (sWidth * P[0])),
                    (-2.0f / (sHeight * P[5])),
                    ((1.0f - P[2]) / P[0]),
                    ((1.0f + P[6]) / P[5]));

            /** The height in pixels of a 1m object if viewed from 1m away. */
            float pixelsPerMeterAtOneMeter = sWidth / (-2.0f * (float)(Math.Tan(GetComponent<Camera>().fieldOfView / 180.0 * Math.PI * 0.5)));
            ssrMaterial.SetFloat("_PixelsPerMeterAtOneMeter", pixelsPerMeterAtOneMeter);


            float sx = sWidth / 2.0f;
            float sy = sHeight / 2.0f;

            Matrix4x4 warpToScreenSpaceMatrix = new Matrix4x4();
            warpToScreenSpaceMatrix.SetRow(0, new Vector4(sx, 0.0f, 0.0f, sx));
            warpToScreenSpaceMatrix.SetRow(1, new Vector4(0.0f, sy, 0.0f, sy));
            warpToScreenSpaceMatrix.SetRow(2, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
            warpToScreenSpaceMatrix.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            Matrix4x4 projectToPixelMatrix = warpToScreenSpaceMatrix * P;

            ssrMaterial.SetVector("_ScreenSize", new Vector2(sWidth, sHeight));
            ssrMaterial.SetVector("_ReflectionBufferSize", new Vector2(rtW, rtH));
            Vector2 invScreenSize = new Vector2((float)(1.0 / sWidth), (float)(1.0 / sHeight));

            Matrix4x4 worldToCameraMatrix = GetComponent<Camera>().worldToCameraMatrix;
            Matrix4x4 cameraToWorldMatrix = GetComponent<Camera>().worldToCameraMatrix.inverse;
            ssrMaterial.SetVector("_InvScreenSize", invScreenSize);
            ssrMaterial.SetVector("_ProjInfo", projInfo); // used for unprojection
            ssrMaterial.SetMatrix("_ProjectToPixelMatrix", projectToPixelMatrix);
            ssrMaterial.SetMatrix("_WorldToCameraMatrix", worldToCameraMatrix);
            ssrMaterial.SetMatrix("_CameraToWorldMatrix", cameraToWorldMatrix);
            ssrMaterial.SetInt("_EnableRefine", settings.advancedSettings.reduceBanding ? 1 : 0);
            ssrMaterial.SetInt("_AdditiveReflection", settings.basicSettings.additiveReflection ? 1 : 0);
            ssrMaterial.SetInt("_ImproveCorners", settings.advancedSettings.improveCorners ? 1 : 0);
            ssrMaterial.SetFloat("_ScreenEdgeFading", settings.basicSettings.screenEdgeFading);
            ssrMaterial.SetFloat("_MipBias", mipBias);
            ssrMaterial.SetInt("_UseOcclusion", useOcclusion ? 1 : 0);
            ssrMaterial.SetInt("_BilateralUpsampling", settings.advancedSettings.bilateralUpsample ? 1 : 0);
            ssrMaterial.SetInt("_FallbackToSky", fallbackToSky ? 1 : 0);
            ssrMaterial.SetInt("_TreatBackfaceHitAsMiss", settings.advancedSettings.treatBackfaceHitAsMiss ? 1 : 0);
            ssrMaterial.SetInt("_AllowBackwardsRays", settings.advancedSettings.allowBackwardsRays ? 1 : 0);
            ssrMaterial.SetInt("_TraceEverywhere", settings.advancedSettings.traceEverywhere ? 1 : 0);

            float z_f = GetComponent<Camera>().farClipPlane;
            float z_n = GetComponent<Camera>().nearClipPlane;

            Vector3 cameraClipInfo = (float.IsPositiveInfinity(z_f)) ?
                new Vector3(z_n, -1.0f, 1.0f) :
                new Vector3(z_n * z_f, z_n - z_f, z_f);

            ssrMaterial.SetVector("_CameraClipInfo", cameraClipInfo);
            ssrMaterial.SetFloat("_MaxRayTraceDistance", settings.basicSettings.maxDistance);
            ssrMaterial.SetFloat("_FadeDistance", settings.basicSettings.fadeDistance);
            ssrMaterial.SetFloat("_LayerThickness", settings.reflectionSettings.widthModifier);

            const int maxMip = 5;
            RenderTexture[] reflectionBuffers;
            RenderTextureFormat intermediateFormat = settings.basicSettings.enableHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;

            reflectionBuffers = new RenderTexture[maxMip];
            for (int i = 0; i < maxMip; ++i)
            {
                if (fullResolutionFiltering)
                    reflectionBuffers[i] = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, intermediateFormat);
                else
                    reflectionBuffers[i] = m_RTU.GetTemporaryRenderTexture(rtW >> i, rtH >> i, 0, intermediateFormat);
                // We explicitly interpolate during bilateral upsampling.
                reflectionBuffers[i].filterMode = settings.advancedSettings.bilateralUpsample ? FilterMode.Point : FilterMode.Bilinear;
            }

            ssrMaterial.SetInt("_EnableSSR", 1);
            ssrMaterial.SetInt("_DebugMode", (int)settings.debugSettings.debugMode);

            ssrMaterial.SetInt("_TraceBehindObjects", settings.advancedSettings.traceBehindObjects ? 1 : 0);

            ssrMaterial.SetInt("_MaxSteps", settings.reflectionSettings.maxSteps);

            RenderTexture rayHitTexture = m_RTU.GetTemporaryRenderTexture(rtW, rtH);

            // We have 5 passes for different step sizes
            int tracePass = Mathf.Clamp(settings.reflectionSettings.rayStepSize, 0, 4);
            Graphics.Blit(source, rayHitTexture, ssrMaterial, tracePass);

            ssrMaterial.SetTexture("_HitPointTexture", rayHitTexture);
            // Resolve the hitpoints into the mirror reflection buffer
            Graphics.Blit(source, reflectionBuffers[0], ssrMaterial, (int)PassIndex.HitPointToReflections);

            ssrMaterial.SetTexture("_ReflectionTexture0", reflectionBuffers[0]);
            ssrMaterial.SetInt("_FullResolutionFiltering", fullResolutionFiltering ? 1 : 0);

            ssrMaterial.SetFloat("_MaxRoughness", 1.0f - settings.reflectionSettings.smoothFallbackThreshold);
            ssrMaterial.SetFloat("_RoughnessFalloffRange", settings.reflectionSettings.smoothFallbackDistance);

            ssrMaterial.SetFloat("_SSRMultiplier", settings.basicSettings.reflectionMultiplier);

            RenderTexture[] edgeTextures = new RenderTexture[maxMip];
            if (settings.advancedSettings.bilateralUpsample && useEdgeDetector)
            {
                edgeTextures[0] = m_RTU.GetTemporaryRenderTexture(rtW, rtH);
                Graphics.Blit(source, edgeTextures[0], ssrMaterial, (int)PassIndex.EdgeGeneration);
                for (int i = 1; i < maxMip; ++i)
                {
                    edgeTextures[i] = m_RTU.GetTemporaryRenderTexture(rtW >> i, rtH >> i);
                    ssrMaterial.SetInt("_LastMip", i - 1);
                    Graphics.Blit(edgeTextures[i - 1], edgeTextures[i], ssrMaterial, (int)PassIndex.MinMipGeneration);
                }
            }

            if (settings.advancedSettings.highQualitySharpReflections)
            {
                RenderTexture filteredReflections = m_RTU.GetTemporaryRenderTexture(reflectionBuffers[0].width, reflectionBuffers[0].height, 0, reflectionBuffers[0].format);
                filteredReflections.filterMode = reflectionBuffers[0].filterMode;
                reflectionBuffers[0].filterMode = FilterMode.Bilinear;
                Graphics.Blit(reflectionBuffers[0], filteredReflections, ssrMaterial, (int)PassIndex.PoissonBlur);

                // Replace the unfiltered buffer with the newly filtered one.
                m_RTU.ReleaseTemporaryRenderTexture(reflectionBuffers[0]);
                reflectionBuffers[0] = filteredReflections;
                ssrMaterial.SetTexture("_ReflectionTexture0", reflectionBuffers[0]);
            }

            // Generate the blurred low-resolution buffers
            for (int i = 1; i < maxMip; ++i)
            {
                RenderTexture inputTex = reflectionBuffers[i - 1];

                RenderTexture hBlur;
                if (fullResolutionFiltering)
                    hBlur = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, intermediateFormat);
                else
                {
                    int lowMip = i;
                    hBlur = m_RTU.GetTemporaryRenderTexture(rtW >> lowMip, rtH >> (i - 1), 0, intermediateFormat);
                }
                for (int j = 0; j < (fullResolutionFiltering ? (i * i) : 1); ++j)
                {
                    // Currently we blur at the resolution of the previous mip level, we could save bandwidth by blurring directly to the lower resolution.
                    ssrMaterial.SetVector("_Axis", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    ssrMaterial.SetFloat("_CurrentMipLevel", i - 1.0f);

                    Graphics.Blit(inputTex, hBlur, ssrMaterial, (int)PassIndex.Blur);

                    ssrMaterial.SetVector("_Axis", new Vector4(0.0f, 1.0f, 0.0f, 0.0f));

                    inputTex = reflectionBuffers[i];
                    Graphics.Blit(hBlur, inputTex, ssrMaterial, (int)PassIndex.Blur);
                }

                ssrMaterial.SetTexture("_ReflectionTexture" + i, reflectionBuffers[i]);

                m_RTU.ReleaseTemporaryRenderTexture(hBlur);
            }


            if (settings.advancedSettings.bilateralUpsample && useEdgeDetector)
            {
                for (int i = 0; i < maxMip; ++i)
                    ssrMaterial.SetTexture("_EdgeTexture" + i, edgeTextures[i]);
            }
            ssrMaterial.SetInt("_UseEdgeDetector", useEdgeDetector ? 1 : 0);

            RenderTexture averageRayDistanceBuffer = m_RTU.GetTemporaryRenderTexture(source.width, source.height, 0, RenderTextureFormat.RHalf);
            if (computeAverageRayDistance)
            {
                Graphics.Blit(source, averageRayDistanceBuffer, ssrMaterial, (int)PassIndex.AverageRayDistanceGeneration);
            }
            ssrMaterial.SetInt("_UseAverageRayDistance", computeAverageRayDistance ? 1 : 0);
            ssrMaterial.SetTexture("_AverageRayDistanceBuffer", averageRayDistanceBuffer);
            bool resolveDiffersFromTraceRes = (settings.advancedSettings.resolution == SSRResolution.HalfTraceFullResolve);
            RenderTexture finalReflectionBuffer = m_RTU.GetTemporaryRenderTexture(resolveDiffersFromTraceRes ? source.width : rtW, resolveDiffersFromTraceRes ? source.height : rtH, 0, intermediateFormat);

            ssrMaterial.SetFloat("_FresnelFade", settings.reflectionSettings.fresnelFade);
            ssrMaterial.SetFloat("_FresnelFadePower", settings.reflectionSettings.fresnelFadePower);
            ssrMaterial.SetFloat("_DistanceBlur", settings.reflectionSettings.distanceBlur);
            ssrMaterial.SetInt("_HalfResolution", (settings.advancedSettings.resolution != SSRResolution.FullResolution) ? 1 : 0);
            ssrMaterial.SetInt("_HighlightSuppression", settings.advancedSettings.highlightSuppression ? 1 : 0);
            Graphics.Blit(reflectionBuffers[0], finalReflectionBuffer, ssrMaterial, (int)PassIndex.CompositeSSR);
            ssrMaterial.SetTexture("_FinalReflectionTexture", finalReflectionBuffer);


            RenderTexture temporallyFilteredBuffer = m_RTU.GetTemporaryRenderTexture(resolveDiffersFromTraceRes ? source.width : rtW, resolveDiffersFromTraceRes ? source.height : rtH, 0, intermediateFormat);
            if (doTemporalFilterThisFrame)
            {
                ssrMaterial.SetInt("_UseTemporalConfidence", settings.advancedSettings.useTemporalConfidence ? 1 : 0);
                ssrMaterial.SetFloat("_TemporalAlpha", settings.advancedSettings.temporalFilterStrength);
                ssrMaterial.SetMatrix("_CurrentCameraToPreviousCamera", m_PreviousWorldToCameraMatrix * cameraToWorldMatrix);
                ssrMaterial.SetTexture("_PreviousReflectionTexture", m_PreviousReflectionBuffer);
                ssrMaterial.SetTexture("_PreviousCSZBuffer", m_PreviousDepthBuffer);
                Graphics.Blit(source, temporallyFilteredBuffer, ssrMaterial, (int)PassIndex.TemporalFilter);

                ssrMaterial.SetTexture("_FinalReflectionTexture", temporallyFilteredBuffer);
            }

            if (settings.advancedSettings.temporalFilterStrength > 0.0)
            {
                m_PreviousWorldToCameraMatrix = worldToCameraMatrix;
                PreparePreviousBuffers(source.width, source.height);
                Graphics.Blit(source, m_PreviousDepthBuffer, ssrMaterial, (int)PassIndex.BlitDepthAsCSZ);
                Graphics.Blit(rayHitTexture, m_PreviousHitBuffer);
                Graphics.Blit(doTemporalFilterThisFrame ? temporallyFilteredBuffer : finalReflectionBuffer, m_PreviousReflectionBuffer);

                m_HasInformationFromPreviousFrame = true;
            }


            Graphics.Blit(source, destination, ssrMaterial, (int)PassIndex.CompositeFinal);

            m_RTU.ReleaseAllTemporyRenderTexutres();
        }
    }
}
