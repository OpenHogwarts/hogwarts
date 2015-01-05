
#pragma strict

@script ExecuteInEditMode
@script RequireComponent(Camera)
@script AddComponentMenu("Image Effects/Color Adjustments/Contrast Enhance (Unsharp Mask)")

class ContrastEnhance extends PostEffectsBase {
	public var intensity : float = 0.5;
	public var threshhold : float = 0.0;
	
	private var separableBlurMaterial : Material;
	private var contrastCompositeMaterial : Material;
	
	public var blurSpread : float = 1.0;
	
	public var separableBlurShader : Shader = null;
	public var contrastCompositeShader : Shader = null;

	function CheckResources () : boolean {	
		CheckSupport (false);
		
		contrastCompositeMaterial = CheckShaderAndCreateMaterial (contrastCompositeShader, contrastCompositeMaterial);
		separableBlurMaterial = CheckShaderAndCreateMaterial (separableBlurShader, separableBlurMaterial);
		
		if(!isSupported)
			ReportAutoDisable ();
		return isSupported;		
	}
	
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	
		if(CheckResources()==false) {
			Graphics.Blit (source, destination);
			return;
		}

		var rtW : int = source.width;
		var rtH : int = source.height;
				
		var color2 : RenderTexture = RenderTexture.GetTemporary (rtW/2, rtH/2, 0);	
			
		// downsample

		Graphics.Blit (source, color2);
		var color4a : RenderTexture = RenderTexture.GetTemporary (rtW/4, rtH/4, 0);
		Graphics.Blit (color2, color4a); 
		RenderTexture.ReleaseTemporary (color2);
	
		// blur
		
		separableBlurMaterial.SetVector ("offsets", Vector4 (0.0, (blurSpread * 1.0) / color4a.height, 0.0, 0.0));	
		var color4b : RenderTexture = RenderTexture.GetTemporary (rtW/4, rtH/4, 0);
		Graphics.Blit (color4a, color4b, separableBlurMaterial);
		RenderTexture.ReleaseTemporary (color4a);

		separableBlurMaterial.SetVector ("offsets", Vector4 ((blurSpread * 1.0) / color4a.width, 0.0, 0.0, 0.0));	
		color4a = RenderTexture.GetTemporary (rtW/4, rtH/4, 0);
		Graphics.Blit (color4b, color4a, separableBlurMaterial); 
		RenderTexture.ReleaseTemporary (color4b);
	
		// composite

		contrastCompositeMaterial.SetTexture ("_MainTexBlurred", color4a);
		contrastCompositeMaterial.SetFloat ("intensity", intensity);
		contrastCompositeMaterial.SetFloat ("threshhold", threshhold);
		Graphics.Blit (source, destination, contrastCompositeMaterial); 
		
		RenderTexture.ReleaseTemporary (color4a);
	}
}