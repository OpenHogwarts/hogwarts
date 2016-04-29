using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PpAssignVariables {

	public GameObject 	gameObject;
	
	public Texture2D 	orgTex;
	public Material 	orgMat;
	public Vector2[] 	orgUv;
	public Mesh			orgMesh;

	public Vector2[] 	uvPacked;
	
	
	public Mesh 	mesh;
	public Material mat;
	
	public string collectionName;
	public int sizeMax;
	public int sizePadding;
	
	public List<GameObject> selectionGameObjects;


	/// <summary>
	/// Assign PackerPro Component (PpComponent) and assign to be stored values
	/// </summary>
	public void AssignComponent(Material packedMaterial){
		PpComponent cmp = gameObject.AddComponent<PpComponent>();
		
		cmp.SetOriginalTexture( orgTex );
		cmp.originalMaterial 	= orgMat;
		cmp.mat 		= mat;//NOW MATERIAL
		cmp.newMesh 	= mesh;//MESH REFERENCE
		
		cmp.uvOriginal = orgUv;
		cmp.uvPacked = uvPacked;


		cmp.gameObject.GetComponent<Renderer>().sharedMaterial = packedMaterial;//ASSIGN NEW MATERIAL


		cmp.relatedGameObjects = selectionGameObjects;//ASSIGN RELATED COMPONENTS
		
		cmp.collectionName 		= collectionName;
		cmp.collectionMaxSize	= sizeMax;
		cmp.collectionPadding	= sizePadding;

	}

}
