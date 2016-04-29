using UnityEngine;
using System.Collections;
using UnityEditor;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpTexturePlatformSetting {
	
	public TextureImporter texImporter;
	
	public string 	platformId;
	public int 		subMaxTexSize;
	public TextureImporterFormat textureImportFormat;
	public int 		compressionQuality;
	
	/// <summary>
	/// Texture compression settings for platform specifc configurations e.g. Android, iPhone, web,..
	/// </summary>
	public PpTexturePlatformSetting(TextureImporter texImporter, string platformId ){
		this.texImporter	= texImporter;
		this.platformId 	= platformId;
	}
	
	
	
}
