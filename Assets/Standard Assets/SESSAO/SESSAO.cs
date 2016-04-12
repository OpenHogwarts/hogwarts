using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Sonic Ether/SESSAO")]
public class SESSAO : MonoBehaviour
{
	private Material material;
	public bool visualizeSSAO;
	private Texture2D ditherTexture;
	private Texture2D ditherTextureSmall;
	private bool skipThisFrame = false;
	
	[Range(0.02f, 5.0f)]
	public float radius = 1.0f;
	[Range(-0.2f, 0.5f)]
	public float bias = 0.1f;
	[Range(0.1f, 3.0f)]
	public float bilateralDepthTolerance = 0.2f;
	[Range(1.0f, 5.0f)]
	public float zThickness = 2.35f;
	[Range(0.5f, 5.0f)]
	public float occlusionIntensity = 1.3f;
	[Range(1.0f, 6.0f)]	
	public float sampleDistributionCurve = 1.15f;
	[Range(0.0f, 1.0f)]
	public float colorBleedAmount = 1.0f;
	[Range(0.1f, 3.0f)]
	public float brightnessThreshold;
	public float drawDistance = 500.0f;
	public float drawDistanceFadeSize = 1.0f;
	
	public bool reduceSelfBleeding = true;
	public bool useDownsampling = false;
	public bool halfSampling = false;
	public bool preserveDetails = false;
	
	[HideInInspector]
	public Camera attachedCamera;
	
	private object initChecker = null;
	
	void CheckInit()
	{
		if (initChecker == null)
		{
			Init();
		}
	}
	
	void Init()
	{
		skipThisFrame = false;
		Shader shader = Shader.Find("Hidden/SESSAO");
		if (!shader)
		{
			skipThisFrame = true;
			return;
		}
		material = new Material(shader);
		
		attachedCamera = this.GetComponent<Camera>();
		attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
		attachedCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
		SetupDitherTexture();
		SetupDitherTextureSmall();
		initChecker = new object();
	}
	
	void Cleanup()
	{
		DestroyImmediate(material);
		initChecker = null;
	}
	
	void SetupDitherTextureSmall()
	{
		ditherTextureSmall = new Texture2D(3, 3, TextureFormat.Alpha8, false);
		ditherTextureSmall.filterMode = FilterMode.Point;
		
		float[] ditherPattern = new float[9]
		//		{9, 2, 7,
		//		 4, 5, 6,
		//		 3, 8, 1};
		{8, 1, 6,
			3, 0, 4,
			7, 2, 5};
		
		for (int i = 0; i < 9; i++)
		{
			Color pixelColor = new Color(0f, 0f, 0f, ditherPattern [i] / 9.0f);
			
			int xCoord = i % 3;
			int yCoord = Mathf.FloorToInt((float)i / 3.0f);
			
			ditherTextureSmall.SetPixel(xCoord, yCoord, pixelColor);
		}
		
		ditherTextureSmall.Apply();
		
		ditherTextureSmall.hideFlags = HideFlags.HideAndDontSave;
	}
	
	void SetupDitherTexture()
	{
		ditherTexture = new Texture2D(5, 5, TextureFormat.Alpha8, false);
		ditherTexture.filterMode = FilterMode.Point;
		
		float[] ditherPattern = new float[25]
		{12.0f, 1.0f,  10.0f, 3.0f,  20.0f, 
			5.0f,  18.0f, 7.0f,  16.0f, 9.0f, 
			24.0f, 2.0f,  11.0f, 6.0f,  22.0f, 
			15.0f, 8.0f,  0.0f,  13.0f, 19.0f, 
			4.0f,  21.0f, 14.0f, 23.0f, 17.0f};
		
		for (int i = 0; i < 25; i++)
		{
			Color pixelColor = new Color(0f, 0f, 0f, ditherPattern [i] / 25.0f);
			
			int xCoord = i % 5;
			int yCoord = Mathf.FloorToInt((float)i / 5.0f);
			
			ditherTexture.SetPixel(xCoord, yCoord, pixelColor);
		}
		
		ditherTexture.Apply();
		
		ditherTexture.hideFlags = HideFlags.HideAndDontSave;
	}
	
	void Start()
	{
		CheckInit();
	}
	
	void OnEnable()
	{
		CheckInit();		
	}
	
	void OnDisable()
	{
		Cleanup();
	}
	
	void Update()
	{
		drawDistance = Mathf.Max(0.0f, drawDistance);
		drawDistanceFadeSize = Mathf.Max(0.001f, drawDistanceFadeSize);
		bilateralDepthTolerance = Mathf.Max(0.000001f, bilateralDepthTolerance);
	}
	
	[ImageEffectOpaque]
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CheckInit();		
		
		if (skipThisFrame)
		{
			Graphics.Blit(source, destination);
			return;
		}
		
		material.hideFlags = HideFlags.HideAndDontSave;
		
		material.SetTexture("_DitherTexture", preserveDetails ? ditherTextureSmall : ditherTexture);
		material.SetInt("PreserveDetails", preserveDetails ? 1 : 0);
		
		material.SetMatrix("ProjectionMatrixInverse", GetComponent<Camera>().projectionMatrix.inverse);
		
		RenderTexture ssao1 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
		RenderTexture ssao2 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
		
		RenderTexture colorDownsampled1 = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, source.format);
		colorDownsampled1.wrapMode = TextureWrapMode.Clamp;
		colorDownsampled1.filterMode = FilterMode.Bilinear;
		Graphics.Blit(source, colorDownsampled1);
		
		material.SetTexture("_ColorDownsampled", colorDownsampled1);
		
		RenderTexture ssaoDownsampled = null;
		
		material.SetFloat("Radius", radius);
		material.SetFloat("Bias", bias);
		material.SetFloat("DepthTolerance", bilateralDepthTolerance);
		material.SetFloat("ZThickness", zThickness);
		material.SetFloat("Intensity", occlusionIntensity);
		material.SetFloat("SampleDistributionCurve", sampleDistributionCurve);
		material.SetFloat("ColorBleedAmount", colorBleedAmount);
		material.SetFloat("DrawDistance", drawDistance);
		material.SetFloat("DrawDistanceFadeSize", drawDistanceFadeSize);
		material.SetFloat("SelfBleedReduction", reduceSelfBleeding ? 1.0f : 0.0f);
		material.SetFloat("BrightnessThreshold", brightnessThreshold);
		material.SetInt("HalfSampling", halfSampling ? 1 : 0);
		material.SetInt("Orthographic", attachedCamera.orthographic ? 1 : 0);
		
		
		if (useDownsampling)
		{
			ssaoDownsampled = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, RenderTextureFormat.ARGBHalf);
			ssaoDownsampled.filterMode = FilterMode.Bilinear;
			material.SetInt("Downsamp", 1);
			Graphics.Blit(source, ssaoDownsampled, material, colorBleedAmount <= 0.0001f ? 1 : 0);
		} 
		else
		{
			material.SetInt("Downsamp", 0);			
			Graphics.Blit(source, ssao1, material, colorBleedAmount <= 0.0001f ? 1 : 0);			
		}
		
		RenderTexture.ReleaseTemporary(colorDownsampled1);
		
		material.SetFloat("BlurDepthTolerance", 0.1f);
		
		int bilateralBlurPass = attachedCamera.orthographic ? 6 : 2;
		
		if (attachedCamera.orthographic)
		{
			material.SetFloat("Near", attachedCamera.nearClipPlane);
			material.SetFloat("Far", attachedCamera.farClipPlane);
		}
		
		if (useDownsampling) 
		{
			material.SetVector("Kernel", new Vector2(2.0f, 0.0f));
			Graphics.Blit(ssaoDownsampled, ssao2, material, bilateralBlurPass);
			
			RenderTexture.ReleaseTemporary(ssaoDownsampled);
			
			material.SetVector("Kernel", new Vector2(0.0f, 2.0f));
			Graphics.Blit(ssao2, ssao1, material, bilateralBlurPass);
			
			material.SetVector("Kernel", new Vector2(2.0f, 0.0f));
			Graphics.Blit(ssao1, ssao2, material, bilateralBlurPass);
			
			material.SetVector("Kernel", new Vector2(0.0f, 2.0f));
			Graphics.Blit(ssao2, ssao1, material, bilateralBlurPass);
		}
		else
		{
			material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
			Graphics.Blit(ssao1, ssao2, material, bilateralBlurPass);
			
			material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
			Graphics.Blit(ssao2, ssao1, material, bilateralBlurPass);
			
			material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
			Graphics.Blit(ssao1, ssao2, material, bilateralBlurPass);
			
			material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
			Graphics.Blit(ssao2, ssao1, material, bilateralBlurPass);
		}
		
		RenderTexture.ReleaseTemporary(ssao2);
		
		material.SetTexture("_SSAO", ssao1);
		
		if (!visualizeSSAO)
		{
			Graphics.Blit(source, destination, material, 3);
		} else
		{
			Graphics.Blit(source, destination, material, 5);
		}
		RenderTexture.ReleaseTemporary(ssao1);
	}
}
