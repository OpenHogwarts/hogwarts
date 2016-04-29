using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
//----------------------------------
//            PackerPro
//  Copyright Â© 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpClusterCollection {
	

	public List<PpCluster> clusters;
	private Hashtable newUVarrays;
	private Hashtable newUVarraysUsed;
	private Hashtable originalTextureSettings;
	
	private List<PpTexturePlatformSetting> specificSettings;
	
	
	
	List<Texture2D> texUnique;//UNIQUE TEXTURES THAT ARE USED
	
	public PpClusterCollection(){
		clusters = new List<PpCluster>();

		newUVarrays = new Hashtable();
		newUVarraysUsed = new Hashtable();
		originalTextureSettings = new Hashtable();
		
		specificSettings = new List<PpTexturePlatformSetting>();
	}
	
	#region UNLOCK TEXTURES
	/// <summary>
	/// Enables the texture to be readable - so that we can read out the pixels and copy them into a new texture
	/// </summary>
	public void UnlockTextures(){


		TextureImporterFormat targetImporterFormat = TextureImporterFormat.AutomaticTruecolor;
		

		string[] platformSpecificOverrideIds = new string[]{};//"Web","Standalone","iPhone","Android", NO Default
		
		if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android){
			platformSpecificOverrideIds = new string[]{"Android"};
			
		}else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS){
			platformSpecificOverrideIds = new string[]{"iPhone"};
			
		}else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer){
			platformSpecificOverrideIds = new string[]{"Web"};
		}
		
		
		
		
		//COLLECT UNIQUE TEXTUES
		texUnique = new List<Texture2D>();
		foreach(PpCluster mpc in clusters){
			if ( !texUnique.Contains(mpc.tex) ){
				texUnique.Add( mpc.tex );
			}
		}
		
		
		originalTextureSettings = new Hashtable();
		
		foreach(Texture2D tex in texUnique){
			
			TextureImporter texImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( tex ) ) as TextureImporter;
			if (texImporter != null){//IS AN ASSET THAT EXISTS IN THE ASSET FOLDER - NOT JUST MEMORY
			
				
				bool isChanged = false;//DO RE IMPORT AFTER THIS HAS BEEN CHANGED TO TRUE

				//PLATFORM SPECIFIC SETTINGS (iPHONE, ANDROID, WEB,..., CHECK IF SOME HAVE CUSTOM OVERRIDES
				foreach(string s in platformSpecificOverrideIds){
					
					PpTexturePlatformSetting platformSetting = new PpTexturePlatformSetting(texImporter, s);

					//LOAD OUT VARIABLES
					texImporter.GetPlatformTextureSettings(
						s, 
						out platformSetting.subMaxTexSize, 
						out platformSetting.textureImportFormat, 
						out platformSetting.compressionQuality
					);
					
					//IF IT DOESN'T MATCH WHAT WE NEED TO READ PIXELS, CHANGE IT AND REMMEBER IT
					if (platformSetting.textureImportFormat != targetImporterFormat){
						specificSettings.Add( platformSetting );

						platformSetting.texImporter.SetPlatformTextureSettings(
							platformSetting.platformId, 
							platformSetting.subMaxTexSize,
							targetImporterFormat
						);
						
						isChanged = true;
					}
				}
				

				PpTextureSetting texSetting = new PpTextureSetting(texImporter);
				if (!texImporter.isReadable || texImporter.textureFormat != TextureImporterFormat.ARGB32){
					
					texImporter.isReadable = true;//NEEDED FOR READING THE PIXELS
					texImporter.textureFormat = TextureImporterFormat.ARGB32;//MAKE SURE WE HAVE ACCESS TO ALL THE PIXELS

					isChanged = true;
					
					texSetting.modified = true;//FLAG THIS AS MODIFIED
				}
				
				
				if (isChanged){//MODIFIED EITHER AS A PLATFORM SPECIFIC SETTING OR THE GENERAL SETTING
					AssetDatabase.ImportAsset(texImporter.assetPath);
				}

				originalTextureSettings.Add(tex, texSetting);//ADD THE GENERIC SETTING SO WE CAN RESTORE IT LATER
			}
		}
	}
	
	/// <summary>
	/// Restores a previous texture import state, e.g. not readable and back to a compression setting
	/// </summary>
	public void RestoreLockTextures(){
		
		
		int step = 0;

		foreach(Texture2D tex in texUnique){
			EditorUtility.DisplayProgressBar("Restoring Locked Textures" ,"Unlocking "+(step+1)+"/"+texUnique.Count,(float)step / (float)texUnique.Count);
			
			PpTextureSetting texSetting = (PpTextureSetting)originalTextureSettings[tex];
			if (texSetting != null){
				texSetting.Restore();
			}
		}
		
		
		//REVERT PLATFORM SPECIFIC SETTINGS
		foreach(PpTexturePlatformSetting psSetting in specificSettings){
			
			psSetting.texImporter.SetPlatformTextureSettings(
				psSetting.platformId, 
				psSetting.subMaxTexSize,
				psSetting.textureImportFormat
			);
			
			AssetDatabase.ImportAsset(psSetting.texImporter.assetPath);//RE-IMPORT
		}
		
		
		
		originalTextureSettings = new Hashtable();//CLEAR
		specificSettings = new List<PpTexturePlatformSetting>();//CLEAR
	}

	/// <summary>
	/// Return texture settings that we collected in the originalTextureSettings array if it was collected earlier
	/// </summary>
	public PpTextureSetting GetTextureSetting(Texture2D tex){
		
		if (originalTextureSettings.Contains(tex)){
			return (PpTextureSetting)originalTextureSettings[tex];
		}
		return null;
	}
	
	
	#endregion
	
	#region PARSE

	/// <summary>
	/// Add a new item to the cluster collection without UV array override
	/// </summary>
	public void ParseAndAddMesh(Transform T, Mesh mesh, Texture2D tex){
		
		ParseAndAddMesh(T,mesh, null, tex);//NO UV OVERRIDE
	}
	
	
	/// <summary>
	/// Add a new item to the cluster collection
	/// </summary>
	public void ParseAndAddMesh(Transform T, Mesh mesh, Vector2[] uvOverride, Texture2D tex){

		int i;
		int j;
		int k;
		int m;
		
		
		if (tex == null){
			return;
		}
		

		//CHECK IF MESH+TEX ALREADY PARSED
		foreach(PpCluster cluster in clusters){
			if (cluster.mesh == mesh && cluster.tex == tex){//ALREADY USED

				cluster.transforms.Add(T);//ADD THIS OBJECT TO THE CLUSTERS
				return;
			}
		}
		
		//FIND THE UV CLUSTERS
		
		Vector2[] uv;
		
		if (uvOverride != null){//USE UV OVERRIDE
			uv = uvOverride;
		}else{
			uv= mesh.uv;
		}
		int[] tris = mesh.triangles;
		int numFaces = tris.Length/3;//TRIS COUNT
		
		Vector2[] ptUvs = new Vector2[3];
		Vector2 ptUv;
		
		//CLUSTER COLLECTION
		List<List<Vector2>> clusterUv = new List<List<Vector2>>();
		List<List<int>> clusterTris = new List<List<int>>();
			
		if (tris.Length != numFaces*3 || uv.Length == 0){//IF THE TRIANGLES AND FACES DO NOT MATCH, ESCAPE THIS (COULD BE DYNAMIC MESH)
			return;
		}

		for(i=0; i < numFaces;i++){
			ptUvs[0] = uv[ tris[ i * 3 ]];
			ptUvs[1] = uv[ tris[ i * 3+1 ]];
			ptUvs[2] = uv[ tris[ i * 3+2 ]];
			
			//LOOK UP IN ANY OF THE CLUSTER_UV'S
			if (clusterUv.Count == 0){//PUSH THE FIRST CLUSTER
				
				clusterUv.Add( new List<Vector2>(){ ptUvs[0], ptUvs[1], ptUvs[2] });
				clusterTris.Add( new List<int>(){i});
				
			}else{

				//CHECK IF VERTEX IS CONNECTED TO ANY EXISITNG ONE...
				List<int> idxFound = new List<int>();
				
				for(j=0; j < clusterUv.Count;j++){
					for(k=0; k < clusterUv[j].Count;k++){
						
						ptUv = clusterUv[j][k];
						
						for(m=0; m < 3;m++){
							if (ptUvs[m].x == ptUv.x && ptUvs[m].y == ptUv.y ){//SAME LOCATION
								if (idxFound.IndexOf(j) == -1){
									idxFound.Add(j);
								}
							}
						}
						
						if (idxFound.IndexOf(j) != -1){//WE ALREADY MARKED THIS CLUSTER
							break;
						}
					}
				}
				
				
				//EITHER PUSH TO CLUSTER, OR CREATE NEW ONE
				if (idxFound.Count == 0){//CREATE A NEW CLUSTER
					
					clusterUv.Add( new List<Vector2>(){ ptUvs[0], ptUvs[1], ptUvs[2] });
					clusterTris.Add( new List<int>(){i});
					
				}else{
					
					
					int idxCluster = idxFound[0];
						
					if(idxFound.Count > 1){//MERGE AS WE FOUND MULTIPLE SHELLS THAT USE THESE VERTS
						
						int idxA = idxFound[0];
						int idxB = idxFound[1];
						
						MergeClusterUV( clusterUv, idxA, idxB);
						MergeClusterTris(clusterTris, idxA, idxB);
					}
					
					//APPEND FACE AND VERTS
					
					bool[] found = new bool[]{false,false,false};//CHECK IF THE UV VERT WAS ALREADY ADDED TO THE LIST
					
					for(k=0; k < clusterUv[	idxCluster	].Count; k++){
						ptUv = clusterUv[	idxCluster	][k];
						for(m=0; m < 3;m++){
							if (ptUvs[m].x == ptUv.x && ptUvs[m].y == ptUv.y ){//MATCHIG COORDINATES WITH AN EXISITNG UV
								found[m] = true;
								break;
							}
						}
					}
					for(m=0; m < 3;m++){
						if (!found[m]){
							clusterUv[	idxCluster	].Add( ptUvs[m] );
						}
					}
					clusterTris[	idxCluster	].Add( i );
				}
			}
		}

		PpCluster mpc;

		for(i=0; i < clusterUv.Count;i++){
			
			mpc = new PpCluster(this, T, mesh, uvOverride, tex, clusterUv[i].ToArray(), clusterTris[i].ToArray() );
			
			mpc.sharedNewUvs = GetNewUVArray(mesh,mpc);//THIS MESH IS SHARED
			mpc.sharedNewUvsSet = GetNewUVSetArray(mesh,mpc);//THIS ARRAY IS SHARED AS WELL	
			clusters.Add( mpc );
		}
	}
	
	#endregion
	
	
	#region MERGE
	/// <summary>
	/// Merges the UV data from index B into A
	/// </summary>
	private void MergeClusterUV(List<List<Vector2>> clusterUv, int idxA, int idxB){
		
		int i;
		for(i=0; i< clusterUv[idxB].Count;i++){

			clusterUv[idxA].Add( clusterUv[idxB][i] );
		}
		clusterUv.RemoveAt(idxB);
	}

	/// <summary>
	/// Merges the tris data from index B into A
	/// </summary>
	private void MergeClusterTris(List<List<int>> clusterTris, int idxA, int idxB){
		int i;
		for(i=0; i< clusterTris[idxB].Count;i++){

			clusterTris[idxA].Add( clusterTris[idxB][i] );
		}
		clusterTris.RemoveAt(idxB);
	}
		
	/// <summary>
	/// Checks if any of the clusters in this collection overlap at least overlapThreshold% and if so merge or group them together
	/// </summary>
	public void MergeOverlappingClusterRects(float overlapThreshold){

		int i;
		int j;
		int count = clusters.Count;
		
		Rect rA;
		Rect rB;
		
		List<PpCluster> removeList = new List<PpCluster>();
		
		for(i=0; i< count;i++){
			
			for(j=i+1; j < count ;j++){
				
				rA = clusters[i].texBounds;
				rB = clusters[j].texBounds;
				
				if (clusters[i].tex == clusters[j].tex ){//NEEDS TO BE THE SAME MESH IN ORDER TO STACK SUTFF
					if (!removeList.Contains(clusters[i]) && !removeList.Contains(clusters[j]) ){
						
						
						Rect rI = new Rect(0f,0f,0f,0f);
						rI.xMin = Mathf.Max( rA.xMin, rB.xMin);
						rI.yMin = Mathf.Max( rA.yMin, rB.yMin);
						rI.xMax = Mathf.Min( rA.xMax, rB.xMax);
						rI.yMax = Mathf.Min( rA.yMax, rB.yMax);
						
						if (rI.width > 0 && rI.height > 0){//THERE IS A INTERSECTION RECTANGLE
							
							float pA = (rI.width * rI.height) / (rA.width * rA.height);
							float pB = (rI.width * rI.height) / (rB.width * rB.height);
							
							if (Mathf.Max(pA, pB) >= overlapThreshold){
								
								
								
								
								if (clusters[i].mesh == clusters[j].mesh ){//SAME MESH, SO WE CAN MERGE IT INTO 1 CLUSTER


									if (pA > pB){//A.) LIKELY TO BE SWALLOWED
										
										clusters[j].SwallowCluster( clusters[i] );
										removeList.Add( clusters[i] );
										
									}else{//B.) LIKELY TO BE SWALLOWED
										
										clusters[i].SwallowCluster( clusters[j] );
										removeList.Add( clusters[j] );
										
									}

								}else{//DON'T MERGE VERTS, BUT MARK AS LINK / REFERENCE

									if (pA > pB){//A.) LIKELY TO BE SWALLOWED
										
										clusters[j].GroupCluster( clusters[i] );
										removeList.Add( clusters[i] );
										
									}else{//B.) LIKELY TO BE SWALLOWED
										
										clusters[i].GroupCluster( clusters[j] );
										removeList.Add( clusters[j] );
										
									}

								}
							}
						}
					}
				}
			}
		}

		//REMOVE CLUSTERS THAT ARE ON THE REMOVE-LIST
		foreach( PpCluster mpc in removeList){
			clusters.Remove( mpc );	
		}

	}
	
	#endregion
	
	#region RESIZE

	/// <summary>
	/// Resizes the collection to a target Texel reference size, this reference is usually selected in the scene by object and texture size
	/// </summary>
	public void ResizeToTexelScale(Transform refTransform, bool isBilinearResize){
		
		// DOWNSAMPLING TEXTURES USUALLY CREATES UNPLEASENT STAIRS OR ARTIFACTS
		// THAT'S WHY ONLY SIZES BELOW 0.1 or 10% IN SIZE DIFFERENCE WILL BE SCALED DOWN

		float scaleThresholdIgnore = 0.1f;//ONLY SCALE DOWN IF MORE THAN 10%
		
		
		if (refTransform){
			
			//GET TEXEL SIZES FOR EACH CLUSTER...
			Mesh 		msh = PpToolsGameObject.GetMesh(refTransform.gameObject);
			Texture2D 	tex = PpToolsGameObject.GetTexture(refTransform.gameObject);
			
			if (msh && tex){
				
				float scaleRef = PpToolsTexel.GetTexelDensity( refTransform, msh.triangles, msh.uv, msh.vertices, tex);
			
				Debug.Log("------------------------------\nref Scale: "+refTransform.name+" = "+scaleRef);
				
				
				int step = 0;
				EditorUtility.DisplayProgressBar("Texel Scale" ,"",0f);
				
				foreach(PpCluster mpc in clusters){
					

					float scaleAvg = 0f;
					foreach(Transform T in mpc.transforms){
						scaleAvg+= PpToolsTexel.GetTexelDensity( T, mpc.mesh.triangles, mpc.mesh.uv, mpc.mesh.vertices, mpc.tex);
					}
					scaleAvg/= mpc.transforms.Count;
					

					float scaleCluster = scaleRef / scaleAvg;

					if (scaleCluster < (1f-scaleThresholdIgnore)){//DO NOT SCALE UP!!
						mpc.Resize(scaleCluster, isBilinearResize);
					}
					EditorUtility.DisplayProgressBar(string.Format("Resize cluster ",(step+1),clusters.Count) ,"Resize "+mpc.T.name+" to "+((scaleCluster*100).ToString("#.##"))+"%", (float)step/ (float)clusters.Count);
					
					Debug.Log("Resize "+mpc.T.name+" to "+((scaleCluster*100).ToString("#.##"))+"%");
					
					
					step++;
				}
				
				EditorUtility.ClearProgressBar();
				
			}
			
		}
	}
	
	#endregion
	
	
	#region GETTERS

	/// <summary>
	/// 
	/// </summary>
	private Vector2[] GetNewUVArray(Mesh meshReference, PpCluster mpc){
		if (!newUVarrays.Contains(meshReference)){
			
			newUVarrays.Add(meshReference, mpc.sharedNewUvs);
			return mpc.sharedNewUvs;
			
		}else{
			return (Vector2[])newUVarrays[meshReference];
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private bool[] GetNewUVSetArray(Mesh meshReference, PpCluster mpc){
		if (!newUVarraysUsed.Contains(meshReference)){
			
			newUVarraysUsed.Add(meshReference, mpc.sharedNewUvsSet);
			return mpc.sharedNewUvsSet;
			
		}else{
			return (bool[])newUVarraysUsed[meshReference];
		}
	}
	
	
	
	
	
	
	#endregion
	
	
	
	#region Unique Used Identifiers
	
	/// <summary>
	/// Return a cluster match for a gameObject.
	/// </summary>
	public PpCluster GetMatchingCluster(GameObject go){
		
		foreach(PpCluster clstr in clusters){

			if (clstr.T.gameObject == go){
				return clstr;
			}

			List<string> dbgOutput = new List<string>();

			//COULDN'T FIND GAMEOBJECT, NOW LOOK INTO THE MERGED TRANSFORM ELEMENTS
			foreach(Transform t in clstr.transforms){
				dbgOutput.Add(t.transform.name);

				if (t == go.transform){//FOUND MISSING LINK
					return clstr;
				}

			}

		}

		return null;	
	}
	
	/// <summary>
	/// Returns the unique used meshes in this cluster collection
	/// </summary>
	public Mesh[] GetUniqueUsedMeshes(){
		Mesh[] meshes = new Mesh[ newUVarrays.Count ];
		
		int i = 0;
		foreach (DictionaryEntry itm in newUVarrays){
			meshes[i] = (Mesh)itm.Key;
			i++;
		}	
		return meshes;	
	}
	/// <summary>
	/// Returns the unique used textures in this cluster collection
	/// </summary>
	public List<Texture2D> GetUniqueUsedTextures(){
		return texUnique;
	}
	
	
	/// <summary>
	/// Returns the unique new list of UV sets
	/// </summary>
	public List<Vector2[]> GetUniqueNewUVs(){
		List<Vector2[]> uvs = new List<Vector2[]>();
		
		foreach (DictionaryEntry itm in newUVarrays){
			uvs.Add((Vector2[])itm.Value);
		}	
		return uvs;	
	}
	/// <summary>
	/// Returns the unique used GameObjects of this cluster collection
	/// </summary>
	public List<GameObject> GetUsedGameObjects(){
		List<GameObject> gameObjects = new List<GameObject>();
		
		foreach(PpCluster cluster in clusters){
			if (!gameObjects.Contains( cluster.T.gameObject)){
				gameObjects.Add( cluster.T.gameObject);
			}

			//SUB OBJECTS
			foreach(Transform t in cluster.transforms){
				if (!gameObjects.Contains( t.gameObject )){
					gameObjects.Add( t.gameObject);
				}
			}
			//Linked Objects
			foreach(PpCluster subCluster in cluster.linkedClusters){
				if (!gameObjects.Contains( subCluster.T.gameObject)){
					gameObjects.Add( subCluster.T.gameObject);
				}
			}
		}
		
		return gameObjects;
	}
	
	
	#endregion
	
	
	/// <summary>
	/// Returns the cluster count
	/// </summary>
	public int count{
		get{
			return clusters.Count;
		}
	}
	
}