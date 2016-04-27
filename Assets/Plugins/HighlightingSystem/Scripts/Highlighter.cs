using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	public partial class Highlighter : MonoBehaviour
	{
		#region Editable Parameters
		// Constant highlighting turning on speed (common property for all Highlighters)
		static private float constantOnSpeed = 4.5f;
		
		// Constant highlighting turning off speed (common property for all Highlighters)
		static private float constantOffSpeed = 4f;

		// Default transparency cutoff value (used for shaders without _Cutoff property)
		static private float transparentCutoff = 0.5f;

		// Builtin layer reserved for the highlighting. This layer shouldn't be used for anything else in the project!
		public const int highlightingLayer = 7;

		// Only these types of Renderers will be highlighted
		static public readonly List<System.Type> types = new List<System.Type>()
		{
			typeof(MeshRenderer), 
			typeof(SkinnedMeshRenderer), 
			typeof(SpriteRenderer), 
			typeof(ParticleRenderer), 
			typeof(ParticleSystemRenderer), 
		};
		#endregion

		#region Public Methods
		/// <summary>
		/// Renderers reinitialization. 
		/// Call this method if your highlighted object has changed it's materials, renderers or child objects.
		/// Can be called multiple times per update - renderers reinitialization will occur only once.
		/// </summary>
		public void ReinitMaterials()
		{
			renderersDirty = true;
		}
		
		/// <summary>
		/// Set color for one-frame highlighting mode.
		/// </summary>
		/// <param name='color'>
		/// Highlighting color.
		/// </param>
		public void OnParams(Color color)
		{
			onceColor = color;
		}
		
		/// <summary>
		/// Turn on one-frame highlighting.
		/// </summary>
		public void On()
		{
			// Highlight object only in this frame
			once = true;
		}
		
		/// <summary>
		/// Turn on one-frame highlighting with given color.
		/// Can be called multiple times per update, color only from the latest call will be used.
		/// </summary>
		/// <param name='color'>
		/// Highlighting color.
		/// </param>
		public void On(Color color)
		{
			// Set new color for one-frame highlighting
			onceColor = color;
			On();
		}
		
		/// <summary>
		/// Set flashing parameters.
		/// </summary>
		/// <param name='color1'>
		/// Starting color.
		/// </param>
		/// <param name='color2'>
		/// Ending color.
		/// </param>
		/// <param name='freq'>
		/// Flashing frequency (times per second).
		/// </param>
		public void FlashingParams(Color color1, Color color2, float freq)
		{
			flashingColorMin = color1;
			flashingColorMax = color2;
			flashingFreq = freq;
		}
		
		/// <summary>
		/// Turn on flashing.
		/// </summary>
		public void FlashingOn()
		{
			flashing = true;
		}
		
		/// <summary>
		/// Turn on flashing from color1 to color2.
		/// </summary>
		/// <param name='color1'>
		/// Starting color.
		/// </param>
		/// <param name='color2'>
		/// Ending color.
		/// </param>
		public void FlashingOn(Color color1, Color color2)
		{
			flashingColorMin = color1;
			flashingColorMax = color2;
			FlashingOn();
		}
		
		/// <summary>
		/// Turn on flashing from color1 to color2 with given frequency.
		/// </summary>
		/// <param name='color1'>
		/// Starting color.
		/// </param>
		/// <param name='color2'>
		/// Ending color.
		/// </param>
		/// <param name='freq'>
		/// Flashing frequency (times per second).
		/// </param>
		public void FlashingOn(Color color1, Color color2, float freq)
		{
			flashingFreq = freq;
			FlashingOn(color1, color2);
		}
		
		/// <summary>
		/// Turn on flashing with given frequency.
		/// </summary>
		/// <param name='f'>
		/// Flashing frequency (times per second).
		/// </param>
		public void FlashingOn(float freq)
		{
			flashingFreq = freq;
			FlashingOn();
		}
		
		/// <summary>
		/// Turn off flashing.
		/// </summary>
		public void FlashingOff()
		{
			flashing = false;
		}
		
		/// <summary>
		/// Switch flashing mode.
		/// </summary>
		public void FlashingSwitch()
		{
			flashing = !flashing;
		}
		
		/// <summary>
		/// Set constant highlighting color.
		/// </summary>
		/// <param name='color'>
		/// Constant highlighting color.
		/// </param>
		public void ConstantParams(Color color)
		{
			constantColor = color;
		}
		
		/// <summary>
		/// Fade in constant highlighting.
		/// </summary>
		public void ConstantOn()
		{
			// Enable constant highlighting
			constantly = true;
			// Start transition
			transitionActive = true;
		}
		
		/// <summary>
		/// Fade in constant highlighting with given color.
		/// </summary>
		/// <param name='color'>
		/// Constant highlighting color.
		/// </param>
		public void ConstantOn(Color color)
		{
			// Set constant highlighting color
			constantColor = color;
			ConstantOn();
		}
		
		/// <summary>
		/// Fade out constant highlighting.
		/// </summary>
		public void ConstantOff()
		{
			// Disable constant highlighting
			constantly = false;
			// Start transition
			transitionActive = true;
		}
		
		/// <summary>
		/// Switch Constant Highlighting.
		/// </summary>
		public void ConstantSwitch()
		{
			// Switch constant highlighting
			constantly = !constantly;
			// Start transition
			transitionActive = true;
		}
		
		/// <summary>
		/// Turn on constant highlighting immediately (without fading in).
		/// </summary>
		public void ConstantOnImmediate()
		{
			constantly = true;
			// Set transition value to 1
			transitionValue = 1f;
			// Stop transition
			transitionActive = false;
		}
		
		/// <summary>
		/// Turn on constant highlighting with given color immediately (without fading in).
		/// </summary>
		/// <param name='color'>
		/// Constant highlighting color.
		/// </param>
		public void ConstantOnImmediate(Color color)
		{
			// Set constant highlighting color
			constantColor = color;
			ConstantOnImmediate();
		}
		
		/// <summary>
		/// Turn off constant highlighting immediately (without fading out).
		/// </summary>
		public void ConstantOffImmediate()
		{
			constantly = false;
			// Set transition value to 0
			transitionValue = 0f;
			// Stop transition
			transitionActive = false;
		}
		
		/// <summary>
		/// Switch constant highlighting immediately (without fading in/out).
		/// </summary>
		public void ConstantSwitchImmediate()
		{
			constantly = !constantly;
			// Set transition value to the final value
			transitionValue = constantly ? 1f : 0f;
			// Stop transition
			transitionActive = false;
		}

		/// <summary>
		/// Enable see-through mode
		/// </summary>
		public void SeeThroughOn()
		{
			seeThrough = true;
		}
		
		/// <summary>
		/// Disable see-through mode
		/// </summary>
		public void SeeThroughOff()
		{
			seeThrough = false;
		}
		
		/// <summary>
		/// Switch see-through mode
		/// </summary>
		public void SeeThroughSwitch()
		{
			seeThrough = !seeThrough;
		}
		
		/// <summary>
		/// Enable occluder mode. Non-see-through occluders will be used only in case frame depth buffer is not accessible.
		/// </summary>
		public void OccluderOn()
		{
			occluder = true;
		}
		
		/// <summary>
		/// Disable occluder mode. Non-see-through occluders will be used only in case frame depth buffer is not accessible.
		/// </summary>
		public void OccluderOff()
		{
			occluder = false;
		}
		
		/// <summary>
		/// Switch occluder mode. Non-see-through occluders will be used only in case frame depth buffer is not accessible.
		/// </summary>
		public void OccluderSwitch()
		{
			occluder = !occluder;
		}
		
		/// <summary>
		/// Turn off all types of highlighting. 
		/// </summary>
		public void Off()
		{
			once = false;
			flashing = false;
			constantly = false;
			// Set transition value to 0
			transitionValue = 0f;
			// Stop transition
			transitionActive = false;
		}
		
		/// <summary>
		/// Destroy this Highlighter component.
		/// </summary>
		public void Die()
		{
			Destroy(this);
		}
		#endregion
	}
}