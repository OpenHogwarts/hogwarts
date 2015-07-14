using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

/*
Base http://forum.unity3d.com/threads/editor-scripting-how-to-add-objects-in-editor-on-click.42097/
 */

[CustomEditor(typeof(WaypointContainer))]
public class WaypointEditor : Editor
{
	
	private static bool m_editMode = false;
	private static int m_count = 0;
	private GameObject m_container;
	
	void OnSceneGUI()
	{
		if (m_editMode)
		{
			if (Event.current.type == EventType.MouseUp)
			{
				Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				RaycastHit hitInfo;
				
				
				if (Physics.Raycast(worldRay, out hitInfo))
				{
					createWaypoint(hitInfo.point);
				}
				
			}
			
			Event.current.Use();
			
		}
	}
	override public void OnInspectorGUI()
	{
		NPC npc = Selection.activeGameObject.GetComponent<NPC>();
		npc.Start(); // we load npcData

		if (m_editMode)
		{
			GUILayout.Label("To add a Waypoint: right-click in the desired position.\n To change the order, edit Waypoint's name.\n");
			if (GUILayout.Button("Stop Editing"))
			{
				WaypointData wp;
				m_editMode = false;

				// destroy old waypoints
				foreach (WaypointData waypoint in Service.db.Select<WaypointData>("FROM " + WaypointData.TABLE_NAME+" WHERE npc==?", npc.Id)) {
					waypoint.delete();
				}

				// save new waypoints
				foreach (Transform child in m_container.transform)
				{
					wp = new WaypointData();
					wp.npc = npc.Id;
					wp.position = child.transform.position;
					wp.create();
				}
				DestroyImmediate(m_container);
				m_count = 0;
			}
		}
		else
		{
			if (GUILayout.Button("Start Editing"))
			{
				m_editMode = true;
				checkContainer();

				// show existing waypoints
				foreach (WaypointData waypoint in npc.waypoints) {
					createWaypoint(waypoint.position);
				}
			}
		}
		
		if (GUILayout.Button("Reset"))
		{
			m_count = 0;
			DestroyImmediate(m_container);
		}
		
	}

	void checkContainer () {
		m_container = GameObject.Find("tmpWaypointContainer");
		
		// create the temporary container
		if (m_container == null) {
			m_container = new GameObject();
			m_container.name = "tmpWaypointContainer";
			m_container.hideFlags = HideFlags.DontSave;
		}
	}
	
	void createWaypoint (Vector3 position)
	{
		checkContainer ();
		GameObject waypoint = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Waypoint.prefab", typeof(GameObject)) as GameObject;
		GameObject waypointInstance = Instantiate(waypoint) as GameObject;
		waypointInstance.hideFlags = HideFlags.DontSave;
		waypointInstance.transform.position = position;
		waypointInstance.name = m_count.ToString("00");
		waypointInstance.transform.parent = m_container.transform;
		
		EditorUtility.SetDirty(waypointInstance);
		
		m_count++;
	}
}
