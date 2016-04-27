using UnityEngine;
using System.Collections;

public class SpectrumController : HighlighterController
{
	public float speed = 200f;
	
	private readonly int period = 1530;
	private float counter = 0f;

	// 
	new void Update()
	{
		base.Update();
		
		int val = (int)counter;
		Color col = new Color(GetColorValue(1020, val), GetColorValue(0, val), GetColorValue(510, val), 1f);
		
		h.ConstantOnImmediate(col);
		
		counter += Time.deltaTime * speed;
		counter %= period;
	}
	
	// Some color spectrum magic
	float GetColorValue(int offset, int x)
	{
		int o = 0;
		x = (x - offset) % period;
		if (x < 0) { x += period; }
		if (x < 255) { o = x; }
		if (x >= 255 && x < 765) { o = 255; }
		if (x >= 765 && x < 1020) { o = 1020 - x; }
		return (float) o / 255f;
	}
}
