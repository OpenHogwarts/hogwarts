namespace UnityStandardAssets.CinematicEffects
{
    using UnityEngine;
    using UnityEngine.Events;
    using System;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Cinematic/Tonemapping and Color Grading")]
    public class TonemappingColorGrading : MonoBehaviour
    {
#if UNITY_EDITOR
        // EDITOR ONLY call for allowing the editor to update the histogram
        public UnityAction<RenderTexture> onFrameEndEditorOnly;

        [SerializeField]
        private ComputeShader m_HistogramComputeShader;
        public ComputeShader histogramComputeShader
        {
            get
            {
                if (m_HistogramComputeShader == null)
                    m_HistogramComputeShader = Resources.Load<ComputeShader>("HistogramCompute");

                return m_HistogramComputeShader;
            }
        }

        [SerializeField]
        private Shader m_HistogramShader;
        public Shader histogramShader
        {
            get
            {
                if (m_HistogramShader == null)
                    m_HistogramShader = Shader.Find("Hidden/TonemappingColorGradingHistogram");

                return m_HistogramShader;
            }
        }

        [SerializeField]
        public bool histogramRefreshOnPlay = true;
#endif

        #region Attributes
        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsGroup : Attribute
        {}

        public class IndentedGroup : PropertyAttribute
        {}

        public class ChannelMixer : PropertyAttribute
        {}

        public class ColorWheelGroup : PropertyAttribute
        {
            public int minSizePerWheel = 60;
            public int maxSizePerWheel = 150;

            public ColorWheelGroup()
            {}

            public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
            {
                this.minSizePerWheel = minSizePerWheel;
                this.maxSizePerWheel = maxSizePerWheel;
            }
        }

        public class Curve : PropertyAttribute
        {
            public Color color = Color.white;

            public Curve()
            {}

            public Curve(float r, float g, float b, float a) // Can't pass a struct in an attribute
            {
                color = new Color(r, g, b, a);
            }
        }
        #endregion

        #region Settings
        [Serializable]
        public struct EyeAdaptationSettings
        {
            public bool enabled;

            [Min(0f), Tooltip("Midpoint Adjustment.")]
            public float middleGrey;

            [Tooltip("The lowest possible exposure value; adjust this value to modify the brightest areas of your level.")]
            public float min;

            [Tooltip("The highest possible exposure value; adjust this value to modify the darkest areas of your level.")]
            public float max;

            [Min(0f), Tooltip("Speed of linear adaptation. Higher is faster.")]
            public float speed;

            [Tooltip("Displays a luminosity helper in the GameView.")]
            public bool showDebug;

            public static EyeAdaptationSettings defaultSettings
            {
                get
                {
                    return new EyeAdaptationSettings
                    {
                        enabled = false,
                        showDebug = false,
                        middleGrey = 0.5f,
                        min = -3f,
                        max = 3f,
                        speed = 1.5f
                    };
                }
            }
        }

        public enum Tonemapper
        {
            ACES,
            Curve,
            Hable,
            HejlDawson,
            Photographic,
            Reinhard
        }

        [Serializable]
        public struct TonemappingSettings
        {
            public bool enabled;

            [Tooltip("Tonemapping technique to use. ACES is the recommended one.")]
            public Tonemapper tonemapper;

            [Min(0f), Tooltip("Adjusts the overall exposure of the scene.")]
            public float exposure;

            [Tooltip("Custom tonemapping curve.")]
            public AnimationCurve curve;

            public static TonemappingSettings defaultSettings
            {
                get
                {
                    return new TonemappingSettings
                    {
                        enabled = false,
                        tonemapper = Tonemapper.ACES,
                        exposure = 1f,
                        curve = CurvesSettings.defaultCurve
                    };
                }
            }
        }

        [Serializable]
        public struct LUTSettings
        {
            public bool enabled;

            [Tooltip("Custom lookup texture (strip format, e.g. 256x16).")]
            public Texture texture;

            [Range(0f, 1f), Tooltip("Blending factor.")]
            public float contribution;

            public static LUTSettings defaultSettings
            {
                get
                {
                    return new LUTSettings
                    {
                        enabled = false,
                        texture = null,
                        contribution = 1f
                    };
                }
            }
        }

        [Serializable]
        public struct ColorWheelsSettings
        {
            [ColorUsage(false)]
            public Color shadows;

            [ColorUsage(false)]
            public Color midtones;

            [ColorUsage(false)]
            public Color highlights;

            public static ColorWheelsSettings defaultSettings
            {
                get
                {
                    return new ColorWheelsSettings
                    {
                        shadows = Color.white,
                        midtones = Color.white,
                        highlights = Color.white
                    };
                }
            }
        }

        [Serializable]
        public struct BasicsSettings
        {
            [Range(-2f, 2f), Tooltip("Sets the white balance to a custom color temperature.")]
            public float temperatureShift;

            [Range(-2f, 2f), Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
            public float tint;

            [Space, Range(-0.5f, 0.5f), Tooltip("Shift the hue of all colors.")]
            public float hue;

            [Range(0f, 2f), Tooltip("Pushes the intensity of all colors.")]
            public float saturation;

            [Range(-1f, 1f), Tooltip("Adjusts the saturation so that clipping is minimized as colors approach full saturation.")]
            public float vibrance;

            [Range(0f, 10f), Tooltip("Brightens or darkens all colors.")]
            public float value;

            [Space, Range(0f, 2f), Tooltip("Expands or shrinks the overall range of tonal values.")]
            public float contrast;

            [Range(0.01f, 5f), Tooltip("Contrast gain curve. Controls the steepness of the curve.")]
            public float gain;

            [Range(0.01f, 5f), Tooltip("Applies a pow function to the source.")]
            public float gamma;

            public static BasicsSettings defaultSettings
            {
                get
                {
                    return new BasicsSettings
                    {
                        temperatureShift = 0f,
                        tint = 0f,
                        contrast = 1f,
                        hue = 0f,
                        saturation = 1f,
                        value = 1f,
                        vibrance = 0f,
                        gain = 1f,
                        gamma = 1f
                    };
                }
            }
        }

        [Serializable]
        public struct ChannelMixerSettings
        {
            public int currentChannel;
            public Vector3[] channels;

            public static ChannelMixerSettings defaultSettings
            {
                get
                {
                    return new ChannelMixerSettings
                    {
                        currentChannel = 0,
                        channels = new[]
                        {
                            new Vector3(1f, 0f, 0f),
                            new Vector3(0f, 1f, 0f),
                            new Vector3(0f, 0f, 1f)
                        }
                    };
                }
            }
        }

        [Serializable]
        public struct CurvesSettings
        {
            [Curve]
            public AnimationCurve master;

            [Curve(1f, 0f, 0f, 1f)]
            public AnimationCurve red;

            [Curve(0f, 1f, 0f, 1f)]
            public AnimationCurve green;

            [Curve(0f, 1f, 1f, 1f)]
            public AnimationCurve blue;

            public static CurvesSettings defaultSettings
            {
                get
                {
                    return new CurvesSettings
                    {
                        master = defaultCurve,
                        red = defaultCurve,
                        green = defaultCurve,
                        blue = defaultCurve
                    };
                }
            }

            public static AnimationCurve defaultCurve
            {
                get { return new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)); }
            }
        }

        public enum ColorGradingPrecision
        {
            Normal = 16,
            High = 32
        }

        [Serializable]
        public struct ColorGradingSettings
        {
            public bool enabled;

            [Tooltip("Internal LUT precision. \"Normal\" is 256x16, \"High\" is 1024x32. Prefer \"Normal\" on mobile devices.")]
            public ColorGradingPrecision precision;

            [Space, ColorWheelGroup]
            public ColorWheelsSettings colorWheels;

            [Space, IndentedGroup]
            public BasicsSettings basics;

            [Space, ChannelMixer]
            public ChannelMixerSettings channelMixer;

            [Space, IndentedGroup]
            public CurvesSettings curves;

            [Space, Tooltip("Use dithering to try and minimize color banding in dark areas.")]
            public bool useDithering;

            [Tooltip("Displays the generated LUT in the top left corner of the GameView.")]
            public bool showDebug;

            public static ColorGradingSettings defaultSettings
            {
                get
                {
                    return new ColorGradingSettings
                    {
                        enabled = false,
                        useDithering = false,
                        showDebug = false,
                        precision = ColorGradingPrecision.Normal,
                        colorWheels = ColorWheelsSettings.defaultSettings,
                        basics = BasicsSettings.defaultSettings,
                        channelMixer = ChannelMixerSettings.defaultSettings,
                        curves = CurvesSettings.defaultSettings
                    };
                }
            }

            internal void Reset()
            {
                curves = CurvesSettings.defaultSettings;
            }
        }

        [SerializeField, SettingsGroup]
        private EyeAdaptationSettings m_EyeAdaptation = EyeAdaptationSettings.defaultSettings;
        public EyeAdaptationSettings eyeAdaptation
        {
            get { return m_EyeAdaptation; }
            set { m_EyeAdaptation = value; }
        }

        [SerializeField, SettingsGroup]
        private TonemappingSettings m_Tonemapping = TonemappingSettings.defaultSettings;
        public TonemappingSettings tonemapping
        {
            get { return m_Tonemapping; }
            set
            {
                m_Tonemapping = value;
                SetTonemapperDirty();
            }
        }

        [SerializeField, SettingsGroup]
        private LUTSettings m_Lut = LUTSettings.defaultSettings;
        public LUTSettings lut
        {
            get { return m_Lut; }
            set
            {
                m_Lut = value;
                SetDirty();
            }
        }

        [SerializeField, SettingsGroup]
        private ColorGradingSettings m_ColorGrading = ColorGradingSettings.defaultSettings;
        public ColorGradingSettings colorGrading
        {
            get { return m_ColorGrading; }
            set
            {
                m_ColorGrading = value;
                SetDirty();
            }
        }
        #endregion

        private Texture2D m_IdentityLut;
        private RenderTexture m_InternalLut;
        private Texture2D m_CurveTexture;
        private Texture2D m_TonemapperCurve;
        private float m_TonemapperCurveRange;

        private Texture2D identityLut
        {
            get
            {
                if (m_IdentityLut == null || m_IdentityLut.height != lutSize)
                {
                    DestroyImmediate(m_IdentityLut);
                    m_IdentityLut = GenerateIdentityLut(lutSize);
                }

                return m_IdentityLut;
            }
        }

        private RenderTexture internalLutRt
        {
            get
            {
                if (m_InternalLut == null || !m_InternalLut.IsCreated() || m_InternalLut.height != lutSize)
                {
                    DestroyImmediate(m_InternalLut);
                    m_InternalLut = new RenderTexture(lutSize * lutSize, lutSize, 0, RenderTextureFormat.ARGB32)
                    {
                        name = "Internal LUT",
                        filterMode = FilterMode.Bilinear,
                        anisoLevel = 0,
                        hideFlags = HideFlags.DontSave
                    };
                }

                return m_InternalLut;
            }
        }

        private Texture2D curveTexture
        {
            get
            {
                if (m_CurveTexture == null)
                {
                    m_CurveTexture = new Texture2D(256, 1, TextureFormat.ARGB32, false, true)
                    {
                        name = "Curve texture",
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Bilinear,
                        anisoLevel = 0,
                        hideFlags = HideFlags.DontSave
                    };
                }

                return m_CurveTexture;
            }
        }

        private Texture2D tonemapperCurve
        {
            get
            {
                if (m_TonemapperCurve == null)
                {
                    TextureFormat format = TextureFormat.RGB24;
                    if (SystemInfo.SupportsTextureFormat(TextureFormat.RFloat))
                        format = TextureFormat.RFloat;
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.RHalf))
                        format = TextureFormat.RHalf;

                    m_TonemapperCurve = new Texture2D(256, 1, format, false, true)
                    {
                        name = "Tonemapper curve texture",
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Bilinear,
                        anisoLevel = 0,
                        hideFlags = HideFlags.DontSave
                    };
                }

                return m_TonemapperCurve;
            }
        }

        [SerializeField]
        private Shader m_Shader;
        public Shader shader
        {
            get
            {
                if (m_Shader == null)
                    m_Shader = Shader.Find("Hidden/TonemappingColorGrading");

                return m_Shader;
            }
        }

        private Material m_Material;
        public Material material
        {
            get
            {
                if (m_Material == null)
                    m_Material = ImageEffectHelper.CheckShaderAndCreateMaterial(shader);

                return m_Material;
            }
        }

        public bool isGammaColorSpace
        {
            get { return QualitySettings.activeColorSpace == ColorSpace.Gamma; }
        }

        public int lutSize
        {
            get { return (int)colorGrading.precision; }
        }

        private enum Pass
        {
            LutGen,
            AdaptationLog,
            AdaptationExpBlend,
            AdaptationExp,
            TonemappingOff,
            TonemappingACES,
            TonemappingCurve,
            TonemappingHable,
            TonemappingHejlDawson,
            TonemappingPhotographic,
            TonemappingReinhard,
            AdaptationDebug
        }

        public bool validRenderTextureFormat { get; private set; }
        public bool validUserLutSize { get; private set; }

        private bool m_Dirty = true;
        private bool m_TonemapperDirty = true;

        private RenderTexture m_SmallAdaptiveRt;
        private RenderTextureFormat m_AdaptiveRtFormat;

        public void SetDirty()
        {
            m_Dirty = true;
        }

        public void SetTonemapperDirty()
        {
            m_TonemapperDirty = true;
        }

        private void OnEnable()
        {
            if (!ImageEffectHelper.IsSupported(shader, false, true, this))
            {
                enabled = false;
                return;
            }

            SetDirty();
            SetTonemapperDirty();
        }

        private void OnDisable()
        {
            if (m_Material != null)
                DestroyImmediate(m_Material);

            if (m_IdentityLut != null)
                DestroyImmediate(m_IdentityLut);

            if (m_InternalLut != null)
                DestroyImmediate(internalLutRt);

            if (m_SmallAdaptiveRt != null)
                DestroyImmediate(m_SmallAdaptiveRt);

            if (m_CurveTexture != null)
                DestroyImmediate(m_CurveTexture);

            if (m_TonemapperCurve != null)
                DestroyImmediate(m_TonemapperCurve);

            m_Material = null;
            m_IdentityLut = null;
            m_InternalLut = null;
            m_SmallAdaptiveRt = null;
            m_CurveTexture = null;
            m_TonemapperCurve = null;
        }

        private void OnValidate()
        {
            SetDirty();
            SetTonemapperDirty();
        }

        private static Texture2D GenerateIdentityLut(int dim)
        {
            Color[] newC = new Color[dim * dim * dim];
            float oneOverDim = 1f / ((float)dim - 1f);

            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                    for (int k = 0; k < dim; k++)
                        newC[i + (j * dim) + (k * dim * dim)] = new Color((float)i * oneOverDim, Mathf.Abs((float)k * oneOverDim), (float)j * oneOverDim, 1f);

            Texture2D tex2D = new Texture2D(dim * dim, dim, TextureFormat.RGB24, false, true)
            {
                name = "Identity LUT",
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0,
                hideFlags = HideFlags.DontSave
            };
            tex2D.SetPixels(newC);
            tex2D.Apply();

            return tex2D;
        }

        // An analytical model of chromaticity of the standard illuminant, by Judd et al.
        // http://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D
        // Slightly modifed to adjust it with the D65 white point (x=0.31271, y=0.32902).
        private float StandardIlluminantY(float x)
        {
            return 2.87f * x - 3f * x * x - 0.27509507f;
        }

        // CIE xy chromaticity to CAT02 LMS.
        // http://en.wikipedia.org/wiki/LMS_color_space#CAT02
        private Vector3 CIExyToLMS(float x, float y)
        {
            float Y = 1f;
            float X = Y * x / y;
            float Z = Y * (1f - x - y) / y;

            float L =  0.7328f * X + 0.4296f * Y - 0.1624f * Z;
            float M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
            float S =  0.0030f * X + 0.0136f * Y + 0.9834f * Z;

            return new Vector3(L, M, S);
        }

        private Vector3 GetWhiteBalance()
        {
            float t1 = colorGrading.basics.temperatureShift;
            float t2 = colorGrading.basics.tint;

            // Get the CIE xy chromaticity of the reference white point.
            // Note: 0.31271 = x value on the D65 white point
            float x = 0.31271f - t1 * (t1 < 0f ? 0.1f : 0.05f);
            float y = StandardIlluminantY(x) + t2 * 0.05f;

            // Calculate the coefficients in the LMS space.
            Vector3 w1 = new Vector3(0.949237f, 1.03542f, 1.08728f); // D65 white point
            Vector3 w2 = CIExyToLMS(x, y);
            return new Vector3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);
        }

        private static Color NormalizeColor(Color c)
        {
            float sum = (c.r + c.g + c.b) / 3f;

            if (Mathf.Approximately(sum, 0f))
                return new Color(1f, 1f, 1f, 1f);

            return new Color
            {
                r = c.r / sum,
                g = c.g / sum,
                b = c.b / sum,
                a = 1f
            };
        }

        private void GenerateLiftGammaGain(out Color lift, out Color gamma, out Color gain)
        {
            Color nLift = NormalizeColor(colorGrading.colorWheels.shadows);
            Color nGamma = NormalizeColor(colorGrading.colorWheels.midtones);
            Color nGain = NormalizeColor(colorGrading.colorWheels.highlights);

            float avgLift = (nLift.r + nLift.g + nLift.b) / 3f;
            float avgGamma = (nGamma.r + nGamma.g + nGamma.b) / 3f;
            float avgGain = (nGain.r + nGain.g + nGain.b) / 3f;

            // Magic numbers
            const float liftScale = 0.1f;
            const float gammaScale = 0.5f;
            const float gainScale = 0.5f;

            float liftR = (nLift.r - avgLift) * liftScale;
            float liftG = (nLift.g - avgLift) * liftScale;
            float liftB = (nLift.b - avgLift) * liftScale;

            float gammaR = Mathf.Pow(2f, (nGamma.r - avgGamma) * gammaScale);
            float gammaG = Mathf.Pow(2f, (nGamma.g - avgGamma) * gammaScale);
            float gammaB = Mathf.Pow(2f, (nGamma.b - avgGamma) * gammaScale);

            float gainR = Mathf.Pow(2f, (nGain.r - avgGain) * gainScale);
            float gainG = Mathf.Pow(2f, (nGain.g - avgGain) * gainScale);
            float gainB = Mathf.Pow(2f, (nGain.b - avgGain) * gainScale);

            const float minGamma = 0.01f;
            float invGammaR = 1f / Mathf.Max(minGamma, gammaR);
            float invGammaG = 1f / Mathf.Max(minGamma, gammaG);
            float invGammaB = 1f / Mathf.Max(minGamma, gammaB);

            lift = new Color(liftR, liftG, liftB);
            gamma = new Color(invGammaR, invGammaG, invGammaB);
            gain = new Color(gainR, gainG, gainB);
        }

        private void GenCurveTexture()
        {
            AnimationCurve master = colorGrading.curves.master;
            AnimationCurve red = colorGrading.curves.red;
            AnimationCurve green = colorGrading.curves.green;
            AnimationCurve blue = colorGrading.curves.blue;

            Color[] pixels = new Color[256];

            for (float i = 0f; i <= 1f; i += 1f / 255f)
            {
                float m = Mathf.Clamp(master.Evaluate(i), 0f, 1f);
                float r = Mathf.Clamp(red.Evaluate(i), 0f, 1f);
                float g = Mathf.Clamp(green.Evaluate(i), 0f, 1f);
                float b = Mathf.Clamp(blue.Evaluate(i), 0f, 1f);
                pixels[(int)Mathf.Floor(i * 255f)] = new Color(r, g, b, m);
            }

            curveTexture.SetPixels(pixels);
            curveTexture.Apply();
        }

        private bool CheckUserLut()
        {
            validUserLutSize = (lut.texture.height == (int)Mathf.Sqrt(lut.texture.width));
            return validUserLutSize;
        }

        private bool CheckSmallAdaptiveRt()
        {
            if (m_SmallAdaptiveRt != null)
                return false;

            m_AdaptiveRtFormat = RenderTextureFormat.ARGBHalf;

            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf))
                m_AdaptiveRtFormat = RenderTextureFormat.RGHalf;

            m_SmallAdaptiveRt = new RenderTexture(1, 1, 0, m_AdaptiveRtFormat);
            m_SmallAdaptiveRt.hideFlags = HideFlags.DontSave;

            return true;
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            int yoffset = 0;

            // Color grading debug
            if (m_InternalLut != null && colorGrading.enabled && colorGrading.showDebug)
            {
                Graphics.DrawTexture(new Rect(0f, yoffset, lutSize * lutSize, lutSize), internalLutRt);
                yoffset += lutSize;
            }

            // Eye Adaptation debug
            if (m_SmallAdaptiveRt != null && eyeAdaptation.enabled && eyeAdaptation.showDebug)
            {
                m_Material.SetPass((int)Pass.AdaptationDebug);
                Graphics.DrawTexture(new Rect(0f, yoffset, 256, 16), m_SmallAdaptiveRt, m_Material);
            }
        }

        [ImageEffectTransformsToLDR]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
#if UNITY_EDITOR
            validRenderTextureFormat = true;

            if (source.format != RenderTextureFormat.ARGBHalf && source.format != RenderTextureFormat.ARGBFloat)
                validRenderTextureFormat = false;
#endif

            if (isGammaColorSpace)
                material.EnableKeyword("GAMMA_COLORSPACE");
            else
                material.DisableKeyword("GAMMA_COLORSPACE");

            material.DisableKeyword("ENABLE_EYE_ADAPTATION");
            material.DisableKeyword("ENABLE_COLOR_GRADING");
            material.DisableKeyword("ENABLE_DITHERING");

            Texture lutUsed = null;
            float lutContrib = 1f;

            RenderTexture rtSquared = null;
            RenderTexture[] rts = null;

            if (eyeAdaptation.enabled)
            {
                bool freshlyBrewedSmallRt = CheckSmallAdaptiveRt();
                int srcSize = source.width < source.height ? source.width : source.height;

                // Fast lower or equal power of 2
                int adaptiveSize = srcSize;
                adaptiveSize |= (adaptiveSize >> 1);
                adaptiveSize |= (adaptiveSize >> 2);
                adaptiveSize |= (adaptiveSize >> 4);
                adaptiveSize |= (adaptiveSize >> 8);
                adaptiveSize |= (adaptiveSize >> 16);
                adaptiveSize -= (adaptiveSize >> 1);

                rtSquared = RenderTexture.GetTemporary(adaptiveSize, adaptiveSize, 0, m_AdaptiveRtFormat);
                Graphics.Blit(source, rtSquared);

                int downsample = (int)Mathf.Log(rtSquared.width, 2f);

                int div = 2;
                rts = new RenderTexture[downsample];
                for (int i = 0; i < downsample; i++)
                {
                    rts[i] = RenderTexture.GetTemporary(rtSquared.width / div, rtSquared.width / div, 0, m_AdaptiveRtFormat);
                    div <<= 1;
                }

                // Downsample pyramid
                var lumRt = rts[downsample - 1];
                Graphics.Blit(rtSquared, rts[0], material, (int)Pass.AdaptationLog);
                for (int i = 0; i < downsample - 1; i++)
                {
                    Graphics.Blit(rts[i], rts[i + 1]);
                    lumRt = rts[i + 1];
                }

                // Keeping luminance values between frames, RT restore expected
                m_SmallAdaptiveRt.MarkRestoreExpected();

                material.SetFloat("_AdaptationSpeed", Mathf.Max(eyeAdaptation.speed, 0.001f));

#if UNITY_EDITOR
                if (Application.isPlaying && !freshlyBrewedSmallRt)
                    Graphics.Blit(lumRt, m_SmallAdaptiveRt, material, (int)Pass.AdaptationExpBlend);
                else
                    Graphics.Blit(lumRt, m_SmallAdaptiveRt, material, (int)Pass.AdaptationExp);
#else
                Graphics.Blit(lumRt, m_SmallAdaptiveRt, material, freshlyBrewedSmallRt ? (int)Pass.AdaptationExp : (int)Pass.AdaptationExpBlend);
#endif

                material.SetFloat("_MiddleGrey", eyeAdaptation.middleGrey);
                material.SetFloat("_AdaptationMin", Mathf.Pow(2f, eyeAdaptation.min));
                material.SetFloat("_AdaptationMax", Mathf.Pow(2f, eyeAdaptation.max));
                material.SetTexture("_LumTex", m_SmallAdaptiveRt);
                material.EnableKeyword("ENABLE_EYE_ADAPTATION");
            }

            int renderPass = (int)Pass.TonemappingOff;

            if (tonemapping.enabled)
            {
                if (tonemapping.tonemapper == Tonemapper.Curve)
                {
                    if (m_TonemapperDirty)
                    {
                        float range = 1f;

                        if (tonemapping.curve.length > 0)
                        {
                            range = tonemapping.curve[tonemapping.curve.length - 1].time;

                            for (float i = 0f; i <= 1f; i += 1f / 255f)
                            {
                                float c = tonemapping.curve.Evaluate(i * range);
                                tonemapperCurve.SetPixel(Mathf.FloorToInt(i * 255f), 0, new Color(c, c, c));
                            }

                            tonemapperCurve.Apply();
                        }

                        m_TonemapperCurveRange = 1f / range;
                        m_TonemapperDirty = false;
                    }

                    material.SetFloat("_ToneCurveRange", m_TonemapperCurveRange);
                    material.SetTexture("_ToneCurve", tonemapperCurve);
                }

                material.SetFloat("_Exposure", tonemapping.exposure);
                renderPass += (int)tonemapping.tonemapper + 1;
            }

            if (lut.enabled)
            {
                Texture tex = lut.texture;

                if (lut.texture == null || !CheckUserLut())
                    tex = identityLut;

                lutUsed = tex;
                lutContrib = lut.contribution;
                material.EnableKeyword("ENABLE_COLOR_GRADING");
            }

            if (colorGrading.enabled)
            {
                if (m_Dirty || !m_InternalLut.IsCreated())
                {
                    if (lutUsed == null)
                    {
                        material.SetVector("_UserLutParams", new Vector4(1f / identityLut.width, 1f / identityLut.height, identityLut.height - 1f, 1f));
                        material.SetTexture("_UserLutTex", identityLut);
                    }
                    else
                    {
                        material.SetVector("_UserLutParams", new Vector4(1f / lutUsed.width, 1f / lutUsed.height, lutUsed.height - 1f, lut.contribution));
                        material.SetTexture("_UserLutTex", lutUsed);
                    }

                    Color lift, gamma, gain;
                    GenerateLiftGammaGain(out lift, out gamma, out gain);
                    GenCurveTexture();

                    material.SetVector("_WhiteBalance", GetWhiteBalance());
                    material.SetVector("_Lift", lift);
                    material.SetVector("_Gamma", gamma);
                    material.SetVector("_Gain", gain);
                    material.SetVector("_ContrastGainGamma", new Vector3(colorGrading.basics.contrast, colorGrading.basics.gain, 1f / colorGrading.basics.gamma));
                    material.SetFloat("_Vibrance", colorGrading.basics.vibrance);
                    material.SetVector("_HSV", new Vector4(colorGrading.basics.hue, colorGrading.basics.saturation, colorGrading.basics.value));
                    material.SetVector("_ChannelMixerRed", colorGrading.channelMixer.channels[0]);
                    material.SetVector("_ChannelMixerGreen", colorGrading.channelMixer.channels[1]);
                    material.SetVector("_ChannelMixerBlue", colorGrading.channelMixer.channels[2]);
                    material.SetTexture("_CurveTex", curveTexture);
                    internalLutRt.MarkRestoreExpected();
                    Graphics.Blit(identityLut, internalLutRt, material, (int)Pass.LutGen);
                    m_Dirty = false;
                }

                lutUsed = internalLutRt;
                lutContrib = 1f;
                material.EnableKeyword("ENABLE_COLOR_GRADING");

                if (colorGrading.useDithering)
                    material.EnableKeyword("ENABLE_DITHERING");
            }

            if (lutUsed != null)
            {
                material.SetTexture("_LutTex", lutUsed);
                material.SetVector("_LutParams", new Vector4(1f / lutUsed.width, 1f / lutUsed.height, lutUsed.height - 1f, lutContrib));
            }

            Graphics.Blit(source, destination, material, renderPass);

            // Cleanup for eye adaptation
            if (eyeAdaptation.enabled)
            {
                for (int i = 0; i < rts.Length; i++)
                    RenderTexture.ReleaseTemporary(rts[i]);

                RenderTexture.ReleaseTemporary(rtSquared);
            }

#if UNITY_EDITOR
            // If we have an on frame end callabck we need to pass a valid result texture
            // if destination is null we wrote to the backbuffer so we need to copy that out.
            // It's slow and not amazing, but editor only
            if (onFrameEndEditorOnly != null)
            {
                if (destination == null)
                {
                    RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
                    Graphics.Blit(source, rt, material, renderPass);
                    onFrameEndEditorOnly(rt);
                    RenderTexture.ReleaseTemporary(rt);
                    RenderTexture.active = null;
                }
                else
                {
                    onFrameEndEditorOnly(destination);
                }
            }
#endif
        }

        public Texture2D BakeLUT()
        {
            Texture2D lut = new Texture2D(internalLutRt.width, internalLutRt.height, TextureFormat.RGB24, false, true);
            RenderTexture.active = internalLutRt;
            lut.ReadPixels(new Rect(0f, 0f, lut.width, lut.height), 0, 0);
            RenderTexture.active = null;
            return lut;
        }
    }
}
