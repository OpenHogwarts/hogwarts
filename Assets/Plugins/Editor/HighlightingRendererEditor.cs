using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HighlightingSystem
{
	[CustomEditor(typeof(HighlightingRenderer))]
	public class HighlightingRendererEditor : HighlightingBaseEditor
	{
		// 
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("Use order of this component (relatively to other Image Effects applied to this camera) to control the point at which highlighting will be applied to the framebuffer. Click on a little gear icon to the right and choose Move Up / Move Down.", MessageType.Info);
			CommonGUI();
		}
	}
}