using UnityEngine;
using UnityEditor;
using System.Collections;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpToolsGui {
	
	
	#region RECTANGLE
	
	/// <summary>
	/// Returns a rectangle based on the texture that fits proportional into the container rectangle. 
	/// </summary>
	public static Rect FitIntoRect(Texture tex, Rect container, float space){
		
		return FitIntoRect(new Rect(0f, 0f, tex.width, tex.height), container, space);
		
	}
	
	/// <summary>
	/// Returns a rectangle based on the rect that fits proportional into the container rectangle. 
	/// </summary>
	public static Rect FitIntoRect(Rect rect, Rect container, float space){
		
		Rect viewRect = new Rect(rect.x, rect.y, rect.width, rect.height);
		
		viewRect.width = Mathf.Min(rect.width, container.width - 2f*space);
		viewRect.height = viewRect.width * (rect.height / rect.width);
		
		if (viewRect.height > (container.height-2f*space)){
			viewRect.height = Mathf.Min(container.height-2f*space, rect.height);
			viewRect.width = viewRect.height * rect.width / rect.height;
		}
		 
		//CENTER VIEW RECT HORIZONTAL
		viewRect.x = container.x + (container.width - viewRect.width)/2f;
		viewRect.y = container.y + (container.height - viewRect.height)/2f;
		
		return viewRect;

	}
	
	#endregion
	
	#region DRAW
	
	/// <summary>
	/// Draws a tiled texture into the rect area
	/// </summary>

	public static void DrawTexTiled (Rect rect, Texture tex){
		GUI.BeginGroup(rect);
		{
			int width  = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);

			for (int y = 0; y < height; y += tex.height){
				for (int x = 0; x < width; x += tex.width){
					
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
					
				}
			}
		}
		GUI.EndGroup();
	}
	
	/// <summary>
	/// Draws a pixel style outline rectangle
	/// </summary>
	
	public static void DrawPixOutline (Rect rect, Color c, float thickness){
		
		if (Event.current.type == EventType.Repaint){
			
			Texture2D tex = EditorGUIUtility.whiteTexture;
			
			GUI.color = c;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, thickness, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, thickness), tex);
			GUI.color = Color.white;
		}
		
	}
	
	/// <summary>
	/// Draws a single pixel line
	/// </summary>
	
	public static void DrawPixLine (int x, int y, int width, Color c, float thickness){
		
		if (Event.current.type == EventType.Repaint){
			
			Texture2D tex = EditorGUIUtility.whiteTexture;
			
			GUI.color = c;
			GUI.DrawTexture(new Rect(x, y, width, thickness), tex);
			
			GUI.color = Color.white;
		}
		
	}
	
	
	#endregion
	
	
	
	
	
}
