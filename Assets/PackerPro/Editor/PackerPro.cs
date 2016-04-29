using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------

public class PackerPro : EditorWindow {
	
	public static string versionNumber = "1.85.0";
	#region RECOMPILE
	
	PackerPro(){

		ResetApplication();//FIRST THING WE DO IS RESET PACKER PRO
		PackerPro.instance = this;//COMPILED NEW
		
	}

	/// <summary>
	/// Reset PackerPro's local variables
	/// </summary>
	public static void InitVarsColor(){//ON RESET OR EXTERN COMPONENT EDITOR RENDER CALL

		colorUiFocus 		= PpToolsColor.HexToColor_24bit("afdafb");//BLUE
		colorUiWarning 		= PpToolsColor.HexToColor_24bit("ec1616");//RED

	}
	
	
	/// <summary>
	/// Reset PackerPro's local variables
	/// </summary>
	public void ResetApplication(){//IS CALLED WHEN THE SCRIPT RECOMPILES
//		PpDebug.Clear();
		Debug.Log("RESET PACKERPRO APPLICATION");
		
		PpLanguage.Init();
		PpLanguage.Setup("en","en-context");

		selectionGameObjects	= new List<GameObject>();//CLEAR THE SELECTION SET
		selectionGameObjectTextures = new List<Texture2D>();
		selectionGameObjectTexturesIgnore = new List<Texture2D>();
		packedUsedObjects 		= new List<GameObject>();
		
		InitVarsColor();//SPRITSE AND COLORS
		
	
		texSprDragDrop 			= Resources.Load("PackerPro_sprDropArea") as Texture;
		texSprAlphaTile			= Resources.Load("PackerPro_sprAlphaTile") as Texture;
		texSprFoldTile			= Resources.Load("PackerPro_sprFoldOutBkg") as Texture;
		texSprFoldArrowFalse	= Resources.Load("PackerPro_sprFoldOutArrowFalse") as Texture;
		texSprFoldArrowTrue		= Resources.Load("PackerPro_sprFoldOutArrowTrue") as Texture;
		texSprSMG				= Resources.Load("PackerPro_sprSMG") as Texture;
		texSprIconPackerPro		= Resources.Load("PackerPro_icoGizmo") as Texture;
		
		ResetResult();//RESET RESULTS
		
		SettingsLoad();
		
		
		EditorUtility.ClearProgressBar();
 
	}
	/// <summary>
	/// PackerPro Constructor, PpComponent depends on this menu item entry
	/// </summary>
	//INFO: PpComponent depends on this ID
	[MenuItem("Assets/Packer Pro")]
	public static void InitPackerPro(){



		if (PackerPro.instance != null){
			PackerPro.instance.ResetApplication();
			PackerPro.instance.ResetResult();
	
			//ADD SELECTION!!
			PackerPro.instance.InitAddSelection();
		}
	
		if (window != null){
			window.Close();
		}
		window = EditorWindow.GetWindow( typeof(PackerPro) );
		window.title = "Packer Pro";
		window.minSize = new Vector2(256,256+32);

		Debug.Log("Windoiw: "+window.ToString()+" x,y: "+window.position.ToString());
		window.Show(true);
		window.Focus();

	}
	/// <summary>
	/// When PackerPro gets enabled, add the current scene selection
	/// </summary>
	void OnEnable(){
		InitAddSelection();
	}

	
		
	#endregion
	
	
	
	#region BASE
	
		
		void OnFocus() {
			
		}
	
		/// <summary>
		/// When PackerPro gets closed, save the last settings and destroy the singleton
		/// </summary>
		void OnDestroy() {
			SettingsSave();
			PackerPro.instance = null;
	    }
		
	
		/// <summary>
		/// When the scene changes, make sure that we repaint the UI as it might reflect the selection change.
		/// </summary>
		void OnSelectionChange(){
			//CLEAR STATUS
			
			Repaint();//FORCE A REPAINT OF THE UI
			
		}
		/// <summary>
		/// Save the editor settings
		/// </summary>
		void SettingsSave(){

			EditorPrefs.SetInt( "PackerPro.sizeMax", sizeMax );
			EditorPrefs.SetInt( "PackerPro.sizePadding", sizePadding );
			EditorPrefs.SetBool( "PackerPro.isShowWireframe", isShowWireframe );
			
			EditorPrefs.SetBool( "PackerPro.isSquare", isSquare );
			EditorPrefs.SetBool( "PackerPro.isPadPixels", isPadPixels );
			EditorPrefs.SetBool( "PackerPro.isIgnoreAlpha", isIgnoreAlpha );
			
			EditorPrefs.SetBool( "PackerPro.isStack", isStack );
			EditorPrefs.SetFloat( "PackerPro.stackIfOverlap", stackIfOverlap );
			

			EditorPrefs.SetString( "PackerPro.language", PpLanguage.languageId);
			EditorPrefs.SetString( "PackerPro.collectionName", collectionName);
			
			EditorPrefs.SetBool( "PackerPro.isTexelSmoothResize", isTexelSmoothResize );
			EditorPrefs.SetBool( "PackerPro.isTexelScale", isTexelScale );
			
		}
		/// <summary>
		/// Load the editor settings
		/// </summary>
		void SettingsLoad(){
		
			sizeMax 		= EditorPrefs.GetInt( "PackerPro.sizeMax", sizeMax );
			sizePadding 	= EditorPrefs.GetInt( "PackerPro.sizePadding", sizePadding );
			isShowWireframe	= EditorPrefs.GetBool( "PackerPro.isShowWireframe", isShowWireframe );
			
			isSquare		= EditorPrefs.GetBool( "PackerPro.isSquare", isSquare );
			isPadPixels		= EditorPrefs.GetBool( "PackerPro.isPadPixels", isPadPixels );
			isIgnoreAlpha	= EditorPrefs.GetBool( "PackerPro.isIgnoreAlpha", isIgnoreAlpha );
			
			isStack			= EditorPrefs.GetBool( "PackerPro.isStack", isStack );
			stackIfOverlap	= EditorPrefs.GetFloat( "PackerPro.stackIfOverlap", stackIfOverlap );

			collectionName = EditorPrefs.GetString( "PackerPro.collectionName", "Packed Set");
			
			isTexelSmoothResize = EditorPrefs.GetBool( "PackerPro.isTexelSmoothResize", isTexelSmoothResize );
			isTexelScale		= EditorPrefs.GetBool( "PackerPro.isTexelScale", isTexelScale );
		
			string languageId = EditorPrefs.GetString( "PackerPro.language", "en");
			PpLanguage.Setup(languageId, languageId+"-context" );
		}
	
	
	#endregion
	
	
	#region GUI
		
		


	/// <summary>
	/// Draws the main GUI loop
	/// </summary>
	void OnGUI () {

	
		colorUiBkg = GUI.backgroundColor;
	
		UiContextMenu();//RENDER AND RECEIVE CONTEXT MENU OPTIONS
	
		
		int minWidth = 304;
		int maxWidth = minWidth+72;
	
		bool isViewSingleColumn = position.width <= maxWidth;
	
	
		if (isViewSingleColumn){//SINGLE COLUMN VIEW
		
			EditorGUILayout.Space();
			GuiDrawMain();
			EditorGUILayout.Space();
			GuiDrawSelectionSet();
			EditorGUILayout.Space();
			GuiDrawTextures();
			EditorGUILayout.Space();
			GuiDrawAdvanced();

			EditorGUILayout.Space();
			if (GuiDrawFold( PpLanguage.GetGUI("mainResult_title"), ref isFoldResult)){
				GuiDrawAtlas();
			}
		
		}else{//SPLIT LAYOUT
			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical(GUILayout.MinWidth(minWidth));
		
					EditorGUILayout.Space();
					GuiDrawMain();
					EditorGUILayout.Space();
					GuiDrawSelectionSet();
					EditorGUILayout.Space();
					GuiDrawTextures();
					EditorGUILayout.Space();
					GuiDrawAdvanced();
		
				EditorGUILayout.EndVertical();
		
				EditorGUILayout.BeginVertical();
		
					GuiDrawAtlas();
		
		
				EditorGUILayout.EndVertical();
				
			EditorGUILayout.EndHorizontal();
		
		}
	
    }


	/// <summary>
	/// Draws a sub UI label with a centered text and a thin line
	/// </summary>
	private void GuiDrawSubLabel(GUIContent labelContent){
	
		GUIStyle styleTextCenter = new GUIStyle(EditorStyles.label);
		styleTextCenter.fontStyle = FontStyle.Normal;
		styleTextCenter.alignment = TextAnchor.MiddleCenter;

		EditorGUILayout.Space();
		GUILayout.Box( labelContent  ,styleTextCenter, GUILayout.ExpandWidth(true));
		Rect lastUiRect = GUILayoutUtility.GetLastRect();
	
		Color colorLine = styleTextCenter.active.textColor;
		colorLine.a = 0.25f;
		PpToolsGui.DrawPixLine((int)lastUiRect.x, (int)lastUiRect.yMax, (int)lastUiRect.width, colorLine, 1f);
	
		EditorGUILayout.Space();	
	}

	/// <summary>
	/// Draws a custom Fold Sub Section
	/// </summary>
	private bool GuiDrawFold(GUIContent labelContent, ref bool switcher){
		
		//http://answers.unity3d.com/questions/48541/custom-inspector-changing-foldouts-text-color.html

		labelContent.text = " "+labelContent.text+"";
	
	
		Rect rect = GUILayoutUtility.GetRect(16f, 24f, GUILayout.ExpandWidth(true));//, GUILayout.ExpandWidth(true)
		GUIStyle style = new GUIStyle(EditorStyles.miniButton);
	
		style.fontStyle = FontStyle.Normal;
		style.fontSize = 11;
	

		Color myStyleColor = PpToolsColor.HexToColor_24bit("e1e1e1");
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = myStyleColor;
		style.onNormal.textColor = myStyleColor;
		style.hover.textColor = myStyleColor;
		style.onHover.textColor = myStyleColor;
		style.focused.textColor = myStyleColor;
		style.onFocused.textColor = myStyleColor;
		style.active.textColor = myStyleColor;
		style.onActive.textColor = myStyleColor;
				
		style.normal.background = null;		

		if (GUI.Button(rect, labelContent,style)){
			switcher = !switcher;
		}
		if (Event.current.type == EventType.Repaint){
			GUI.DrawTexture(rect, texSprFoldTile);
			
			GUI.Box(rect, labelContent,style);
		
			//DRAW TEXTURE...
			Rect rectArrowSprite = new Rect( rect.x, rect.y, 24f, 24f);
		
			GUI.DrawTexture(rectArrowSprite, switcher ? texSprFoldArrowTrue : texSprFoldArrowFalse );

		}
		
		return switcher;
	}
	
	/// <summary>
	/// Draws Top Main UI with the packing button
	/// </summary>
	private void GuiDrawMain(){
		EditorGUILayout.BeginVertical();
		
	

		GUI.enabled = (selectionGameObjects.Count > 0);
	
		if (GUILayout.Button(

			PpLanguage.GetGUI("mainSetting_createNewSet") )){
				
			ResetApplication();//CLEAR LIST AND RESULTS
			
			//ADD A NEW COLLECTION NAME SET BASED ON THE SCENE SELECTION
			if (Selection.transforms.Length > 0){
				collectionName = Selection.transforms[0].name + " set";
			}else{
				collectionName = "new packing set";
			}
		}
		GUI.enabled = true;

	
		collectionName = EditorGUILayout.TextField(PpLanguage.GetGUI("mainSetting_setName"), collectionName);

		sizeMax 		= EditorGUILayout.IntPopup(		
			PpLanguage.Get("mainSetting_maxSize")	,
			sizeMax, 
			sizesLabel, 
			sizes 
		);
		sizePadding 	= EditorGUILayout.IntSlider(	
			PpLanguage.Get("mainSetting_pixPadding") ,
			sizePadding,
			0, 
			16
		);

	
		GUI.enabled = selectionGameObjects.Count > 0;
		GUI.backgroundColor = (selectionGameObjects.Count > 0 && !isCreated) ? colorUiFocus : colorUiBkg;
		
		if (GUILayout.Button(
			new GUIContent(	
				PpLanguage.Get("mainSetting_packItems", selectionGameObjects.Count.ToString() ) ,
				PpLanguage.Get("mainSetting_packItems",true)
			),GUILayout.MinHeight(28f+8f))){
				Build();
		}
		GUI.backgroundColor = colorUiBkg;
		GUI.enabled = true;
	
	
		GuiDrawDragDrop();

	
		EditorGUILayout.EndVertical();
	}
	
	/// <summary>
	/// Draws the drag and drop area for accepting new gameObjects to pack
	/// </summary>
	private void GuiDrawDragDrop(){
		
		Rect rectDropUi = GUILayoutUtility.GetRect(0f, 70f, GUILayout.ExpandWidth(true));
		GUI.Box (rectDropUi, "");//

		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		{
			GUIStyle style = new GUIStyle(EditorStyles.label);
			style.alignment = TextAnchor.MiddleCenter;
		
			GUI.Box(rectDropUi, PpLanguage.GetGUI("mainSelect_dragDrop_message"),style);
		}
		EditorGUILayout.EndHorizontal();


		if (texSprDragDrop != null){
			Rect rectSprDropArea = PpToolsGui.FitIntoRect(texSprDragDrop, rectDropUi,8f);
			GUI.DrawTexture(rectSprDropArea, texSprDragDrop);
		}
		
		
			
		Rect rectDropEvent = GUILayoutUtility.GetLastRect();
		rectDropEvent.yMin = rectDropUi.yMin;

		// Handle events
        Event evt = Event.current;
		if (rectDropEvent.Contains(evt.mousePosition)){
	       	if ( evt.type == EventType.DragUpdated){
                
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
			

			}else if(evt.type == EventType.DragPerform){
                
				foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences) {
					if (dragged_object.GetType() == typeof(GameObject)){
						
						//FILTER OUT OBJECTS THAT SHOULD NOT BE ADDED
						List<GameObject> listNewToAdd =  getSelectedNewToAdd( new Transform[]{((GameObject) dragged_object).transform});
						if (selectionGameObjects.Count == 0){
							ResetResult();
						}
						//NOW ADD THE FILTERED OBJECTS TO THE ACTIVE PACKING LIST
						foreach(GameObject go in listNewToAdd){
							selectionGameObjects.Add( go );
						}
						
						Update_ApplySelectionSetComponents();
					
					
					}else{
						Debug.Log("UNKOWN DRAG OBJECT: "+dragged_object.GetType());
					}
                }
			
				Update_ApplySelectionSetComponents();

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            DragAndDrop.AcceptDrag();
            evt.Use();
                
	        }
		}
		
	}

	/// <summary>
	/// Draws the advanced settings section
	/// </summary>
	private void GuiDrawAdvanced(){
		EditorGUILayout.BeginVertical();
	
		if (GuiDrawFold( PpLanguage.GetGUI("mainSettingAdvanced_title"), ref isFoldAdvanced)){
		
			listScrollAdvancedPos = EditorGUILayout.BeginScrollView(listScrollAdvancedPos);
		
			GuiDrawSubLabel( PpLanguage.GetGUI("mainSettingAdvanced_titleMaterial") );
			{

				matAssign = (Material)EditorGUILayout.ObjectField(PpLanguage.GetGUI("mainSettingsAdvanced_material"), matAssign, typeof(Material), true);
				
				if (matAssign == null){

					EditorGUILayout.HelpBox("If you don't assign a material a new one will be created for you", MessageType.Info);
				}
				GUI.color = Color.white;
			
			}
		
			GuiDrawSubLabel( PpLanguage.GetGUI("mainSettingAdvanced_titleTexture") );
			{
				
				isSquare			= EditorGUILayout.Toggle( PpLanguage.GetGUI("mainSettingAdvanced_forceSquare"),isSquare);
				isPadPixels			= EditorGUILayout.Toggle( PpLanguage.GetGUI("mainSettingAdvanced_pixelPad"),isPadPixels);

				isIgnoreAlpha		= EditorGUILayout.Toggle( PpLanguage.GetGUI("mainSettingAdvanced_ignoreAlphaChannel"),isIgnoreAlpha);
				
			}
		
			GuiDrawSubLabel( PpLanguage.GetGUI("mainSettingAdvanced_titleStack") );
			{
				isStack			= EditorGUILayout.Toggle( PpLanguage.GetGUI("mainSettingAdvanced_stackIfEnable"),isStack);
			
				GUI.enabled = isStack;
				stackIfOverlap	= EditorGUILayout.Slider(PpLanguage.GetGUI("mainSettingAdvanced_stackIfOverlaps"), stackIfOverlap, 0.80f, 1f );
				GUI.enabled = true;	

			}	
		

			GuiDrawSubLabel( PpLanguage.GetGUI("mainSettingAdvanced_titleTexel") );
			{
			
				isTexelScale	= EditorGUILayout.Toggle( PpLanguage.GetGUI("mainSettingAdvanced_texelDo"),isTexelScale);
				
				GUI.enabled = isTexelScale;
			
				Transform cacheTransform = texelRefTransform as Transform;
				texelRefTransform = EditorGUILayout.ObjectField(PpLanguage.GetGUI("mainSettingAdvanced_texelReference"), texelRefTransform, typeof(Transform), true) as Transform;
		
				if( texelRefTransform != cacheTransform){
					if (texelRefTransform == null){
						texelRefTransform = null;
						texelRefSummary = "";
					}else{
						Mesh 		mesh 	= PpToolsGameObject.GetMesh(texelRefTransform.gameObject);
						Texture2D 	tex 	= PpToolsGameObject.GetTexture(texelRefTransform.gameObject);
						
						if (mesh && tex){
							//TODO:TRANSLATE THIS INFO MESSAGE
							texelRefSummary = string.Format("Texture size: {1} x {2}\nTexel size: {0} pix^2 for each 1m.",
								(PpToolsTexel.GetTexelDensity(texelRefTransform)).ToString("#.##"),
								tex.width,
								tex.height
							);
						}else{
							//TODO:TRANSLATE THIS ERROR MESSAGE
							EditorUtility.DisplayDialog("Not compatible", "Please reference a GameObject that has a mesh renderer and texture assigned to it.", "Ok");	
							texelRefTransform = null;
							texelRefSummary = "";
						}
					}
				}
				if (texelRefTransform == null)
				{
					texelRefSummary = "";
				}

				if (texelRefSummary != "")
				{
					EditorGUILayout.HelpBox(texelRefSummary, MessageType.Info);
				}
		
				isTexelSmoothResize = EditorGUILayout.Toggle( PpLanguage.GetGUI("mainSettingAdvanced_texelSmoothResize"),isTexelSmoothResize);

				GUI.enabled	= true;
			}
		
		
			EditorGUILayout.Space();
		
			EditorGUILayout.EndScrollView();
		
		}
	
		EditorGUILayout.EndVertical();
	}


	/// <summary>
	/// Draws the packing list UI section
	/// </summary>
	private void GuiDrawSelectionSet(){
	
		EditorGUILayout.BeginVertical();
		{
			
			GUIContent title;
			if (selectionGameObjects.Count == 0){
				title = PpLanguage.GetGUI("mainSelect_title");
			}else{
				title = PpLanguage.GetGUI("mainSelect_titleContains",selectionGameObjects.Count.ToString());
			}
			
			if (GuiDrawFold( title, ref isFoldSelection)){
			
				int newAdd = getSelectedNewToAdd(Selection.transforms).Count;//IsGameObject_AlreadyInSelection
				
			
			
				GUI.backgroundColor = (newAdd == 0 && Selection.transforms.Length == 0) ? colorUiFocus : colorUiBkg;
				
				if (newAdd < 4){
					if (newAdd == 0){
						EditorGUILayout.HelpBox(PpLanguage.Get("mainSelect_info_noSelectionAndItems"),MessageType.Warning);
					}else{
						EditorGUILayout.HelpBox(PpLanguage.Get("mainSelect_info_alreadyItems"),MessageType.Info);
					}
				}
				GUI.backgroundColor = colorUiBkg;

				
			
				EditorGUILayout.BeginHorizontal();
			
					//DETECT IF ANY OF THE LIST ITEMS HAS ALREADY AN COMPONENT  IF SO RED
					GUI.backgroundColor = (newAdd > 0 && selectionGameObjects.Count == 0) ? colorUiFocus : colorUiBkg;
					GUI.enabled = newAdd > 0;

					if (GUILayout.Button( (Selection.transforms.Length == 0 ? PpLanguage.GetGUI("mainSelect_btn_addEmpty") : PpLanguage.GetGUI("mainSelect_btn_addAmount",newAdd.ToString())), GUILayout.MinHeight(28f))){
						if (newAdd > 0){
							BtnAddSelection();
						}
					}
					GUI.enabled = true;

					GUI.enabled = selectionGameObjects.Count > 0;
					GUI.backgroundColor = selectionGameObjects.Count > 0 ? colorUiWarning : colorUiBkg;//	
					if (GUILayout.Button( PpLanguage.GetGUI("mainSelect_btn_clearAll") ,GUILayout.MinHeight(28f))){
				
						BtnClearSelection();
						
					}
					GUI.backgroundColor = colorUiBkg;
					GUI.enabled = true;
			
				EditorGUILayout.EndHorizontal();
			
				if (selectionGameObjects.Count > 0){
					if (GUILayout.Button( PpLanguage.GetGUI("mainSelect_btn_selectSet",selectionGameObjects.Count.ToString()) ,GUILayout.MinHeight(28f))){
						
						GameObject[] selection = new GameObject[selectionGameObjects.Count];
						for(int i =0; i<selection.Length;i++){
							selection[i] = selectionGameObjects[i];
						}
						
						Selection.objects = selection;

					}
				}
			
			
				
				listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);
				GuiDrawSelectionSetList();
				EditorGUILayout.EndScrollView();

			}

		}
		EditorGUILayout.EndVertical();
	}



	/// <summary>
	/// Draws the texture swatch list of the selected objects
	/// </summary>
	private void GuiDrawTextures(){

		GUIContent title;

		if (selectionGameObjects.Count == 0){
			title = PpLanguage.GetGUI("mainTextures_title");
		}else{
			title = PpLanguage.GetGUI("mainTextures_titleContains", selectionGameObjectTextures.Count+"x" );
		}


		
		if (GuiDrawFold( title, ref isFoldTextures)){

			//START TEXTURE THUMBNAIL AREA

			listScrollTexturesPos = EditorGUILayout.BeginScrollView(listScrollTexturesPos);
			if (selectionGameObjectTextures.Count > 0){
				EditorGUILayout.LabelField("Textures selected: "+ (selectionGameObjectTextures.Count - selectionGameObjectTexturesIgnore.Count) + " / " +selectionGameObjectTextures.Count);
			}
			
			int columns = 3;
			int rows = Mathf.CeilToInt( (float)selectionGameObjectTextures.Count / (float)columns);
			
			int k=0;
			for (int i = 0; i < rows; i++) {
				EditorGUILayout.BeginHorizontal();
				for (int j = 0; j < columns; j++,k++) {
					GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.MinHeight(80));
					Rect rect = GUILayoutUtility.GetLastRect();
					
					if (k < selectionGameObjectTextures.Count){
						
						float p = 0.2f;
						GUI.color = selectionGameObjectTexturesIgnore.Contains( selectionGameObjectTextures[k] ) ? new Color(1f,1f,1f,p) : Color.white;
						EditorGUI.DrawPreviewTexture(rect, selectionGameObjectTextures[k], null, ScaleMode.ScaleToFit );
						GUI.color = Color.white;
						
						bool toggleOn = !selectionGameObjectTexturesIgnore.Contains( selectionGameObjectTextures[k] );
						bool toggleSet = EditorGUI.Toggle(rect, toggleOn);
						if (toggleSet != toggleOn){
							if (!toggleSet){
								if (!selectionGameObjectTexturesIgnore.Contains( selectionGameObjectTextures[k]) ){
									selectionGameObjectTexturesIgnore.Add( selectionGameObjectTextures[k] );
								}
							}else{
								if (selectionGameObjectTexturesIgnore.Contains( selectionGameObjectTextures[k]) ){
									selectionGameObjectTexturesIgnore.Remove( selectionGameObjectTextures[k] );
								}
							}
						}
					}
					
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndScrollView();

		}

	}

	
	/// <summary>
	/// Draws the packing set list items
	/// </summary>
	private void GuiDrawSelectionSetList(){

		if (selectionGameObjects.Count > 0){
		
			foreach(GameObject go in selectionGameObjects){
			
				EditorGUILayout.BeginHorizontal();
				
				if (go == null){
					selectionGameObjects.Remove(go);//DOESN'T EXIST ANYMORE, REMOVE IT AND UPDATE THIS LIST!
					Update_ApplySelectionSetComponents();
					break;
				}else{
				
					//DRAW ICON	
					PpComponent[] cmps = go.GetComponentsInChildren<PpComponent>();
				
				
				
					Rect rectIcon = GUILayoutUtility.GetRect(24f, 24f,GUILayout.MaxWidth(24f));// GUILayout.ExpandWidth(true)
				
					if (cmps.Length > 0){//YES WE ALREADY HAVE SOME COMPONENTS
						if (texSprIconPackerPro != null){
							Rect rectSprDropArea = PpToolsGui.FitIntoRect(texSprIconPackerPro, rectIcon,2f);
							GUI.DrawTexture(rectSprDropArea, texSprIconPackerPro);
						}
					}else{
						//NO COMPONENT
					}
					
					//IF SELECTED
					if ( Array.IndexOf( Selection.transforms, go.transform) != -1){
						GUI.backgroundColor = colorUiFocus;
					}
			
					if ( GUILayout.Button("", GUILayout.MinWidth(100f)) ){
					
						EditorGUIUtility.PingObject( go );
						Selection.objects = new GameObject[]{go};
					}
					if (Event.current.type == EventType.Repaint){
					
						Rect rect = GUILayoutUtility.GetLastRect();
						EditorGUI.LabelField(rect," "+go.name);
					}
				
				
				
					int numMeshes = go.GetComponentsInChildren<MeshFilter>().Length;
					EditorGUILayout.LabelField(numMeshes+"x",GUILayout.MaxWidth(24f));
				
				

					//IF WE ARE ABOUT TO REMOVE THE MESH PACKING RESULT
					if (cmps.Length > 0){
						GUI.backgroundColor = colorUiWarning;//	
					}

					//REMOVE BUTTON
				
					GUIContent guiDelete = cmps.Length > 0 ? PpLanguage.GetGUI("mainSelect_btn_listRemoveItemComponent") : PpLanguage.GetGUI("mainSelect_btn_listRemoveItem");
				
					if (GUILayout.Button( guiDelete ,GUILayout.MaxWidth(24f)) ){
					
						if (cmps.Length > 0){
							foreach(PpComponent cmp in cmps){//FOR ALL SUB COMPONENTS OF THIS TRANSFORM
								cmp.Undo();//UNDO ANY FORMER MESH PACKING 
							}
						}
						selectionGameObjects.Remove(go);
						Update_ApplySelectionSetComponents();//UPDATE THE LIST
					
						break;
					}
			
					GUI.backgroundColor = colorUiBkg;
				}
			
				EditorGUILayout.EndHorizontal();
			
			}
		}
	}
	
	/// <summary>
	/// The texture preview window
	/// </summary>
	private void GuiDrawAtlas(){
	
		EditorGUILayout.BeginVertical();
	
		EditorGUILayout.BeginHorizontal();
		{	
			GUI.enabled = isCreated;
			GUI.backgroundColor = (isCreated) ? colorUiFocus : colorUiBkg;	
				//Change button label if we are creating a material


			//DON'T RENDER IF THE GAME IS RUNNING
			if (!Application.isPlaying){
				GUIContent guiButtonLabel = PpLanguage.GetGUI("mainResult_btn_assignResultWithMaterial") ;
				if (GUILayout.Button(guiButtonLabel ,GUILayout.MinHeight(28f+8f))){
					Assign();
				}
			}

			GUI.backgroundColor = colorUiBkg;
		
				
			if (GUILayout.Button(PpLanguage.GetGUI("mainResult_btn_createMesh"),GUILayout.MinHeight(28f+8f))){
				CreateSingleMeshPrefab();
			}
		
			if (GUILayout.Button(PpLanguage.GetGUI("mainResult_btn_exportMesh"),GUILayout.MinHeight(28f+8f))){
				if( ExportMesh() ){
					this.ShowNotification( PpLanguage.GetGUI("exportObj_success") );
				}
			}
		
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		
	
		if(isCreated){

			EditorGUILayout.HelpBox(stats_message,MessageType.Info);
		}	

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
			
			GUI.enabled = isCreated;
			isShowWireframe = EditorGUILayout.Toggle(PpLanguage.Get("mainResult_setting_showWireFrame",false),isShowWireframe);
			GUI.enabled = true;
	
		EditorGUILayout.EndHorizontal();	
	
		GUI.backgroundColor = Color.black;
		GUILayout.Box("",GUILayout.MinHeight(128),GUILayout.MinWidth(128),GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) );//,GUILayout.MinWidth(256)
	
	
		float space = 8f;
	
	
		if (isCreated || packedTexture != null && selectionGameObjects.Count > 0){
			
			Rect maxRect = GUILayoutUtility.GetLastRect();
			Rect viewRect = GUILayoutUtility.GetLastRect();
			viewRect.width = Mathf.Min(packedTexture.width, maxRect.width - 2f*space);
			viewRect.height = viewRect.width * packedTexture.height / packedTexture.width;
	
			if (viewRect.height > (maxRect.height-2f*space)){
				viewRect.height = Mathf.Min(maxRect.height-2f*space, packedTexture.height);
				viewRect.width = viewRect.height * packedTexture.width / packedTexture.height;
			}

			//CENTER VIEW RECT HORIZONTAL
			viewRect.x = maxRect.x + (maxRect.width - viewRect.width)/2f;
			viewRect.y = maxRect.y + (maxRect.height - viewRect.height)/2f;
		
			PpToolsGui.DrawTexTiled(maxRect, texSprAlphaTile);
			if (!isCreated){
				float bColor = 0.8f;
				GUI.color = new Color(bColor,bColor,bColor,0.5f);
			}
		
		
			GUI.DrawTexture(viewRect, packedTexture);
			if (isShowWireframe){
				if (wireframeTexture != null){
					GUI.DrawTexture(viewRect, wireframeTexture);
				}
			}
			GUI.color = Color.white;
		
		
			PpToolsGui.DrawPixOutline(viewRect, Color.black, 1);


		}else{

			//SHOW EMPTY CANVAS, NO RESULT TO SHOW
			Rect maxRect = GUILayoutUtility.GetLastRect();
			Rect viewRect = new Rect(0f,0f, texSprSMG.width, texSprSMG.height );
			viewRect.x = maxRect.x + (maxRect.width-viewRect.width)/2f;
			viewRect.y = maxRect.y + (maxRect.height-viewRect.height)/2f;

			PpToolsGui.DrawTexTiled(maxRect, texSprAlphaTile);
			
			if (maxRect.width >= viewRect.width && maxRect.height >= viewRect.height){


		    	GUI.DrawTexture(viewRect, texSprSMG);
			}
		}
		
		EditorGUILayout.EndVertical();
	}	

	#endregion
	

	#region CONTEXT MENU


	/// <summary>
	/// Constructor for the Context menu
	/// </summary>
	private void UiContextMenu(){

		Event evt = Event.current;

        if (evt.type == EventType.ContextClick)
        {
            GenericMenu menu = new GenericMenu();
		
			menu.AddItem(new GUIContent("Website"), false, new GenericMenu.MenuFunction(OnContextHelp));
			menu.AddItem(new GUIContent("Contact Support"), false, new GenericMenu.MenuFunction(OnContextSupport));
			
			menu.AddSeparator("");
	
			string lId = PpLanguage.languageId;
	
			menu.AddItem(new GUIContent("Language: English"), lId == "en", new GenericMenu.MenuFunction2(OnContextSetLanguage), "en");
            menu.AddItem(new GUIContent("Language: Chinese"), lId == "zh", new GenericMenu.MenuFunction2(OnContextSetLanguage), "zh");
			menu.AddItem(new GUIContent("Language: Korean"), lId == "ko", new GenericMenu.MenuFunction2(OnContextSetLanguage), "ko");
			menu.AddItem(new GUIContent("Language: Japanese"), lId == "ja", new GenericMenu.MenuFunction2(OnContextSetLanguage), "ja");
	
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Version "+versionNumber),false, new GenericMenu.MenuFunction(OnContextVersionNumber));
	
            menu.ShowAsContext();
            evt.Use();
        }
	}

	/// <summary>
	/// Help button
	/// </summary>
	void OnContextHelp(){
		Application.OpenURL("http://smgstudio.com/packer-pro/");
	}

	void OnContextVersionNumber(){
		
	}

	/// <summary>
	/// Support button
	/// </summary>
	void OnContextSupport(){
		Application.OpenURL("http://packerpro.uservoice.com/");
	}

	/// <summary>
	/// Setting a language
	/// </summary>
	void OnContextSetLanguage(object languageId){
		string id = languageId as string;
		PpLanguage.Setup(id, id+"-context");
		SettingsSave();
	}

		

	#endregion

	


	#region PROGRESS BAR

	/// <summary>
	/// Helper method for updating the UI
	/// </summary>
	private void UiUpdateProgressBar(int step, int steps, int stepOffset, int stepAhead,string title, string note){
		
		float val = (float)(step+stepOffset);
		float tot = (float)(steps+stepOffset+stepAhead);
	
		UiUpdateProgressBar(val / tot, title, note);
	}

	/// <summary>
	/// Helper method for updating the UI, extended
	/// </summary>
	private void UiUpdateProgressBar(float p,string title, string note){
	
		EditorUtility.DisplayProgressBar(title ,note,p);
	
		if (p >= 1f){
			EditorUtility.ClearProgressBar();
		}
	}


	#endregion


	#region SELECTION SET TOOLS
	
	/// <summary>
	/// Clear the packing list
	/// </summary>
	private void BtnClearSelection(){
		 
		PpComponent cmp;
		
		List<GameObject> copyList = PpToolsList.Clone<GameObject>(selectionGameObjects);
		
		foreach(GameObject go in copyList){
			
			cmp = go.GetComponent<PpComponent>();
			if (cmp != null){
				cmp.UndoAll();//UNDO ANY FORMER MESH PACKING 
			}
			
			Transform[] allChildren = go.GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren) {
				
				cmp = child.GetComponent<PpComponent>();
				
				if (cmp != null){
					cmp.UndoAll();//UNDO ANY FORMER MESH PACKING 
				}
			}
		}
		

		selectionGameObjects = new List<GameObject>();
		selectionGameObjectTexturesIgnore = new List<Texture2D>();
		ResetResult();
		
		Update_ApplySelectionSetComponents();		
	}
	

	/// <summary>
	/// Add the scene transform selection to the packing list
	/// </summary>
	private void InitAddSelection(){

		BtnAddSelection();
		
		//LOAD CONFIG FROM A PACKED ELEMENT, CollectionName, sizeMax, sizePadding
		bool found = false;
		foreach(GameObject go in selectionGameObjects){
			
			List<PpGameObject> gameObjects = PpToolsGameObject.GetAllCompatibleGameObjects(go.transform, new List<Texture2D>{});
			
			foreach(PpGameObject mGo in gameObjects){
				
				if (mGo.component != null){//HAVE AN EXISTING COMPONENT
					
					sizeMax 		= mGo.component.collectionMaxSize;
					sizePadding 	= mGo.component.collectionPadding;
					collectionName 	= mGo.component.collectionName;
					
					//VERIFY SETTINGS
					sizeMax			= Math.Max(sizeMax, sizes[0]);//NEEDS TO BE AT LEAST 256+
					
					
					//COPY TEXTURE
					if (mGo.material != null){
						if ( PpToolsGameObject.GetTexture( mGo.gameObject ) != null){
							PackerPro.instance.packedTexture = PpToolsGameObject.GetTexture( mGo.gameObject );
						}
					}

					found = true;
					break;
				}
			}
			if( found){
				UpdateTextureCollection();
				break;
			}
		}
	}
	
	/// <summary>
	/// Add the scene transform selection to the packing list
	/// </summary>
	private void BtnAddSelection(){
		
		List<GameObject> listNewToAdd =  getSelectedNewToAdd(Selection.transforms);
		
		if (selectionGameObjects.Count == 0){
			ResetResult();
		}

		foreach(GameObject go in listNewToAdd){
			selectionGameObjects.Add( go );
		}
		
		Update_ApplySelectionSetComponents();
	}
	
	/// <summary>
	/// Rebuilds related gameObject lists and assigns them to excisting PackerPro Components
	/// </summary>
	private void Update_ApplySelectionSetComponents(){
		//UPDATE COMPONENTS IF EXISTING
		
		foreach(GameObject go in selectionGameObjects){
			PpComponent cmp = go.GetComponent<PpComponent>();
			if(cmp != null){
				cmp.relatedGameObjects = selectionGameObjects;
			}
		}
		UpdateTextureCollection();
		OnSelectionChange();
		SceneView.RepaintAll();//UPDATE THE VIEWPORT REDRAW
	}

	/// <summary>
	/// Checks wether the mGo GameObject is already part of the collection list
	/// </summary>
	private bool IsGameObject_AlreadyInSelection(GameObject mGo){

		foreach( GameObject go in selectionGameObjects){
			if (go == mGo){
				return true;
			}
			//GO THROUGH ALL THE SUB OBJECTS
			Transform[] allChildren = go.GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren) {
			    if (child.gameObject == mGo){
					return true;	
				}
			}
		}

		return false;
	}

	/// <summary>
	/// UI Texture collection needs to be updated
	/// </summary>
	private void UpdateTextureCollection(){
		
		selectionGameObjectTextures = new List<Texture2D>();
		foreach(GameObject go in selectionGameObjects){
			
			List<PpGameObject> gameObjects = PpToolsGameObject.GetAllCompatibleGameObjects(go.transform, new List<Texture2D>{});
			foreach(PpGameObject obj in gameObjects){

				//CHECK IF THERE IS ALREADY AN PpComponent AND IF THERE IS A ORIGINAL TEXTURE WE CAN RETURN INSTEAD OF THE PACKED TEXTURE
				PpComponent cmp = obj.gameObject.GetComponent<PpComponent>();
				if (cmp != null){

					if (cmp.originalTexture != null){
						if(!selectionGameObjectTextures.Contains( cmp.originalTexture )){
							selectionGameObjectTextures.Add( cmp.originalTexture );
						}
					}

				}else{

					if(!selectionGameObjectTextures.Contains( obj.texture )){
						selectionGameObjectTextures.Add( obj.texture );
					}
				}
			}
		}
	}
	
	#endregion

	
	#region BUILD & ASSIGN
	
	/// <summary>
	/// Reset the rendered texture and selection set
	/// </summary>
	private void ResetResult(){
		//RESET THE RENDERED TEXTURE AND SELECTION SET
		
		packedTexture = null;
		wireframeTexture = null;
		clusterCollection = new PpClusterCollection();//CLEAR
		packedUsedObjects = new List<GameObject>();
		selectionGameObjectTexturesIgnore = new List<Texture2D>();

		Repaint();
	}
	
	
	/// <summary>
	/// The main build method for building the texture and UV sets for all related objects.
	/// </summary>
	private bool Build(){


		SettingsSave();

		UiUpdateProgressBar(0f, PpLanguage.Get("progressBarBuild_title"), "Collect clusters");

		clusterCollection = new PpClusterCollection();
		
		PpDebugMeasureTime.Start("Build Texture Atlas");
		
		packedUsedObjects = new List<GameObject>();
		
		//GET ALL MESHES AND TEXTURES TO PACK
		foreach(GameObject go in selectionGameObjects){
			
			List<PpGameObject> gameObjects = PpToolsGameObject.GetAllCompatibleGameObjects(go.transform, selectionGameObjectTexturesIgnore);
			
			foreach(PpGameObject mGo in gameObjects){
				
				if (mGo.component != null){//HAVE AN EXISTING COMPONENT
					clusterCollection.ParseAndAddMesh( mGo.gameObject.transform, mGo.component.originalMesh != null ? mGo.component.originalMesh : mGo.mesh , mGo.component.uvOriginal, mGo.component.GetOriginalTexture() );		
				}else{
					clusterCollection.ParseAndAddMesh( mGo.gameObject.transform, mGo.mesh, mGo.texture);
				}
			}

			packedUsedObjects.Add( go );
		}		

		clusterCollection.UnlockTextures();

		if (clusterCollection.count == 0){
			EditorUtility.DisplayDialog(PpLanguage.Get("mainResult_errorPackingNoMeshes",false), PpLanguage.Get("mainResult_errorPackingNoMeshes", true), "Ok");
			UiUpdateProgressBar(1f,"","");
			return false;
		}
		
		UiUpdateProgressBar(0.05f, PpLanguage.Get("progressBarBuild_title"), "Merge clusters");

		if (isStack){
			clusterCollection.MergeOverlappingClusterRects(stackIfOverlap);
		}

		if (isTexelScale && texelRefTransform != null){
			clusterCollection.ResizeToTexelScale( texelRefTransform, isTexelSmoothResize   );
		}


		List<Rect> rects = new List<Rect>();
		foreach(PpCluster mpc in clusterCollection.clusters){
			rects.Add( mpc.texBounds );
		}
		
		UiUpdateProgressBar(0.1f, PpLanguage.Get("progressBarBuild_title"), "Pack Rectangles");


		//START PACKING
		List<PpIntegerRectangle> 	resultRects = PpRectPackerAutomatic.Pack(rects, sizePadding, sizeMax, isSquare);
		width		= PpRectPackerAutomatic.width;//TEXTURE TARGET WIDTH
		height		= PpRectPackerAutomatic.height;//TEXTURE TARGET HEIGHT

		if (resultRects.Count == 0){
			
			UiUpdateProgressBar(1f, PpLanguage.Get("progressBarBuild_title"), "Error");

			ResetResult();
			Repaint();//UPDATE UI
		
			//SHOW ERROR POPUP...
			EditorUtility.DisplayDialog(PpLanguage.Get("mainResult_errorPackingSize",false), PpLanguage.Get("mainResult_errorPackingSize", true), "Ok");
			
			return false;
			
		}else{
			
			if (!isIgnoreAlpha){
				packedTexture 	= new Texture2D(width,height,TextureFormat.ARGB32,false);
			}else{
				packedTexture 	= new Texture2D(width,height,TextureFormat.RGB24,false);
			}
			wireframeTexture= new Texture2D(width,height,TextureFormat.ARGB32,false);
			wireframeTexture.filterMode = FilterMode.Trilinear;
			
			//GENERATE A BLACK DEFAULT CANVAS WITH 0 OPACITY
			Color[] colors = new Color[width*height];
			Color c = new Color(0f,0f,0f,0f);
			for(int a = 0; a < width*height; a++) {
			    colors[a]=c;
			}
			packedTexture.SetPixels(colors);	
			wireframeTexture.SetPixels(colors);	
			
			
			PpCluster mpc;
			
			//COPY TEXTURES
			int i; 
			int pixPad = isPadPixels ? Mathf.CeilToInt(sizePadding/2f): 0;
			
			
			for(i=0; i< resultRects.Count; i++){
				
				UiUpdateProgressBar(i,resultRects.Count,3,2, PpLanguage.Get("progressBarBuild_title"), "Applying offset to cluster "+(i+1)+"/"+resultRects.Count);
				
				mpc = clusterCollection.clusters[ resultRects[i].id ];

				//FOR EACH GROUPED  LINKED ITEM APPLY SAME OFFSET
				foreach(PpCluster mC in mpc.linkedClusters){
				
					//THE OFFSET DIFFERENCE BETWEEN THE MOTHER CLUSTER AND LINKED SUB CLUSTER IN PIXELS
					float sx = (mC.texBounds.xMin - mpc.texBounds.xMin);
					float sy = (mC.texBounds.yMin - mpc.texBounds.yMin);
				
					mC.ApplyOffset( packedTexture, resultRects[i].x+ Mathf.RoundToInt(sx), resultRects[i].y+Mathf.RoundToInt(sy), pixPad );
				}

				mpc.ApplyOffset(packedTexture,  resultRects[i].x, resultRects[i].y, pixPad);

				//DRAW THE BOUNDS NOW INTO THE TEXTURE
				mpc.DrawBoundsIntoTexture(packedTexture, resultRects[i].ToRect(), pixPad);

			}

			clusterCollection.RestoreLockTextures();

			List<GameObject> usedGameObjects =  clusterCollection.GetUsedGameObjects();//USED FOR COLOR ID'S

			//BAKE WIREFRAMES
			UiUpdateProgressBar(0.95f,PpLanguage.Get("progressBarBuild_title")  , "Draw Wireframes");
			for(i=0; i< resultRects.Count; i++){
				
				mpc = clusterCollection.clusters[ resultRects[i].id ];
					
				Color color = PpToolsColor.GetUniqueColorId( (float) usedGameObjects.IndexOf(mpc.T.gameObject) / (float)usedGameObjects.Count);
					
				
				mpc.DrawWireFrames(wireframeTexture, color);
				
				foreach(PpCluster mC in mpc.linkedClusters){//FOR EACH GROUPED  LINKED ITEM APPLY SAME OFFSET
					mC.DrawWireFrames(wireframeTexture, color);
				}
			
			}
			UiUpdateProgressBar(1f,PpLanguage.Get("progressBarBuild_title"), "Done");
		}

		packedTexture.Apply();
		wireframeTexture.Apply();


		PpDebugMeasureTime.Stop();
		
		//GET STATISTICS
		float sumTextureSpace = 0;
		List<Texture2D> usedTextures = clusterCollection.GetUniqueUsedTextures();
		foreach(Texture2D tex in usedTextures){
			sumTextureSpace+= (tex.width * tex.height);
		}

		float usedTotal = Mathf.Sqrt( sumTextureSpace );
		float usedNew	= Mathf.Sqrt( (float)(packedTexture.width * packedTexture.height));
		
		
		stats_message = "Size: "+packedTexture.width+" x "+packedTexture.height;
		stats_message += "\nUV Clusters: "+clusterCollection.clusters.Count+"x";
		stats_message += "\nCombined Textures: "+usedTextures.Count+"x";
		stats_message += "\nUsed Total Space: ~"+Mathf.FloorToInt(usedNew / usedTotal*100f)+"%";

		return false;

	}

	
	/// <summary>
	/// After building the texture, this method will assign the material and or textures to the objects in the set list
	/// </summary>
	private void Assign(){
		
		int i;
		int j;

		Mesh[] meshes = clusterCollection.GetUniqueUsedMeshes();
		List<Vector2[]> uvs = clusterCollection.GetUniqueNewUVs();


		
		UiUpdateProgressBar(0f, PpLanguage.Get("progressBarAssign_title"), "Create Texture");
		
		if (collectionName == ""){
			collectionName = selectionGameObjects[0].name;
		}
		
		GenerateAssetTexture(ref packedTexture);
		Material material = GenerateAssetMaterial(ref packedTexture);

		UiUpdateProgressBar(0f, PpLanguage.Get("progressBarAssign_title"), "Create Texture");

		if (meshes.Length == uvs.Count){
			
			List<GameObject> relatedObjects = clusterCollection.GetUsedGameObjects();

			//STEP CLEAR COMPONENT ARRAYS
			List<PpAssignVariables> assignListComponents 			= new List<PpAssignVariables>();

			for(i=0; i< relatedObjects.Count; i++){

				UiUpdateProgressBar(i,relatedObjects.Count, 2, 1, PpLanguage.Get("progressBarAssign_title"), "Assign UV and texture "+(i+1)+"/"+relatedObjects.Count);
				AssignToGameObject(assignListComponents, relatedObjects[i], material, meshes, uvs);

			}

			//PROCESS COMPONENT ARRAYS

			for(i=0; i< assignListComponents.Count; i++){
				assignListComponents[i].AssignComponent(  material );//NOW ASSIGN VARIABLES
			}

			//ASSIGN NEW UV'S
			for(i=0; i< meshes.Length; i++){

				Mesh m = (Mesh)Instantiate(meshes[i]);
				m.uv = uvs[i];//USE NEW PACKED UV'S

				GenerateAssetMesh( ref m, i);//SAVE THIS MESH


				for(j=0; j< assignListComponents.Count; j++){
					if (assignListComponents[j].orgMesh == meshes[i] || assignListComponents[j].mesh == meshes[i]  ){

						//NOW ASSIGN 
						PpComponent cmp = assignListComponents[j].gameObject.GetComponent<PpComponent>();
						cmp.originalMesh = meshes[i];
						cmp.SetMesh( m );

					}
				}

			}
			
			for(i=0; i< relatedObjects.Count; i++){
				EditorUtility.SetDirty( relatedObjects[i] );//MARK IT DIRTY SO THE NEW VARIABLES WILL BE SAVED WITH THE SCENE
				EditorUtility.SetDirty( relatedObjects[i].GetComponent<PpComponent>() );//MARK IT DIRTY SO THE NEW VARIABLES WILL BE SAVED WITH THE SCENE
			}
			
			instance.ShowNotification(PpLanguage.GetGUI("mainResult_msg_filesDirtySaveMessage"));

			OnSelectionChange();
		}
		
		UiUpdateProgressBar(1f, PpLanguage.Get("progressBarAssign_title"), "");
	
	}
	
	/// <summary>
	/// Assigns the new UV, Material and reference the original Texture, Material and UV's into an component.
	/// </summary>
	private void AssignToGameObject(List<PpAssignVariables> assignListComponents, GameObject go ,Material material, Mesh[] meshes, List<Vector2[]> uvs){
					
		int j;

		Texture2D orgTex;
		Material orgMat;
		Vector2[] orgUv;
		Mesh	orgMesh = null;
		
		PpComponent cmp = go.GetComponent<PpComponent>();
		
		PpAssignVariables assignVars 	= new PpAssignVariables();
		assignVars.gameObject 			= go;
		assignVars.collectionName 		= collectionName;
		assignVars.sizeMax 				= sizeMax;
		assignVars.sizePadding 			= sizePadding;
		assignVars.selectionGameObjects = selectionGameObjects;
		
		
		if (cmp != null){
			orgTex 	= cmp.GetOriginalTexture();
			orgMat 	= cmp.originalMaterial;
			orgUv 	= cmp.uvOriginal;
			orgMesh = cmp.originalMesh;

			DestroyImmediate( cmp );//Destory Component
		}else{
			orgTex 	= PpToolsGameObject.GetTexture(go);
			orgMat 	= PpToolsGameObject.GetMaterial(go);
			orgUv 	= null;
		}
		
		
		//NOW MAKE SURE THAT WE ASSIGN THE ORIGINAL TEXTURE REFERENCE SO WE CAN UNDO THE PACKED TEXTURE LATER
		assignVars.orgTex = orgTex;
		 

		//REFERENCE THE ORIGINAL MATERIAL

		assignVars.orgMat = orgMat;
		assignVars.mat   	= PpToolsGameObject.GetMaterial( go );
		assignVars.mesh   	= PpToolsGameObject.GetMesh( go );		//MESH REFERENCE
		assignVars.orgUv 	= new Vector2[assignVars.mesh.uv.Length];
		assignVars.orgMesh 	= orgMesh;

		if (orgUv != null){
			assignVars.orgUv = orgUv;
		}else{
			System.Array.Copy( PpToolsGameObject.GetUV( go ) , assignVars.orgUv, assignVars.mesh.uv.Length);//DEEP COPY
		}

		//NEW UV'S
		for(j=0; j< meshes.Length; j++){
			if (assignVars.mesh == meshes[j]){//FIND THE MESH REFERENCE	
				assignVars.uvPacked = uvs[j];
				break;
			}
		}

		assignListComponents.Add(assignVars);
	}





	/// <summary>
	/// Create a combined mesh from the current packed state
	/// </summary>
	private Mesh CreateNewCombinedMesh(){
		int i;

		//COLLECT MESHES AND UPDATED UV'S

		Mesh[] meshes = clusterCollection.GetUniqueUsedMeshes();
		List<Vector2[]> uvs = clusterCollection.GetUniqueNewUVs();
		List<GameObject> gos = clusterCollection.GetUsedGameObjects();

		if (meshes.Length == uvs.Count){//USED MESHES


			CombineInstance[] combineArray = new CombineInstance[gos.Count];
			int countTrisTotal = 0;

			for (i = 0; i < gos.Count; i++) {

				GameObject go = gos[i];
			


				//FIND MESH AND CORROSPONDING NEW UV



				Mesh goMesh = PpToolsGameObject.GetMesh(go);
				int idx = Array.IndexOf(meshes, goMesh);


				combineArray[i] = new CombineInstance();
				combineArray[i].mesh = Instantiate(meshes[idx]) as Mesh;//DON'T REfERENCE BUT COPY

				countTrisTotal+= combineArray[i].mesh.vertices.Length;

				combineArray[i].mesh.uv = uvs[ idx ];//OVERRIDE UV'S
				combineArray[i].transform = go.transform.localToWorldMatrix;


			}


			if ( countTrisTotal > UInt16.MaxValue){
				EditorUtility.DisplayDialog("Exceeding Vertex Limit", "The mesh to be generated exceeds the vertex limit of "+UInt16.MaxValue+", it can't be processed as a single Mesh inside Unity", "Ok");
				return null;
			}

			Mesh m = new Mesh();
			m.name = "CombinedMesh";

			m.CombineMeshes(combineArray);//WILL CRASH IF EXCEEDING 64.000 K!!

			return m;
		}

		return null;
		
	}
	
	/// <summary>
	/// Creates a new prefab that contains a single Mesh of the packed result with the packed texture and a new Material.
	/// </summary>
	public void CreateSingleMeshPrefab(){
		int i;
		
		Undo.RegisterSceneUndo("PackerPro Create Combined Mesh");

		try{

			Mesh m = CreateNewCombinedMesh();//TRY AND COMBINE THE MESHES TOGETHER
			if (m != null){

				GameObject newGo = new GameObject(collectionName);
				MeshFilter 		mFilter		= newGo.AddComponent<MeshFilter>();
				mFilter.sharedMesh = m;

				//ADD MESH RENDERER FOR THE MESH
				MeshRenderer 	mRender 	= newGo.AddComponent<MeshRenderer>();


				//SAVE TO ASSETS
				GenerateAssetTexture(ref packedTexture);
				mRender.sharedMaterial = GenerateAssetMaterial(ref packedTexture);

				GenerateAssetMesh(ref m, 0);
				GenerateAssetPrefabMesh(newGo);//SAVE PREFAB

				//SELECT NEW OBJECT
				Selection.objects = new GameObject[]{newGo};
	
			}
		}catch{
			
			EditorUtility.DisplayDialog(PpLanguage.Get("mainResult_errorBuildGeneral",false), PpLanguage.Get("mainResult_errorBuildGeneral", true), "Ok");
			UiUpdateProgressBar(1f,"","");

		}
	}

	/// <summary>
	/// EXPORT OBJ AND PNG ASSETS OUTSIDE OF UNITY
	/// </summary>
	public bool ExportMesh(){
		int i;
		//PpToolsExportObj.

		//1.) GENERATE THE SINGLE MESH....


		try{
			//COLLECT MESHES AND UPDATED UV'S
			Mesh m = CreateNewCombinedMesh();
			if (m != null && packedTexture != null){


				
				string path_obj = EditorUtility.SaveFilePanel(
					"Save model as OBJ",
					"",
					collectionName + ".obj",
					"obj"
				);
				
				if(path_obj.Length != 0) {

					string path_png = path_obj.Replace(".obj",".png");



					TextureImporter textureImporter = null;
					
					string texturePath = AssetDatabase.GetAssetPath(packedTexture);
					if (texturePath != ""){

						textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
						if(!textureImporter.isReadable){
							textureImporter.isReadable = true;//MAKE IT ACCESSABLE
							AssetDatabase.ImportAsset(texturePath);
						}

					}
						

					//GENERATE PNG FILE
					if(packedTexture.format != TextureFormat.ARGB32 && packedTexture.format != TextureFormat.RGB24){
						Texture2D newTexture = new Texture2D(packedTexture.width, packedTexture.height);
						newTexture.SetPixels(packedTexture.GetPixels(0),0);
						packedTexture = newTexture;
					}
					byte[] pngData = packedTexture.EncodeToPNG();
					if (pngData != null){
						System.IO.File.WriteAllBytes(path_png, pngData);
					}


					if (textureImporter != null){
						textureImporter.isReadable = false;
						AssetDatabase.ImportAsset(texturePath);

						packedTexture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;//UPDATE THE TEXTURE IN THE UI
					}







					Material material = new Material(
						"Shader \"Alpha Additive\" {" +
	                 "Properties { _Color (\"Main Color\", Color) = (1,1,1,0) }" +
	                 "SubShader {" +
	                 "	Tags { \"Queue\" = \"Transparent\" }" +
	                 "	Pass {" +
	                 "		Blend One One ZWrite Off ColorMask RGB" +
	                 "		Material { Diffuse [_Color] Ambient [_Color] }" +
	                 "		Lighting On" +
	                 "		SetTexture [_Dummy] { combine primary double, primary }" +
	                 "	}" +
	                 "}" +
	                 "}"
					 );
					material.name = collectionName;


					//BUILD OBJ FILE
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					
					sb.Append("g ").Append(m.name).Append("\n");
					foreach(Vector3 v in m.vertices) {
						sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
					}
					sb.Append("\n");
					foreach(Vector3 v in m.normals) {
						sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
					}
					sb.Append("\n");
					foreach(Vector3 v in m.uv) {
						sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
					}

					//MATERIAL
					sb.Append("\n");
					sb.Append("usemtl ").Append(material.name).Append("\n");
					sb.Append("usemap ").Append(material.name).Append("\n");
					
					int[] triangles = m.GetTriangles(0);
					for (i=0;i<triangles.Length;i+=3) {
						sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
						                        triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
					}

					using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path_obj)) 
					{
						sw.Write( sb.ToString() );
					}

					return true;//RETURN TRUE FOR SUCCESS
				}
			}

				

		}catch{
			
			EditorUtility.DisplayDialog(PpLanguage.Get("mainResult_errorBuildGeneral",false), PpLanguage.Get("mainResult_errorBuildGeneral", true), "Ok");
			UiUpdateProgressBar(1f,"","");
			
		}
		return false;//FAILED
	}
	

	#endregion
	
	#region CREATE ASSETS
	
	public void GenerateAssetTexture(ref Texture2D tex){
		TextureImporter texImporter;

		string path = CreatePath(collectionName+".png");

		
		//CHECK IF IT WAS SAVED BEFORE
		texImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( tex ) ) as TextureImporter;
		if (texImporter != null){//EXISTS ALREADY, IS ALREADY SAVED
			//ALREADY SAVED
			
			AssetDatabase.ImportAsset(texImporter.assetPath);//UPDATE
			
		}else{
			
			byte[] pngData = tex.EncodeToPNG();
	 
			if(pngData != null){
			    System.IO.File.WriteAllBytes(path, pngData);

				AssetDatabase.Refresh();
				DestroyImmediate(tex, true);
				
				
				tex = (Texture2D) (AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)));
				
				//SET TEXTURE SETTINGS....
				texImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( tex ) ) as TextureImporter;
				if (texImporter != null){//IS AN ASSET THAT EXISTS IN THE ASSET FOLDER - NOT JUST MEMORY
					texImporter.textureType = TextureImporterType.Advanced;
					texImporter.maxTextureSize = sizeMax;
					texImporter.isReadable = false;//UNDO AGAIN FOR PERFORMACE
					
					AssetDatabase.ImportAsset(texImporter.assetPath);
				}
			}
			
			
		}
		
		
	}

	/// <summary>
	/// Create a Mesh file in the AssetDatabase
	/// </summary>
	public void GenerateAssetMesh(ref Mesh mesh, int idx){

		string path = CreatePath(collectionName+"_"+idx.ToString("D3")+"_"+mesh.name+".asset");
		AssetDatabase.CreateAsset( mesh, path);

	}





	/// <summary>
	/// Create a Material in the AssetDatabase
	/// </summary>
	public Material GenerateAssetMaterial(ref Texture2D tex){

		if (matAssign == null){
			string path = CreatePath(collectionName+".mat");
			
			matAssign = new Material( Shader.Find("Diffuse") );
			AssetDatabase.CreateAsset(matAssign, path);
		}

		matAssign.mainTexture = tex;
		return matAssign;

	}
	
	/// <summary>
	/// Create a Prefab of the packed Mesh + Textures + Material in the AssetDatabase
	/// </summary>
	public void GenerateAssetPrefabMesh(GameObject go){

		string path = CreatePath(collectionName+".prefab");
		
		UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(path);
		PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);

	}
	
	/// <summary>
	/// Generate a unique path for an AssetDatabase item 
	/// </summary>
	public string CreatePath(string fileName){
		
		string folderName = "Packed Assets";
		
		if (!System.IO.Directory.Exists("Assets/"+folderName)){//CREATE FOLDER IF DOESN'T EXIST YET
			AssetDatabase.CreateFolder("Assets", folderName);
		}
		
		string pathReturn = AssetDatabase.GenerateUniqueAssetPath("Assets/"+folderName+"/"+fileName);
		return pathReturn;
		
	}
	
	#endregion
	
	
	
	
	
	
	#region GETTERS
	
	
	
	
	
	/// <summary>
	/// Return a list of objects that are in the Unity editor selection but not yet part of PackerPro's object collection to pack.
	/// </summary>
	public List<GameObject> getSelectedNewToAdd(Transform[] selection){
		
		List<GameObject> list = new List<GameObject>();
		
		foreach(Transform t in selection){
			
			if ( !IsGameObject_AlreadyInSelection( t.gameObject )){
				list.Add(t.gameObject);
			}
		}
		return list;
		
	}
	
	/// <summary>
	/// Returns if a result has been packed
	/// </summary>
	public bool isCreated{
		get{
			 
			if (selectionGameObjects.Count == 0){
				return false;
			}
			if (packedUsedObjects.Count == 0){
				return false;
			}
			if (packedTexture == null){
				return false;
			}
			

			foreach(GameObject go in selectionGameObjects){
				if(!packedUsedObjects.Contains(go)){
					return false;//OBJECT IS NOT IN THE PREVIOUS PACKED RESULTS
				}
			}
			
			foreach(GameObject go in packedUsedObjects){
				bool contains = false;
				foreach(GameObject goSub in selectionGameObjects){
					if (goSub == go){
						contains = true;
						break;
					}
				}
				
				if (!contains){//OBJECT BAKED IS NOT IN THE LIST
					return false;
				}
			}

			return true;
		}
	}
	
	#endregion
	
	
	#region PRIVATE VARS
		
		private static EditorWindow window;
	
		private Texture2D 			packedTexture;
		private Texture2D			wireframeTexture;
		private List<GameObject> 	packedUsedObjects = new List<GameObject>();
		
		private string stats_message	="";
	
//		private List<MpSelectionMeshSet>	selectionMeshSets		= new List<MpSelectionMeshSet>();
		private List<GameObject>		selectionGameObjects		= new List<GameObject>();
		private List<Texture2D>			selectionGameObjectTextures	= new List<Texture2D>();
		private List<Texture2D>			selectionGameObjectTexturesIgnore	= new List<Texture2D>();

		
		private PpClusterCollection clusterCollection = new PpClusterCollection();

		private int 	width = 32;
		private int 	height = 32;
	
		//ICONS AND UI GRAPHICS
		private 	Texture		texSprDragDrop;
		private 	Texture 	texSprAlphaTile;
		private 	Texture 	texSprFoldTile;
		private 	Texture 	texSprFoldArrowFalse;
		private 	Texture 	texSprFoldArrowTrue;
		private 	Texture 	texSprSMG;

	
		
		private 	Texture		texSprIconPackerPro;
		
		private 	int[] sizes 		= new []{64,128,256,512,1024,2048,4096,8192};
		private 	string[] sizesLabel = new []{"64","128","256","512","1024","2048\tMobile","4096","8192\tDX10+"};
		private  	int sizeMax 		= 2048;
		
		private  	string collectionName	= "Packed Object";
		
		
		
	
	
//		public int idxSizeMax = 6;//1024??
	
	
		private int 	sizePadding 	= 2;
		
		private bool 	isSquare 		= false;
		private bool 	isIgnoreAlpha	= false;
		private bool 	isStack 		= true;
		private bool 	isPadPixels		= true;
		
		private Material matAssign	= null;
		private bool 	isShowWireframe = true;
	
		private float 	stackIfOverlap 	= 0.7f;
	
		private Transform 	texelRefTransform;
		private bool 		isTexelScale = false;
		private bool		isTexelSmoothResize = true;
		private string		texelRefSummary = "";
		
	
		private Vector2 listScrollPos 	= Vector2.zero;
		private Vector2 listScrollTexturesPos 	= Vector2.zero;
		private Vector2 listScrollAdvancedPos 	= Vector2.zero;
		
		
		private bool 	isFoldAdvanced 	= false;
		private bool 	isFoldSelection = true;
		private bool 	isFoldTextures = true;
		private bool 	isFoldResult 	= true;
		
	#endregion
	
	#region STATIC VARS
		public static 	Color colorUiBkg;
		public static	Color colorUiFocus;
		public static	Color colorUiWarning;
	
		public static 	PackerPro 		instance;
		private static 	GameObject[] 	externalInitWithTheseObjects;
	#endregion
		
}
