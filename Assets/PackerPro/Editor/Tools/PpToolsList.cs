using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpToolsList {
	
	/// <summary>
	/// Similar method as the Actionscript and JS equivialent of Array.pop(), removes the last item and returns that last item.
	/// </summary>
	public static T Pop<T>(List<T> theList){
		T local = theList[theList.Count - 1];
		theList.RemoveAt(theList.Count - 1);
		return local;
	}
	
	/// <summary>
	/// Similar method as the Actionscript and JS equivialent of Array.push(), pushes a new item to the end of the array
	/// </summary>
	public static void Push<T>(List<T> theList, T item)
	{	
		theList.Add(item);
	}
	
	/// <summary>
	/// Clones the array and returns a new array with the exact same values
	/// </summary>
	public static List<T> Clone<T>(List<T> theList)
	{	
		List<T> clone = new List<T>();
		foreach(T item in theList){
			clone.Add(item);
		}
		
		return clone;
	}
	
}
