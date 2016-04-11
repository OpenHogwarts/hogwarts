using UnityEngine;
using System;

namespace UnityStandardAssets.CinematicEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Cinematic/Lens Aberrations")]
    public class LensAberrations : MonoBehaviour
    {
        #region Attributes
        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsGroup : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class SimpleSetting : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class AdvancedSetting : Attribute
        {}
        #endregion

        #region Settings
        public enum SettingsMode
        {
            Simple,
            Advanced
        }

        [Serializable]
        public struct DistortionSettings
        {
            public bool enabled;

            [Range(-100f, 100f), Tooltip("Distortion amount.")]
            public float amount;

            [Range(0f, 1f), Tooltip("Distortion center point (X axis).")]
            public float centerX;

            [Range(0f, 1f), Tooltip("Distortion center point (Y axis).")]
            public float centerY;

            [Range(0f, 2f), Tooltip("Amount multiplier on X axis.")]
            public float amountX;

            [Range(0f, 2f), Tooltip("Amount multiplier on Y axis.")]
            public float amountY;

            [Range(0.5f, 2f), Tooltip("Global screen scaling.")]
            public float scale;

            public static DistortionSettings defaultSettings
            {
                get
                {
                    return new DistortionSettings
                           {
                               enabled = false,
                               amount = 0f,
                               centerX = 0.5f,
                               centerY = 0.5f,
                               amountX = 1f,
                               amountY = 1f,
                               scale = 1f
                           };
                }
            }
        }

        [Serializable]
        public struct VignetteSettings
        {
            public bool enabled;

            [Tooltip("Use the \"Advanced\" mode if you need more control over the vignette shape and smoothness at the expense of performances.")]
            public SettingsMode mode;

            [Tooltip("Vignette color. Use the alpha channel for transparency.")]
            public Color color;

            [SimpleSetting, Range(0f, 3f), Tooltip("Amount of vignetting on screen.")]
            public float intensity;

            [SimpleSetting, Range(0.1f, 3f), Tooltip("Smoothness of the vignette borders.")]
            public float smoothness;

            [AdvancedSetting, Range(0f, 1f), Tooltip("Vignette radius in screen coordinates.")]
            public float radius;

            [AdvancedSetting, Range(0f, 1f), Tooltip("Smoothness of the vignette border. Tweak this at the same time as \"Falloff\" to get more control over the vignette gradient.")]
            public float spread;

            [AdvancedSetting, Range(0f, 1f), Tooltip("Smoothness of the vignette border. Tweak this at the same time as \"Spread\" to get more control over the vignette gradient.")]
            public float falloff;

            [AdvancedSetting, Range(0f, 1f), Tooltip("Lower values will make a square-ish vignette.")]
            public float roundness;

            [Range(0f, 1f), Tooltip("Blurs the corners of the screen. Leave this at 0 to disable it.")]
            public float blur;

            [Range(0f, 1f), Tooltip("Desaturate the corners of the screen. Leave this to 0 to disable it.")]
            public float desaturate;

            public static VignetteSettings defaultSettings
            {
                get
                {
                    return new VignetteSettings
                           {
                               enabled = false,
                               mode = SettingsMode.Simple,
                               color = Color.black,
                               intensity = 1.2f,
                               smoothness = 1.5f,
                               radius = 0.7f,
                               spread = 0.4f,
                               falloff = 0.5f,
                               roundness = 1f,
                               blur = 0f,
                               desaturate = 0f
                           };
                }
            }
        }

        [Serializable]
        public struct ChromaticAberrationSettings
        {
            public bool enabled;

            [Tooltip("Use the \"Advanced\" mode if you need more control over the chromatic aberrations at the expense of performances.")]
            public SettingsMode mode;

            [Range(-2f, 2f)]
            public float tangential;

            [AdvancedSetting, Range(0f, 2f)]
            public float axial;

            [AdvancedSetting, Range(0f, 2f)]
            public float contrastDependency;

            public static ChromaticAberrationSettings defaultSettings
            {
                get
                {
                    return new ChromaticAberrationSettings
                           {
                               enabled = false,
                               mode = SettingsMode.Simple,
                               tangential = 0f,
                               axial = 0f,
                               contrastDependency = 0f
                           };
                }
            }
        }
        #endregion

        [SettingsGroup]
        public DistortionSettings distortion = DistortionSettings.defaultSettings;

        [SettingsGroup]
        public VignetteSettings vignette = VignetteSettings.defaultSettings;

        [SettingsGroup]
        public ChromaticAberrationSettings chromaticAberration = ChromaticAberrationSettings.defaultSettings;

        private enum Pass
        {
            BlurPrePass,
            Simple,
            Desaturate,
            Blur,
            BlurDesaturate,
            ChromaticAberrationOnly,
            DistortOnly
        }

        [SerializeField]
        private Shader m_Shader;
        public Shader shader
        {
            get
            {
                if (m_Shader == null)
                    m_Shader = Shader.Find("Hidden/LensAberrations");

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

        private void OnEnable()
        {
            if (!ImageEffectHelper.IsSupported(shader, false, false, this))
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
            if (!vignette.enabled && !chromaticAberration.enabled && !distortion.enabled)
            {
                Graphics.Blit(source, destination);
                return;
            }

            material.DisableKeyword("DISTORT");
            material.DisableKeyword("UNDISTORT");

            if (distortion.enabled)
            {
                float amount = 1.6f * Math.Max(Mathf.Abs(distortion.amount), 1f);
                float theta = 0.01745329251994f * Math.Min(160f, amount);
                float sigma = 2f * Mathf.Tan(theta * 0.5f);
                Vector4 p0 = new Vector4(2f * distortion.centerX - 1f, 2f * distortion.centerY - 1f, distortion.amountX, distortion.amountY);
                Vector3 p1 = new Vector3(distortion.amount >= 0f ? theta : 1f / theta, sigma, 1f / distortion.scale);
                material.SetVector("_DistCenterScale", p0);
                material.SetVector("_DistAmount", p1);

                if (distortion.amount >= 0f)
                    material.EnableKeyword("DISTORT");
                else
                    material.EnableKeyword("UNDISTORT");
            }

            material.SetColor("_VignetteColor", vignette.color);

            if (vignette.mode == SettingsMode.Simple)
            {
                material.SetVector("_Vignette1", new Vector4(vignette.intensity, vignette.smoothness, vignette.blur, 1f - vignette.desaturate));
                material.DisableKeyword("VIGNETTE_ADVANCED");
            }
            else
            {
                float r1 = 0.5f * vignette.radius;
                float r2 = r1 + vignette.spread;
                float falloff = Math.Max(0.000001f, (1f - vignette.falloff) * 0.5f);
                float roundness = (1f - vignette.roundness) * 6f + vignette.roundness * 2f;
                material.SetVector("_Vignette1", new Vector4(r1, 1f / (r2 - r1), vignette.blur, 1f - vignette.desaturate));
                material.SetVector("_Vignette2", new Vector3(falloff, 0.5f / falloff, roundness));
                material.EnableKeyword("VIGNETTE_ADVANCED");
            }

            material.DisableKeyword("CHROMATIC_SIMPLE");
            material.DisableKeyword("CHROMATIC_ADVANCED");

            if (chromaticAberration.enabled && !Mathf.Approximately(chromaticAberration.tangential, 0f))
            {
                if (chromaticAberration.mode == SettingsMode.Advanced)
                    material.EnableKeyword("CHROMATIC_ADVANCED");
                else
                    material.EnableKeyword("CHROMATIC_SIMPLE");

                Vector4 chromaParams = new Vector4(2.5f * chromaticAberration.tangential, 5f * chromaticAberration.axial, 5f / Mathf.Max(Mathf.Epsilon, chromaticAberration.contrastDependency), 5f);
                material.SetVector("_ChromaticAberration", chromaParams);
            }

            if (vignette.enabled && vignette.blur > 0f)
            {
                // Downscale + gaussian blur (2 passes)
                int w = source.width / 2;
                int h = source.height / 2;
                RenderTexture tmp1 = RenderTexture.GetTemporary(w, h, 0, source.format);
                RenderTexture tmp2 = RenderTexture.GetTemporary(w, h, 0, source.format);

                material.SetVector("_BlurPass", new Vector2(1f / w, 0f));
                Graphics.Blit(source, tmp1, material, (int)Pass.BlurPrePass);
                material.SetVector("_BlurPass", new Vector2(0f, 1f / h));
                Graphics.Blit(tmp1, tmp2, material, (int)Pass.BlurPrePass);

                material.SetVector("_BlurPass", new Vector2(1f / w, 0f));
                Graphics.Blit(tmp2, tmp1, material, (int)Pass.BlurPrePass);
                material.SetVector("_BlurPass", new Vector2(0f, 1f / h));
                Graphics.Blit(tmp1, tmp2, material, (int)Pass.BlurPrePass);

                material.SetTexture("_BlurTex", tmp2);

                if (vignette.desaturate > 0f)
                    Graphics.Blit(source, destination, material, (int)Pass.BlurDesaturate);
                else
                    Graphics.Blit(source, destination, material, (int)Pass.Blur);

                RenderTexture.ReleaseTemporary(tmp2);
                RenderTexture.ReleaseTemporary(tmp1);
            }
            else if (vignette.enabled && vignette.desaturate > 0f)
            {
                Graphics.Blit(source, destination, material, (int)Pass.Desaturate);
            }
            else if (vignette.enabled)
            {
                Graphics.Blit(source, destination, material, (int)Pass.Simple);
            }
            else if (chromaticAberration.enabled)
            {
                Graphics.Blit(source, destination, material, (int)Pass.ChromaticAberrationOnly);
            }
            else // Distortion enabled
            {
                Graphics.Blit(source, destination, material, (int)Pass.DistortOnly);
            }
        }
    }
}
