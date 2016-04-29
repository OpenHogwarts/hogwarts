using UnityEngine;
using System.Collections;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpGameObject {
	
	public Mesh mesh;
	public Material material;
	public Texture2D texture;
	public GameObject gameObject;
	
	public static bool IsValid(GameObject go){
		return false;
	}
	/// <summary>
	/// PpGameObject is used in the PackerPro interface for collecting compatible pack able objects. It holds variables for quicker access in the code.
	/// </summary>
	public PpGameObject(GameObject go){
		
		this.gameObject = go;
		
		this.mesh 		= PpToolsGameObject.GetMesh(go);
		this.material 	= PpToolsGameObject.GetMaterial(go);
		this.texture 	= PpToolsGameObject.GetTexture(go);
	}
	
	public PpComponent component{
		get{
			return gameObject.GetComponent<PpComponent>();
		}
	}

	
}
