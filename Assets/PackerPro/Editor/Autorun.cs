using UnityEngine;
using UnityEditor;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
[InitializeOnLoad]
public class Autorun{
	
	
	private static double timeStart = 0;
	private static bool startPackerPro = false;

	/// <summary>
	/// Autorun Class that triggers when PackerPro Compiles or runs. When a new version is detected it will launch Packer Pro
	/// </summary>
    static Autorun(){
		 
		
		
		string firstRunVersion = EditorPrefs.GetString( "PackerPro.firstRunVersion", "0.00.0" );
		
		
		
		
		if (firstRunVersion != PackerPro.versionNumber ){//PACKER PRO CHANGED OR IS ABOUT TO START FOR THE FIRST TIME, LAUNCH IT
			
			Debug.Log("PackerPro autorun "+firstRunVersion+" / "+PackerPro.versionNumber);
			 
			EditorPrefs.SetString( "PackerPro.firstRunVersion", PackerPro.versionNumber );
			
			
			startPackerPro = true;
			
			//START A TIMER
			timeStart = EditorApplication.timeSinceStartup;
			EditorApplication.update += Update;
		}
    }
	
	
	public static void Update(){
		
		
		double tDiff = EditorApplication.timeSinceStartup - timeStart;

		if (tDiff > 1.0){
			EditorApplication.update -= Update;//REMOVE TIMER
			
			if (startPackerPro){
				try{
					EditorApplication.ExecuteMenuItem("Assets/Packer Pro");
				}catch{};
			}	
		}
	}
}