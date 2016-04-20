using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmartFootstepSystem : MonoBehaviour {
	
	[System.Serializable]
	public class GroundType{
		public string name;
		public Texture2D[] textures;
		public AudioClip[] sounds;
		
	}
	
	public AudioSource footstepAudio;
	public float groundCheckDistance = 0.25f;
	public AudioClip baseSound;
	public List <GroundType> groundTypes = new List<GroundType>();
	private Terrain terrain;
	private TerrainData terrainData;
	private SplatPrototype[] splatPrototypes;
	private RaycastHit hit;
	[HideInInspector]public Texture2D currentTexture;
	[HideInInspector]public bool onTerrain;

	void Start(){
		GetTerrainInfo();
	}
	
	void GetTerrainInfo(){
		if(Terrain.activeTerrain){
			terrain = Terrain.activeTerrain;
			terrainData = terrain.terrainData;
			splatPrototypes = terrain.terrainData.splatPrototypes;
		}
	}
	
	void Update () {
	
	    Ray ray = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);
		
		//check if the character is currently on a terrain or renderer and get the tecture at that position
		if(Physics.Raycast(ray, out hit, groundCheckDistance)){
		
			if(hit.collider.GetComponent<Terrain>()){
				currentTexture = splatPrototypes[GetMainTexture(transform.position)].texture;
				onTerrain = true;
			}
			if(hit.collider.GetComponent<Renderer>()){
				currentTexture = GetRendererTexture();
				onTerrain = false;
			}
		}
		
		//helper to visualize the ground checker ray
        #if UNITY_EDITOR
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance),Color.green);
    	#endif
	}
	
	public void Footstep(){
		
		bool found = false;
		footstepAudio.volume = Random.Range (0.06f, 0.12f);
		footstepAudio.pitch = Random.Range (0.95f, 1.05f);
		footstepAudio.Stop ();

		for(int i = 0; i < groundTypes.Count; i++){
			for(int k = 0; k < groundTypes[i].textures.Length; k++){
				if(currentTexture == groundTypes[i].textures[k]){
					footstepAudio.clip = (groundTypes[i].sounds[Random.Range(0,groundTypes[i].sounds.Length)]);
					found = true;
				}
			}
		}
		if (!found) {
			footstepAudio.PlayOneShot(baseSound);
		}

		footstepAudio.Play ();
	}
	
	/*returns an array containing the relative mix of textures
       on the main terrain at this world position.*/
	public float[] GetTextureMix(Vector3 worldPos) {
		
		terrain = Terrain.activeTerrain;
		terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;
		
		int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		
		float[,,] splatmapData = terrainData.GetAlphamaps(mapX,mapZ,1,1);
		
		float[] cellMix = new float[splatmapData.GetUpperBound(2)+1];
		for (int n=0; n<cellMix.Length; ++n){
			cellMix[n] = splatmapData[0,0,n];    
		}
		
		return cellMix;        
	}
	
	/*returns the zero-based index of the most dominant texture
       on the main terrain at this world position.*/
	public int GetMainTexture(Vector3 worldPos) {
		
		float[] mix = GetTextureMix(worldPos);
		float maxMix = 0;
		int maxIndex = 0;
		
		for (int n=0; n<mix.Length; ++n){
			
			if (mix[n] > maxMix){
				maxIndex = n;
				maxMix = mix[n];
			}
		}
		
		return maxIndex;
	}
	
	//returns the mainTexture of a renderer's material at this position
	public Texture2D GetRendererTexture(){
		Texture2D texture = null;
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, groundCheckDistance)){
			if(hit.collider.gameObject.GetComponent<Renderer>()){
				MeshFilter meshFilter = (MeshFilter)hit.collider.GetComponent(typeof(MeshFilter));
				Mesh mesh = meshFilter.mesh;
				int totalSubMeshes = mesh.subMeshCount;
				int[] subMeshes = new int[totalSubMeshes];
				for(int i = 0; i < totalSubMeshes; i++){
					subMeshes[i] = mesh.GetTriangles(i).Length / 3;
				}
				
				int hitSubMesh = 0;
				int maxVal = 0;
				
				for(int i = 0; i < totalSubMeshes; i ++){
					maxVal += subMeshes[i];
						if(hit.triangleIndex <= maxVal - 1){
							hitSubMesh = i + 1;
							break;
						}
				}
					texture = (Texture2D)hit.collider.gameObject.GetComponent<Renderer>().materials[hitSubMesh - 1].mainTexture;
			}
		}
		return texture;
	}
}
