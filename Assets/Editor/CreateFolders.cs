using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class CreateFolders : Editor {

	[MenuItem("Spell Maker/Create Folders")]
	
	static void Init()
	{

		if(!Directory.Exists(Application.dataPath + "/Resources/Spells"))
		 Directory.CreateDirectory(Application.dataPath + "/Resources/Spells");

	}
	
}
