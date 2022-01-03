Location Prefab Editor
by kaboissonneault

# Description

Location Loader needs two types of data to load new locations: location prefabs, and location instances. The prefab wraps a bunch of objects together to give the template for a location, while the instance puts a copy of a prefab somewhere in the world. For this article, we will focus on how to create or edit prefabs.

Like Uncanny_Valley's interior editor, or the more recent DFU WorldData Editor, the Location Prefab Editor (or just the prefab editor) reuses the scene editing tools offered by Unity. It uses a Unity scene and its scene manipulation tools to help place objects and preview the result.  If you're familiar with any game engine, or the Elder Scrolls Construction Kit, the same principles should apply here. 

Let's get right into it.

# Installation

You will need to download and install the source of Location Loader to get access to the location tools.

1. If you don't have Unity installed already, install **Unity 2019.4.28f1** (assuming Daggerfall Unity 0.12 or 0.13). You allegedly can do this from Unity Hub, but this has never worked for me, so you can find an archive here: https://unity3d.com/get-unity/download/archive. Use "Downloads (Win)" (or your OS) and select "Unity Installer" or "Unity Editor 64-bit", and install it wherever you choose. 
2. Get a Git tool to download Github repositories. If you're not familiar with Git, you can use Github Desktop.
3. If you don't have it already, clone the Daggerfall Unity source repository (https://github.com/Interkarma/daggerfall-unity.git). From Github Desktop, this should be under File -> Clone a repository (Ctrl+Shift+O), and then under the URL tab. This will download the DFU main branch and setup a Git repo on your computer at the location specified under Local path.
	3.5. If you already had Daggerfall Unity, be sure to Pull up to the latest release, at least.
4. From the Daggerfall Unity repository you just cloned, go to Assets/Game/Mods. This is where you install mods, like Location Loader.
5. Clone Location Loader (https://github.com/KABoissonneault/DFU-LocationLoader.git) into Assets/Game/Mods. **This should create a new folder named "DFU-LocationLoader" by default, but be sure to name this "LocationLoader".**
6. Launch Unity 2019.4.28f1 (should be the default one on your computer now), and open the Daggerfall Unity repository.

You should now have all you need to launch the prefab editor. If you've opened the Daggerfall Unity repository, you should see a "Daggerfall Tools" section on the top menu bar. If you've installed Location Loader properly, you should see "Location Instance Editor" and "Location Prefab Editor" under that section, along with other Daggerfall tools such as "Mod Builder" or "WorldData Editor". Open "Location Prefab Editor".

The "Location Prefab Editor" tab should appear somewhere, probably as a small floating window. You can arrange it however you like in Unity fashion, but I recommend keeping it as a floating window about as big as half your screen, and putting it on a second monitor so you can still see the Unity Scene at the same time.

# Playing with an existing prefab

