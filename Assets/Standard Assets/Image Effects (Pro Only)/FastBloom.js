
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Bloom and Glow/Bloom (Optimized)")

class FastBloom extends PostEffectsBase {

	public enum Resolution {
		Low = 0,
		High = 1,
	}

	public enum BlurType {
		Standard = 0,
		Sgx = 1,
	}

	@Range(0.0f, 1.5f)
	public var threshhold : float = 0.25f;
	@Range(0.0f, 2.5f)
	public var intensity : float = 0.75f;

	@Range(0.25f, 5.5f)
	public var blurSize : float = 1.0f;
	
	var resolution : Resolution = Resolution.Low;
	@Range(1, 4)
	public var blurIterations : int = 1;

	public var blurType = BlurType.Standard;

	public var fastBloomShader : Shader;
	private var fastBloomMaterial : Material = null;
	
	function CheckResources () : boolean {	
		CheckSupport (false);	
	
		fastBloomMaterial = CheckShaderAndCreateMaterial (fastBloomShader, fastBloomMaterial);
		
		if(!isSupported)
			ReportAutoDisable ();
		return isSupported;				
	}

	function OnDisable() {
		if(fastBloomMaterial)
			DestroyImmediate (fastBloomMaterial);
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	
		if(CheckResources() == false) {
			Graphics.Blit (source, destination);
			return;
		}

		var divider : int = resolution == Resolution.Low ? 4 : 2;
		var widthMod : float = resolution == Resolution.Low ? 0.5f : 1.0f;

		fastBloomMaterial.SetVector ("_Parameter", Vector4 (blurSize * widthMod, 0.0f, threshhold, intensity));
		source.filterMode = FilterMode.Bilinear;

		var rtW = source.width/divider;
		var rtH = source.height/divider;

		// downsample
		var rt : RenderTexture = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
		rt.filterMode = FilterMode.Bilinear;
		Graphics.Blit (source, rt, fastBloomMaterial, 1);

		var passOffs = blurType == BlurType.Standard ? 0 : 2;
		
		for(var i : int = 0; i < blurIterations; i++) {
			fastBloomMaterial.SetVector ("_Parameter", Vector4 (blurSize * widthMod + (i*1.0f), 0.0f, threshhold, intensity));

			// vertical blur
			var rt2 : RenderTexture = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
			rt2.filterMode = FilterMode.Bilinear;
			Graphics.Blit (rt, rt2, fastBloomMaterial, 2 + passOffs);
			RenderTexture.ReleaseTemporary (rt);
			rt = rt2;

			// horizontal blur
			rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
			rt2.filterMode = FilterMode.Bilinear;
			Graphics.Blit (rt, rt2, fastBloomMaterial, 3 + passOffs);
			RenderTexture.ReleaseTemporary (rt);
			rt = rt2;
		}
		
		fastBloomMaterial.SetTexture ("_Bloom", rt);

		Graphics.Blit (source, destination, fastBloomMaterial, 0);

		RenderTexture.ReleaseTemporary (rt);
	}	
}
