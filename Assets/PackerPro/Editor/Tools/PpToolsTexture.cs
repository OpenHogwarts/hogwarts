using UnityEngine;
using System.Collections;

//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpToolsTexture {

	/// <summary>
	/// Draws a rectangle wireframe into a texture
	/// </summary>
	public static void DrawRect(Texture2D tex, PpIntegerRectangle r, Color c){
		
		DrawRect(tex, r.ToRect(), c);
		
	}
	
	/// <summary>
	/// Draws a rectangle wireframe into a texture
	/// </summary>
	public static void DrawRect(Texture2D tex, Rect r, Color c){
		int i;
		int y;
		int x;
		for(i=0; i< r.height;i++){
			
			y = tex.height - ((int)r.y + i);
			
			tex.SetPixel((int)r.x,y		 , c);
			tex.SetPixel((int)r.x + (int)r.width,y , c);
		}
		for(i=0; i< r.width;i++){
			
			x= (int)r.x+ i;
			y = tex.height - ((int)r.y+1);
			tex.SetPixel(x, y , c);
			y = tex.height - ((int)r.y+(int)r.height-1);
			tex.SetPixel(x, y , c);
		}
	}
	
	/// <summary>
	/// Draws a solid rectangle into a texture
	/// </summary>
	public static void DrawSolidRect(Texture2D tex, Rect r, Color c){
		int i; int j;
		int y;
		int x;
		
		for(i=0; i< r.height;i++){
			for(j=0; j< r.width;j++){
				
				x= (int)r.x+ j;
				y = tex.height - ((int)r.y+1+i);
				tex.SetPixel(x, y , c);
			}
		}
	}
	
	
	
	/// <summary>
	/// Draws a line from A to B with the Color c into the texture tex
	/// </summary>
	public static void DrawLine(Texture2D tex, Vector2 A, Vector2 B, Color c ){
		
		int steps = (int)Mathf.Max(Mathf.Abs(B.x - A.x), Mathf.Abs( B.y - A.y));
		float p;
		int x;
		int y;

		for(int i = 0; i < steps;i++){
			
			p = (float)i/(float)steps;

			x = (int)(A.x + p * (B.x - A.x) );
			y = (int)(A.y + p * (B.y - A.y) );
		
		
		
			if (x <= tex.width && y <= tex.height && x >=0 && y >= 0){
				tex.SetPixel(x,y,c);
			}
			
		
		}
		
		
	}
	
	
	/// <summary>
	/// Copy pixels from a rectangle area into specified rectangle area of a Color[] array 
	/// </summary>
	public static void CopyPixelsToColorArray(int canvasW, int canvasH, Rect rectSource, Rect rectTarget, Texture2D texSource, Color[] pixCanvas){
		
		int i;int j;
		int x;int y;
		
		
		int idxTarget;int idxSource;
		
		int width 	= (int)rectSource.width;
		int height 	= (int)rectSource.height;
		int sx = (int)rectTarget.x;
		int sy = (int)rectTarget.y;
		
		Color[] pixCopy = texSource.GetPixels( (int)rectSource.x, (int)rectSource.y, width, height);
		
		
		
		for(i = 0; i< height; i++){
			for(j = 0; j< width; j++){

				x = j + sx;
				y = i + sy;
				
				idxTarget = ( y * canvasW  ) + x;//TARGET ID
		
				x = j;
				y = i;
				
				idxSource = ( y * (int)rectSource.width  ) + x;//SOURCE ID
				
				if (idxTarget < pixCanvas.Length && idxSource < pixCopy.Length){
					pixCanvas[idxTarget] = pixCopy[ idxSource];
				}

			}
		}
		
	}
}
