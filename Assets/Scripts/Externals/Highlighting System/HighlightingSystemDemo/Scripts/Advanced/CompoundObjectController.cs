using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompoundObjectController : FlashingController
{
	// Cached transform component
	private Transform tr;
	
	// Cached list of child objects
	private List<GameObject> objects;
	
	private int currentShaderID = 0;
	private string[] shaderNames = new string[] {"Standard", "Standard (Specular setup)", "Bumped Specular"};

	// GUI controls offset
	private int ox = -220;

	// 
	protected override void Start()
	{
		base.Start();

		tr = GetComponent<Transform>();
		objects = new List<GameObject>();
		StartCoroutine(DelayFlashing());
	}

	// 
	void OnGUI()
	{
		int oy = (Screen.height / 2) - 90;
		float newX = Screen.width + ox;
		GUI.Label(new Rect(newX, oy, 500, 100), "Compound object controls:");
		if (GUI.Button(new Rect(newX, oy + 30, 200, 30), "Add Random Primitive")) { AddObject(); }
		if (GUI.Button(new Rect(newX, oy + 70, 200, 30), "Change Material")) { ChangeMaterial(); }
		if (GUI.Button(new Rect(newX, oy + 110, 200, 30), "Change Shader")) { ChangeShader(); }
		if (GUI.Button(new Rect(newX, oy + 150, 200, 30), "Remove Object")) { RemoveObject(); }
	}

	// 
	void AddObject()
	{
		PrimitiveType primitiveType = (PrimitiveType)Random.Range(0, 4);
		GameObject newObject = GameObject.CreatePrimitive(primitiveType);
		Transform newObjectTransform = newObject.GetComponent<Transform>();
		newObjectTransform.parent = tr;
		newObjectTransform.localPosition = Random.insideUnitSphere * 2f;
		objects.Add(newObject);
		
		// Reinitialize highlighting materials, because child objects has changed
		h.ReinitMaterials();
	}

	// 
	void ChangeMaterial()
	{
		if (objects.Count < 1) { AddObject(); }

		currentShaderID++;
		if (currentShaderID >= shaderNames.Length) { currentShaderID = 0; }

		foreach (GameObject obj in objects)
		{
			Renderer renderer = obj.GetComponent<Renderer>();
			Shader newShader = Shader.Find(shaderNames[currentShaderID]);
			renderer.material = new Material(newShader);
		}
		
		// Reinitialize highlightable materials, because material(s) has changed
		h.ReinitMaterials();
	}

	// 
	void ChangeShader()
	{
		if (objects.Count < 1) { AddObject(); }

		currentShaderID++;
		if (currentShaderID >= shaderNames.Length) { currentShaderID = 0; }

		foreach (GameObject obj in objects)
		{
			Renderer renderer = obj.GetComponent<Renderer>();
			Shader newShader = Shader.Find(shaderNames[currentShaderID]);
			renderer.material.shader = newShader;
		}
		
		// Reinitialize highlightable materials, because shader(s) has changed
		h.ReinitMaterials();
	}

	// 
	void RemoveObject()
	{
		if (objects.Count < 1) { return; }
		
		GameObject toRemove = objects[objects.Count-1];
		objects.Remove(toRemove);
		Destroy(toRemove);
		
		// Reinitialize highlighting materials, because child objects has changed
		h.ReinitMaterials();
	}
}
