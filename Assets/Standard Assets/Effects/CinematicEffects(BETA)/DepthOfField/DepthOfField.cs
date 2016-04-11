using UnityEngine;
using System;

namespace UnityStandardAssets.CinematicEffects
{
    //Improvement ideas:
    //  Use rgba8 buffer in ldr / in some pass in hdr (in correlation to previous point and remapping coc from -1/0/1 to 0/0.5/1)
    //  Use temporal stabilisation.
    //  Add a mode to do bokeh texture in quarter res as well
    //  Support different near and far blur for the bokeh texture
    //  Try distance field for the bokeh texture.
    //  Try to separate the output of the blur pass to two rendertarget near+far, see the gain in quality vs loss in performance.
    //  Try swirl effect on the samples of the circle blur.

    //References :
    //  This DOF implementation use ideas from public sources, a big thank to them :
    //  http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
    //  http://www.crytek.com/download/Sousa_Graphics_Gems_CryENGINE3.pdf
    //  http://graphics.cs.williams.edu/papers/MedianShaderX6/
    //  http://http.developer.nvidia.com/GPUGems/gpugems_ch24.html
    //  http://vec3.ca/bicubic-filtering-in-fewer-taps/

    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Cinematic/Depth Of Field")]
    [RequireComponent(typeof(Camera))]
    public class DepthOfField : MonoBehaviour
    {
        private const float kMaxBlur = 35.0f;

        #region Render passes
        private enum Passes
        {
            BlurAlphaWeighted                 =  0 ,
            BoxBlur                           =  1 ,
            DilateFgCocFromColor              =  2 ,
            DilateFgCoc                       =  3 ,
            CaptureCoc                        =  4 ,
            CaptureCocExplicit                =  5 ,
            VisualizeCoc                      =  6 ,
            VisualizeCocExplicit              =  7 ,
            CocPrefilter                      =  8 ,
            CircleBlur                        =  9 ,
            CircleBlurWithDilatedFg           =  10,
            CircleBlurLowQuality              =  11,
            CircleBlowLowQualityWithDilatedFg =  12,
            Merge                             =  13,
            MergeExplicit                     =  14,
            MergeBicubic                      =  15,
            MergeExplicitBicubic              =  16,
            ShapeLowQuality                   =  17,
            ShapeLowQualityDilateFg           =  18,
            ShapeLowQualityMerge              =  19,
            ShapeLowQualityMergeDilateFg      =  20,
            ShapeMediumQuality                =  21,
            ShapeMediumQualityDilateFg        =  22,
            ShapeMediumQualityMerge           =  23,
            ShapeMediumQualityMergeDilateFg   =  24,
            ShapeHighQuality                  =  25,
            ShapeHighQualityDilateFg          =  26,
            ShapeHighQualityMerge             =  27,
            ShapeHighQualityMergeDilateFg     =  28
        }

        private enum MedianPasses
        {
            Median3 = 0,
            Median3X3 = 1
        }

        private enum BokehTexturesPasses
        {
            Apply = 0,
            Collect = 1
        }
        #endregion

        public enum TweakMode
        {
            Basic,
            Advanced,
            Explicit
        }

        public enum ApertureShape
        {
            Circular,
            Hexagonal,
            Octogonal
        }

        public enum QualityPreset
        {
            Simple,
            Low,
            Medium,
            High,
            VeryHigh,
            Ultra,
            Custom
        }

        public enum FilterQuality
        {
            None,
            Normal,
            High
        }

        #region Attributes
        [AttributeUsage(AttributeTargets.Field)]
        public class TopLevelSettings : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsGroup : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class AllTweakModes : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class Basic : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class Advanced : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class Explicit : Attribute
        {}
        #endregion

        #region Settings
        [Serializable]
        public struct GlobalSettings
        {
            [Tooltip("Allows to view where the blur will be applied. Yellow for near blur, blue for far blur.")]
            public bool visualizeBluriness;

            [Tooltip("Setup mode. Use \"Advanced\" if you need more control on blur settings and/or want to use a bokeh texture. \"Explicit\" is the same as \"Advanced\" but makes use of \"Near Plane\" and \"Far Plane\" values instead of \"F-Stop\".")]
            public TweakMode tweakMode;

            [Tooltip("Quality presets. Use \"Custom\" for more advanced settings.")]
            public QualityPreset quality;

            [Space, Tooltip("\"Circular\" is the fastest, followed by \"Hexagonal\" and \"Octogonal\".")]
            public ApertureShape apertureShape;

            [Range(0f, 179f), Tooltip("Rotates the aperture when working with \"Hexagonal\" and \"Ortogonal\".")]
            public float apertureOrientation;

            public static GlobalSettings defaultSettings
            {
                get
                {
                    return new GlobalSettings
                    {
                        visualizeBluriness = false,
                        tweakMode = TweakMode.Basic,
                        quality = QualityPreset.High,
                        apertureShape = ApertureShape.Circular,
                        apertureOrientation = 0f
                    };
                }
            }
        }

        [Serializable]
        public struct QualitySettings
        {
            [Tooltip("Enable this to get smooth bokeh.")]
            public bool prefilterBlur;

            [Tooltip("Applies a median filter for even smoother bokeh.")]
            public FilterQuality medianFilter;

            [Tooltip("Dilates near blur over in focus area.")]
            public bool dilateNearBlur;

            [Tooltip("Uses high quality upsampling.")]
            public bool highQualityUpsampling;

            [Tooltip("Prevent haloing from bright in focus region over dark out of focus region.")]
            public bool preventHaloing;

            public static QualitySettings[] presetQualitySettings =
            {
                // Simple
                new QualitySettings
                {
                    prefilterBlur = false,
                    medianFilter = FilterQuality.None,
                    dilateNearBlur = false,
                    highQualityUpsampling = false,
                    preventHaloing = false
                },

                // Low
                new QualitySettings
                {
                    prefilterBlur = true,
                    medianFilter = FilterQuality.None,
                    dilateNearBlur = false,
                    highQualityUpsampling = false,
                    preventHaloing = false
                },

                // Medium
                new QualitySettings
                {
                    prefilterBlur = true,
                    medianFilter = FilterQuality.Normal,
                    dilateNearBlur = false,
                    highQualityUpsampling = false,
                    preventHaloing = false
                },

                // High
                new QualitySettings
                {
                    prefilterBlur = true,
                    medianFilter = FilterQuality.Normal,
                    dilateNearBlur = true,
                    highQualityUpsampling = false,
                    preventHaloing = false
                },

                // Very high
                new QualitySettings
                {
                    prefilterBlur = true,
                    medianFilter = FilterQuality.High,
                    dilateNearBlur = true,
                    highQualityUpsampling = false,
                    preventHaloing = true
                },

                // Ultra
                new QualitySettings
                {
                    prefilterBlur = true,
                    medianFilter = FilterQuality.High,
                    dilateNearBlur = true,
                    highQualityUpsampling = true,
                    preventHaloing = true
                }
            };
        }

        [Serializable]
        public struct FocusSettings
        {
            [Basic, Advanced, Explicit, Tooltip("Auto-focus on a selected transform.")]
            public Transform transform;

            [Basic, Advanced, Explicit, Range(0f, 1f), Tooltip("Focus distance.")]
            public float plane;

            [Explicit, Range(0f, 1f), Tooltip("Near focus distance.")]
            public float nearPlane;

            [Explicit, Range(0f, 1f), Tooltip("Far focus distance.")]
            public float farPlane;

            [Basic, Advanced, Range(0f, 32f), Tooltip("Simulates focal ratio. Lower values will result in a narrow depth of field.")]
            public float fStops;

            [Basic, Advanced, Explicit, Range(0f, 1f), Tooltip("Focus range/spread. Use this to fine-tune the F-Stop range.")]
            public float rangeAdjustment;

            public static FocusSettings defaultSettings
            {
                get
                {
                    return new FocusSettings
                    {
                        transform = null,
                        plane = 0.225f,
                        nearPlane = 0f,
                        farPlane = 1f,
                        fStops = 5f,
                        rangeAdjustment = 0.9f
                    };
                }
            }
        }

        [Serializable]
        public struct BokehTextureSettings
        {
            [Advanced, Explicit, Tooltip("Adding a texture to this field will enable the use of \"Bokeh Textures\". Use with care. This feature is only available on Shader Model 5 compatible-hardware and performance scale with the amount of bokeh.")]
            public Texture2D texture;

            [Advanced, Explicit, Range(0.01f, 5f), Tooltip("Maximum size of bokeh textures on screen.")]
            public float scale;

            [Advanced, Explicit, Range(0.01f, 100f), Tooltip("Bokeh brightness.")]
            public float intensity;

            [Advanced, Explicit, Range(0.01f, 50f), Tooltip("Controls the amount of bokeh textures. Lower values mean more bokeh splats.")]
            public float threshold;

            [Advanced, Explicit, Range(0.01f, 1f), Tooltip("Controls the spawn conditions. Lower values mean more visible bokeh.")]
            public float spawnHeuristic;

            public static BokehTextureSettings defaultSettings
            {
                get
                {
                    return new BokehTextureSettings
                    {
                        texture = null,
                        scale = 1f,
                        intensity = 50f,
                        threshold = 2f,
                        spawnHeuristic = 0.15f
                    };
                }
            }
        }

        [Serializable]
        public struct BlurSettings
        {
            [Basic, Advanced, Explicit, Range(0f, kMaxBlur), Tooltip("Maximum blur radius for the near plane.")]
            public float nearRadius;

            [Basic, Advanced, Explicit, Range(0f, kMaxBlur), Tooltip("Maximum blur radius for the far plane.")]
            public float farRadius;

            [Advanced, Explicit, Range(0.5f, 4f), Tooltip("Blur luminosity booster threshold for the near and far boost amounts.")]
            public float boostPoint;

            [Advanced, Explicit, Range(0f, 1f), Tooltip("Boosts luminosity in the near blur.")]
            public float nearBoostAmount;

            [Advanced, Explicit, Range(0f, 1f), Tooltip("Boosts luminosity in the far blur.")]
            public float farBoostAmount;

            public static BlurSettings defaultSettings
            {
                get
                {
                    return new BlurSettings
                    {
                        nearRadius = 20f,
                        farRadius = 20f,
                        boostPoint = 0.75f,
                        nearBoostAmount = 0f,
                        farBoostAmount = 0f,
                    };
                }
            }
        }
        #endregion

        [TopLevelSettings]
        public GlobalSettings settings = GlobalSettings.defaultSettings;

        [SettingsGroup, AllTweakModes]
        public QualitySettings quality = QualitySettings.presetQualitySettings[3];

        [SettingsGroup]
        public FocusSettings focus = FocusSettings.defaultSettings;

        [SettingsGroup]
        public BokehTextureSettings bokehTexture = BokehTextureSettings.defaultSettings;

        [SettingsGroup]
        public BlurSettings blur = BlurSettings.defaultSettings;

        [SerializeField]
        private Shader m_FilmicDepthOfFieldShader;

        public Shader filmicDepthOfFieldShader
        {
            get
            {
                if (m_FilmicDepthOfFieldShader == null)
                    m_FilmicDepthOfFieldShader = Shader.Find("Hidden/DepthOfField/DepthOfField");

                return m_FilmicDepthOfFieldShader;
            }
        }

        [SerializeField]
        private Shader m_MedianFilterShader;

        public Shader medianFilterShader
        {
            get
            {
                if (m_MedianFilterShader == null)
                    m_MedianFilterShader = Shader.Find("Hidden/DepthOfField/MedianFilter");

                return m_MedianFilterShader;
            }
        }

        [SerializeField]
        private Shader m_TextureBokehShader;

        public Shader textureBokehShader
        {
            get
            {
                if (m_TextureBokehShader == null)
                    m_TextureBokehShader = Shader.Find("Hidden/DepthOfField/BokehSplatting");

                return m_TextureBokehShader;
            }
        }

        private RenderTextureUtility m_RTU = new RenderTextureUtility();

        private Material m_FilmicDepthOfFieldMaterial;

        public Material filmicDepthOfFieldMaterial
        {
            get
            {
                if (m_FilmicDepthOfFieldMaterial == null)
                    m_FilmicDepthOfFieldMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(filmicDepthOfFieldShader);

                return m_FilmicDepthOfFieldMaterial;
            }
        }

        private Material m_MedianFilterMaterial;

        public Material medianFilterMaterial
        {
            get
            {
                if (m_MedianFilterMaterial == null)
                    m_MedianFilterMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(medianFilterShader);

                return m_MedianFilterMaterial;
            }
        }

        private Material m_TextureBokehMaterial;

        public Material textureBokehMaterial
        {
            get
            {
                if (m_TextureBokehMaterial == null)
                    m_TextureBokehMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(textureBokehShader);

                return m_TextureBokehMaterial;
            }
        }

        private ComputeBuffer m_ComputeBufferDrawArgs;

        public ComputeBuffer computeBufferDrawArgs
        {
            get
            {
                if (m_ComputeBufferDrawArgs == null)
                {
#if (UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3)
                    m_ComputeBufferDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.DrawIndirect);
#else
                    m_ComputeBufferDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
#endif
                    m_ComputeBufferDrawArgs.SetData(new[] {0, 1, 0, 0});
                }

                return m_ComputeBufferDrawArgs;
            }
        }

        private ComputeBuffer m_ComputeBufferPoints;

        public ComputeBuffer computeBufferPoints
        {
            get
            {
                if (m_ComputeBufferPoints == null)
                    m_ComputeBufferPoints = new ComputeBuffer(90000, 12 + 16, ComputeBufferType.Append);

                return m_ComputeBufferPoints;
            }
        }

        private QualitySettings m_CurrentQualitySettings;
        private float m_LastApertureOrientation;
        private Vector4 m_OctogonalBokehDirection1;
        private Vector4 m_OctogonalBokehDirection2;
        private Vector4 m_OctogonalBokehDirection3;
        private Vector4 m_OctogonalBokehDirection4;
        private Vector4 m_HexagonalBokehDirection1;
        private Vector4 m_HexagonalBokehDirection2;
        private Vector4 m_HexagonalBokehDirection3;

        private void OnEnable()
        {
            if (!ImageEffectHelper.IsSupported(filmicDepthOfFieldShader, true, true, this) || !ImageEffectHelper.IsSupported(medianFilterShader, true, true, this))
            {
                enabled = false;
                return;
            }

            if (ImageEffectHelper.supportsDX11 && !ImageEffectHelper.IsSupported(textureBokehShader, true, true, this))
            {
                enabled = false;
                return;
            }

            ComputeBlurDirections(true);
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        private void OnDisable()
        {
            ReleaseComputeResources();

            if (m_FilmicDepthOfFieldMaterial != null)
                DestroyImmediate(m_FilmicDepthOfFieldMaterial);

            if (m_TextureBokehMaterial != null)
                DestroyImmediate(m_TextureBokehMaterial);

            if (m_MedianFilterMaterial != null)
                DestroyImmediate(m_MedianFilterMaterial);

            m_FilmicDepthOfFieldMaterial = null;
            m_TextureBokehMaterial = null;
            m_MedianFilterMaterial = null;

            m_RTU.ReleaseAllTemporaryRenderTextures();
        }

        //-------------------------------------------------------------------//
        // Main entry point                                                  //
        //-------------------------------------------------------------------//
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (medianFilterMaterial == null || filmicDepthOfFieldMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (settings.visualizeBluriness)
            {
                Vector4 blurrinessParam;
                Vector4 blurrinessCoe;
                ComputeCocParameters(out blurrinessParam, out blurrinessCoe);
                filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurrinessParam);
                filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurrinessCoe);
                Graphics.Blit(null, destination, filmicDepthOfFieldMaterial, (settings.tweakMode == TweakMode.Explicit) ? (int)Passes.VisualizeCocExplicit : (int)Passes.VisualizeCoc);
            }
            else
            {
                DoDepthOfField(source, destination);
            }

            m_RTU.ReleaseAllTemporaryRenderTextures();
        }

        private void DoDepthOfField(RenderTexture source, RenderTexture destination)
        {
            m_CurrentQualitySettings = quality;

            if (settings.quality != QualityPreset.Custom)
                m_CurrentQualitySettings = QualitySettings.presetQualitySettings[(int)settings.quality];

            float radiusAdjustement = source.height / 720.0f;

            float textureBokehScale = radiusAdjustement;
            float textureBokehMaxRadius = Mathf.Max(blur.nearRadius, blur.farRadius) * textureBokehScale * 0.75f;

            float nearBlurRadius = blur.nearRadius * radiusAdjustement;
            float farBlurRadius = blur.farRadius * radiusAdjustement;
            float maxBlurRadius = Mathf.Max(nearBlurRadius, farBlurRadius);
            switch (settings.apertureShape)
            {
                case ApertureShape.Hexagonal:
                    maxBlurRadius *= 1.2f;
                    break;
                case ApertureShape.Octogonal:
                    maxBlurRadius *= 1.15f;
                    break;
            }

            if (maxBlurRadius < 0.5f)
            {
                Graphics.Blit(source, destination);
                return;
            }

            // Quarter resolution
            int rtW = source.width / 2;
            int rtH = source.height / 2;
            Vector4 blurrinessCoe = new Vector4(nearBlurRadius * 0.5f, farBlurRadius * 0.5f, 0.0f, 0.0f);
            RenderTexture colorAndCoc = m_RTU.GetTemporaryRenderTexture(rtW, rtH);
            RenderTexture colorAndCoc2 = m_RTU.GetTemporaryRenderTexture(rtW, rtH);

            if (m_CurrentQualitySettings.preventHaloing)
                filmicDepthOfFieldMaterial.EnableKeyword("USE_SPECIAL_FETCH_FOR_COC");
            else
                filmicDepthOfFieldMaterial.DisableKeyword("USE_SPECIAL_FETCH_FOR_COC");

            // Downsample to Color + COC buffer and apply boost
            Vector4 cocParam;
            Vector4 cocCoe;
            ComputeCocParameters(out cocParam, out cocCoe);
            filmicDepthOfFieldMaterial.SetVector("_BlurParams", cocParam);
            filmicDepthOfFieldMaterial.SetVector("_BlurCoe", cocCoe);
            filmicDepthOfFieldMaterial.SetVector("_BoostParams", new Vector4(nearBlurRadius * blur.nearBoostAmount * -0.5f, farBlurRadius * blur.farBoostAmount * 0.5f, blur.boostPoint, 0.0f));
            Graphics.Blit(source, colorAndCoc2, filmicDepthOfFieldMaterial, (settings.tweakMode == TweakMode.Explicit) ? (int)Passes.CaptureCocExplicit : (int)Passes.CaptureCoc);
            RenderTexture src = colorAndCoc2;
            RenderTexture dst = colorAndCoc;

            // Collect texture bokeh candidates and replace with a darker pixel
            if (shouldPerformBokeh)
            {
                // Blur a bit so we can do a frequency check
                RenderTexture blurred = m_RTU.GetTemporaryRenderTexture(rtW, rtH);
                Graphics.Blit(src, blurred, filmicDepthOfFieldMaterial, (int)Passes.BoxBlur);
                filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0.0f, 1.5f, 0.0f, 1.5f));
                Graphics.Blit(blurred, dst, filmicDepthOfFieldMaterial, (int)Passes.BlurAlphaWeighted);
                filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(1.5f, 0.0f, 0.0f, 1.5f));
                Graphics.Blit(dst, blurred, filmicDepthOfFieldMaterial, (int)Passes.BlurAlphaWeighted);

                // Collect texture bokeh candidates and replace with a darker pixel
                textureBokehMaterial.SetTexture("_BlurredColor", blurred);
                textureBokehMaterial.SetFloat("_SpawnHeuristic", bokehTexture.spawnHeuristic);
                textureBokehMaterial.SetVector("_BokehParams", new Vector4(bokehTexture.scale * textureBokehScale, bokehTexture.intensity, bokehTexture.threshold, textureBokehMaxRadius));
                Graphics.SetRandomWriteTarget(1, computeBufferPoints);
                Graphics.Blit(src, dst, textureBokehMaterial, (int)BokehTexturesPasses.Collect);
                Graphics.ClearRandomWriteTargets();
                SwapRenderTexture(ref src, ref dst);
                m_RTU.ReleaseTemporaryRenderTexture(blurred);
            }

            filmicDepthOfFieldMaterial.SetVector("_BlurParams", cocParam);
            filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurrinessCoe);
            filmicDepthOfFieldMaterial.SetVector("_BoostParams", new Vector4(nearBlurRadius * blur.nearBoostAmount * -0.5f, farBlurRadius * blur.farBoostAmount * 0.5f, blur.boostPoint, 0.0f));

            // Dilate near blur factor
            RenderTexture blurredFgCoc = null;
            if (m_CurrentQualitySettings.dilateNearBlur)
            {
                RenderTexture blurredFgCoc2 = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, RenderTextureFormat.RGHalf);
                blurredFgCoc = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, RenderTextureFormat.RGHalf);
                filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0.0f, nearBlurRadius * 0.75f, 0.0f, 0.0f));
                Graphics.Blit(src, blurredFgCoc2, filmicDepthOfFieldMaterial, (int)Passes.DilateFgCocFromColor);
                filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(nearBlurRadius * 0.75f, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(blurredFgCoc2, blurredFgCoc, filmicDepthOfFieldMaterial, (int)Passes.DilateFgCoc);
                m_RTU.ReleaseTemporaryRenderTexture(blurredFgCoc2);
                blurredFgCoc.filterMode = FilterMode.Point;
            }

            // Blur downsampled color to fill the gap between samples
            if (m_CurrentQualitySettings.prefilterBlur)
            {
                Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, (int)Passes.CocPrefilter);
                SwapRenderTexture(ref src, ref dst);
            }

            // Apply blur : Circle / Hexagonal or Octagonal (blur will create bokeh if bright pixel where not removed by "m_UseBokehTexture")
            switch (settings.apertureShape)
            {
                case ApertureShape.Circular:
                    DoCircularBlur(blurredFgCoc, ref src, ref dst, maxBlurRadius);
                    break;
                case ApertureShape.Hexagonal:
                    DoHexagonalBlur(blurredFgCoc, ref src, ref dst, maxBlurRadius);
                    break;
                case ApertureShape.Octogonal:
                    DoOctogonalBlur(blurredFgCoc, ref src, ref dst, maxBlurRadius);
                    break;
            }

            // Smooth result
            switch (m_CurrentQualitySettings.medianFilter)
            {
                case FilterQuality.Normal:
                {
                    medianFilterMaterial.SetVector("_Offsets", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    Graphics.Blit(src, dst, medianFilterMaterial, (int)MedianPasses.Median3);
                    SwapRenderTexture(ref src, ref dst);
                    medianFilterMaterial.SetVector("_Offsets", new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
                    Graphics.Blit(src, dst, medianFilterMaterial, (int)MedianPasses.Median3);
                    SwapRenderTexture(ref src, ref dst);
                    break;
                }
                case FilterQuality.High:
                {
                    Graphics.Blit(src, dst, medianFilterMaterial, (int)MedianPasses.Median3X3);
                    SwapRenderTexture(ref src, ref dst);
                    break;
                }
            }

            // Merge to full resolution (with boost) + upsampling (linear or bicubic)
            filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurrinessCoe);
            filmicDepthOfFieldMaterial.SetVector("_Convolved_TexelSize", new Vector4(src.width, src.height, 1.0f / src.width, 1.0f / src.height));
            filmicDepthOfFieldMaterial.SetTexture("_SecondTex", src);
            int mergePass = (settings.tweakMode == TweakMode.Explicit) ? (int)Passes.MergeExplicit : (int)Passes.Merge;
            if (m_CurrentQualitySettings.highQualityUpsampling)
                mergePass = (settings.tweakMode == TweakMode.Explicit) ? (int)Passes.MergeExplicitBicubic : (int)Passes.MergeBicubic;

            // Apply texture bokeh
            if (shouldPerformBokeh)
            {
                RenderTexture tmp = m_RTU.GetTemporaryRenderTexture(source.height, source.width, 0, source.format);
                Graphics.Blit(source, tmp, filmicDepthOfFieldMaterial, mergePass);

                Graphics.SetRenderTarget(tmp);
                ComputeBuffer.CopyCount(computeBufferPoints, computeBufferDrawArgs, 0);
                textureBokehMaterial.SetBuffer("pointBuffer", computeBufferPoints);
                textureBokehMaterial.SetTexture("_MainTex", bokehTexture.texture);
                textureBokehMaterial.SetVector("_Screen", new Vector3(1.0f / (1.0f * source.width), 1.0f / (1.0f * source.height), textureBokehMaxRadius));
                textureBokehMaterial.SetPass((int)BokehTexturesPasses.Apply);
                Graphics.DrawProceduralIndirect(MeshTopology.Points, computeBufferDrawArgs, 0);
                Graphics.Blit(tmp, destination); // hackaround for DX11 flipfun (OPTIMIZEME)
            }
            else
            {
                Graphics.Blit(source, destination, filmicDepthOfFieldMaterial, mergePass);
            }
        }

        //-------------------------------------------------------------------//
        // Blurs                                                             //
        //-------------------------------------------------------------------//
        private void DoHexagonalBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
        {
            ComputeBlurDirections(false);

            int blurPass;
            int blurPassMerge;
            GetDirectionalBlurPassesFromRadius(blurredFgCoc, maxRadius, out blurPass, out blurPassMerge);
            filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
            RenderTexture tmp = m_RTU.GetTemporaryRenderTexture(src.width, src.height, 0, src.format);


            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection1);
            Graphics.Blit(src, tmp, filmicDepthOfFieldMaterial, blurPass);

            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection2);
            Graphics.Blit(tmp, src, filmicDepthOfFieldMaterial, blurPass);

            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection3);
            filmicDepthOfFieldMaterial.SetTexture("_ThirdTex", src);
            Graphics.Blit(tmp, dst, filmicDepthOfFieldMaterial, blurPassMerge);
            m_RTU.ReleaseTemporaryRenderTexture(tmp);
            SwapRenderTexture(ref src, ref dst);
        }

        private void DoOctogonalBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
        {
            ComputeBlurDirections(false);

            int blurPass;
            int blurPassMerge;
            GetDirectionalBlurPassesFromRadius(blurredFgCoc, maxRadius, out blurPass, out blurPassMerge);
            filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
            RenderTexture tmp = m_RTU.GetTemporaryRenderTexture(src.width, src.height, 0, src.format);

            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection1);
            Graphics.Blit(src, tmp, filmicDepthOfFieldMaterial, blurPass);

            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection2);
            Graphics.Blit(tmp, dst, filmicDepthOfFieldMaterial, blurPass);

            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection3);
            Graphics.Blit(src, tmp, filmicDepthOfFieldMaterial, blurPass);

            filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection4);
            filmicDepthOfFieldMaterial.SetTexture("_ThirdTex", dst);
            Graphics.Blit(tmp, src, filmicDepthOfFieldMaterial, blurPassMerge);
            m_RTU.ReleaseTemporaryRenderTexture(tmp);
        }

        private void DoCircularBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
        {
            int bokehPass;
            if (blurredFgCoc != null)
            {
                filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
                bokehPass = (maxRadius > 10.0f) ? (int)Passes.CircleBlurWithDilatedFg : (int)Passes.CircleBlowLowQualityWithDilatedFg;
            }
            else
            {
                bokehPass = (maxRadius > 10.0f) ? (int)Passes.CircleBlur : (int)Passes.CircleBlurLowQuality;
            }
            Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, bokehPass);
            SwapRenderTexture(ref src, ref dst);
        }

        //-------------------------------------------------------------------//
        // Helpers                                                           //
        //-------------------------------------------------------------------//
        private void ComputeCocParameters(out Vector4 blurParams, out Vector4 blurCoe)
        {
            Camera sceneCamera = GetComponent<Camera>();
            float focusDistance01 = focus.transform
                ? (sceneCamera.WorldToViewportPoint(focus.transform.position)).z / (sceneCamera.farClipPlane)
                : (focus.plane * focus.plane * focus.plane * focus.plane);

            if (settings.tweakMode == TweakMode.Basic || settings.tweakMode == TweakMode.Advanced)
            {
                float focusRange01 = focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment;
                float focalLength = 4.0f / Mathf.Tan(0.5f * sceneCamera.fieldOfView * Mathf.Deg2Rad);
                float aperture = focalLength / focus.fStops;
                blurCoe = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                blurParams = new Vector4(aperture, focalLength, focusDistance01, focusRange01);
            }
            else
            {
                float nearDistance01 = focus.nearPlane * focus.nearPlane * focus.nearPlane * focus.nearPlane;
                float farDistance01 = focus.farPlane * focus.farPlane * focus.farPlane * focus.farPlane;
                float nearFocusRange01 = focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment * focus.rangeAdjustment;
                float farFocusRange01 = nearFocusRange01;

                if (focusDistance01 <= nearDistance01)
                    focusDistance01 = nearDistance01 + 0.0000001f;
                if (focusDistance01 >= farDistance01)
                    focusDistance01 = farDistance01 - 0.0000001f;
                if ((focusDistance01 - nearFocusRange01) <= nearDistance01)
                    nearFocusRange01 = (focusDistance01 - nearDistance01 - 0.0000001f);
                if ((focusDistance01 + farFocusRange01) >= farDistance01)
                    farFocusRange01 = (farDistance01 - focusDistance01 - 0.0000001f);

                float a1 = 1.0f / (nearDistance01 - focusDistance01 + nearFocusRange01);
                float a2 = 1.0f / (farDistance01 - focusDistance01 - farFocusRange01);
                float b1 = (1.0f - a1 * nearDistance01), b2 = (1.0f - a2 * farDistance01);
                const float c1 = -1.0f;
                const float c2 = 1.0f;
                blurParams = new Vector4(c1 * a1, c1 * b1, c2 * a2, c2 * b2);
                blurCoe = new Vector4(0.0f, 0.0f, (b2 - b1) / (a1 - a2), 0.0f);
            }
        }

        private void ReleaseComputeResources()
        {
            if (m_ComputeBufferDrawArgs != null)
                m_ComputeBufferDrawArgs.Release();

            if (m_ComputeBufferPoints != null)
                m_ComputeBufferPoints.Release();

            m_ComputeBufferDrawArgs = null;
            m_ComputeBufferPoints = null;
        }

        private void ComputeBlurDirections(bool force)
        {
            if (!force && Math.Abs(m_LastApertureOrientation - settings.apertureOrientation) < float.Epsilon)
                return;

            m_LastApertureOrientation = settings.apertureOrientation;

            float rotationRadian = settings.apertureOrientation * Mathf.Deg2Rad;
            float cosinus = Mathf.Cos(rotationRadian);
            float sinus = Mathf.Sin(rotationRadian);

            m_OctogonalBokehDirection1 = new Vector4(0.5f, 0.0f, 0.0f, 0.0f);
            m_OctogonalBokehDirection2 = new Vector4(0.0f, 0.5f, 1.0f, 0.0f);
            m_OctogonalBokehDirection3 = new Vector4(-0.353553f, 0.353553f, 1.0f, 0.0f);
            m_OctogonalBokehDirection4 = new Vector4(0.353553f, 0.353553f, 1.0f, 0.0f);

            m_HexagonalBokehDirection1 = new Vector4(0.5f, 0.0f, 0.0f, 0.0f);
            m_HexagonalBokehDirection2 = new Vector4(0.25f, 0.433013f, 1.0f, 0.0f);
            m_HexagonalBokehDirection3 = new Vector4(0.25f, -0.433013f, 1.0f, 0.0f);

            if (rotationRadian > float.Epsilon)
            {
                Rotate2D(ref m_OctogonalBokehDirection1, cosinus, sinus);
                Rotate2D(ref m_OctogonalBokehDirection2, cosinus, sinus);
                Rotate2D(ref m_OctogonalBokehDirection3, cosinus, sinus);
                Rotate2D(ref m_OctogonalBokehDirection4, cosinus, sinus);
                Rotate2D(ref m_HexagonalBokehDirection1, cosinus, sinus);
                Rotate2D(ref m_HexagonalBokehDirection2, cosinus, sinus);
                Rotate2D(ref m_HexagonalBokehDirection3, cosinus, sinus);
            }
        }

        private bool shouldPerformBokeh
        {
            get { return ImageEffectHelper.supportsDX11 && bokehTexture.texture != null && textureBokehMaterial && settings.tweakMode != TweakMode.Basic; }
        }

        private static void Rotate2D(ref Vector4 direction, float cosinus, float sinus)
        {
            Vector4 source = direction;
            direction.x = source.x * cosinus - source.y * sinus;
            direction.y = source.x * sinus + source.y * cosinus;
        }

        private static void SwapRenderTexture(ref RenderTexture src, ref RenderTexture dst)
        {
            RenderTexture tmp = dst;
            dst = src;
            src = tmp;
        }

        private static void GetDirectionalBlurPassesFromRadius(RenderTexture blurredFgCoc, float maxRadius, out int blurPass, out int blurAndMergePass)
        {
            if (blurredFgCoc == null)
            {
                if (maxRadius > 10.0f)
                {
                    blurPass = (int)Passes.ShapeHighQuality;
                    blurAndMergePass = (int)Passes.ShapeHighQualityMerge;
                }
                else if (maxRadius > 5.0f)
                {
                    blurPass = (int)Passes.ShapeMediumQuality;
                    blurAndMergePass = (int)Passes.ShapeMediumQualityMerge;
                }
                else
                {
                    blurPass = (int)Passes.ShapeLowQuality;
                    blurAndMergePass = (int)Passes.ShapeLowQualityMerge;
                }
            }
            else
            {
                if (maxRadius > 10.0f)
                {
                    blurPass = (int)Passes.ShapeHighQualityDilateFg;
                    blurAndMergePass = (int)Passes.ShapeHighQualityMergeDilateFg;
                }
                else if (maxRadius > 5.0f)
                {
                    blurPass = (int)Passes.ShapeMediumQualityDilateFg;
                    blurAndMergePass = (int)Passes.ShapeMediumQualityMergeDilateFg;
                }
                else
                {
                    blurPass = (int)Passes.ShapeLowQualityDilateFg;
                    blurAndMergePass = (int)Passes.ShapeLowQualityMergeDilateFg;
                }
            }
        }
    }
}
