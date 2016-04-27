using UnityEngine;
using System.Collections;

public class FlashingController : HighlighterController
{
	public Color flashingStartColor = Color.blue;
	public Color flashingEndColor = Color.cyan;
	public float flashingDelay = 2.5f;
	public float flashingFrequency = 2f;

	// 
	protected override void Start()
	{
		base.Start();

		StartCoroutine(DelayFlashing());
	}

	// 
	protected IEnumerator DelayFlashing()
	{
		yield return new WaitForSeconds(flashingDelay);
		
		// Start object flashing after delay
		h.FlashingOn(flashingStartColor, flashingEndColor, flashingFrequency);
	}
}
