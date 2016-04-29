using UnityEngine;
using System.Collections;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpToolsTexel {
	
	
	/// <summary>
	/// Get the texel density of a Mesh in world space of Transform T
	/// </summary>
	public static float GetTexelDensity(Transform T){
		Mesh 		mesh 	= PpToolsGameObject.GetMesh(T.gameObject);
		Texture2D 	tex 	= PpToolsGameObject.GetTexture(T.gameObject);
		
		if (mesh && tex){
			return GetTexelDensity(T, mesh, tex);
		}else{
			return 0f;
		}
	}
	
	/// <summary>
	/// Get the texel density of a Mesh in world space of Transform T
	/// </summary>
	public static float GetTexelDensity(Transform T, Mesh mesh, Texture2D tex){
		//NO CUSTOM SELECTION OF TRIANGLES, PASS NULL
		return GetTexelDensity(T, null, mesh.triangles, mesh.uv, mesh.vertices, tex);
	}
	
	/// <summary>
	/// Get the texel density of a Mesh in world space of Transform T
	/// </summary>
	public static float GetTexelDensity(Transform T, int[] tris, Vector2[] uvs, Vector3[] vtcs, Texture2D tex){
		//NO CUSTOM SELECTION OF TRIANGLES, PASS NULL
		return GetTexelDensity(T, null, tris, uvs, vtcs, tex);
	}
	
	/// <summary>
	/// Get the texel density of a Mesh in world space of Transform T with a specified set of face ID's to process.
	/// </summary>
	public static float GetTexelDensity(Transform T, int[] trisIdsSet, int[] tris, Vector2[] uvs, Vector3[] vtcs, Texture2D tex){
		
		
		int i;

		
		Vector3[] vt3 = new Vector3[3];
		Vector2[] uv3 = new Vector2[3];
		
		float sum_uv = 0f;
		float sum_vtx = 0f;
		int trisCount;
		
		if (trisIdsSet == null){
			
			trisCount = tris.Length/3;
			
			for(i=0; i < trisCount;i++){
	
				uv3[0] = uvs [ tris[ i * 3+0 ] ];
				uv3[1] = uvs [ tris[ i * 3+1 ] ];
				uv3[2] = uvs [ tris[ i * 3+2 ] ];
				sum_uv+= GetSurfaceArea(uv3[0], uv3[1], uv3[2]);
				
				vt3[0] = vtcs [ tris[ i * 3+0 ] ];
				vt3[1] = vtcs [ tris[ i * 3+1 ] ];
				vt3[2] = vtcs [ tris[ i * 3+2 ] ];
				
				sum_vtx+= GetSurfaceArea(vt3[0], vt3[1], vt3[2]);
			}
			
		}else{
			
			int idx;
			trisCount = trisIdsSet.Length;
			
			for(i=0; i < trisCount;i++){
	
				idx = trisIdsSet[i];
				
				uv3[0] = uvs [ tris[ idx * 3+0 ] ];
				uv3[1] = uvs [ tris[ idx * 3+1 ] ];
				uv3[2] = uvs [ tris[ idx * 3+2 ] ];
				sum_uv+= GetSurfaceArea(uv3[0], uv3[1], uv3[2]);
				
				vt3[0] = vtcs [ tris[ idx * 3+0 ] ];
				vt3[1] = vtcs [ tris[ idx * 3+1 ] ];
				vt3[2] = vtcs [ tris[ idx * 3+2 ] ];
				
				sum_vtx+= GetSurfaceArea(vt3[0], vt3[1], vt3[2]);
			}
			
		}
		
		
		float scale = (T.lossyScale.x + T.lossyScale.y + T.lossyScale.z)/3f;//AVERAGE GLOBAL OBJECT SCALE
		
		sum_vtx*=scale;//SCALE BY OBJECT SCALE, APPLY TO WORLD VERTECIES OF THE MESH
		
		return Mathf.Sqrt((sum_uv * (float)(tex.width*tex.height) / sum_vtx ));//average pix² used for the mesh in world scale
	}
	
	/// <summary>
	/// Returns the surface area in ^2 of a triangle
	/// </summary>
	private static float GetSurfaceArea(Vector3 A, Vector3 B, Vector3 C){
		
		//Heron's Formula
		//http://www.wikihow.com/Sample/Area-of-a-Triangle-Side-Length
		
		float a = Mathf.Abs((B - A).magnitude);
		float b = Mathf.Abs((B - C).magnitude);
		float c = Mathf.Abs((C - A).magnitude);
		
		float s = (a + b + c)/2f;
		
		return Mathf.Sqrt(s* (s-a) * (s-b) * (s-c));
	}
}
