using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	static public class HighlighterManager
	{
		static private int dirtyFrame = -1;
		static public bool isDirty
		{
			get
			{
				return dirtyFrame == Time.frameCount;
			}
			private set
			{
				dirtyFrame = value ? Time.frameCount : -1;
			}
		}

		static private HashSet<Highlighter> highlighters = new HashSet<Highlighter>();
		
		// 
		static public void Add(Highlighter highlighter)
		{
			highlighters.Add(highlighter);
		}
		
		// 
		static public void Remove(Highlighter instance)
		{
			if (highlighters.Remove(instance) && instance.highlighted)
			{
				isDirty = true;
			}
		}
		
		// 
		static public HashSet<Highlighter>.Enumerator GetEnumerator()
		{
			return highlighters.GetEnumerator();
		}
	}
}