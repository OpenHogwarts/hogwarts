using UnityEngine;
using System;

namespace UnityStandardAssets.CinematicEffects
{
    //Improvement ideas:
    //  In hdr do local tonemapping/inverse tonemapping to stabilize bokeh.
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
    [AddComponentMenu("Image Effects/Other/DepthOfField")]
    [RequireComponent(typeof(Camera))]
    public class DepthOfField : MonoBehaviour
    {
        [AttributeUsage(AttributeTargets.Field)]
        public sealed class GradientRangeAttribute : PropertyAttribute
        {
            public readonly float max;
            public readonly float min;

            // Attribute used to make a float or int variable in a script be restricted to a specific range.
            public GradientRangeAttribute(float min, float max)
            {
                this.min = min;
                this.max = max;
            }
        }

        const float kMaxBlur = 35.0f;

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

        public enum MedianPasses
        {
            Median3 = 0,
            Median3X3 = 1
        }

        public enum BokehTexturesPasses
        {
            Apply = 0,
            Collect = 1
        }

        public enum UIMode
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
        public enum FilterQuality
        {
            None,
            Normal,
            High
        }

        [Tooltip("Allow to view where the blur will be applied. Yellow for near blur, Blue for far blur.")]
        public bool visualizeBluriness  = false;

        [Tooltip("When enabled quality settings can be hand picked, rather than being driven by the quality slider.")]
        public bool customizeQualitySettings = false;

        public bool  prefilterBlur = true;
        public FilterQuality medianFilter = FilterQuality.High;
        public bool  dilateNearBlur = true;
        public bool  highQualityUpsampling = true;

        [GradientRange(0.0f, 100.0f)]
        [Tooltip("Color represent relative performance. From green (faster) to yellow (slower).")]
        public float quality = 100.0f;

        [Range(0.0f, 1.0f)]
        public float focusPlane  = 0.225f;
        [Range(0.0f, 1.0f)]
        public float focusRange = 0.9f;
        [Range(0.0f, 1.0f)]
        public float nearPlane = 0.0f;
        [Range(0.0f, kMaxBlur)]
        public float nearRadius = 20.0f;
        [Range(0.0f, 1.0f)]
        public float farPlane  = 1.0f;
        [Range(0.0f, kMaxBlur)]
        public float farRadius  = 20.0f;
        [Range(0.0f, kMaxBlur)]
        public float radius = 20.0f;
        [Range(0.5f, 4.0f)]
        public float boostPoint  = 0.75f;
        [Range(0.0f, 1.0f)]
        public float nearBoostAmount  = 0.0f;
        [Range(0.0f, 1.0f)]
        public float farBoostAmount  = 0.0f;
        [Range(0.0f, 32.0f)]
        public float fStops  = 5.0f;

        [Range(0.01f, 5.0f)]
        public float textureBokehScale = 1.0f;
        [Range(0.01f, 100.0f)]
        public float textureBokehIntensity = 50.0f;
        [Range(0.01f, 50.0f)]
        public float textureBokehThreshold = 2.0f;
        [Range(0.01f, 1.0f)]
        public float textureBokehSpawnHeuristic = 0.15f;

        public Transform focusTransform = null;
        public Texture2D bokehTexture = null;
        public ApertureShape apertureShape = ApertureShape.Circular;
        [Range(0.0f, 179.0f)]
        public float apertureOrientation = 0.0f;

        [Tooltip("Use with care Bokeh texture are only available on shader model 5, and performance scale with the number of bokehs.")]
        public bool useBokehTexture;

        public UIMode uiMode = UIMode.Basic;

        public Shader filmicDepthOfFieldShader;
        public Shader medianFilterShader;
        public Shader textureBokehShader;

        [NonSerialized]
        private RenderTexureUtility m_RTU = new RenderTexureUtility();

        public Material filmicDepthOfFieldMaterial
        {
            get
            {
                if (m_FilmicDepthOfFieldMaterial == null)
                    m_FilmicDepthOfFieldMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(filmicDepthOfFieldShader);

                return m_FilmicDepthOfFieldMaterial;
            }
        }

        public Material medianFilterMaterial
        {
            get
            {
                if (m_MedianFilterMaterial == null)
                    m_MedianFilterMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(medianFilterShader);

                return m_MedianFilterMaterial;
            }
        }

        public Material textureBokehMaterial
        {
            get
            {
                if (m_TextureBokehMaterial == null)
                    m_TextureBokehMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(textureBokehShader);

                return m_TextureBokehMaterial;
            }
        }

        public ComputeBuffer computeBufferDrawArgs
        {
            get
            {
                if (m_ComputeBufferDrawArgs == null)
                {
                    m_ComputeBufferDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
                    var args = new int[4];
                    args[0] = 0;
                    args[1] = 1;
                    args[2] = 0;
                    args[3] = 0;
                    m_ComputeBufferDrawArgs.SetData(args);
                }
                return m_ComputeBufferDrawArgs;
            }
        }

        public ComputeBuffer computeBufferPoints
        {
            get
            {
                if (m_ComputeBufferPoints == null)
                {
                    m_ComputeBufferPoints = new ComputeBuffer(90000, 12 + 16, ComputeBufferType.Append);
                }
                return m_ComputeBufferPoints;
            }
        }

        private ComputeBuffer m_ComputeBufferDrawArgs;
        private ComputeBuffer m_ComputeBufferPoints;
        private Material m_FilmicDepthOfFieldMaterial;
        private Material m_MedianFilterMaterial;
        private Material m_TextureBokehMaterial;
        private float m_LastApertureOrientation;
        private Vector4 m_OctogonalBokehDirection1;
        private Vector4 m_OctogonalBokehDirection2;
        private Vector4 m_OctogonalBokehDirection3;
        private Vector4 m_OctogonalBokehDirection4;
        private Vector4 m_HexagonalBokehDirection1;
        private Vector4 m_HexagonalBokehDirection2;
        private Vector4 m_HexagonalBokehDirection3;

        protected void OnEnable()
        {
            if (filmicDepthOfFieldShader == null)
                filmicDepthOfFieldShader = Shader.Find("Hidden/DepthOfField/DepthOfField");

            if (medianFilterShader == null)
                medianFilterShader = Shader.Find("Hidden/DepthOfField/MedianFilter");

            if (textureBokehShader == null)
                textureBokehShader = Shader.Find("Hidden/DepthOfField/BokehSplatting");

            if (!ImageEffectHelper.IsSupported(filmicDepthOfFieldShader, true, true, this)
                || !ImageEffectHelper.IsSupported(medianFilterShader, true, true, this)
                )
            {
                enabled = false;
                Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
                return;
            }

            if (ImageEffectHelper.supportsDX11)
            {
                if (!ImageEffectHelper.IsSupported(textureBokehShader, true, true, this))
                {
                    enabled = false;
                    Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
                    return;
                }
            }

            ComputeBlurDirections(true);
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        protected void OnDisable()
        {
            ReleaseComputeResources();

            if (m_FilmicDepthOfFieldMaterial)
                DestroyImmediate(m_FilmicDepthOfFieldMaterial);
            if (m_TextureBokehMaterial)
                DestroyImmediate(m_TextureBokehMaterial);
            if (m_MedianFilterMaterial)
                DestroyImmediate(m_MedianFilterMaterial);

            m_TextureBokehMaterial = null;
            m_FilmicDepthOfFieldMaterial = null;
            m_MedianFilterMaterial = null;

            m_RTU.ReleaseAllTemporyRenderTexutres();
        }

        //-------------------------------------------------------------------//
        // Main entry point                                                  //
        //-------------------------------------------------------------------//
        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (medianFilterMaterial == null || filmicDepthOfFieldMaterial == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (visualizeBluriness)
            {
                Vector4 blurrinessParam;
                Vector4 blurrinessCoe;
                ComputeCocParameters(out blurrinessParam, out blurrinessCoe);
                filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurrinessParam);
                filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurrinessCoe);
                Graphics.Blit(null, destination, filmicDepthOfFieldMaterial, (uiMode == UIMode.Explicit) ? (int)Passes.VisualizeCocExplicit : (int)Passes.VisualizeCoc);
            }
            else
            {
                DoDepthOfField(source, destination);
            }

            m_RTU.ReleaseAllTemporyRenderTexutres();
        }

        private void DoDepthOfField(RenderTexture source, RenderTexture destination)
        {
            float radiusAdjustement = source.height / 720.0f;

            float textureBokehScale = radiusAdjustement;
            float textureBokehMaxRadius = Mathf.Max(nearRadius, farRadius) * textureBokehScale * 0.75f;

            float nearBlurRadius = nearRadius * radiusAdjustement;
            float farBlurRadius = farRadius * radiusAdjustement;
            float maxBlurRadius = Mathf.Max(nearBlurRadius, farBlurRadius);
            switch (apertureShape)
            {
                case ApertureShape.Hexagonal: maxBlurRadius *= 1.2f; break;
                case ApertureShape.Octogonal: maxBlurRadius *= 1.15f; break;
            }

            if (maxBlurRadius < 0.5f)
            {
                Graphics.Blit(source, destination);
                return;
            }

            //Quarter resolution
            int rtW = source.width / 2;
            int rtH = source.height / 2;
            Vector4 blurrinessCoe = new Vector4(nearBlurRadius * 0.5f, farBlurRadius * 0.5f, 0.0f, 0.0f);
            RenderTexture colorAndCoc  = m_RTU.GetTemporaryRenderTexture(rtW, rtH);
            RenderTexture colorAndCoc2 = m_RTU.GetTemporaryRenderTexture(rtW, rtH);


            // Downsample to Color + COC buffer and apply boost
            Vector4 cocParam;
            Vector4 cocCoe;
            ComputeCocParameters(out cocParam, out cocCoe);
            filmicDepthOfFieldMaterial.SetVector("_BlurParams", cocParam);
            filmicDepthOfFieldMaterial.SetVector("_BlurCoe", cocCoe);
            filmicDepthOfFieldMaterial.SetVector("_BoostParams", new Vector4(nearBlurRadius * nearBoostAmount * -0.5f, farBlurRadius * farBoostAmount * 0.5f, boostPoint, 0.0f));
            Graphics.Blit(source, colorAndCoc2, filmicDepthOfFieldMaterial, (uiMode == UIMode.Explicit) ? (int)Passes.CaptureCocExplicit : (int)Passes.CaptureCoc);
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
                textureBokehMaterial.SetFloat("_SpawnHeuristic", textureBokehSpawnHeuristic);
                textureBokehMaterial.SetVector("_BokehParams", new Vector4(this.textureBokehScale * textureBokehScale, textureBokehIntensity, textureBokehThreshold, textureBokehMaxRadius));
                Graphics.SetRandomWriteTarget(1, computeBufferPoints);
                Graphics.Blit(src, dst, textureBokehMaterial, (int)BokehTexturesPasses.Collect);
                Graphics.ClearRandomWriteTargets();
                SwapRenderTexture(ref src, ref dst);
                m_RTU.ReleaseTemporaryRenderTexture(blurred);
            }

            filmicDepthOfFieldMaterial.SetVector("_BlurParams", cocParam);
            filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurrinessCoe);
            filmicDepthOfFieldMaterial.SetVector("_BoostParams", new Vector4(nearBlurRadius * nearBoostAmount * -0.5f, farBlurRadius * farBoostAmount * 0.5f, boostPoint, 0.0f));

            // Dilate near blur factor
            RenderTexture blurredFgCoc = null;
            if (dilateNearBlur)
            {
                RenderTexture blurredFgCoc2 = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, RenderTextureFormat.RGHalf);
                blurredFgCoc = m_RTU.GetTemporaryRenderTexture(rtW, rtH, 0, RenderTextureFormat.RGHalf);
                filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0.0f, nearBlurRadius * 0.75f, 0.0f, 0.0f));
                Graphics.Blit(src, blurredFgCoc2, filmicDepthOfFieldMaterial, (int)Passes.DilateFgCocFromColor);
                filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(nearBlurRadius * 0.75f, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(blurredFgCoc2, blurredFgCoc, filmicDepthOfFieldMaterial, (int)Passes.DilateFgCoc);
                m_RTU.ReleaseTemporaryRenderTexture(blurredFgCoc2);
            }

            // Blur downsampled color to fill the gap between samples
            if (prefilterBlur)
            {
                Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, (int)Passes.CocPrefilter);
                SwapRenderTexture(ref src, ref dst);
            }

            // Apply blur : Circle / Hexagonal or Octagonal (blur will create bokeh if bright pixel where not removed by "m_UseBokehTexture")
            switch (apertureShape)
            {
                case ApertureShape.Circular: DoCircularBlur(blurredFgCoc, ref src, ref dst, maxBlurRadius); break;
                case ApertureShape.Hexagonal: DoHexagonalBlur(blurredFgCoc, ref src, ref dst, maxBlurRadius); break;
                case ApertureShape.Octogonal: DoOctogonalBlur(blurredFgCoc, ref src, ref dst, maxBlurRadius); break;
            }

            // Smooth result
            switch (medianFilter)
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
            int mergePass = (uiMode == UIMode.Explicit) ? (int)Passes.MergeExplicit : (int)Passes.Merge;
            if (highQualityUpsampling)
            {
                mergePass = (uiMode == UIMode.Explicit) ? (int)Passes.MergeExplicitBicubic : (int)Passes.MergeBicubic;
            }

            // Apply texture bokeh
            if (shouldPerformBokeh)
            {
                RenderTexture tmp = m_RTU.GetTemporaryRenderTexture(source.height, source.width, 0, source.format);
                Graphics.Blit(source, tmp, filmicDepthOfFieldMaterial, mergePass);

                Graphics.SetRenderTarget(tmp);
                ComputeBuffer.CopyCount(computeBufferPoints, computeBufferDrawArgs, 0);
                textureBokehMaterial.SetBuffer("pointBuffer", computeBufferPoints);
                textureBokehMaterial.SetTexture("_MainTex", bokehTexture);
                textureBokehMaterial.SetVector("_Screen", new Vector3(1.0f / (1.0f * source.width), 1.0f / (1.0f * source.height), textureBokehMaxRadius));
                textureBokehMaterial.SetPass((int)BokehTexturesPasses.Apply);
                Graphics.DrawProceduralIndirect(MeshTopology.Points, computeBufferDrawArgs, 0);
                Graphics.Blit(tmp, destination);// hackaround for DX11 flipfun (OPTIMIZEME)
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
            float focusDistance01 = focusTransform ? (sceneCamera.WorldToViewportPoint(focusTransform.position)).z / (sceneCamera.farClipPlane) : (focusPlane * focusPlane * focusPlane * focusPlane);

            if (uiMode == UIMode.Basic || uiMode == UIMode.Advanced)
            {
                float focusRange01 = focusRange * focusRange * focusRange * focusRange;
                float focalLength = 4.0f / Mathf.Tan(0.5f * sceneCamera.fieldOfView * Mathf.Deg2Rad);
                float aperture = focalLength / fStops;
                blurCoe = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                blurParams = new Vector4(aperture, focalLength, focusDistance01, focusRange01);
            }
            else
            {
                float nearDistance01 = nearPlane * nearPlane * nearPlane * nearPlane;
                float farDistance01 = farPlane * farPlane * farPlane * farPlane;
                float nearFocusRange01 = focusRange * focusRange * focusRange * focusRange;
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
                float a2 = 1.0f / (farDistance01  - focusDistance01 - farFocusRange01);
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
            m_ComputeBufferDrawArgs = null;
            if (m_ComputeBufferPoints != null)
                m_ComputeBufferPoints.Release();
            m_ComputeBufferPoints = null;
        }

        private void ComputeBlurDirections(bool force)
        {
            if (!force && Math.Abs(m_LastApertureOrientation - apertureOrientation) < float.Epsilon) return;

            m_LastApertureOrientation = apertureOrientation;

            float rotationRadian = apertureOrientation * Mathf.Deg2Rad;
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
            get { return ImageEffectHelper.supportsDX11 && useBokehTexture && textureBokehMaterial; }
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
