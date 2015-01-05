
#pragma strict

@script ExecuteInEditMode
@script RequireComponent (Camera)
@script AddComponentMenu ("Image Effects/Rendering/Screen Space Ambient Obscurance")

class AmbientObscurance extends PostEffectsBase {
	@Range (0,3)	
	public var intensity : float = 0.5;
	@Range (0.1,3)	
	public var radius : float = 0.2;
	@Range (0,3)
	public var blurIterations : int = 1;
	@Range (0,5)
	public var blurFilterDistance : float = 1.25;
	@Range (0,1)
	public var downsample : int = 0;

	public var rand : Texture2D;
	public var aoShader : Shader;

	private var aoMaterial : Material = null;	

	function CheckResources () : boolean {	
		CheckSupport (true);
		
		aoMaterial = CheckShaderAndCreateMaterial (aoShader, aoMaterial);
		
		if (!isSupported)
			ReportAutoDisable ();
		return isSupported;			
	}

	function OnDisable () {
		if(aoMaterial)
			DestroyImmediate (aoMaterial);
		aoMaterial = null;
	}
	
	@ImageEffectOpaque
	function OnRenderImage (source : RenderTexture, destination : RenderTexture) {	
		if (CheckResources () == false) {
			Graphics.Blit (source, destination);
			return;
		}

		var P : Matrix4x4 = camera.projectionMatrix;
		var invP = P.inverse;
		var projInfo : Vector4 = new Vector4
	        ((-2.0f / (Screen.width * P[0])), 
	         (-2.0f / (Screen.height * P[5])),
	         ((1.0f - P[2]) / P[0]), 
	         ((1.0f + P[6]) / P[5]));

		aoMaterial.SetVector ("_ProjInfo", projInfo); // used for unprojection
		aoMaterial.SetMatrix ("_ProjectionInv", invP); // only used for reference
		aoMaterial.SetTexture ("_Rand", rand); // not needed for DX11 :)
		aoMaterial.SetFloat ("_Radius", radius);
		aoMaterial.SetFloat ("_Radius2", radius*radius);
		aoMaterial.SetFloat ("_Intensity", intensity);
		aoMaterial.SetFloat ("_BlurFilterDistance", blurFilterDistance);

		var rtW : int = source.width;
		var rtH : int = source.height;

		var tmpRt : RenderTexture  = RenderTexture.GetTemporary (rtW>>downsample, rtH>>downsample);
		var tmpRt2 : RenderTexture;

		Graphics.Blit (source, tmpRt, aoMaterial, 0);

		if (downsample > 0) {
			tmpRt2 = RenderTexture.GetTemporary (rtW, rtH);
			Graphics.Blit(tmpRt, tmpRt2, aoMaterial, 4);
			RenderTexture.ReleaseTemporary (tmpRt);
			tmpRt = tmpRt2;

			// @NOTE: it's probably worth a shot to blur in low resolution 
			//  instead with a bilat-upsample afterwards ...
		}
		
		for (var i : int = 0; i < blurIterations; i++) {
			aoMaterial.SetVector("_Axis", Vector2(1.0f,0.0f));
			tmpRt2 = RenderTexture.GetTemporary (rtW, rtH);
			Graphics.Blit (tmpRt, tmpRt2, aoMaterial, 1);
			RenderTexture.ReleaseTemporary (tmpRt);

			aoMaterial.SetVector("_Axis", Vector2(0.0f,1.0f));
			tmpRt = RenderTexture.GetTemporary (rtW, rtH);
			Graphics.Blit (tmpRt2, tmpRt, aoMaterial, 1);
			RenderTexture.ReleaseTemporary (tmpRt2);
		}

		aoMaterial.SetTexture ("_AOTex", tmpRt);		
		Graphics.Blit (source, destination, aoMaterial, 2);

		RenderTexture.ReleaseTemporary (tmpRt);
	}	
}
