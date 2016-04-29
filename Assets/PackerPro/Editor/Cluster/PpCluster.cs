using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpCluster {
	
	#region VARS
		private Vector2[] uvOverride;//UV OVERRIDE FOR REFERENCE MESH
		private Rect packBounds;	
		private PpClusterCollection clusterCollection;

		public Mesh mesh;//ORIGINAL MESH REFERENCE
		public Texture2D tex;
		public bool is2D;
		
		//SHELL UV VERTS AND TRIS FACES
		public Vector2[] uvs;
		public int[] tris;
		
		public Vector2[] sharedNewUvs;//NEW UV COORDINATES
		public bool[] sharedNewUvsSet;//NEW UV COORDINATES SET?
		

		public Rect texBounds;//BOUNDS OF THE TEXTURE AREA OF THIS CLUSTER
		public Rect uvBounds;//BOUNDS OF THE UV AREA IN NORMALIZED UNITS OF THIS CLUSTER
		
		public float resize = 1f;
	
		public List<PpCluster> linkedClusters;//OBJECTS THAT ARE LINKED WITH THIS CLUSTER BUT NOT THE SAME MESH (E.G. THEY SHARE THE SAME TEXTURE AREA
		
		public Transform T;
		public List<Transform> transforms =new List<Transform>();//THIS AND OTHER TRANSFORMS
	
	#endregion	
	
	/// <summary>
	/// A cluster is the smallest packable element in PackerPro. It usually consists of a single UV island or a series / group.
	/// </summary>
	public PpCluster(PpClusterCollection clusterCollection, Transform T, Mesh mesh, Vector2[] uvOverride, Texture2D tex, Vector2[] uvs, int[] tris){
		this.T = T;
		this.transforms.Add(T);
		
		this.clusterCollection = clusterCollection;
		this.mesh = mesh;
		this.tex  = tex;
		this.uvs = uvs;
		this.tris = tris;
		
		if (uvOverride == null){
			uvOverride = mesh.uv;
		}
		
		this.uvOverride = uvOverride;
		
		this.sharedNewUvs = new Vector2[mesh.uv.Length];
		System.Array.Copy(uvOverride, sharedNewUvs, mesh.uv.Length);

		
		this.sharedNewUvsSet = new bool[mesh.uv.Length];
		
		linkedClusters = new List<PpCluster>();
		

		CalculateMeta();
	}
	
	
	
	#region HELPERS
	
	/// <summary>
	/// Estimates the bounds in UV space and in Texture pixel space.
	/// </summary>
	private void CalculateMeta(){

		int i;Vector2 pt;
		texBounds = new Rect(uvs[0].x, 1f- uvs[0].y, 0f, 0f);//CREATE INITIAL TEXTURE BOUNDS OBJECT
		uvBounds = new Rect(uvs[0].x, uvs[0].y, 0f, 0f);//CREATE INITIAL UV BOUNDS OBJECT
		
		for(i=1; i < uvs.Length;i++){//NOW MAX AND MIN OUT THE BOUNDS OF THE UV AND TEXTURE BOUNDS
			pt = new Vector2(uvs[i].x, 1f- uvs[i].y);
			texBounds.xMin = Mathf.Min( pt.x, texBounds.xMin);
			texBounds.xMax = Mathf.Max( pt.x, texBounds.xMax);
			texBounds.yMin = Mathf.Min( pt.y, texBounds.yMin);
			texBounds.yMax = Mathf.Max( pt.y, texBounds.yMax);
			// UV BOUNDS
			pt = uvs[i];
			uvBounds.xMin = Mathf.Min( pt.x, uvBounds.xMin);
			uvBounds.xMax = Mathf.Max( pt.x, uvBounds.xMax);
			uvBounds.yMin = Mathf.Min( pt.y, uvBounds.yMin);
			uvBounds.yMax = Mathf.Max( pt.y, uvBounds.yMax);
		}

		//APPLY TEXTURE UNITS TO TEXTURE BOUNDS
		texBounds.x*=tex.width;
		texBounds.y*=tex.height;
		texBounds.width*=tex.width;
		texBounds.height*=tex.height;
		
	}
	
	
	public void Resize(float scale, bool isBilinearResize){
		resize = scale;
		
		texBounds.x*=resize;
		texBounds.y*=resize;
		texBounds.width*=resize;
		texBounds.height*=resize;
		
		
		texBounds.width = Mathf.Max(1f, texBounds.width);
		texBounds.height = Mathf.Max(1f, texBounds.height);


		//CREATE A TEXTURE CLONE - WITHOUT RELYING ON INSTANCIATE WHICH ONLY WORKS ON MONOBEHAVIOUR
		Texture2D texClone = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
		texClone.SetPixels( tex.GetPixels());
		
		//RESIZE
		if (isBilinearResize){
			PpToolsTextureScale.Bilinear(texClone, (int)(Mathf.Max(1f,tex.width * resize)), (int)(Mathf.Max(1f,tex.height * resize)));
		}else{//FASTER
			PpToolsTextureScale.Point(texClone, (int)(tex.width * resize), (int)(tex.height * resize));
		}
		
		tex = texClone;//NOW REFERENE THE CLONE
		texClone = null;//clear memory
		
//		GC.Collect()
		
		packBounds.width*= resize;
		packBounds.height*= resize;
		
		//APPLY TO LINKED CLUSTERS
		foreach(PpCluster linked in linkedClusters){
			linked.Resize(scale, isBilinearResize);
		}
		
		
		
	}
	
	/// <summary>
	/// Applies the new location offset to the UV arrays. This seems to be one of the most processing intense operations.
	/// </summary>
	public void ApplyOffset(Texture2D texFin, int xOffset, int yOffset, int padding){
		int i;
		int j;
		
		xOffset+= padding;
		yOffset+= padding;
		
		float textureWidth = (float)texFin.width;//WIDTH OF THE FINAL TEXTURE IS NEEDED AS FLOAT FOR DOT OPERATIONS
		float textureHeight = (float)texFin.height;//HEIGHT OF THE FINAL TEXTURE IS NEEDED AS FLOAT FOR DOT OPERATIONS

		packBounds = new Rect(xOffset, yOffset, texBounds.width, texBounds.height);

		float x;
		float y;
		int idx;
		
		//TRANSFORMS
		float transBoundsX = uvBounds.x;//ORIGINAL INITIAL CLUSTER UV OFFSET-X
		float transBoundsY = uvBounds.y;//ORIGINAL INITIAL CLUSTER UV OFFSET-Y
		
		float transScaleX = (float)tex.width / textureWidth;//X-SCALE FOR THIS CLUSTER
		float transScaleY = (float)tex.height / textureHeight;//Y-SCALE FOR THIS CLUSTER
		
		float offUVSpaceX = (float)xOffset / textureWidth;
		float offUVSpaceY = ((float)(textureHeight - (yOffset + texBounds.height)) / textureHeight);
		
					
		for(i=0; i < tris.Length;i++){
			
			for(j=0; j < 3;j++){
				idx = mesh.triangles[ tris[i] * 3+j ];
				
				if ( !sharedNewUvsSet[idx] ){//ONLY UPDATE UV ONCE
					sharedNewUvsSet[idx] = true;
					
					x = uvOverride[ idx ].x;//UV OVERRIDE CONTAINS EITHER THE MESH UV OR COMPONENT ORIGINAL MESH UV
					y = uvOverride[ idx ].y;

					//SET TO 0,0
					x-= transBoundsX;
					y-= transBoundsY;
					
					//SCALE INTO NEW SPACE
					x*= transScaleX;
					y*= transScaleY;
					
					//OFFSET IN UV SPACE
					x+= offUVSpaceX;
					y+= offUVSpaceY;
					
					//APPLY TO NEW MESH
					sharedNewUvs[	idx	].x = x;
					sharedNewUvs[	idx	].y = y;
				}
			}
		}
	}
	
	
	
	
	#endregion
	
	
	#region COMBINE AND MERGE
		
	
	/// <summary>
	/// Swallows a target cluster and expands if needed.
	/// </summary>
	public void SwallowCluster(PpCluster cluster){
		int i;
		
		if (mesh == cluster.mesh || tex == cluster.tex){

			int countUvs 	= uvs.Length;
			int countTris 	= tris.Length;
			System.Array.Resize<Vector2>(ref uvs, (uvs.Length + cluster.uvs.Length));
			System.Array.Resize<int>(ref tris, tris.Length + cluster.tris.Length);
			
			for(i=0; i < cluster.uvs.Length;i++){
				uvs[countUvs+i] = cluster.uvs[i];
			}
			for(i=0; i < cluster.tris.Length;i++){
				tris[countTris+i] = cluster.tris[i];
			}

			//MERGE SUB TRANSFORMS
			foreach(Transform subT in cluster.transforms){
			if (!transforms.Contains(subT)){
					transforms.Add(subT);
				}
			}
			
			CalculateMeta();//GET TEX BOUNDS
			
		}
	}
	
	/// <summary>
	/// Links another cluster with this one even though they don't share the same mesh
	/// </summary>
	public void GroupCluster(PpCluster cluster){
		if (tex == cluster.tex){//THEY NEED TO REFERENCE THE SAME TEXTURE!
			//NEED TO OFFSET THE LOCAL OFFSET...!!
			linkedClusters.Add( cluster );


			//MERGE SUB TRANSFORMS
			foreach(Transform subT in cluster.transforms){
				if (!transforms.Contains(subT)){
					transforms.Add(subT);
				}
			}

		}
	
	}
		
	#endregion
	
	
	
	#region DRAW TEXTURE
	
	
	/// <summary>
	/// This Method draws this cluster's bounds into a target texture at a target rectangle with a defined padding size. Padding is only applied if enough padding pixels are available from the source texture.
	/// </summary>
	public void DrawBoundsIntoTexture(Texture2D texTarget, Rect recTarget, int padding){//, Rect recTarget
		int j;
		int k;
		
		Rect recSource = new Rect(texBounds.x, texBounds.y, texBounds.width, texBounds.height);
		

		if (recSource.xMin < 0f || recSource.yMin < 0f || recSource.xMax > tex.width || recSource.yMax > tex.height){

			recTarget.x+= padding;
			recTarget.y+= padding;

		}else{
			
			float paddingX = padding;
			float paddingY = padding;
		
			
			//LIMIT BY DESTINATION BOUNDS
			paddingX = Mathf.Min( texTarget.width - recTarget.xMax, paddingX);
			paddingY = Mathf.Min( texTarget.height - recTarget.yMax, paddingY);
		
			//LIMIT BY SOURCE BOUNDS - ONLY WHEN NOT EXCEEDING UV BOUNDS
		
			paddingX = Mathf.Min( recSource.x, paddingX);
			paddingY = Mathf.Min( recSource.y, paddingY);
			paddingX = Mathf.Min( tex.width - recSource.xMax, paddingX);
			paddingY = Mathf.Min( tex.height - recSource.yMax, paddingY);
			
			recSource.xMin-=paddingX;
			recSource.xMax+=paddingX;
			recSource.yMin-=paddingY;
			recSource.yMax+=paddingY;
			
			recTarget.x+= padding-paddingX;
			recTarget.y+= padding-paddingY;
			
			recTarget.width = recSource.width;
			recTarget.height = recSource.height;
			
			//Protect from exceeding bounds
			recTarget.x= Mathf.Min( texTarget.width - recTarget.width, recTarget.x);
			recTarget.y= Mathf.Min( texTarget.height - recTarget.height, recTarget.y);

		}

		//FLIP Y - TO UV AND TEXTURE COORDINATES
		recSource.y = (tex.height - recSource.yMax);
		recTarget.y = (texTarget.height - recTarget.yMax);


		if (recSource.width <= 0f || recSource.height <= 0f || recSource.width > texTarget.width || recSource.height > texTarget.height){
			
			Debug.Log("CLUSTER DOESN'T FIT IN TARGET TEXURE!!! "+recSource+" | "+texTarget);
			
		}else{
			

			
			Color[] pixels = new Color[]{};
			
			
			//TILED TEXTURE, GOES BEYOND OR PAST THE 0-1 UV AREA
			if (recSource.yMin < 0 || recSource.xMin < 0 || recSource.xMax >= tex.width || recSource.yMax >= tex.height ){

				//DO QUAD STITCH RENDERING APPROACH
				float c_x_A = Mathf.Floor(recSource.xMin / tex.width);//CELL_X_A BEGIN
				float c_x_B = Mathf.Floor(recSource.xMax / tex.width);//CELL_X_B END
				float c_y_A = Mathf.Floor(recSource.yMin / tex.height);//CELL_Y_A BEGIN
				float c_y_B = Mathf.Floor(recSource.yMax / tex.height);//CELL_Y_B END
				
				int c_x_count = (int)Mathf.Abs(c_x_B - c_x_A)+1;
				int c_y_count = (int)Mathf.Abs(c_y_B - c_y_A)+1;
				
				Rect[,] c_lastDrawn_rects = new Rect[c_y_count,c_x_count];//STORE AND COMPARE LATER ALL DRAWN RECTS
				
				
				pixels = new Color[(int)(recSource.width * recSource.height)];//TEXTURE ARRAY THAT WE NEED TO FILL
				
				
				for(j=0; j <  c_y_count;j++){
					for(k=0; k < c_x_count;k++){
						
						
						float x_min 	= (k == 0) ? (recSource.xMin + c_x_count*tex.width)% tex.width : 0f;
						float x_max		= (k ==c_x_count-1) ? (recSource.xMax + c_x_count*tex.width)%tex.width : tex.width;
						
						float y_min 	= (j == 0) ? (recSource.yMin + c_y_count*tex.height)%tex.height : 0f;
						float y_max		= (j ==c_y_count-1) ? (recSource.yMax + c_y_count*tex.height)%tex.height : tex.height;
						
						Rect	r_Source = new Rect( x_min, y_min, x_max - x_min, y_max - y_min);
						Rect	r_Target = new Rect(0f, 0f , r_Source.width, r_Source.height);
						
						//APPEND PREVIOUS OFFSET IN X OR Y AXIS
						if (k > 0){
							r_Target.x = c_lastDrawn_rects[j,k-1].x + c_lastDrawn_rects[j,k-1].width;
						}
						if (j > 0){
							r_Target.y = c_lastDrawn_rects[j-1,k].y + c_lastDrawn_rects[j-1,k].height;
						}
						
						
						PpToolsTexture.CopyPixelsToColorArray(
							(int)recSource.width,
							(int)recSource.height,
							r_Source,
							r_Target,
							tex,
							pixels
						);
						
						c_lastDrawn_rects[j,k] = r_Target;
						

					}
				}
				
				
			}else{
	
			 	pixels = tex.GetPixels(
					(int)recSource.x,
					(int)recSource.y,
					(int)recSource.width, 
					(int)recSource.height
				);
			
			}
			
			
			//CHECK FOR TEXTURE IMPORT SETTING, IF THIS TEXTURE IS NOT SUPPOSED TO BE OPAQUE
			PpTextureSetting texImportSetting = clusterCollection.GetTextureSetting(tex);
			
			if (texImportSetting != null){
				if (!texImportSetting.showsAlpha){
					//SET ALL ALPHA VALUES TO 1f
					int count = pixels.Length;
					for(j=0; j <  count;j++){
						pixels[j].a = 1f;
					}
				}
			}

			texTarget.SetPixels(	
				(int)recTarget.x,
				(int)recTarget.y,
				(int)recSource.width,
				(int)recSource.height,
				pixels
			);

		}
	}
	
	#endregion
	
	/// <summary>
	/// Draws the UV wireframes into a texture with a color c
	/// </summary>
	public void DrawWireFrames(Texture2D texDraw, Color c){

		int i;
		int j;
		

		Vector2[] uv = new Vector2[3];
		Vector2[] uvTx = new Vector2[3];
		
		
		c.a = 1f;
		
		//DRAW UV
		Color cWireframes = c;//UNIQUE COLOR BY ID

		
		for(i=0; i < tris.Length;i++){
			int idx = tris[i];
			uv[0] = sharedNewUvs[ mesh.triangles[ idx * 3+0 ] ];
			uv[1] = sharedNewUvs[ mesh.triangles[ idx * 3+1 ] ];
			uv[2] = sharedNewUvs[ mesh.triangles[ idx * 3+2 ] ];
			
			for(j=0; j < 3;j++){//SET TO PIXEL COORDINATES
				uvTx[j] = new Vector2(
					uv[j].x * (float)texDraw.width,
					uv[j].y * (float)texDraw.height
				);
			}	
			
			PpToolsTexture.DrawLine(texDraw, uvTx[0], uvTx[1], cWireframes);
			PpToolsTexture.DrawLine(texDraw, uvTx[1], uvTx[2], cWireframes);
			PpToolsTexture.DrawLine(texDraw, uvTx[2], uvTx[0], cWireframes);
		}
		
		//DRAW BOUNDS
//		float darken = 0.3f;
//		PpToolsTexture.DrawRect(texDraw, packBounds, new Color(c.r*darken, c.g*darken, c.b*darken,1f));
		
		//DRAW CENTROID DOT
		Rect recCenter = PpToolsGui.FitIntoRect(new Rect(packBounds.x, packBounds.y, 6,6), packBounds, 2f);
		PpToolsTexture.DrawSolidRect(texDraw, recCenter, c);

	}
	
	
	
	
	
	
	
}

