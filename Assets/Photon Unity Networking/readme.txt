
Photon Unity Networking (PUN)
	This package is a re-implementation of Unity's Networking, using the Photon Cloud Service.
	Also included: A setup wizard, demo scenes, documentation and editor extensions.


PUN & PUN+
	PUN is the free package of Photon Unity Networking. Export to iOS or Android requires the Unity iOS PRO and Unity Android PRO licenses.
	PUN+ contains special native plugins that allow export to mobiles from Unity Free. You also get a Photon Cloud subscription upgrade. See below.


Android and iOS Exports
	See "PUN & PUN+"
    iOS:    Set the stripping level to "Strip Bytecode" and use "DotNet 2.0 Subset" in the player settings. 
            More aggressive stripping will break the runtime and you can't connect anymore with PUN Free.


Help and Documentation
	Please read the included chm (or pdf).
	Unity Forum Thread (up to date info): http://forum.unity3d.com/threads/101734-Submitting-Photon-Unity-Networking
	Exit Games Forum: http://forum.exitgames.com/viewforum.php?f=17
	Online documentation: http://doc.exitgames.com/en/pun


Integration
	This package adds a editor window "PUN Wizard" for connection setup:
		Menu -> Window -> Photon Unity Networking (shortcut: ALT+P)
	It also adds a commonly used component "PhotonView" to this menu:
		Menu -> Component -> Miscellaneous -> PhotonView (shortcut: ALT+V)
	When imported into a new, empty project, the "PunStartup" script opens the "demo hub" and setup scenes to build.


Clean PUN Import (no demos)
	To get a clean import of PUN and PUN+ into your project, just skip the folders "Demos" and "UtilityScripts".
    UtilityScripts can be useful for repid prototyping but are not needed either.
    "Important Files" are listed below.


Server
	Exit Games Photon can be run on your servers or you can subscribe to our cloud service.
	
	The window "Photon Unity Networking" will help you setup a Photon Cloud account.
	This service is geared towards room-based games and the server cannot be modified.
	Read more about it: http://cloud.exitgames.com

	Alternatively, download the Server SDK and run your own Photon Server.
	The SDK has the binaries to run immediately but also includes the source code and projects
	for the game logic. You can use that as basis to modify and extend it.
	A 100 concurrent user license is free (also for commercial use) per game.
	Read more about it: http://www.exitgames.com/en/OnPremise


PUN+ and Networking Guide Subscriptions
    Follow these steps when you bought an asset that includes an upgrade for a Photon Cloud subscription:
        • Use an existing Photon Cloud Account or register here: https://cloud.exitgames.com/Account/SignUp 
        • Sign in and open the Dashboard. 
        • Select the Subscription you want to upgrade and click the button "Apply Unity Purchase".
        • Enter your Unity Invoice Number and click "Redeem".


Important Files

	Documentation
		PhotonNetwork-Documentation.chm (a pdf is also included)
		changelog.txt

	Extensions & Source
		Photon Unity Networking\Editor\PhotonNetwork\*.*
		Photon Unity Networking\Plugins\PhotonNetwork\*.*
        Plugins\**\Photon*.*
        

	The server-setup will be saved as file (can be moved into any Resources folder and edited in inspector)
		Photon Unity Networking\Resources\PhotonServerSettings.asset

	Demos
		All demos are in separate folders in Photon Unity Networking\Demos\. Delete this folder in your projects.
		Each has a Demo<name>-Scene.
