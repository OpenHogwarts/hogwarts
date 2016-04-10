using UnityEngine;
using System;
using UnityEngine.Events;

namespace UnityStandardAssets.CinematicEffects
{
    // TODO: Retina support for the wheels (not sure how Unity handles Retina)
    // TODO: Cleanup all the temp stuff

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Color Adjustments/Tonemapping and Color Grading")]
    public class TonemappingColorGrading : MonoBehaviour
    {
        #region Temp stuff, should be removed before release

        [NonSerialized] public bool fastMode = false;
        public bool debugClamp = false;

        #endregion

#if UNITY_EDITOR
        // EDITOR ONLY call for allowing the editor to update
        // the histogram
        public UnityAction<RenderTexture, Material> onFrameEndEditorOnly;

        [SerializeField] public ComputeShader histogramComputeShader;

        [SerializeField] public Shader histogramShader;

        [NonSerialized]
        private RenderTexureUtility m_RTU = new RenderTexureUtility();
#endif

        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsGroup : Attribute
        {
        }

        public class DrawFilmicCurveAttribute : Attribute
        {
        }

        public enum Passes
        {
            ThreeD = 0,
            OneD = 1,
            ThreeDDebug = 2,
            OneDDebug = 3
        }


        [Serializable]
        public struct FilmicCurve
        {
            public bool enabled;

            // LUT
            [Range(-4f, 4f)][Tooltip("Exposure Bias|Adjusts the overall exposure of the scene")] public float exposureBias;

            [Range(0f, 2f)][Tooltip("Contrast|Contrast adjustment (log-space)")] public float contrast;

            [Range(0f, 1f)][Tooltip("Toe|Toe of the filmic curve; affects the darker areas of the scene")] public float toe;

            [Range(0f, 1f)][Tooltip("Shoulder|Shoulder of the filmic curve; brings overexposed highlights back into range")] public float lutShoulder;

            public static FilmicCurve defaultFilmicCurve = new FilmicCurve
            {
                enabled = false,
                exposureBias = 0.0f,
                contrast = 1.0f,
                toe = 0.0f,
                lutShoulder = 0.0f
            };
        }

        public class ColorWheelGroup : PropertyAttribute
        {
            public int minSizePerWheel = 60;
            public int maxSizePerWheel = 150;

            public ColorWheelGroup()
            {
            }

            public ColorWheelGroup(int minSizePerWheel, int maxSizePerWheel)
            {
                this.minSizePerWheel = minSizePerWheel;
                this.maxSizePerWheel = maxSizePerWheel;
            }
        }

        [Serializable]
        public struct ColorGradingColors
        {
            [Tooltip("Shadows|Shadows color")] public Color shadows;

            [Tooltip("Midtones|Midtones color")] public Color midtones;

            [Tooltip("Highlights|Highlights color")] public Color highlights;

            public static ColorGradingColors defaultGradingColors = new ColorGradingColors
            {
                shadows = new Color(1, 1, 1),
                midtones = new Color(1, 1, 1),
                highlights = new Color(1, 1, 1)
            };
        }

        [Serializable]
        public struct ColorGrading
        {
            public bool enabled;

            [ColorUsage(false)][Tooltip("White Balance|Adjusts the white color before tonemapping")] public Color whiteBalance;

            [Range(0f, 2f)][Tooltip("Vibrance|Pushes the intensity of all colors")] public float saturation;

            [Range(0f, 5f)][Tooltip("Gamma|Adjusts the gamma")] public float gamma;

            [ColorWheelGroup] public ColorGradingColors lutColors;

            public static ColorGrading defaultColorGrading = new ColorGrading
            {
                whiteBalance = Color.white,
                enabled = false,
                saturation = 1.0f,
                gamma = 1.0f,
                lutColors = ColorGradingColors.defaultGradingColors
            };
        }

        [NonSerialized] private bool m_Dirty = true;

        public void SetDirty()
        {
            m_Dirty = true;
        }

        [SerializeField][SettingsGroup][DrawFilmicCurve] private FilmicCurve m_FilmicCurve = FilmicCurve.defaultFilmicCurve;

        public FilmicCurve filmicCurve
        {
            get { return m_FilmicCurve; }
            set
            {
                m_FilmicCurve = value;
                SetDirty();
            }
        }

        [SerializeField][SettingsGroup] private ColorGrading m_ColorGrading = ColorGrading.defaultColorGrading;

        public ColorGrading colorGrading
        {
            get { return m_ColorGrading; }
            set
            {
                m_ColorGrading = value;
                SetDirty();
            }
        }

        // called in editor when UI is changed
        private void OnValidate()
        {
            SetDirty();
        }

        // The actual texture that we build
        private Texture3D m_LutTex;

        // 1D curves
        private Texture2D m_LutCurveTex1D;

        private bool isLinearColorSpace
        {
            get { return QualitySettings.activeColorSpace == ColorSpace.Linear; }
        }


        [SerializeField][Tooltip("Lookup Texture|Custom lookup texture")] private Texture2D m_UserLutTexture;

        public Texture2D userLutTexture
        {
            get { return m_UserLutTexture; }
            set
            {
                m_UserLutTexture = value;
                SetDirty();
            }
        }

        public struct SimplePolyFunc
        {
            // f(x) = signY * A * (signX * x - x0) ^ b + y0
            public float A;
            public float B;
            public float x0;
            public float y0;
            public float signX;
            public float signY;

            public float logA;

            public float Eval(float x)
            {
                // Standard function
                //return signY * A * Mathf.Pow(signX * x - x0, B) + y0;

                // Slightly more complicated but numerically stable function
                return signY * Mathf.Exp(logA + B * Mathf.Log(signX * x - x0)) + y0;
            }

            // Create a function going from (0,0) to (x_end,y_end) where the
            // derivative at x_end is m
            public void Initialize(float x_end, float y_end, float m)
            {
                A = 0.0f;
                B = 1.0f;
                x0 = 0.0f;
                y0 = 0.0f;
                signX = 1.0f;
                signY = 1.0f;

                // Invalid case, slope must be positive and the
                // y that we are trying to hit must be positve.
                if (m <= 0.0f || y_end <= 0.0f)
                {
                    return;
                }

                // Also invalid
                if (x_end <= 0.0f)
                {
                    return;
                }

                B = (m * x_end) / y_end;

                float p = Mathf.Pow(x_end, B);
                A = y_end / p;
                logA = Mathf.Log(y_end) - B * Mathf.Log(x_end);
            }
        };

        // Usual & internal stuff
        public Shader tonemapShader = null;
        public bool validRenderTextureFormat = true;
        private Material m_TonemapMaterial;

        public Material tonemapMaterial
        {
            get
            {
                if (m_TonemapMaterial == null)
                    m_TonemapMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(tonemapShader);

                return m_TonemapMaterial;
            }
        }

        private int m_UserLutDim = 16;
        private Color[] m_UserLutData;


        protected void OnEnable()
        {
            if (tonemapShader == null)
                tonemapShader = Shader.Find("Hidden/TonemappingColorGrading");

            if (ImageEffectHelper.IsSupported(tonemapShader, false, true, this))
                return;

            enabled = false;
            Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
        }

        float GetHighlightRecovery()
        {
            return Mathf.Max(0.0f, m_FilmicCurve.lutShoulder * 3.0f);
        }

        public float GetWhitePoint()
        {
            return Mathf.Pow(2.0f, Mathf.Max(0.0f, GetHighlightRecovery()));
        }

        static float LutToLin(float x, float lutA)
        {
            x = (x >= 1.0f) ? 1.0f : x;
            float temp = x / lutA;
            return temp / (1.0f - temp);
        }

        static float LinToLut(float x, float lutA)
        {
            return Mathf.Sqrt(x / (x + lutA));
        }

        static float LiftGammaGain(float x, float lift, float invGamma, float gain)
        {
            float xx = Mathf.Sqrt(x);
            float ret = gain * (lift * (1.0f - xx) + Mathf.Pow(xx, invGamma));
            return ret * ret;
        }

        static float LogContrast(float x, float linRef, float contrast)
        {
            x = Mathf.Max(x, 1e-5f);

            float logRef = Mathf.Log(linRef);
            float logVal = Mathf.Log(x);
            float logAdj = logRef + (logVal - logRef) * contrast;
            float dstVal = Mathf.Exp(logAdj);
            return dstVal;
        }

        static Color NormalizeColor(Color c)
        {
            float sum = (c.r + c.g + c.b) / 3.0f;

            if (sum == 0.0f)
                return new Color(1.0f, 1.0f, 1.0f, 1.0f);

            Color ret = new Color();
            ret.r = c.r / sum;
            ret.g = c.g / sum;
            ret.b = c.b / sum;
            ret.a = 1.0f;
            return ret;
        }

        static public float GetLutA()
        {
            // Our basic function is f(x) = A * x / (x + 1)
            // We want the function to actually be able to hit 1.0f (to use
            // the full range of the 3D lut) and that's what A is for.

            // Tried a bunch numbers and 1.05 seems to work pretty well.
            return 1.05f;
        }

        void SetIdentityLut()
        {
            int dim = 16;
            Color[] newC = new Color[dim * dim * dim];
            float oneOverDim = 1.0f / (1.0f * dim - 1.0f);

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    for (int k = 0; k < dim; k++)
                    {
                        newC[i + (j * dim) + (k * dim * dim)] = new Color((i * 1.0f) * oneOverDim, (j * 1.0f) * oneOverDim, (k * 1.0f) * oneOverDim, 1.0f);
                    }
                }
            }

            m_UserLutData = newC;
            m_UserLutDim = dim;
        }

        int ClampLutDim(int src)
        {
            return Mathf.Clamp(src, 0, m_UserLutDim - 1);
        }

        Color SampleLutNearest(int r, int g, int b)
        {
            r = ClampLutDim(r);
            g = ClampLutDim(g);
            g = ClampLutDim(b);
            return m_UserLutData[r + (g * m_UserLutDim) + (b * m_UserLutDim * m_UserLutDim)];
        }

        // Does the lookup without bounds checking
        Color SampleLutNearestUnsafe(int r, int g, int b)
        {
            return m_UserLutData[r + (g * m_UserLutDim) + (b * m_UserLutDim * m_UserLutDim)];
        }

        Color SampleLutLinear(float srcR, float srcG, float srcB)
        {
            float sampleOffset = 0.0f;
            float sampleScale = (float)(m_UserLutDim - 1);

            float r = srcR * sampleScale + sampleOffset;
            float g = srcG * sampleScale + sampleOffset;
            float b = srcB * sampleScale + sampleOffset;

            int r0 = Mathf.FloorToInt(r);
            int g0 = Mathf.FloorToInt(g);
            int b0 = Mathf.FloorToInt(b);

            r0 = ClampLutDim(r0);
            g0 = ClampLutDim(g0);
            b0 = ClampLutDim(b0);

            int r1 = ClampLutDim(r0 + 1);
            int g1 = ClampLutDim(g0 + 1);
            int b1 = ClampLutDim(b0 + 1);

            float tr = (r) - (float)r0;
            float tg = (g) - (float)g0;
            float tb = (b) - (float)b0;

            Color c000 = SampleLutNearestUnsafe(r0, g0, b0);
            Color c001 = SampleLutNearestUnsafe(r0, g0, b1);
            Color c010 = SampleLutNearestUnsafe(r0, g1, b0);
            Color c011 = SampleLutNearestUnsafe(r0, g1, b1);
            Color c100 = SampleLutNearestUnsafe(r1, g0, b0);
            Color c101 = SampleLutNearestUnsafe(r1, g0, b1);
            Color c110 = SampleLutNearestUnsafe(r1, g1, b0);
            Color c111 = SampleLutNearestUnsafe(r1, g1, b1);

            Color c00 = Color.Lerp(c000, c001, tb);
            Color c01 = Color.Lerp(c010, c011, tb);
            Color c10 = Color.Lerp(c100, c101, tb);
            Color c11 = Color.Lerp(c110, c111, tb);

            Color c0 = Color.Lerp(c00, c01, tg);
            Color c1 = Color.Lerp(c10, c11, tg);

            Color c = Color.Lerp(c0, c1, tr);

            return c;
        }

        void UpdateUserLut()
        {
            // Conversion fun: the given 2D texture needs to be of the format
            //  w * h, wheras h is the 'depth' (or 3d dimension 'dim') and w = dim * dim
            if (userLutTexture == null)
            {
                SetIdentityLut();
                return;
            }

            if (!ValidDimensions(userLutTexture))
            {
                Debug.LogWarning("The given 2D texture " + userLutTexture.name + " cannot be used as a 3D LUT. Reverting to identity.");
                SetIdentityLut();
                return;
            }

            int dim = userLutTexture.height;
            Color[] c = userLutTexture.GetPixels();
            Color[] newC = new Color[c.Length];

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    for (int k = 0; k < dim; k++)
                    {
                        int j_ = dim - j - 1;
                        Color dst = c[k * dim + i + j_ * dim * dim];
                        newC[i + (j * dim) + (k * dim * dim)] = dst;
                    }
                }
            }

            m_UserLutDim = dim;
            m_UserLutData = newC;
        }

        public float EvalFilmicHelper(float src, float lutA,
            SimplePolyFunc polyToe,
            SimplePolyFunc polyLinear,
            SimplePolyFunc polyShoulder,
            float x0, float x1, float linearW)
        {
            // Figure out the linear value of this 3d texel
            float dst = LutToLin(src, lutA);

            if (m_FilmicCurve.enabled)
            {
                // We could allow this to be customized, but most people probably
                // would not understand it and it would just create complexity.
                // 18% grey is the standard film reference grey so let's just go with that.
                float linRef = .18f;
                dst = LogContrast(dst, linRef, m_FilmicCurve.contrast);

                SimplePolyFunc polyR = polyToe;
                if (dst >= x0)
                    polyR = polyLinear;
                if (dst >= x1)
                    polyR = polyShoulder;

                dst = Mathf.Min(dst, linearW);
                dst = polyR.Eval(dst);
            }

            return dst;
        }

        float EvalCurveGradingHelper(float src, float lift, float invGamma, float gain)
        {
            float dst = src;

            if (m_ColorGrading.enabled)
            {
                // lift/gamma/gain
                dst = LiftGammaGain(dst, lift, invGamma, gain);
            }

            // Max with zero
            dst = Mathf.Max(dst, 0.0f);

            if (m_ColorGrading.enabled)
            {
                // Overall gamma
                dst = Mathf.Pow(dst, m_ColorGrading.gamma);
            }
            return dst;
        }

        void Create3DLut(float lutA,
            SimplePolyFunc polyToe,
            SimplePolyFunc polyLinear,
            SimplePolyFunc polyShoulder,
            float x0, float x1, float linearW,
            float liftR, float invGammaR, float gainR,
            float liftG, float invGammaG, float gainG,
            float liftB, float invGammaB, float gainB)
        {
            int dim = 32;
            Color[] newC = new Color[dim * dim * dim];
            float oneOverDim = 1.0f / (1.0f * dim - 1.0f);

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    for (int k = 0; k < dim; k++)
                    {
                        float srcR = (i * 1.0f) * oneOverDim;
                        float srcG = (j * 1.0f) * oneOverDim;
                        float srcB = (k * 1.0f) * oneOverDim;

                        float dstR = EvalFilmicHelper(srcR, lutA,
                                polyToe,
                                polyLinear,
                                polyShoulder,
                                x0, x1, linearW);

                        float dstG = EvalFilmicHelper(srcG, lutA,
                                polyToe,
                                polyLinear,
                                polyShoulder,
                                x0, x1, linearW);

                        float dstB = EvalFilmicHelper(srcB, lutA,
                                polyToe,
                                polyLinear,
                                polyShoulder,
                                x0, x1, linearW);

                        Color c = SampleLutLinear(dstR, dstG, dstB);
                        dstR = c.r;
                        dstG = c.g;
                        dstB = c.b;

                        dstR = EvalCurveGradingHelper(dstR, liftR, invGammaR, gainR);
                        dstG = EvalCurveGradingHelper(dstG, liftG, invGammaG, gainG);
                        dstB = EvalCurveGradingHelper(dstB, liftB, invGammaB, gainB);

                        if (m_ColorGrading.enabled)
                        {
                            // Saturation
                            float lum = dstR * 0.2125f + dstG * 0.7154f + dstB * 0.0721f;
                            dstR = lum + (dstR - lum) * m_ColorGrading.saturation;
                            dstG = lum + (dstG - lum) * m_ColorGrading.saturation;
                            dstB = lum + (dstB - lum) * m_ColorGrading.saturation;
                        }

                        newC[i + (j * dim) + (k * dim * dim)] = new Color(dstR, dstG, dstB, 1.0f);
                    }
                }
            }

            if (m_LutTex == null)
            {
                m_LutTex = new Texture3D(dim, dim, dim, TextureFormat.RGB24, false);
                m_LutTex.filterMode = FilterMode.Bilinear;
                m_LutTex.wrapMode = TextureWrapMode.Clamp;
                m_LutTex.hideFlags = HideFlags.DontSave;
            }

            m_LutTex.SetPixels(newC);
            m_LutTex.Apply();
        }

        void Create1DLut(float lutA,
            SimplePolyFunc polyToe,
            SimplePolyFunc polyLinear,
            SimplePolyFunc polyShoulder,
            float x0, float x1, float linearW,
            float liftR, float invGammaR, float gainR,
            float liftG, float invGammaG, float gainG,
            float liftB, float invGammaB, float gainB)
        {
            int curveLen = 128;

            Color[] newC = new Color[curveLen * 2];
            float oneOverDim = 1.0f / (1.0f * curveLen - 1.0f);

            for (int i = 0; i < curveLen; i++)
            {
                float srcR = (i * 1.0f) * oneOverDim;
                float srcG = (i * 1.0f) * oneOverDim;
                float srcB = (i * 1.0f) * oneOverDim;

                float dstR = EvalFilmicHelper(srcR, lutA,
                        polyToe,
                        polyLinear,
                        polyShoulder,
                        x0, x1, linearW);

                float dstG = EvalFilmicHelper(srcG, lutA,
                        polyToe,
                        polyLinear,
                        polyShoulder,
                        x0, x1, linearW);

                float dstB = EvalFilmicHelper(srcB, lutA,
                        polyToe,
                        polyLinear,
                        polyShoulder,
                        x0, x1, linearW);

                Color c = SampleLutLinear(dstR, dstG, dstB);
                dstR = c.r;
                dstG = c.g;
                dstB = c.b;

                dstR = EvalCurveGradingHelper(dstR, liftR, invGammaR, gainR);
                dstG = EvalCurveGradingHelper(dstG, liftG, invGammaG, gainG);
                dstB = EvalCurveGradingHelper(dstB, liftB, invGammaB, gainB);

                // Saturation is done in the shader as it can't be baked into color curves

                if (isLinearColorSpace)
                {
                    dstR = Mathf.LinearToGammaSpace(dstR);
                    dstG = Mathf.LinearToGammaSpace(dstG);
                    dstB = Mathf.LinearToGammaSpace(dstB);
                }

                newC[i + 0 * curveLen] = new Color(dstR, dstG, dstB, 1.0f);
                newC[i + 1 * curveLen] = new Color(dstR, dstG, dstB, 1.0f);
            }

            if (m_LutCurveTex1D == null)
            {
                m_LutCurveTex1D = new Texture2D(curveLen, 2, TextureFormat.RGB24, false);
                m_LutCurveTex1D.filterMode = FilterMode.Bilinear;
                m_LutCurveTex1D.wrapMode = TextureWrapMode.Clamp;
                m_LutCurveTex1D.hideFlags = HideFlags.DontSave;
            }

            m_LutCurveTex1D.SetPixels(newC);
            m_LutCurveTex1D.Apply();
        }

        void UpdateLut()
        {
            UpdateUserLut();

            float lutA = GetLutA();

            SimplePolyFunc polyToe;
            SimplePolyFunc polyLinear;
            SimplePolyFunc polyShoulder;

            float gammaSpace = 2.2f;

            float x0 = Mathf.Pow(1.0f / 3.0f, gammaSpace);
            float shoulderBase = .7f;
            float x1 = Mathf.Pow(shoulderBase, gammaSpace);
            float gammaHighY = Mathf.Pow(shoulderBase, 1.0f + (m_FilmicCurve.lutShoulder) * 1.0f);
            float y1 = Mathf.Pow(gammaHighY, gammaSpace);

            float y0;
            {
                float t = x0 / x1;
                float lin = t * y1;
                float low = lin * (1.0f - m_FilmicCurve.toe * .5f);
                y0 = low;
            }

            float dx = x1 - x0;
            float dy = y1 - y0;

            float m = 0.0f;
            if (dx > 0 && dy > 0)
                m = dy / dx;

            // Linear section, power is 1, slope is m
            polyLinear.x0 = x0;
            polyLinear.y0 = y0;
            polyLinear.A = m;
            polyLinear.B = 1.0f;
            polyLinear.signX = 1.0f;
            polyLinear.signY = 1.0f;
            polyLinear.logA = Mathf.Log(m);

            // Toe
            polyToe = polyLinear;
            polyToe.Initialize(x0, y0, m);

            float linearW = GetWhitePoint();

            {
                // Shoulder, first think about it "backwards"
                float offsetX = linearW - x1;
                float offsetY = 1.0f - y1;

                polyShoulder = polyLinear;
                polyShoulder.Initialize(offsetX, offsetY, m);

                // Flip horizontal
                polyShoulder.signX = -1.0f;
                polyShoulder.x0 = -linearW;

                // Flip vertical
                polyShoulder.signY = -1.0f;
                polyShoulder.y0 = 1.0f;
            }

            Color normS = NormalizeColor(m_ColorGrading.lutColors.shadows);
            Color normM = NormalizeColor(m_ColorGrading.lutColors.midtones);
            Color normH = NormalizeColor(m_ColorGrading.lutColors.highlights);

            float avgS = (normS.r + normS.g + normS.b) / 3.0f;
            float avgM = (normM.r + normM.g + normM.b) / 3.0f;
            float avgH = (normH.r + normH.g + normH.b) / 3.0f;

            // These are magic numbers
            float liftScale = .1f;
            float gammaScale = .5f;
            float gainScale = .5f;

            float liftR = (normS.r - avgS) * liftScale;
            float liftG = (normS.g - avgS) * liftScale;
            float liftB = (normS.b - avgS) * liftScale;

            float gammaR = Mathf.Pow(2.0f, (normM.r - avgM) * gammaScale);
            float gammaG = Mathf.Pow(2.0f, (normM.g - avgM) * gammaScale);
            float gammaB = Mathf.Pow(2.0f, (normM.b - avgM) * gammaScale);

            float gainR = Mathf.Pow(2.0f, (normH.r - avgH) * gainScale);
            float gainG = Mathf.Pow(2.0f, (normH.g - avgH) * gainScale);
            float gainB = Mathf.Pow(2.0f, (normH.b - avgH) * gainScale);

            float minGamma = .01f;
            float invGammaR = 1.0f / Mathf.Max(minGamma, gammaR);
            float invGammaG = 1.0f / Mathf.Max(minGamma, gammaG);
            float invGammaB = 1.0f / Mathf.Max(minGamma, gammaB);

            if (!fastMode)
            {
                Create3DLut(lutA,
                    polyToe,
                    polyLinear,
                    polyShoulder,
                    x0, x1, linearW,
                    liftR, invGammaR, gainR,
                    liftG, invGammaG, gainG,
                    liftB, invGammaB, gainB);
            }
            else
            {
                // Instad of doing a single 3D lut, I tried doing this as 3x 1D luts.  Or rather,
                // a single lut with separate curves baked into RGB channels.  It wasn't actually faster
                // do it's disabled.  But there are two reasons why in the future it might be useful:

                // 1.  If it turns out that 3x 1D luts are faster on some hardware, it might be worth it.
                // 2.  Updating the 3D LUT is quite slow so you can't change it every frame.  If the
                //        parameters need to lerp than the 1D version might  be worthwhile.
                Create1DLut(lutA,
                    polyToe,
                    polyLinear,
                    polyShoulder,
                    x0, x1, linearW,
                    liftR, invGammaR, gainR,
                    liftG, invGammaG, gainG,
                    liftB, invGammaB, gainB);
            }
        }

        public bool ValidDimensions(Texture2D tex2d)
        {
            if (!tex2d) return false;
            int h = tex2d.height;
            if (h != Mathf.FloorToInt(Mathf.Sqrt(tex2d.width))) return false;
            return true;
        }

        public void Convert(Texture2D temp2DTex)
        {
#if false
            // Conversion fun: the given 2D texture needs to be of the format
            //  w * h, wheras h is the 'depth' (or 3d dimension 'dim') and w = dim * dim

            if (temp2DTex)
            {
                int dim = temp2DTex.width * temp2DTex.height;
                dim = temp2DTex.height;

                if (!ValidDimensions(temp2DTex))
                {
                    Debug.LogWarning("The given 2D texture " + temp2DTex.name + " cannot be used as a 3D LUT.");
                    //basedOnTempTex = "";
                    return;
                }

                Color[] c = temp2DTex.GetPixels();
                Color[] newC = new Color[c.Length];

                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        for (int k = 0; k < dim; k++)
                        {
                            int j_ = dim - j - 1;
                            newC[i + (j * dim) + (k * dim * dim)] = c[k * dim + i + j_ * dim * dim];
                        }
                    }
                }

                if (converted3DLut)
                    DestroyImmediate(converted3DLut);
                converted3DLut = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, false);
                converted3DLut.SetPixels(newC);
                converted3DLut.Apply();
                userLutTexName = temp2DTex.name;
            }
            else
            {
                // error, something went terribly wrong
                //Debug.LogError("Couldn't color correct with 3D LUT texture. Image Effect will be disabled.");
                SetIdentityLut();
                userLutTexName = "";
            }
#endif
        }

        void OnDisable()
        {
            if (m_TonemapMaterial)
            {
                DestroyImmediate(m_TonemapMaterial);
                m_TonemapMaterial = null;
            }

            if (m_LutTex)
            {
                DestroyImmediate(m_LutTex);
                m_LutTex = null;
            }

            if (m_LutCurveTex1D)
            {
                DestroyImmediate(m_LutCurveTex1D);
                m_LutCurveTex1D = null;
            }

#if UNITY_EDITOR
            m_RTU.ReleaseAllTemporyRenderTexutres();
#endif
        }

        // The image filter chain will continue in LDR
        [ImageEffectTransformsToLDR]
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (tonemapMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (m_LutTex == null || m_Dirty)
            {
                UpdateLut();
                m_Dirty = false;
            }

#if UNITY_EDITOR
            validRenderTextureFormat = true;
            if (source.format != RenderTextureFormat.ARGBHalf)
                validRenderTextureFormat = false;
#endif
            if (fastMode)
                tonemapMaterial.SetTexture("_LutTex1D", m_LutCurveTex1D);
            else
                tonemapMaterial.SetTexture("_LutTex", m_LutTex);

            float lutA = GetLutA();

            float exposureBias = Mathf.Pow(2.0f, m_FilmicCurve.enabled ? m_FilmicCurve.exposureBias : 0.0f);
            Vector4 exposureMult = new Vector4(exposureBias, exposureBias, exposureBias, 1.0f);

            Color linWB = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            if (m_ColorGrading.enabled)
            {
                linWB.r = Mathf.Pow(m_ColorGrading.whiteBalance.r, 2.2f);
                linWB.g = Mathf.Pow(m_ColorGrading.whiteBalance.g, 2.2f);
                linWB.b = Mathf.Pow(m_ColorGrading.whiteBalance.b, 2.2f);

                Color normWB = NormalizeColor(linWB);
                exposureMult.x *= normWB.r;
                exposureMult.y *= normWB.g;
                exposureMult.z *= normWB.b;
            }

            tonemapMaterial.SetFloat("_LutA", lutA);
            tonemapMaterial.SetVector("_LutExposureMult", exposureMult);
            tonemapMaterial.SetFloat("_Vibrance", m_ColorGrading.enabled ? m_ColorGrading.saturation : 1f);

            int pass;
            if (debugClamp)
                pass = (int)(fastMode ? Passes.OneDDebug : Passes.ThreeDDebug);
            else
                pass = (int)(fastMode ? Passes.OneD : Passes.ThreeD);

            Graphics.Blit(source, destination, tonemapMaterial, pass);

#if UNITY_EDITOR
            // if we have an on frame end callabck
            // we need to pass a valid result texture
            // if destination is null we wrote to the
            // backbuffer so we need to copy that out.
            // It's slow and not amazing, but editor only
            if (onFrameEndEditorOnly != null)
            {
                if (destination == null)
                {
                    var temp = m_RTU.GetTemporaryRenderTexture(source.width, source.height);
                    Graphics.Blit(source, temp, tonemapMaterial, pass);
                    onFrameEndEditorOnly(temp, tonemapMaterial);
                    m_RTU.ReleaseTemporaryRenderTexture(temp);
                }
                else
                    onFrameEndEditorOnly(destination, tonemapMaterial);
            }
#endif
        }
    }
}
