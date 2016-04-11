using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Cinematic/Bloom")]
    public class Bloom : MonoBehaviour
    {
        [Serializable]
        public struct Settings
        {
            [SerializeField]
            [Tooltip("Filters out pixels under this level of brightness.")]
            public float threshold;

            public float thresholdGamma
            {
                set { threshold = value; }
                get { return Mathf.Max(0.0f, threshold); }
            }

            public float thresholdLinear
            {
                set { threshold = Mathf.LinearToGammaSpace(value); }
                get { return Mathf.GammaToLinearSpace(thresholdGamma); }
            }

            [SerializeField, Range(1, 7)]
            [Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
            public float radius;

            [SerializeField]
            [Tooltip("Blend factor of the result image.")]
            public float intensity;

            [SerializeField]
            [Tooltip("Controls filter quality and buffer resolution.")]
            public bool highQuality;

            [SerializeField]
            [Tooltip("Reduces flashing noise with an additional filter.")]
            public bool antiFlicker;

            public static Settings defaultSettings
            {
                get
                {
                    var settings = new Settings
                    {
                        threshold = 0.9f,
                        radius = 2.0f,
                        intensity = 0.7f,
                        highQuality = true,
                        antiFlicker = false
                    };
                    return settings;
                }
            }
        }

        #region Public Properties

        [SerializeField]
        public Settings settings = Settings.defaultSettings;

        #endregion

        [SerializeField, HideInInspector]
        private Shader m_Shader;

        public Shader shader
        {
            get
            {
                if (m_Shader == null)
                {
                    const string shaderName = "Hidden/Image Effects/Cinematic/Bloom";
                    m_Shader = Shader.Find(shaderName);
                }

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

        #region Private Members

        const int kMaxIterations = 16;
        RenderTexture[] m_blurBuffer1 = new RenderTexture[kMaxIterations];
        RenderTexture[] m_blurBuffer2 = new RenderTexture[kMaxIterations];

        private void OnEnable()
        {
            if (!ImageEffectHelper.IsSupported(shader, true, false, this))
                enabled = false;
        }

        private void OnDisable()
        {
            if (m_Material != null)
                DestroyImmediate(m_Material);

            m_Material = null;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var useRGBM = Application.isMobilePlatform;
            var isGamma = QualitySettings.activeColorSpace == ColorSpace.Gamma;

            // source texture size
            var tw = source.width;
            var th = source.height;

            // halve the texture size for the low quality mode
            if (!settings.highQuality)
            {
                tw /= 2;
                th /= 2;
            }

            // blur buffer format
            var rtFormat = useRGBM ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

            // determine the iteration count
            var logh = Mathf.Log(th, 2) + settings.radius - 8;
            var logh_i = (int)logh;
            var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

            // update the shader properties
            var threshold = settings.thresholdLinear;
            material.SetFloat("_Threshold", threshold);

            const float softKneeRatio = 0.5f;
            var knee = threshold * softKneeRatio + 1e-5f;
            var curve = new Vector3(threshold - knee, knee * 2, 0.25f / knee);
            material.SetVector("_Curve", curve);

            var pfo = !settings.highQuality && settings.antiFlicker;
            material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

            material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
            material.SetFloat("_Intensity", Mathf.Max(0.0f, settings.intensity));

            if (settings.highQuality)
                material.EnableKeyword("HIGH_QUALITY");
            else
                material.DisableKeyword("HIGH_QUALITY");

            if (settings.antiFlicker)
                material.EnableKeyword("ANTI_FLICKER");
            else
                material.DisableKeyword("ANTI_FLICKER");

            if (isGamma)
            {
                material.DisableKeyword("LINEAR_COLOR");
                material.EnableKeyword("GAMMA_COLOR");
            }
            else
            {
                material.EnableKeyword("LINEAR_COLOR");
                material.DisableKeyword("GAMMA_COLOR");
            }

            // prefilter pass
            var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);
            Graphics.Blit(source, prefiltered, material, 0);

            // construct a mip pyramid
            var last = prefiltered;
            for (var level = 0; level < iterations; level++)
            {
                m_blurBuffer1[level] = RenderTexture.GetTemporary(last.width / 2, last.height / 2, 0, rtFormat);
                Graphics.Blit(last, m_blurBuffer1[level], material, (level == 0) ? 1 : 2);
                last = m_blurBuffer1[level];
            }

            // upsample and combine loop
            for (var level = iterations - 2; level >= 0; level--)
            {
                var basetex = m_blurBuffer1[level];
                material.SetTexture("_BaseTex", basetex);
                m_blurBuffer2[level] = RenderTexture.GetTemporary(basetex.width, basetex.height, 0, rtFormat);
                Graphics.Blit(last, m_blurBuffer2[level], material, 3);
                last = m_blurBuffer2[level];
            }

            // finish process
            material.SetTexture("_BaseTex", source);
            Graphics.Blit(last, destination, material, 4);

            // release the temporary buffers
            for (var i = 0; i < kMaxIterations; i++)
            {
                if (m_blurBuffer1[i] != null) RenderTexture.ReleaseTemporary(m_blurBuffer1[i]);
                if (m_blurBuffer2[i] != null) RenderTexture.ReleaseTemporary(m_blurBuffer2[i]);
                m_blurBuffer1[i] = null;
                m_blurBuffer2[i] = null;
            }

            RenderTexture.ReleaseTemporary(prefiltered);
        }

        #endregion
    }
}
