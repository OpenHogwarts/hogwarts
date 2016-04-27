using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	public partial class Highlighter : MonoBehaviour
	{
		// Internal class for renderers caching
		private class RendererCache
		{
			private static readonly string sRenderType = "RenderType";
			private static readonly string sOpaque = "Opaque";
			private static readonly string sTransparent = "Transparent";
			private static readonly string sTransparentCutout = "TransparentCutout";
			private static readonly string sMainTex = "_MainTex";

			private struct Data
			{
				public Material material;
				public int submeshIndex;
				public bool transparent;
			}

			private const int opaquePassID = 0;
			private const int transparentPassID = 1;

			public bool visible { get; private set; }

			private GameObject go;
			private Renderer renderer;
			private List<Data> data;

			// Constructor
			public RendererCache(Renderer r, Material sharedOpaqueMaterial, float zTest, float stencilRef)
			{
				data = new List<Data>();
				renderer = r;
				go = r.gameObject;

				Material[] materials = r.sharedMaterials;

				if (materials != null)
				{
					for (int i = 0; i < materials.Length; i++)
					{
						Material sourceMat = materials[i];

						if (sourceMat == null) { continue; }

						Data d = new Data();
						
						string tag = sourceMat.GetTag(sRenderType, true, sOpaque);
						if (tag == sTransparent || tag == sTransparentCutout)
						{
							Material replacementMat = new Material(transparentShader);
							replacementMat.SetFloat(ShaderPropertyID._ZTest, zTest);
							replacementMat.SetFloat(ShaderPropertyID._StencilRef, stencilRef);
							if (r is SpriteRenderer) { replacementMat.SetFloat(ShaderPropertyID._Cull, cullOff); }
							if (sourceMat.HasProperty(ShaderPropertyID._MainTex))
							{
								replacementMat.SetTexture(ShaderPropertyID._MainTex, sourceMat.mainTexture);
								replacementMat.SetTextureOffset(sMainTex, sourceMat.mainTextureOffset);
								replacementMat.SetTextureScale(sMainTex, sourceMat.mainTextureScale);
							}
							
							int cutoff = ShaderPropertyID._Cutoff;
							replacementMat.SetFloat(cutoff, sourceMat.HasProperty(cutoff) ? sourceMat.GetFloat(cutoff) : transparentCutoff);

							d.material = replacementMat;
							d.transparent = true;
						}
						else
						{
							d.material = sharedOpaqueMaterial;
							d.transparent = false;
						}

						d.submeshIndex = i;
						data.Add(d);
					}
				}

				visible = !IsDestroyed() && IsVisible();
			}

			// 
			public bool UpdateVisibility()
			{
				bool visibleNow = !IsDestroyed() && IsVisible();

				if (visible != visibleNow)
				{
					visible = visibleNow;
					return true;
				}
				return false;
			}

			// Fills given command buffer with this highlighter rendering commands
			public bool FillBuffer(CommandBuffer buffer)
			{
				if (IsDestroyed()) { return false; }

				if (IsVisible())
				{
					for (int i = 0, imax = data.Count; i < imax; i++)
					{
						Data d = data[i];
						buffer.DrawRenderer(renderer, d.material, d.submeshIndex);
					}
				}

				return true;
			}

			// Sets given color as highlighting color on all transparent materials of this renderer
			public void SetColorForTransparent(Color clr)
			{
				for (int i = 0, imax = data.Count; i < imax; i++)
				{
					Data d = data[i];
					if (d.transparent)
					{
						d.material.SetColor(ShaderPropertyID._Outline, clr);
					}
				}
			}

			// Sets ZTest parameter on all transparent materials of this renderer
			public void SetZTestForTransparent(float zTest)
			{
				for (int i = 0, imax = data.Count; i < imax; i++)
				{
					Data d = data[i];
					if (d.transparent)
					{
						d.material.SetFloat(ShaderPropertyID._ZTest, zTest);
					}
				}
			}
			
			// Sets Stencil Ref parameter on all transparent materials of this renderer
			public void SetStencilRefForTransparent(float stencilRef)
			{
				for (int i = 0, imax = data.Count; i < imax; i++)
				{
					Data d = data[i];
					if (d.transparent)
					{
						d.material.SetFloat(ShaderPropertyID._StencilRef, stencilRef);
					}
				}
			}

			// 
			private bool IsVisible()
			{
				return renderer.enabled && renderer.isVisible;
			}

			// 
			public bool IsDestroyed()
			{
				return go == null || renderer == null;
			}
		}
	}
}