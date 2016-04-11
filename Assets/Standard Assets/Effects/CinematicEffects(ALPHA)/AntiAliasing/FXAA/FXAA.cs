using UnityEngine;
using System;

using Object = UnityEngine.Object;

namespace UnityStandardAssets.CinematicEffects
{
    [Serializable]
    public class FXAA : IAntiAliasing
    {
        private Shader m_Shader;
        private Shader shader
        {
            get
            {
                if (m_Shader == null)
                    m_Shader = Shader.Find("Hidden/Fast Approximate Anti-aliasing");

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

        [Serializable]
        public struct QualitySettings
        {
            [Tooltip("The amount of desired sub-pixel aliasing removal. Effects the sharpeness of the output.")]
            [Range(0.0f, 1.0f)]
            public float subpixelAliasingRemovalAmount;

            [Tooltip("The minimum amount of local contrast required to qualify a region as containing an edge.")]
            [Range(0.063f, 0.333f)]
            public float edgeDetectionThreshold;

            [Tooltip("Local contrast adaptation value to disallow the algorithm from executing on the darker regions.")]
            [Range(0.0f, 0.0833f)]
            public float minimumRequiredLuminance;
        }

        [Serializable]
        public struct ConsoleSettings
        {
            [Tooltip("The amount of spread applied to the sampling coordinates while sampling for subpixel information.")]
            [Range(0.33f, 0.5f)]
            public float subpixelSpreadAmount;

            [Tooltip("This value dictates how sharp the edges in the image are kept; a higher value implies sharper edges.")]
            [Range(2.0f, 8.0f)]
            public float edgeSharpnessAmount;

            [Tooltip("The minimum amount of local contrast required to qualify a region as containing an edge.")]
            [Range(0.125f, 0.25f)]
            public float edgeDetectionThreshold;

            [Tooltip("Local contrast adaptation value to disallow the algorithm from executing on the darker regions.")]
            [Range(0.04f, 0.06f)]
            public float minimumRequiredLuminance;
        }

        [Serializable]
        public struct Preset
        {
            [AttributeUsage(AttributeTargets.Field)]
            public class LayoutAttribute : PropertyAttribute
            {}

            [Layout]
            public QualitySettings qualitySettings;

            [Layout]
            public ConsoleSettings consoleSettings;

            private static readonly Preset s_ExtremePerformance = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 0.0f,
                    edgeDetectionThreshold = 0.333f,
                    minimumRequiredLuminance = 0.0833f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.33f,
                    edgeSharpnessAmount = 8.0f,
                    edgeDetectionThreshold = 0.25f,
                    minimumRequiredLuminance = 0.06f
                }
            };

            private static readonly Preset s_Performance = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 0.25f,
                    edgeDetectionThreshold = 0.25f,
                    minimumRequiredLuminance = 0.0833f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.33f,
                    edgeSharpnessAmount = 8.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.06f
                }
            };

            private static readonly Preset s_Default = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 0.75f,
                    edgeDetectionThreshold = 0.166f,
                    minimumRequiredLuminance = 0.0833f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.5f,
                    edgeSharpnessAmount = 8.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.05f
                }
            };

            private static readonly Preset s_Quality = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 1.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.0625f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.5f,
                    edgeSharpnessAmount = 4.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.04f
                }
            };

            private static readonly Preset s_ExtremeQuality = new Preset
            {
                qualitySettings = new QualitySettings
                {
                    subpixelAliasingRemovalAmount = 1.0f,
                    edgeDetectionThreshold = 0.063f,
                    minimumRequiredLuminance = 0.0312f
                },

                consoleSettings = new ConsoleSettings
                {
                    subpixelSpreadAmount = 0.5f,
                    edgeSharpnessAmount = 2.0f,
                    edgeDetectionThreshold = 0.125f,
                    minimumRequiredLuminance = 0.04f
                }
            };

            public static Preset extremePerformancePreset
            {
                get { return s_ExtremePerformance; }
            }

            public static Preset performancePreset
            {
                get { return s_Performance; }
            }

            public static Preset defaultPreset
            {
                get { return s_Default; }
            }

            public static Preset qualityPreset
            {
                get { return s_Quality; }
            }

            public static Preset extremeQualityPreset
            {
                get { return s_ExtremeQuality; }
            }
        }

        [SerializeField, HideInInspector]
        public Preset preset = Preset.defaultPreset;

        public static Preset[] availablePresets =
        {
            Preset.extremePerformancePreset,
            Preset.performancePreset,
            Preset.defaultPreset,
            Preset.qualityPreset,
            Preset.extremeQualityPreset
        };

        public bool validSourceFormat { get; private set; }

        public void OnEnable(AntiAliasing owner)
        {
            if (!ImageEffectHelper.IsSupported(shader, true, false, owner))
                owner.enabled = false;
        }

        public void OnDisable()
        {
            if (m_Material != null)
                Object.DestroyImmediate(m_Material);
        }

        public void OnPreCull(Camera camera)
        {
        }

        public void OnPostRender(Camera camera)
        {
        }

        public void OnRenderImage(Camera camera, RenderTexture source, RenderTexture destination)
        {
#if UNITY_EDITOR
            validSourceFormat = true;

            if (source.format == RenderTextureFormat.ARGBHalf
                || source.format == RenderTextureFormat.ARGBFloat
                || source.format == RenderTextureFormat.ARGB2101010)
                validSourceFormat = false;
#endif

            material.SetVector("_QualitySettings", new Vector3(preset.qualitySettings.subpixelAliasingRemovalAmount,
                    preset.qualitySettings.edgeDetectionThreshold, preset.qualitySettings.minimumRequiredLuminance));

            material.SetVector("_ConsoleSettings", new Vector4(preset.consoleSettings.subpixelSpreadAmount,
                    preset.consoleSettings.edgeSharpnessAmount, preset.consoleSettings.edgeDetectionThreshold,
                    preset.consoleSettings.minimumRequiredLuminance));

            Graphics.Blit(source, destination, material, 0);
        }
    }
}
