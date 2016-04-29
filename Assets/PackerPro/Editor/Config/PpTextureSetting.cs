using UnityEngine;
using UnityEditor;
using System.Collections;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpTextureSetting {
	
	
	public bool						modified = false;
	public bool 					doesSourceTextureHaveAlpha = false;
	
	
	private TextureImporterFormat 	textureFormat;
	private bool 					isReadable;
	private TextureImporter 		textureImporter;
	
	
	public PpTextureSetting(TextureImporter texImp){
		isReadable 		= texImp.isReadable;
		textureFormat 	= texImp.textureFormat;
		doesSourceTextureHaveAlpha	= texImp.DoesSourceTextureHaveAlpha();
		textureImporter = texImp;
	}
	
	/// <summary>
	/// Returns if the texture settings are intending to use the alpha Channel
	/// </summary>
	public bool showsAlpha{
		get{

			string format = textureFormat.ToString().ToLower();

			if (format.IndexOf("argb") != -1 || format.IndexOf("rgba") != -1){
				return true;
			}
			if (format.IndexOf("automatic") != -1){
				return doesSourceTextureHaveAlpha;//IF THE ORIGINAL TEXTURE CARRIES ALPHA
			}
			
			return false;
		}
	}
	
	
	/// <summary>
	/// Restore the texture settings. This is needed because we need to temporarily enable read and write mode for the textures when reading them.
	/// </summary>
	public void Restore(){
		
		if (modified){//ONLY IF IT WAS CHANGED BEFORE
			if (textureImporter != null){
				
				textureImporter.textureFormat = textureFormat;
				textureImporter.isReadable = isReadable;
				
				AssetDatabase.ImportAsset(textureImporter.assetPath);
			}
		}
		
	}
}
