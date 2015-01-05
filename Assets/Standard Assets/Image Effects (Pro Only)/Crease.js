
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Edge Detection/Crease Shading")

class Crease extends PostEffectsBase {
	public var intensity : float = 0.5;
	public var softness : int = 1;
	public var spread : float = 1.0;
	
	public var blurShader : Shader;
	private var blurMaterial : Material = null;	
	
	public var depthFetchShader : Shader;
	private var depthFetchMaterial : Material = null;
	
	public var creaseApplyShader : Shader;
	private var creaseApplyMaterial : Material = null;	
	
	function CheckResources () : boolean {	
		CheckSupport (true);
		
		blurMaterial = CheckShaderAndCreateMaterial (blurShader, blurMaterial);
		depthFetchMaterial = CheckShaderAndCreateMaterial (depthFetchShader, depthFetchMaterial);
		creaseApplyMaterial = CheckShaderAndCreateMaterial (creaseApplyShader, creaseApplyMaterial);
		
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
		
		var widthOverHeight : float = (1.0f * rtW) / (1.0f * rtH);
		var oneOverBaseSize : float = 1.0f / 512.0f;		

		var hrTex : RenderTexture = RenderTexture.GetTemporary (rtW, rtH, 0); 
		var lrTex1 : RenderTexture = RenderTexture.GetTemporary (rtW/2, rtH/2, 0); 
		
		Graphics.Blit (source,hrTex, depthFetchMaterial);
		Graphics.Blit (hrTex, lrTex1);
		
		for(var i : int = 0; i < softness; i++) {
			var lrTex2 : RenderTexture = RenderTexture.GetTemporary (rtW/2, rtH/2, 0);
			blurMaterial.SetVector ("offsets", Vector4 (0.0, spread * oneOverBaseSize, 0.0, 0.0));
			Graphics.Blit (lrTex1, lrTex2, blurMaterial);
			RenderTexture.ReleaseTemporary (lrTex1);
			lrTex1 = lrTex2;

			lrTex2 = RenderTexture.GetTemporary (rtW/2, rtH/2, 0);
			blurMaterial.SetVector ("offsets", Vector4 (spread * oneOverBaseSize / widthOverHeight,  0.0, 0.0, 0.0));		
			Graphics.Blit (lrTex1, lrTex2, blurMaterial);
			RenderTexture.ReleaseTemporary (lrTex1);
			lrTex1 = lrTex2;
		}
		
		creaseApplyMaterial.SetTexture ("_HrDepthTex", hrTex);
		creaseApplyMaterial.SetTexture ("_LrDepthTex", lrTex1);
		creaseApplyMaterial.SetFloat ("intensity", intensity);
		Graphics.Blit (source,destination, creaseApplyMaterial);

		RenderTexture.ReleaseTemporary (hrTex);
		RenderTexture.ReleaseTemporary (lrTex1);
	}	
}
