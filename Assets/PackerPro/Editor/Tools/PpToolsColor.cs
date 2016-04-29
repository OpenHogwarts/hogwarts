using UnityEngine;
using System.Collections;
//----------------------------------
//            PackerPro
//  Copyright © 2014 SMG Studio
//      http://smgstudio.com
//----------------------------------
public class PpToolsColor {
	
	/// <summary>
	/// Convert a unity color object to a hex string
	/// </summary>
	public static string ColorToHex_24bit(Color32 color){
		
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
		
	}
	/// <summary>
	/// Returns a Unity Color object with a given HEX input e.g. ff0000 for red
	/// </summary>
	public static Color HexToColor_24bit(string hex){
		
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
		
	}
	
	/// <summary>
	/// Return a random color
	/// </summary>
	public static Color Random(){
		return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);
		
	}
	/// <summary>
	/// Get's a unique HSB Color with 75% brightness and 100% saturation between the 0 and 180 degree range
	/// </summary>
	public static Color GetUniqueColorId(float p){
		return PpToolsColor.HsbToColor(p,1f, 0.75f);
	}
	
	public static Color HsbToColor(float hue, float saturation, float brightness){
    	return HsbToColor(hue, saturation, brightness, 1f);
	}
	
	public static Color HsbToColor(float hue, float saturation, float brightness, float alpha){
        float r = brightness;
        float g = brightness;
        float b = brightness;
        if (saturation != 0)
        {
            float max = brightness;
            float dif = brightness * saturation;
            float min = brightness - dif;
 
            float h = hue * 360f;
 
            if (h < 60f)
            {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            }
            else if (h < 120f)
            {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f)
            {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            }
            else if (h < 240f)
            {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            }
            else if (h < 300f)
            {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f)
            {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
        }
 
        return new Color(Mathf.Clamp01(r),Mathf.Clamp01(g),Mathf.Clamp01(b),alpha);
    }
	
	
	
	
}
