using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	public partial class Highlighter : MonoBehaviour
	{
		// Constants (don't touch this!)
		#region Constants
		// 2 * PI constant required for flashing
		private const float doublePI = 2f * Mathf.PI;
		
		// Occlusion color
		private readonly Color occluderColor = new Color(0f, 0f, 0f, 0f);

		// ZTest LEqual
		private const float zTestLessEqual = (float)CompareFunction.LessEqual;
		
		// ZTest Always
		private const float zTestAlways = (float)CompareFunction.Always;

		// Cull Off
		private const float cullOff = (float)CullMode.Off;
		#endregion

		#region Static Fields
		// Global highlighting shaders ZWrite property
		static private float zWrite = -1f;				// Set to unusual default value to force initialization on start

		// Global highlighting shaders Offset Factor property
		static private float offsetFactor = float.NaN;	// Set to unusual default value to force initialization on start

		// Global highlighting shaders Offset Units property
		static private float offsetUnits = float.NaN;	// Set to unusual default value to force initialization on start
		#endregion

		#region Public Fields
		// Current state of highlighting (true if highlighted and visible)
		public bool highlighted { get; private set; }
		#endregion

		#region Private Fields
		// Cached transform component reference
		private Transform tr;

		// Cached Renderers
		private List<RendererCache> highlightableRenderers;

		// Contains frame number, in which Highlighter visibility has been checked
		private int visibilityCheckFrame = -1;

		// Visibility changed in this frame
		private bool visibilityChanged = false;

		// At least 1 renderer is visible in this frame
		private bool visible = false;

		// Materials reinitialization is required flag
		private bool renderersDirty = true;
		
		// Current highlighting color
		private Color currentColor;
		
		// Transition is active flag
		private bool transitionActive = false;
		
		// Current transition value
		private float transitionValue = 0f;
		
		// Flashing frequency (times per second)
		private float flashingFreq = 2f;
		
		// One-frame highlighting flag
		private int _once = 0;
		private bool once
		{
			get { return _once == Time.frameCount; }
			set { _once = value ? Time.frameCount : 0; }
		}
		
		// One-frame highlighting color
		private Color onceColor = Color.red;
		
		// Flashing state flag
		private bool flashing = false;
		
		// Flashing from color
		private Color flashingColorMin = new Color(0f, 1f, 1f, 0f);
		
		// Flashing to color
		private Color flashingColorMax = new Color(0f, 1f, 1f, 1f);
		
		// Constant highlighting state flag
		private bool constantly = false;
		
		// Constant highlighting color
		private Color constantColor = Color.yellow;
		
		// Occluder mode enabled flag
		private bool occluder = false;
		
		// See-through mode flag (should have same initial value with zTest and renderQueue variables!)
		private bool seeThrough = true;
		
		// RenderQueue (0 = Geometry, 1 = Geometry+1 (for seethrough mode on by default), 2 = Geometry+2)
		private int renderQueue = 1;
		
		// Current ZTest value (true = Always, false = LEqual)
		private bool zTest = true;
		
		// Current Stencil Ref value (true = 1, false = 0)
		private bool stencilRef = true;

		// Returns real ZTest float value which will be passed to the materials
		private float zTestFloat { get { return zTest ? zTestAlways : zTestLessEqual; } }
		
		// Returns real Stencil Ref float value which will be passed to the materials
		private float stencilRefFloat { get { return stencilRef ? 1f : 0f; } }

		// Opaque shader cached reference
		static private Shader _opaqueShader;
		static public Shader opaqueShader
		{
			get
			{
				if (_opaqueShader == null)
				{
					_opaqueShader = Shader.Find("Hidden/Highlighted/Opaque");
				}
				return _opaqueShader;
			}
		}
		
		// Transparent shader cached reference
		static private Shader _transparentShader;
		static public Shader transparentShader
		{
			get
			{
				if (_transparentShader == null)
				{
					_transparentShader = Shader.Find("Hidden/Highlighted/Transparent");
				}
				return _transparentShader;
			}
		}
		
		// Shared (for this component) replacement material for opaque geometry highlighting
		private Material _opaqueMaterial;
		private Material opaqueMaterial
		{
			get
			{
				if (_opaqueMaterial == null)
				{
					_opaqueMaterial = new Material(opaqueShader);
					
					// Make sure that ShaderPropertyIDs is initialized
					ShaderPropertyID.Initialize();
					
					// Make sure that shader will have proper default values
					_opaqueMaterial.SetFloat(ShaderPropertyID._ZTest, zTestFloat);
					_opaqueMaterial.SetFloat(ShaderPropertyID._StencilRef, stencilRefFloat);
				}
				return _opaqueMaterial;
			}
		}
		#endregion

		#region MonoBehaviour
		// 
		private void Awake()
		{
			tr = GetComponent<Transform>();
			ShaderPropertyID.Initialize();
		}
		
		// 
		private void OnEnable()
		{
			if (!CheckInstance()) { return; }

			HighlighterManager.Add(this);
		}
		
		// 
		private void OnDisable()
		{
			HighlighterManager.Remove(this);

			// Clear cached renderers
			if (highlightableRenderers != null) { highlightableRenderers.Clear(); }
			
			// Reset highlighting parameters to default values
			renderersDirty = true;
			highlighted = false;
			currentColor = Color.clear;
			transitionActive = false;
			transitionValue = 0f;
			once = false;
			flashing = false;
			constantly = false;
			occluder = false;
			seeThrough = false;

			/* 
			// Reset custom parameters of the highlighting
			onceColor = Color.red;
			flashingColorMin = new Color(0f, 1f, 1f, 0f);
			flashingColorMax = new Color(0f, 1f, 1f, 1f);
			flashingFreq = 2f;
			constantColor = Color.yellow;
			*/
		}

		// 
		private void Update()
		{
			// Update transition value
			PerformTransition();
		}
		#endregion

		#region Public Methods
		// Returns true in case CommandBuffer should be rebuilt
		public bool UpdateHighlighting(bool isDepthAvailable)
		{
			bool wasHighlighted = highlighted;
			bool changed = false;
			
			changed |= UpdateRenderers();
			
			// Is any highlighting mode is enabled?
			highlighted = (once || flashing || constantly || transitionActive);
			
			int rq = 0;
			
			// Render as highlighter
			if (highlighted)
			{
				// ZTest = (seeThrough ? Always : LEqual), Stencil Ref = 1
				UpdateShaderParams(seeThrough, true);
				// RenderQueue = (seeThrough ? Geometry+1 : Geometry)
				rq = seeThrough ? 2 : 0;
			}
			// Render as occluder
			else if (occluder && (seeThrough || !isDepthAvailable))
			{
				// ZTest = (isDepthAvailable ? LEqual : Always), Stencil Ref = seeThrough ? 1 : 0
				UpdateShaderParams(false, seeThrough);

				// RenderQueue = (seeThrough ? Occluder queue : Geometry queue)
				rq = seeThrough ? 1 : 0;

				highlighted = true;
			}

			// In case renderer should be put to another render queue
			if (renderQueue != rq)
			{
				renderQueue = rq;
				changed = true;
			}
			
			if (highlighted)
			{
				changed |= UpdateVisibility();
				
				if (visible)
				{
					UpdateColors();
				}
				else
				{
					highlighted = false;
				}
			}
			
			changed |= (wasHighlighted != highlighted);
			
			return changed;
		}
		
		// Fills given CommandBuffer with this Highlighter rendering commands
		public void FillBuffer(CommandBuffer buffer, int renderQueue)
		{
			if (!highlighted) { return; }
			
			if (this.renderQueue != renderQueue) { return; }
			
			for (int i = highlightableRenderers.Count - 1; i >= 0; i--)
			{
				RendererCache renderer = highlightableRenderers[i];
				if (!renderer.FillBuffer(buffer))
				{
					highlightableRenderers.RemoveAt(i);
				}
			}
		}
		#endregion

		#region Private Methods
		// Allow only single instance of the Highlighter component on a GameObject
		private bool CheckInstance()
		{
			Highlighter[] highlighters = GetComponents<Highlighter>();
			if (highlighters.Length > 1 && highlighters[0] != this)
			{
				enabled = false;
				Debug.LogWarning("HighlightingSystem : Multiple Highlighter components on a single GameObject is not allowed! Highlighter has been disabled on a GameObject with name '" + gameObject.name + "'.");
				return false;
			}
			return true;
		}

		// This method defines the way in which renderers are initialized
		private bool UpdateRenderers()
		{
			if (renderersDirty)
			{
				List<Renderer> renderers = new List<Renderer>();
				
				// Find all renderers which should be controlled by this Highlighter component
				GrabRenderers(tr, ref renderers);
				
				// Cache found renderers
				highlightableRenderers = new List<RendererCache>();
				int l = renderers.Count;
				for (int i = 0; i < l; i++)
				{
					RendererCache cache = new RendererCache(renderers[i], opaqueMaterial, zTestFloat, stencilRefFloat);
					highlightableRenderers.Add(cache);
				}
				
				// Reset
				highlighted = false;
				renderersDirty = false;
				currentColor = Color.clear;
				
				return true;
			}
			else
			{
				// To avoid null-reference exceptions when cached GameObject or Renderer has been removed but ReinitMaterials wasn't called
				bool changed = false;
				for (int i = highlightableRenderers.Count - 1; i >= 0; i--)
				{
					if (highlightableRenderers[i].IsDestroyed())
					{
						highlightableRenderers.RemoveAt(i);
						changed = true;
					}
				}
				return changed;
			}
		}

		// Returns true in case visibility changed in this frame
		private bool UpdateVisibility()
		{
			if (visibilityCheckFrame == Time.frameCount) { return visibilityChanged; }
			visibilityCheckFrame = Time.frameCount;

			visible = false;
			visibilityChanged = false;
			for (int i = 0, imax = highlightableRenderers.Count; i < imax; i++)
			{
				RendererCache rc = highlightableRenderers[i];
				visibilityChanged |= rc.UpdateVisibility();
				visible |= rc.visible;
			}

			return visibilityChanged;
		}

		// Follows hierarchy of objects from t, searches for Renderers and adds them to the list. Breaks if another Highlighter component found
		private void GrabRenderers(Transform t, ref List<Renderer> renderers)
		{
			GameObject g = t.gameObject;
			IEnumerator e;
			
			// Find all Renderers of all types on current GameObject g and add them to the renderers list
			for (int i = 0, imax = types.Count; i < imax; i++)
			{
				Component[] c = g.GetComponents(types[i]);
				
				e = c.GetEnumerator();
				while (e.MoveNext())
				{
					renderers.Add(e.Current as Renderer);
				}
			}
			
			// Return if transform t doesn't have any children
			if (t.childCount == 0) { return; }
			
			// Recursively cache renderers on all child GameObjects
			e = t.GetEnumerator();
			while (e.MoveNext())
			{
				Transform childTransform = e.Current as Transform;
				GameObject childGameObject = childTransform.gameObject;
				Highlighter h = childGameObject.GetComponent<Highlighter>();
				
				// Do not cache Renderers of this childTransform in case it has it's own Highlighter component
				if (h != null) { continue; }
				
				GrabRenderers(childTransform, ref renderers);
			}
		}

		// Sets RenderQueue, ZTest and Stencil Ref parameters on all materials of all renderers of this object
		private void UpdateShaderParams(bool zt, bool sr)
		{
			// ZTest
			if (zTest != zt)
			{
				zTest = zt;
				float ztf = zTestFloat;
				opaqueMaterial.SetFloat(ShaderPropertyID._ZTest, ztf);
				for (int i = 0; i < highlightableRenderers.Count; i++) 
				{
					highlightableRenderers[i].SetZTestForTransparent(ztf);
				}
			}
			
			// Stencil Ref
			if (stencilRef != sr)
			{
				stencilRef = sr;
				float srf = stencilRefFloat;
				opaqueMaterial.SetFloat(ShaderPropertyID._StencilRef, srf);
				for (int i = 0; i < highlightableRenderers.Count; i++)
				{
					highlightableRenderers[i].SetStencilRefForTransparent(srf);
				}
			}
		}

		// Update highlighting color if necessary
		private void UpdateColors()
		{
			if (once)
			{
				SetColor(onceColor);
				return;
			}
			
			if (flashing)
			{
				// Flashing frequency is not affected by Time.timeScale
				Color c = Color.Lerp(flashingColorMin, flashingColorMax, 0.5f * Mathf.Sin(Time.realtimeSinceStartup * flashingFreq * doublePI) + 0.5f);
				SetColor(c);
				return;
			}
			
			if (transitionActive)
			{
				Color c = new Color(constantColor.r, constantColor.g, constantColor.b, constantColor.a * transitionValue);
				SetColor(c);
				return;
			}
			else if (constantly)
			{
				SetColor(constantColor);
				return;
			}
			
			if (occluder)
			{
				SetColor(occluderColor);
				return;
			}
		}

		// Set given highlighting color
		private void SetColor(Color value)
		{
			if (currentColor == value) { return; }
			currentColor = value;
			opaqueMaterial.SetColor(ShaderPropertyID._Outline, currentColor);
			for (int i = 0; i < highlightableRenderers.Count; i++)
			{
				highlightableRenderers[i].SetColorForTransparent(currentColor);
			}
		}
		
		// Calculate new transition value if necessary
		private void PerformTransition()
		{
			if (transitionActive == false) { return; }
			
			float targetValue = constantly ? 1f : 0f;
			
			// Is transition finished?
			if (transitionValue == targetValue)
			{
				transitionActive = false;
				return;
			}
			
			if (Time.timeScale != 0f)
			{
				// Calculating delta time untouched by Time.timeScale
				float unscaledDeltaTime = Time.deltaTime / Time.timeScale;
				
				// Calculating new transition value
				transitionValue = Mathf.Clamp01(transitionValue + (constantly ? constantOnSpeed : -constantOffSpeed) * unscaledDeltaTime);
			}
			else { return; }
		}
		#endregion

		#region Static Methods
		// Globally sets ZWrite shader parameter for all highlighting materials
		static public void SetZWrite(float value)
		{
			if (zWrite == value) { return; }
			zWrite = value;
			Shader.SetGlobalFloat(ShaderPropertyID._HighlightingZWrite, zWrite);
		}

		// Globally sets Offset Factor shader parameter for all highlighting materials
		static public void SetOffsetFactor(float value)
		{
			if (offsetFactor == value) { return; }
			offsetFactor = value;
			Shader.SetGlobalFloat(ShaderPropertyID._HighlightingOffsetFactor, offsetFactor);
		}

		// Globally sets Offset Units shader parameter for all highlighting materials
		static public void SetOffsetUnits(float value)
		{
			if (offsetUnits == value) { return; }
			offsetUnits = value;
			Shader.SetGlobalFloat(ShaderPropertyID._HighlightingOffsetUnits, offsetUnits);
		}
		#endregion
	}
}