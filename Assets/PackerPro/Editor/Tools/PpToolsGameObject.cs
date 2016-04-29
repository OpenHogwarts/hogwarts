using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//----------------------------------
//            PackerPro
//  Copyright Â© 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpToolsGameObject {
		
	
	
	
	
	/// <summary>
	/// Scan and return the mesh part of a GameObject. As objects can either have a mesh renderer or a skinned mesh renderer or none.
	/// </summary>
	public static Mesh GetMesh(GameObject go){

		MeshFilter mf = go.GetComponent<MeshFilter>();
		
		if(mf != null) {
			return mf.sharedMesh;
		}else{
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if (smr != null){
				return smr.sharedMesh;
//				Debug.Log("FOUND SMR: "+go.name);
			}
		}
		return null;
	}
	
	/// <summary>
	/// Get UV array of a GameObject's mesh
	/// </summary>
	public static Vector2[] GetUV(GameObject go){
		Mesh m = GetMesh(go);
		if(m != null) {
			return m.uv;
		}
		return null;
	}
	
	/// <summary>
	/// Get the object's main texture
	/// </summary>
	public static Texture2D GetTexture(GameObject go){
		
		if (go.GetComponent<Renderer>() != null){
			if (go.GetComponent<Renderer>().sharedMaterial != null){
				if (go.GetComponent<Renderer>().sharedMaterial.mainTexture){



					if (go.GetComponent<Renderer>().sharedMaterial.mainTexture is RenderTexture){

						RenderTexture rtt = (RenderTexture)go.GetComponent<Renderer>().sharedMaterial.mainTexture;
						Texture2D buffer = new Texture2D(rtt.width, rtt.height);
						buffer.filterMode = FilterMode.Point;//QUICKER

						RenderTexture.active = rtt;



						buffer.ReadPixels(new Rect(0,0,rtt.width,rtt.height), 0, 0, false);
						buffer.Apply();

						RenderTexture.active = null;


						return buffer;

					}else{
						try{
							return (Texture2D)go.GetComponent<Renderer>().sharedMaterial.mainTexture;
						}catch{
							Debug.Log("Can't cast 1...");
							Debug.Log("Can't cast: "+go.GetComponent<Renderer>().sharedMaterial.mainTexture.name);
							return null;
						}
					}



				}
			}
		}
		return null;
	}
	
	/// <summary>
	/// Get an object's shared material
	/// </summary>
	public static Material GetMaterial(GameObject go){
		
		if (go.GetComponent<Renderer>() != null){
			if (go.GetComponent<Renderer>().sharedMaterial != null){
				return go.GetComponent<Renderer>().sharedMaterial;
			}
		}
		return null;
	}
	
	/// <summary>
	/// Collect all compatible packable objects in a List
	/// </summary>
	public static List<PpGameObject> GetAllCompatibleGameObjects(Transform T, List<Texture2D> ignoreTextures){
		List<PpGameObject> list = new List<PpGameObject>();
		
		
		MeshRenderer[] mRenders = T.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] sRenders = T.GetComponentsInChildren<SkinnedMeshRenderer>();
		
		foreach(MeshRenderer mr in mRenders){
			
			if (mr.sharedMaterial != null){
				if (mr.sharedMaterial.mainTexture != null){

					if (!ignoreTextures.Contains(  mr.sharedMaterial.mainTexture as Texture2D )){
						list.Add( new PpGameObject(mr.gameObject ) );
					}

				}
			}
		}
		
		foreach(SkinnedMeshRenderer smr in sRenders){
			
			if (smr.sharedMaterial != null){
				if (smr.sharedMaterial.mainTexture != null){

					if (!ignoreTextures.Contains( smr.sharedMaterial.mainTexture as Texture2D )){
						list.Add( new PpGameObject(smr.gameObject ) );
					}

				}
			}
		}
		
		return list;
	}
	
}
