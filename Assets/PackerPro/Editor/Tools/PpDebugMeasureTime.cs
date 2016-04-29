using UnityEngine;
using System.Collections;
using System;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpDebugMeasureTime {

	private static bool isActive = false;
	private static string lastLabel = "";
	private static DateTime lastTimeA;
	
	/// <summary>
	/// Start measuring a performance with a label
	/// </summary>
	public static void Start(string label){
		
		if (isActive){
			Stop();
		}
		
		lastLabel = label;
		isActive = true;
		
		lastTimeA = DateTime.Now;
		
		
	}
	
	/// <summary>
	/// End measuring and log the taken time
	/// </summary>
	public static void Stop(){
		int t = (int)DateTime.Now.Subtract(lastTimeA).TotalMilliseconds;
//		Debug.Log("\t| "+t+"ms | \t"+lastLabel);
		
		lastLabel = "";
		isActive = false;
	}
	
	
}
