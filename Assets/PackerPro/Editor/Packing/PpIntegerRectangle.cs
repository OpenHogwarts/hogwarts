using UnityEngine;
using System.Collections;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpIntegerRectangle {
	public int x = 0;
   	public int y = 0;
    public int width = 0;
    public int height = 0;
	
    public int right = 0;
    public int bottom = 0;
    public int id = 0;
	
	/// <summary>
	/// Integer rectangle that is used for the packing algorithm
	/// </summary>
    public PpIntegerRectangle(int x , int y, int width, int height){
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.right = x + width;
        this.bottom = y + height;

    }

	/// <summary>
	/// Convert to Unity float based Rect object
	/// </summary>
	public Rect ToRect(){
		return new Rect(x,y,width, height);
	}
}
