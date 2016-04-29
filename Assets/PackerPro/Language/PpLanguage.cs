using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PpLanguage : MonoBehaviour {
	
	private static string data = "";
	private static string[] columnsFirstLine = new string[]{};
	private static List<List<string>> cells;
	
	private static int idxIdLanguage = -1;
	private static int idxIdLanguageContext = -1;
	
	public static string languageId =  "";
	
	public static void Init(){
		data 			= Resources.Load("PackerProLanguages").ToString();

		string[] rows 	= data.Split('\n');
		int i;
		int j;
		
		for(i=0;i < rows.Length; i++){
			
			if (i == 0){
				columnsFirstLine = rows[i].Split('\t');
			}else{
				break;
			}
		}
		

		cells = new List<List<string>>();
			
		for(i=0;i < columnsFirstLine.Length; i++){
			cells.Add( new List<string>( rows.Length ) );
			
			for(j=0;j < rows.Length; j++){
				cells[i].Add("");
			}
			
		}
		
		for(i=0;i < rows.Length; i++){
			string[] col = rows[i].Split('\t');
			for(j=0;j < col.Length; j++){
				cells[j][i] = col[j];
			}
		}
		
	}
	
	public static void Setup(string idLanguage, string idLanguageContext){
		languageId = idLanguage;
		
		idxIdLanguage = System.Array.IndexOf( columnsFirstLine, idLanguage );
		idxIdLanguageContext = System.Array.IndexOf( columnsFirstLine, idLanguageContext );
		
	}
	
	
	public static string Get(string id){
		return Get(id, "", false);
	}
	public static string Get(string id, bool isContext){
		return Get(id, "", isContext);
	}
	public static string Get(string id, string replace){
		return Get(id, replace, false);
	}
	
	
	
	public static string Get(string id, string replace, bool isContext){

		if (cells.Count > 0){
			
			int idx = cells[0].IndexOf( id );
			
			
			if (idx >= 0){
				if (!isContext){
					return (string)(cells[idxIdLanguage][idx]).Replace("@",replace);
				}else{
					return cells[idxIdLanguageContext][idx];
				}
			}
		}
		
		//COULDN'T FIND
		id = "?_"+id;
		
		if (!isContext){
			return (string)id.Replace("@",replace);
		}
		return id;
	}
	
	
	public static GUIContent GetGUI(string id){
		return GetGUI(id, "");
	}
	public static GUIContent GetGUI(string id, string replace){
		
		return new GUIContent( Get(id,replace, false), Get(id,replace, true));
	}

	
}
