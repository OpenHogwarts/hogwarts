using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
//----------------------------------
//            PackerPro
//  Copyright Â© 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------

[CanEditMultipleObjects]
[CustomEditor(typeof(PpComponent))] 
public class PpComponentInspector : Editor {
	
	private Color colorBkg;
	private PpComponent cmp;
	
	private Texture texSprAlphaTile;
	
	
	
	/// <summary>
	/// Component GUI gets initialized
	/// </summary>
	public void OnEnable(){

		texSprAlphaTile		= Resources.Load("PackerPro_sprAlphaTile") as Texture;
		
		//LOAD LANGUAGE ID
		PpLanguage.Init();
		string languageId = EditorPrefs.GetString( "PackerPro.language", "en");
		PpLanguage.Setup(languageId, languageId+"-context" );
		
		//INIT UI COLORS
//		if (PackerPro.instance != null){
			PackerPro.InitVarsColor();
//		}
		
	}
	
	
	#region GUI
	public override void OnInspectorGUI() {
		

		//DON'T RENDER IF THE GAME IS RUNNING
		if (Application.isPlaying){
			EditorGUILayout.HelpBox( PpLanguage.Get("component_errorNoRuntimeEditing"),MessageType.Warning);
			GUI.enabled = false;
		}
		
	
	
		
		float h = 28f;//
		
		
		colorBkg = GUI.backgroundColor;

		cmp = (PpComponent) target;
		
		
		
		GUI.backgroundColor = PackerPro.colorUiFocus;
		if (GUILayout.Button( PpLanguage.GetGUI("component_btnSelectSet",cmp.collectionName), GUILayout.MinHeight(h))){
			
			OnBtnSelectSet(cmp);	
			
		}
		GUI.backgroundColor = colorBkg;
		
		
		
		OnGuiInfo();
		
		
		
		GUILayout.BeginHorizontal();
		OnGuiTextures();
		GUILayout.EndHorizontal();
		
		
		
		
		
		GUILayout.BeginHorizontal();
			
			GUI.backgroundColor = PackerPro.colorUiWarning;
			if (GUILayout.Button(PpLanguage.GetGUI("component_btnRemoveSet"), GUILayout.MinHeight(h))){
			
				cmp.UndoAll();
			
			}
			GUI.backgroundColor = colorBkg;
			
			if (GUILayout.Button(PpLanguage.GetGUI("component_btnEditSet", cmp.collectionName), GUILayout.MinHeight(h))){
			
				OnBtnEditSet(cmp);
					
			}
			
			
		GUILayout.EndHorizontal();

    }
	
	
	/// <summary>
	/// Render GUI Information area
	/// </summary>
	private void OnGuiInfo(){
		
//		string message = "Related Objects:\t\t "+cmp.relatedGameObjects.Count+"x";
//		message+="\nPacked Texture:\t\t"+cmp.mat.mainTexture.width+"x"+cmp.mat.mainTexture.height;
		
		string message = "";
		message+="Name Set:\t\t\t"+cmp.collectionName;
		message+="\nMaximum Size:\t\t"+cmp.collectionMaxSize;
		message+="\nPadding Size:\t\t"+cmp.collectionPadding;
		
		message+="\n\nRelated Objects:\t"+cmp.relatedGameObjects.Count+"x";
//		message+="\nPacked Texture:\t"+cmp.mat.mainTexture.width+"x"+cmp.mat.mainTexture.height;
		
		
		
		
		
		EditorGUILayout.HelpBox(message,MessageType.Info);
		
		
	}
	
	/// <summary>
	/// Render GUI texture area
	/// </summary>
	private void OnGuiTextures(){
		
		GUILayout.BeginHorizontal();

		if (cmp.gameObject.GetComponent<Renderer>() != null){
			Renderer r = cmp.gameObject.GetComponent<Renderer>();
			if (r.sharedMaterial != null){
				if (r.sharedMaterial.mainTexture != null){
					DrawTexturePreview( r.sharedMaterial.mainTexture, PpLanguage.Get("component_labelTextureAfter") );
				}
			}
		}
		
		GUILayout.EndHorizontal();
	}
	
	/// <summary>
	/// Render single texture preview
	/// </summary>
	private void DrawTexturePreview(Texture tex, string label){
		
		
		GUILayout.BeginVertical();

		EditorGUILayout.SelectableLabel(label+"\n\t"+tex.width+" x "+tex.height);
		
		
		
		Rect rect = GUILayoutUtility.GetRect(128f, 128f);//, GUILayout.ExpandWidth(true)
		
		Rect rectInset 	= PpToolsGui.FitIntoRect(rect, rect, 8f);
		Rect rectProp 	= PpToolsGui.FitIntoRect(tex, rect, 12f);

		if (GUI.Button(rect,"")){
			
			EditorGUIUtility.PingObject( tex );
			
		}
		if (Event.current.type == EventType.Repaint){//DRAW TEXTURE ON TOP OF THE BUTTON

			PpToolsGui.DrawTexTiled(rectInset, texSprAlphaTile);
			GUI.DrawTexture(rectProp, tex);
			PpToolsGui.DrawPixOutline(rectProp, PpToolsColor.HexToColor_24bit("02131b"), 1);
		}
		
		GUILayout.EndVertical();
		
	}
	
	
	#endregion
	
	
	
	#region BUTTONS
	
	/// <summary>
	/// Selects the related GameObjects of this packed set.
	/// </summary>
	public void OnBtnSelectSet(PpComponent cmp){
		
		GameObject[] selection = new GameObject[cmp.relatedGameObjects.Count];
		
		for(int i =0; i<selection.Length;i++){
			selection[i] = cmp.relatedGameObjects[i].gameObject;
		}
		
		Selection.objects = selection;
		
	}
	
	/// <summary>
	/// Opens PackerPro and edits this set
	/// </summary>
	public void OnBtnEditSet(PpComponent cmp){
		

		GameObject[] gObjects =  new GameObject[cmp.relatedGameObjects.Count];
	
		for(int i=0; i < cmp.relatedGameObjects.Count; i++){
			gObjects[i] = cmp.relatedGameObjects[i].gameObject;	
		}
		Selection.objects = gObjects;
	
		EditorApplication.ExecuteMenuItem("Assets/Packer Pro");
		//WAIT TILL ITS INITIALIZED...
		
	}
	

	
	#endregion
	
}
