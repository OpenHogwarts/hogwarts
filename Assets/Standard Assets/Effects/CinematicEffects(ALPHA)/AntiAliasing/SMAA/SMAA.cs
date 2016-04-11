using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace UnityStandardAssets.CinematicEffects
{
    [Serializable]
    public class SMAA : IAntiAliasing
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class SettingsGroup : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class TopLevelSettings : Attribute
        {}

        [AttributeUsage(AttributeTargets.Field)]
        public class ExperimentalGroup : Attribute
        {}

        public enum DebugPass
        {
            Off,
            Edges,
            Weights,
            Accumulation
        }

        public enum QualityPreset
        {
            Low = 0,
            Medium = 1,
            High = 2,
            Ultra = 3,
            Custom
        }

        public enum EdgeDetectionMethod
        {
            Luma = 1,
            Color = 2,
            Depth = 3
        }

        [Serializable]
        public struct GlobalSettings
        {
            [Tooltip("Use this to fine tune your settings when working in Custom quality mode. \"Accumulation\" only works when \"Temporal Filtering\" is enabled.")]
            public DebugPass debugPass;

            [Tooltip("Low: 60% of the quality.\nMedium: 80% of the quality.\nHigh: 95% of the quality.\nUltra: 99% of the quality (overkill).")]
            public QualityPreset quality;

            [Tooltip("You've three edge detection methods to choose from: luma, color or depth.\nThey represent different quality/performance and anti-aliasing/sharpness tradeoffs, so our recommendation is for you to choose the one that best suits your particular scenario:\n\n- Depth edge detection is usually the fastest but it may miss some edges.\n- Luma edge detection is usually more expensive than depth edge detection, but catches visible edges that depth edge detection can miss.\n- Color edge detection is usually the most expensive one but catches chroma-only edges.")]
            public EdgeDetectionMethod edgeDetectionMethod;

            public static GlobalSettings defaultSettings
            {
                get
                {
                    return new GlobalSettings
                    {
                        debugPass = DebugPass.Off,
                        quality = QualityPreset.High,
                        edgeDetectionMethod = EdgeDetectionMethod.Color
                    };
                }
            }
        }

        [Serializable]
        public struct QualitySettings
        {
            [Tooltip("Enables/Disables diagonal processing.")]
            public bool diagonalDetection;

            [Tooltip("Enables/Disables corner detection. Leave this on to avoid blurry corners.")]
            public bool cornerDetection;

            [Range(0f, 0.5f)]
            [Tooltip("Specifies the threshold or sensitivity to edges. Lowering this value you will be able to detect more edges at the expense of performance.\n0.1 is a reasonable value, and allows to catch most visible edges. 0.05 is a rather overkill value, that allows to catch 'em all.")]
            public float threshold;

            [Min(0.0001f)]
            [Tooltip("Specifies the threshold for depth edge detection. Lowering this value you will be able to detect more edges at the expense of performance.")]
            public float depthThreshold;

            [Range(0, 112)]
            [Tooltip("Specifies the maximum steps performed in the horizontal/vertical pattern searches, at each side of the pixel.\nIn number of pixels, it's actually the double. So the maximum line length perfectly handled by, for example 16, is 64 (by perfectly, we meant that longer lines won't look as good, but still antialiased).")]
            public int maxSearchSteps;

            [Range(0, 20)]
            [Tooltip("Specifies the maximum steps performed in the diagonal pattern searches, at each side of the pixel. In this case we jump one pixel at time, instead of two.\nOn high-end machines it is cheap (between a 0.8x and 0.9x slower for 16 steps), but it can have a significant impact on older machines.")]
            public int maxDiagonalSearchSteps;

            [Range(0, 100)]
            [Tooltip("Specifies how much sharp corners will be rounded.")]
            public int cornerRounding;

            [Min(0f)]
            [Tooltip("If there is an neighbor edge that has a local contrast factor times bigger contrast than current edge, current edge will be discarded.\nThis allows to eliminate spurious crossing edges, and is based on the fact that, if there is too much contrast in a direction, that will hide perceptually contrast in the other neighbors.")]
            public float localContrastAdaptationFactor;

            public static QualitySettings[] presetQualitySettings =
            {
                // Low
                new QualitySettings
                {
                    diagonalDetection = false,
                    cornerDetection = false,
                    threshold = 0.15f,
                    depthThreshold = 0.01f,
                    maxSearchSteps = 4,
                    maxDiagonalSearchSteps = 8,
                    cornerRounding = 25,
                    localContrastAdaptationFactor = 2f
                },

                // Medium
                new QualitySettings
                {
                    diagonalDetection = false,
                    cornerDetection = false,
                    threshold = 0.1f,
                    depthThreshold = 0.01f,
                    maxSearchSteps = 8,
                    maxDiagonalSearchSteps = 8,
                    cornerRounding = 25,
                    localContrastAdaptationFactor = 2f
                },

                // High
                new QualitySettings
                {
                    diagonalDetection = true,
                    cornerDetection = true,
                    threshold = 0.1f,
                    depthThreshold = 0.01f,
                    maxSearchSteps = 16,
                    maxDiagonalSearchSteps = 8,
                    cornerRounding = 25,
                    localContrastAdaptationFactor = 2f
                },

                // Ultra
                new QualitySettings
                {
                    diagonalDetection = true,
                    cornerDetection = true,
                    threshold = 0.05f,
                    depthThreshold = 0.01f,
                    maxSearchSteps = 32,
                    maxDiagonalSearchSteps = 16,
                    cornerRounding = 25,
                    localContrastAdaptationFactor = 2f
                },
            };
        }

        [Serializable]
        public struct TemporalSettings
        {
            [Tooltip("Temporal filtering makes it possible for the SMAA algorithm to benefit from minute subpixel information available that has been accumulated over many frames.")]
            public bool enabled;

            public bool UseTemporal()
            {
#if UNITY_EDITOR
                return enabled && EditorApplication.isPlayingOrWillChangePlaymode;
#else
                return enabled;
#endif
            }

            [Range(0.5f, 10.0f)]
            [Tooltip("The size of the fuzz-displacement (jitter) in pixels applied to the camera's perspective projection matrix.\nUsed for 2x temporal anti-aliasing.")]
            public float fuzzSize;

            public static TemporalSettings defaultSettings
            {
                get
                {
                    return new TemporalSettings
                    {
                        enabled = false,
                        fuzzSize = 2f
                    };
                }
            }
        }

        [Serializable]
        public struct PredicationSettings
        {
            [Tooltip("Predicated thresholding allows to better preserve texture details and to improve performance, by decreasing the number of detected edges using an additional buffer (the detph buffer).\nIt locally decreases the luma or color threshold if an edge is found in an additional buffer (so the global threshold can be higher).")]
            public bool enabled;

            [Min(0.0001f)]
            [Tooltip("Threshold to be used in the additional predication buffer.")]
            public float threshold;

            [Range(1f, 5f)]
            [Tooltip("How much to scale the global threshold used for luma or color edge detection when using predication.")]
            public float scale;

            [Range(0f, 1f)]
            [Tooltip("How much to locally decrease the threshold.")]
            public float strength;

            public static PredicationSettings defaultSettings
            {
                get
                {
                    return new PredicationSettings
                    {
                        enabled = false,
                        threshold = 0.01f,
                        scale = 2f,
                        strength = 0.4f
                    };
                }
            }
        }

        [TopLevelSettings]
        public GlobalSettings settings = GlobalSettings.defaultSettings;

        [SettingsGroup]
        public QualitySettings quality = QualitySettings.presetQualitySettings[2];

        [SettingsGroup]
        public PredicationSettings predication = PredicationSettings.defaultSettings;

        [SettingsGroup, ExperimentalGroup]
        public TemporalSettings temporal = TemporalSettings.defaultSettings;

        private Matrix4x4 m_ProjectionMatrix;
        private Matrix4x4 m_PreviousViewProjectionMatrix;
        private float m_FlipFlop = 1.0f;
        private RenderTexture m_Accumulation;

        private Shader m_Shader;
        public Shader shader
        {
            get
            {
                if (m_Shader == null)
                    m_Shader = Shader.Find("Hidden/Subpixel Morphological Anti-aliasing");

                return m_Shader;
            }
        }

        private Texture2D m_AreaTexture;
        private Texture2D areaTexture
        {
            get
            {
                if (m_AreaTexture == null)
                    m_AreaTexture = Resources.Load<Texture2D>("AreaTex");
                return m_AreaTexture;
            }
        }

        private Texture2D m_SearchTexture;
        private Texture2D searchTexture
        {
            get
            {
                if (m_SearchTexture == null)
                    m_SearchTexture = Resources.Load<Texture2D>("SearchTex");
                return m_SearchTexture;
            }
        }

        private Material m_Material;
        private Material material
        {
            get
            {
                if (m_Material == null)
                    m_Material = ImageEffectHelper.CheckShaderAndCreateMaterial(shader);

                return m_Material;
            }
        }

        public void OnEnable(AntiAliasing owner)
        {
            if (!ImageEffectHelper.IsSupported(shader, true, false, owner))
                owner.enabled = false;
        }

        public void OnDisable()
        {
            // Cleanup
            if (m_Material != null)
                Object.DestroyImmediate(m_Material);

            if (m_Accumulation != null)
                Object.DestroyImmediate(m_Accumulation);

            m_Material = null;
            m_Accumulation = null;
        }

        public void OnPreCull(Camera camera)
        {
            if (temporal.UseTemporal())
            {
                m_ProjectionMatrix = camera.projectionMatrix;
                m_FlipFlop -= (2.0f * m_FlipFlop);

                Matrix4x4 fuzz = Matrix4x4.identity;

                fuzz.m03 = (0.25f * m_FlipFlop) * temporal.fuzzSize / camera.pixelWidth;
                fuzz.m13 = (-0.25f * m_FlipFlop) * temporal.fuzzSize / camera.pixelHeight;

                camera.projectionMatrix = fuzz * camera.projectionMatrix;
            }
        }

        public void OnPostRender(Camera camera)
        {
            if (temporal.UseTemporal())
                camera.ResetProjectionMatrix();
        }

        public void OnRenderImage(Camera camera, RenderTexture source, RenderTexture destination)
        {
            int width = camera.pixelWidth;
            int height = camera.pixelHeight;

            bool isFirstFrame = false;

            QualitySettings preset = quality;

            if (settings.quality != QualityPreset.Custom)
                preset = QualitySettings.presetQualitySettings[(int)settings.quality];

            // Pass IDs
            int passEdgeDetection = (int)settings.edgeDetectionMethod;
            int passBlendWeights = 4;
            int passNeighborhoodBlending = 5;
            int passResolve = 6;

            // Reprojection setup
            var viewProjectionMatrix = GL.GetGPUProjectionMatrix(m_ProjectionMatrix, true) * camera.worldToCameraMatrix;

            // Uniforms
            material.SetTexture("_AreaTex", areaTexture);
            material.SetTexture("_SearchTex", searchTexture);

            material.SetVector("_Metrics", new Vector4(1f / width, 1f / height, width, height));
            material.SetVector("_Params1", new Vector4(preset.threshold, preset.depthThreshold, preset.maxSearchSteps, preset.maxDiagonalSearchSteps));
            material.SetVector("_Params2", new Vector2(preset.cornerRounding, preset.localContrastAdaptationFactor));

            material.SetMatrix("_ReprojectionMatrix", m_PreviousViewProjectionMatrix * Matrix4x4.Inverse(viewProjectionMatrix));

            float subsampleIndex = (m_FlipFlop < 0.0f) ? 2.0f : 1.0f;
            material.SetVector("_SubsampleIndices", new Vector4(subsampleIndex, subsampleIndex, subsampleIndex, 0.0f));

            // Handle predication & depth-based edge detection
            Shader.DisableKeyword("USE_PREDICATION");

            if (settings.edgeDetectionMethod == EdgeDetectionMethod.Depth)
            {
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }
            else if (predication.enabled)
            {
                camera.depthTextureMode |= DepthTextureMode.Depth;
                Shader.EnableKeyword("USE_PREDICATION");
                material.SetVector("_Params3", new Vector3(predication.threshold, predication.scale, predication.strength));
            }

            // Diag search & corner detection
            Shader.DisableKeyword("USE_DIAG_SEARCH");
            Shader.DisableKeyword("USE_CORNER_DETECTION");

            if (preset.diagonalDetection)
                Shader.EnableKeyword("USE_DIAG_SEARCH");

            if (preset.cornerDetection)
                Shader.EnableKeyword("USE_CORNER_DETECTION");

            // UV-based reprojection (up to Unity 5.x)
            // TODO: use motion vectors when available!
            Shader.DisableKeyword("USE_UV_BASED_REPROJECTION");

            if (temporal.UseTemporal())
                Shader.EnableKeyword("USE_UV_BASED_REPROJECTION");

            // Persistent textures and lazy-initializations
            if (m_Accumulation == null || (m_Accumulation.width != width || m_Accumulation.height != height))
            {
                if (m_Accumulation)
                    RenderTexture.ReleaseTemporary(m_Accumulation);

                m_Accumulation = RenderTexture.GetTemporary(width, height, 0, source.format, RenderTextureReadWrite.Linear);
                m_Accumulation.hideFlags = HideFlags.HideAndDontSave;

                isFirstFrame = true;
            }

            RenderTexture rt1 = TempRT(width, height, source.format);
            Graphics.Blit(null, rt1, material, 0); // Clear

            // Edge Detection
            Graphics.Blit(source, rt1, material, passEdgeDetection);

            if (settings.debugPass == DebugPass.Edges)
            {
                Graphics.Blit(rt1, destination);
            }
            else
            {
                RenderTexture rt2 = TempRT(width, height, source.format);
                Graphics.Blit(null, rt2, material, 0); // Clear

                // Blend Weights
                Graphics.Blit(rt1, rt2, material, passBlendWeights);

                if (settings.debugPass == DebugPass.Weights)
                {
                    Graphics.Blit(rt2, destination);
                }
                else
                {
                    // Neighborhood Blending
                    material.SetTexture("_BlendTex", rt2);

                    if (temporal.UseTemporal())
                    {
                        // Temporal filtering
                        Graphics.Blit(source, rt1, material, passNeighborhoodBlending);

                        if (settings.debugPass == DebugPass.Accumulation)
                        {
                            Graphics.Blit(m_Accumulation, destination);
                        }
                        else if (!isFirstFrame)
                        {
                            material.SetTexture("_AccumulationTex", m_Accumulation);
                            Graphics.Blit(rt1, destination, material, passResolve);
                        }
                        else
                        {
                            Graphics.Blit(rt1, destination);
                        }

                        Graphics.Blit(rt1, m_Accumulation);
                        RenderTexture.active = null;
                    }
                    else
                    {
                        Graphics.Blit(source, destination, material, passNeighborhoodBlending);
                    }
                }

                RenderTexture.ReleaseTemporary(rt2);
            }

            RenderTexture.ReleaseTemporary(rt1);

            // Store the future-previous frame's view-projection matrix
            m_PreviousViewProjectionMatrix = viewProjectionMatrix;
        }

        private RenderTexture TempRT(int width, int height, RenderTextureFormat format)
        {
            // Skip the depth & stencil buffer creation when DebugPass is set to avoid flickering
            // TODO: Stencil buffer not working for some reason
            // int depthStencilBits = DebugPass == DebugPass.Off ? 24 : 0;
            int depthStencilBits = 0;
            return RenderTexture.GetTemporary(width, height, depthStencilBits, format, RenderTextureReadWrite.Linear);
        }
    }
}
