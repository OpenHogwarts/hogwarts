using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Other/SMAA")]
    public class AntiAliasing : MonoBehaviour
    {
        public enum DebugDisplay
        {
            Off,
            Edges,
            Weights,
            Depth,
            Accumulation
        }

        public enum EdgeType
        {
            Luminance,
            Color,
            Depth
        }

        public enum TemporalType
        {
            Off,
            SMAA_2x,
            Standard_2x,
            Standard_4x,
            Standard_8x,
            Standard_16x
        }

        private enum Passes
        {
            Copy = 0,
            LumaDetection = 1,
            ClearToBlack = 2,
            WeightCalculation = 3,
            WeightsAndBlend1 = 4,
            WeightsAndBlend2 = 5,
            ColorDetection = 6,
            MergeFrames = 7,
            DepthDetection = 8,
            DebugDepth = 9
        }

        public DebugDisplay displayType = DebugDisplay.Off;

        // we could make this public, but color and luma are less reliable so we can
        // hardcode this to depth for now
        public EdgeType edgeType = EdgeType.Depth;

        public Texture2D areaTex;
        public Texture2D searchTex;


        // temporal AA parameters
        private Matrix4x4 m_BaseProjectionMatrix;
        private Matrix4x4 m_PrevViewProjMat;

        private Camera m_AACamera;

        private int m_SampleIndex;

        [Range(0, 80)] public float K = 1.0f;

        public TemporalType temporalType = TemporalType.Off;
        [Range(0, 1)] public float temporalAccumulationWeight = 0.3f;

        // This should be hidden from view when EdgeType is not Depth
        [Range(0.01f, 1.0f)] public float depthThreshold = 0.1f;

        private static Matrix4x4 CalculateViewProjection(Camera camera, Matrix4x4 prjMatrix)
        {
            Matrix4x4 viewMat = camera.worldToCameraMatrix;
            Matrix4x4 projMat = GL.GetGPUProjectionMatrix(prjMatrix, true);
            return projMat * viewMat;
        }

        private void StoreBaseProjectionMatrix(Matrix4x4 prjMatrix)
        {
            m_BaseProjectionMatrix = prjMatrix;
        }

        private void StorePreviousViewProjMatrix(Matrix4x4 viewPrjMatrix)
        {
            m_PrevViewProjMat = viewPrjMatrix;
        }

        private Camera aaCamera
        {
            get
            {
                if (m_AACamera == null)
                    m_AACamera = GetComponent<Camera>();
                return m_AACamera;
            }
        }

        public void UpdateSampleIndex()
        {
            int numSamples = 1;
            if (temporalType == TemporalType.SMAA_2x || temporalType == TemporalType.Standard_2x)
            {
                numSamples = 2;
            }
            else if (temporalType == TemporalType.Standard_4x)
            {
                numSamples = 4;
            }
            else if (temporalType == TemporalType.Standard_8x)
            {
                numSamples = 8;
            }
            else if (temporalType == TemporalType.Standard_16x)
            {
                numSamples = 16;
            }

            m_SampleIndex = (m_SampleIndex + 1) % numSamples;
        }

        private Vector2 GetJitterStandard2X()
        {
            int[,] samples =
            {
                {4, 4},
                {-4, -4},
            };

            int sampleX = samples[m_SampleIndex, 0];
            int sampleY = samples[m_SampleIndex, 1];

            float v0 = sampleX / 16.0f;
            float v1 = sampleY / 16.0f;
            return new Vector2(v0, v1);
        }

        private Vector2 GetJitterStandard4X()
        {
            int[,] samples =
            {
                {-2, -6},
                {6, -2},
                {-6, 2},
                {2, 6}
            };

            int sampleX = samples[m_SampleIndex, 0];
            int sampleY = samples[m_SampleIndex, 1];

            float v0 = sampleX / 16.0f;
            float v1 = sampleY / 16.0f;
            return new Vector2(v0, v1);
        }

        private Vector2 GetJitterStandard8X()
        {
            int[,] samples =
            {
                {7, -7},
                {-3, -5},
                {3, 7},
                {-7, -1},
                {5, 1},
                {-1, 3},
                {1, -3},
                {-5, 5}
            };

            int sampleX = samples[m_SampleIndex, 0];
            int sampleY = samples[m_SampleIndex, 1];

            float v0 = sampleX / 16.0f;
            float v1 = sampleY / 16.0f;
            return new Vector2(v0, v1);
        }

        private Vector2 GetJitterStandard16X()
        {
            int[,] samples =
            {
                {7, -4},
                {-1, -3},
                {3, -5},
                {-5, -2},
                {6, 7},
                {-2, 6},
                {2, 5},
                {-6, -4},
                {4, -1},
                {-3, 2},
                {1, 1},
                {-8, 0},
                {5, 3},
                {-4, -6},
                {0, -7},
                {-7, -8}
            };

            int sampleX = samples[m_SampleIndex, 0];
            int sampleY = samples[m_SampleIndex, 1];

            float v0 = (sampleX + .5f) / 16.0f;
            float v1 = (sampleY + .5f) / 16.0f;
            return new Vector2(v0, v1);
        }

        private Vector2 GetJitterSMAAX2()
        {
            float jitterAmount = .25f;
            jitterAmount *= (m_SampleIndex == 0) ? -1.0f : 1.0f;

            //Debug.Log("Jitter");
            //jitterAmount = 0.0f;

            float v0 = jitterAmount;
            float v1 = -jitterAmount;
            return new Vector2(v0, v1);
        }

        private Vector2 GetCurrentJitter()
        {
            // add a quarter pixel diagonal translation
            Vector2 jitterOffset = new Vector2(0.0f, 0.0f);

            if (temporalType == TemporalType.SMAA_2x)
            {
                jitterOffset = GetJitterSMAAX2();
            }
            else if (temporalType == TemporalType.Standard_2x)
            {
                jitterOffset = GetJitterStandard2X();
            }
            else if (temporalType == TemporalType.Standard_4x)
            {
                jitterOffset = GetJitterStandard4X();
            }
            else if (temporalType == TemporalType.Standard_8x)
            {
                jitterOffset = GetJitterStandard8X();
            }
            else if (temporalType == TemporalType.Standard_16x)
            {
                jitterOffset = GetJitterStandard16X();
            }

            return jitterOffset;
        }

        private void OnPreCull()
        {
            StoreBaseProjectionMatrix(aaCamera.projectionMatrix);

            if (temporalType != TemporalType.Off)
            {
                // flip
                UpdateSampleIndex();

                Vector2 jitterOffset = GetCurrentJitter();

                Matrix4x4 offset = Matrix4x4.identity;

                offset.m03 = jitterOffset.x * 2.0f / aaCamera.pixelWidth;
                offset.m13 = jitterOffset.y * 2.0f / aaCamera.pixelHeight;

                var offsetMatrix = offset * m_BaseProjectionMatrix;
                aaCamera.projectionMatrix = offsetMatrix;
            }
        }

        private void OnPostRender()
        {
            aaCamera.ResetProjectionMatrix();
        }

        // usual & internal stuff
        public Shader smaaShader = null;
        private Material m_SmaaMaterial;

        public Material smaaMaterial
        {
            get
            {
                if (m_SmaaMaterial == null)
                    m_SmaaMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(smaaShader);

                return m_SmaaMaterial;
            }
        }

        // accumulation render texture
        private RenderTexture m_RtAccum;

        protected void OnEnable()
        {
            if (smaaShader == null)
                smaaShader = Shader.Find("Hidden/SMAA");

            if (!ImageEffectHelper.IsSupported(smaaShader, true, true, this))
            {
                enabled = false;
                Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
                return;
            }

            aaCamera.depthTextureMode |= DepthTextureMode.Depth;
        }

        private void OnDisable()
        {
            aaCamera.ResetProjectionMatrix();

            if (m_SmaaMaterial)
            {
                DestroyImmediate(m_SmaaMaterial);
                m_SmaaMaterial = null;
            }

            if (m_RtAccum)
            {
                DestroyImmediate(m_RtAccum);
                m_RtAccum = null;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (smaaMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            bool isFirst = false;

            // relying on short-circuit evaluation here
            if (m_RtAccum == null || (m_RtAccum.width != source.width || m_RtAccum.height != source.height))
            {
                if (m_RtAccum != null)
                    RenderTexture.ReleaseTemporary(m_RtAccum);

                m_RtAccum = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                m_RtAccum.hideFlags = HideFlags.DontSave;
                isFirst = true;
            }

            // the values for jitter offset are hardcoded based on the SMAA shader
            int actualJitterOffset = 0;
            if (temporalType == TemporalType.SMAA_2x)
                actualJitterOffset = m_SampleIndex < 1 ? 1 : 2;

            var sizeX = source.width;
            var sizeY = source.height;

            // should this always be RGBA8?
            const RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;

            var rtEdges = RenderTexture.GetTemporary(sizeX, sizeY, 0, rtFormat);
            var rtWeights = RenderTexture.GetTemporary(sizeX, sizeY, 0, rtFormat);

            // motion blur matrix
            var matrix = CalculateViewProjection(aaCamera, m_BaseProjectionMatrix);

            Matrix4x4 invViewPrj = Matrix4x4.Inverse(matrix);
            smaaMaterial.SetMatrix("_ToPrevViewProjCombined", m_PrevViewProjMat * invViewPrj);

            smaaMaterial.SetInt("_JitterOffset", actualJitterOffset);

            smaaMaterial.SetTexture("areaTex", areaTex);
            smaaMaterial.SetTexture("searchTex", searchTex);

            smaaMaterial.SetTexture("colorTex", source);

            smaaMaterial.SetVector("_PixelSize", new Vector4(1.0f / source.width, 1.0f / source.height, 0.0f, 0.0f));

            Vector2 pixelOffset = GetCurrentJitter();
            smaaMaterial.SetVector("_PixelOffset", new Vector4(pixelOffset.x / source.width, pixelOffset.y / source.height, 0.0f, 0.0f));

            smaaMaterial.SetTexture("edgesTex", rtEdges);
            smaaMaterial.SetTexture("blendTex", rtWeights);

            smaaMaterial.SetFloat("K", K);
            smaaMaterial.SetFloat("_TemporalAccum", temporalAccumulationWeight);

            // clear
            Graphics.Blit(source, rtEdges, smaaMaterial, (int)Passes.ClearToBlack);

            if (edgeType == EdgeType.Luminance)
            {
                // luma detect
                Graphics.Blit(source, rtEdges, smaaMaterial, (int)Passes.LumaDetection);
            }
            else if (edgeType == EdgeType.Color)
            {
                // color detect
                Graphics.Blit(source, rtEdges, smaaMaterial, (int)Passes.ColorDetection);
            }
            else
            {
                smaaMaterial.SetFloat("_DepthThreshold", 0.01f * depthThreshold);
                // depth detect
                Graphics.Blit(source, rtEdges, smaaMaterial, (int)Passes.DepthDetection);
            }

            // calculate weights
            Graphics.Blit(rtEdges, rtWeights, smaaMaterial, (int)Passes.WeightCalculation);

            if (temporalType == TemporalType.Off)
            {
                Graphics.Blit(source, destination, smaaMaterial, (int)Passes.WeightsAndBlend1);
            }
            else
            {
                // temporal blending
                RenderTexture rtTemp = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                // render for this frame
                if (temporalType == TemporalType.SMAA_2x)
                {
                    // set the accumulation texture
                    smaaMaterial.SetTexture("accumTex", m_RtAccum);

                    if (isFirst)
                    {
                        // if we are the first frame, just copy
                        Graphics.Blit(source, rtTemp, smaaMaterial, (int)Passes.WeightsAndBlend1);
                    }
                    else
                    {
                        // if not first, then blend with accumulation
                        Graphics.Blit(source, rtTemp, smaaMaterial, (int)Passes.WeightsAndBlend2);
                    }

                    // copy to accumulation
                    Graphics.Blit(rtTemp, m_RtAccum, smaaMaterial, (int)Passes.Copy);

                    // copy to destination
                    Graphics.Blit(rtTemp, destination, smaaMaterial, (int)Passes.Copy);
                }
                else
                {
                    // solve SMAA as 1x
                    Graphics.Blit(source, rtTemp, smaaMaterial, 4);

                    if (isFirst)
                    {
                        Graphics.Blit(rtTemp, m_RtAccum, smaaMaterial, 0);
                    }

                    // set the accumulation texture
                    smaaMaterial.SetTexture("accumTex", m_RtAccum);
                    smaaMaterial.SetTexture("smaaTex", rtTemp);

                    rtTemp.filterMode = FilterMode.Bilinear;

                    RenderTexture rtTemp2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                    // copy to accumulation
                    Graphics.Blit(rtTemp, rtTemp2, smaaMaterial, (int)Passes.MergeFrames);

                    // copy to accumulation
                    Graphics.Blit(rtTemp2, m_RtAccum, smaaMaterial, (int)Passes.Copy);

                    // copy to destination
                    Graphics.Blit(rtTemp2, destination, smaaMaterial, (int)Passes.Copy);

                    RenderTexture.ReleaseTemporary(rtTemp2);
                }

                RenderTexture.ReleaseTemporary(rtTemp);
            }

            if (displayType == DebugDisplay.Edges)
            {
                // copy to accumulation
                Graphics.Blit(rtEdges, destination, smaaMaterial, (int)Passes.Copy);
            }
            else if (displayType == DebugDisplay.Weights)
            {
                // copy to accumulation
                Graphics.Blit(rtWeights, destination, smaaMaterial, (int)Passes.Copy);
            }
            else if (displayType == DebugDisplay.Depth)
            {
                Graphics.Blit(null, destination, smaaMaterial, (int)Passes.DebugDepth);
            }
            else if (displayType == DebugDisplay.Accumulation)
            {
                Graphics.Blit(m_RtAccum, destination);
            }

            RenderTexture.ReleaseTemporary(rtEdges);
            RenderTexture.ReleaseTemporary(rtWeights);

            // store matrix for next frame
            StorePreviousViewProjMatrix(matrix);
        }
    }
}
