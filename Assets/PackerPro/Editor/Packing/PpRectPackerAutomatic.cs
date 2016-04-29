using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshPackingResult {
	public float v;
	public int w;
	public int h;
	
	public MeshPackingResult(float v, int w, int h){
		this.v = v;
		this.w = w;
		this.h = h;
		
	}
}

public class PpRectPackerAutomatic {
	
	#region CONSTS

	//THE POSSIBLE PACKING SIZES FOR AN AUTOMATIC DETECTION
	private static int[] texSizes = new int[]{8,16,32,64,128,256,512,1024,2048,4096,8192};
	
	#endregion
	
	private static List<Rect> bmpd = new List<Rect>();

	private static int padding = 2;
	
	
	#region PUBLIC ACCESSABLE VARIABLES
	public static int width 	= texSizes[0];
	public static int height 	= texSizes[0];
	#endregion
	
	
	public static List<PpIntegerRectangle> Pack(List<Rect> rects  ){
		return Pack(rects, 2, 4096, false);
	}
	
	public static List<PpIntegerRectangle> Pack(List<Rect> rects, int padding  ){
		return Pack(rects, padding, 4096, false);
	}
	
	/// <summary>
	/// Pack a list of Rect's with a given padding, maxSize and isSquare option
	/// </summary>
	public static List<PpIntegerRectangle> Pack(List<Rect> bmpd, int padding, int maxSize,  bool isSquare  ){
		
		PpRectPackerAutomatic.bmpd		= bmpd;
		PpRectPackerAutomatic.padding	= padding;
		
		
		
		//SEARCHING WIDTH AND HEIGHT
		width 	= 1; 
		height 	= 1;
		int count 	= 0;
		
		int i; int j;
		
		List<MeshPackingResult> results = new List<MeshPackingResult>();
		
	
		
		//SIZES TO CONSIDER
		List<int> sizes = new List<int>();
		for (i = 0; i < texSizes.Length; i++) {
			if (texSizes[i] <= maxSize) {
				sizes.Add(texSizes[i]);
			}
		}
		
		if (isSquare) {
			
			for (i = 0; i < sizes.Count; i++) {
				count = PackAndReturnPackCount(sizes[i], sizes[i]);
				if (count >= bmpd.Count) {
					results.Add( new MeshPackingResult(
						getPackVolume(i, i, sizes[i], sizes[i]),
						sizes[i],
						sizes[i] 
					));
					break;
				}else {
//					Debug.Log("CAN'T PACK: " + sizes[i] + " = " + count+" , "+ bmpd.Count);
				}
			}
			
		}else{
			
			for (i = 0; i < sizes.Count; i++) {
				for (j = 0; j < sizes.Count; j++) {
					count = PackAndReturnPackCount(sizes[j], sizes[i]);
					if (count >= bmpd.Count) {//TO SMALL, GO BIGGER

						results.Add( new MeshPackingResult(
							getPackVolume(j, i, sizes[j], sizes[i]),
							sizes[j],
							sizes[i] 
						));
						
					}
				}
			}
			
		}

		results.Sort((x, y) => x.v.CompareTo(y.v));//SORT

		if (results.Count > 0) {
			width = results[0].w;
			height = results[0].h;
			PackAndReturnPackCount(width, height);
		}
		
		
		
	
		if (mPacker.rectangleCount == 0 || results.Count == 0) {
			
			
			width 	= 64;
			height 	= 64;

			Debug.Log("COULD NOT PACK SHEET");
			
			return new List<PpIntegerRectangle>();//COULD NOT PACK SHEET
		}else {
			
			
			
			PpIntegerRectangle rect;
			
			//UNDO PADDING
			
			List<PpIntegerRectangle> returnList = new List<PpIntegerRectangle>();
			
			for (i = 0; i < mPacker.rectangleCount; i++){
				rect = mPacker.getRectangle(i);
				rect.width -= padding;
				rect.height-= padding;
				
				returnList.Add(rect);
			}
			
			return returnList;
		}
	}
	
	static private float getPackVolume(int iX, int iY, int w, int h) {
		//return ((w * h) - pow2Size) + (Math.max( iX, iY ) * pow2Size / 8);
		return ((w * h) ) + (Mathf.Max( iX, iY ) * 256f);
	}
	
	
	private static PpRectPacker mPacker = new PpRectPacker(512, 512);
		
	private static int PackAndReturnPackCount(int w, int h){
		int i;
		mPacker.reset(w, h);
		for (i = 0; i < bmpd.Count; i++){
            mPacker.insertRectangle((int)bmpd[i].width+padding, (int)bmpd[i].height+padding, i);
        }
		mPacker.packRectangles(true);

		return mPacker.rectangleCount;
	}
	
	

}
