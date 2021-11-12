# Location Loader
Fork of Uncanny_Valley's "Location Loader" for Daggerfall, with extra contributions from Kamer.

Location Loader is a mod that doesn't do anything on its own. What it allows is for other mods containing "location prefabs" and "location instances" to get loaded by this mod, and be loaded into the world of Daggerfall while exploring the outdoors.

# Background

"Location Loader" was first released in May 2018 by dfworkshop.net user Uncanny_Valley, author of Taverns Redone, Convenient Clock, Uncanny Interface, and many others. The original thread can be found here: http://forums.dfworkshop.net/viewtopic.php?f=27&t=1046. It wasn't too impressive for regular Daggerfall players at first, but seasoned modders could immediately tell the potential of this loader.  

![image](https://user-images.githubusercontent.com/5789925/140196140-101ba8dc-29a7-445f-ae2f-fd65c1fe76db.png)

The mod also came with scripts for creating locations directly from the Unity Editor. 

Still, extensive mods making use of this loader were taking their time to come out. It was only by October 2019, over a year later, that Kamer shared his work on improving Location Loader, in order to make his mod, World of Daggerfall (originally New Locations of Daggerfall). It was both technical work to add new features to the loader, and making original prefabs to then place by hand over the world. One of the more notable technical features was the bandit camps, with functioning static enemies and loot, like a small outdoors dungeon. Already in the screenshots, we could see tents by a hill after a raid, miscreants in ruins guarding their loot, a secret camp within the mountains, rogues guarding an outpost surrounded by a palissade. 

But even for someone like Kamer, this was a lot of work. Making the locations is already quite the task, but placing them everywhere in a game the size of Daggerfall? You'd have to place thousands of them for a player to even find one, maybe. And placing thousands of locations by hand isn't an easy task to do, much less maintain.

Finally, in June 2021, a solution to this problem finally arrived: carademono's GIS Construction Set. It was first announced publicly in this thread: http://forums.dfworkshop.net/viewtopic.php?t=4877. 

![image](https://user-images.githubusercontent.com/5789925/140239072-0ea9b1a0-3f0b-4f2b-befa-43412a3eb3f3.png)

GIS (Geographic Information System) software are tools that specialize in managing extensive geographic data, such as maps of our planet. Building on the open source QGIS, carademono brought this technology to the undisputably extensive geographic data of Daggerfall. With this, a modder can not only view the landscape of the Iliac Bay and its bodies of water, cities, villages, dungeons, even Hazelnut's Basic Roads: they can even place their new locations anywhere on the map. With a satellite's view of the landscape and a grid system, users can easily place and keep track of hundreds or thousands of locations added to the world. If Location Loader could create and load new locations, and GISCS could place them, then what stood between a motivated modder and a whole new world?

It is only a month later that Kamer released a new version of World of Daggerfall on Nexus, using this technology. It featured all the work Kamer had been doing for over a year, with the bandit camps, mountains, and now shrines and docks, among other things. The new scale of this mod showed what kind of things would be possible for the future of Daggerfall, at least if you have Kamer's determination and diversity of skills. The results were impressive, from a player's perspective. But there was still much work to be done.

In light of this, this project is a continuation of the Location Loader part of this work. It aims to improve the performance, and keep adding new features, so modders can deploy new ideas at scale.

# Basics

Location Loader is a mod for loading new "locations" into the game. In Daggerfall terms, a "location" is anything that has a pixel on the region map. These are usually cities, temples, graveyards, or dungeon entrances. When Location Loader started, this was mostly what the mod was intended for: allowing mods to add new locations. Since then, the scope has extended, and all kinds of objects can be placed all over the world: full-on locations, small points of interests, decoration, landscape, ...

In the context of this mod, a **location prefab** refers to a template for a group of objects that could be placed as one anywhere over the world, not just a classic "location". Similarly, a **location instance** is a concrete group of objects based on a prefab placed somewhere in the world. **Location objects** are the elements that make-up a prefab: models, billboards, editor markers, etc.

Therefore, what mods provide for Location Loader can be either or both prefabs and instances. Prefabs give new types of things to place, instances are what's loaded at runtime while playing Daggerfall.

## Objects

Objects come in multiple types, but their data can be pretty similar.

```
LocationObject
  int type
  string name
  int objectID
  string extraData
  Vector3 pos
  Quaternion rot
  Vector3 scale
```

- type: int
  - 0 for 3d models, 1 for billboards, 2 for editor markers
- name: string
  - For model, this is the id of the Daggerfall model. For example, a ladder would be called "41409".
  - For billboards, this is the archive and record of the texture, with the format `ARCHIVE.RECORD`. For example, a cat is "216.8"
  - For editor markers, this is the record of the marker in the archive 199, with the format `199.RECORD`. For example, a quest marker is "199.11".
- objectID: int
  - Unique id, within the prefab.
- extraData: string
  - Extra data for certain object types. For example, a Monster marker uses this for the enemy ID.
- pos: Vector3
  - Relative 3d position of the object within the prefab
- rot: Quaternion
  - Local rotation of the object. Only necessary and useful for models.
- scale: Vector3
  - Local scale of the object. Only useful for models and billboards.

### Models

Model objects are 3d visible objects which can range from large geometry to small props. 

Mods can change what they look like. Mods can also give custom functionality when the player activates certain models (ex: activating a bed opens the Rest screen).

To know which model IDs are available to use, refer to Daggerfall Modelling from [dfworkshop.net](dfworkshop.net)

### Billboards

2d sprites that always face the camera. In Daggerfall, most trees and vegetations, as well as many small props, are made using billboards. If the given record has multiple frames, the billboard will be animated at 5 frames per second.

Mods can replace any archive, as well as add new archives.

To know which archives and records are available to use, refer to Daggerfall Imaging 2 from [dfworkshop.net](dfworkshop.net)

### Editor Markers

Editor markers are special objects used by Location Loader to take special actions when instantiating the prefab, as well as handling certain actions while the instance is running. These markers are normally used in interiors and dungeons in Daggerfall, but Location Loader allows them to be used inside location instances too. The markers Location Loader considers relevant are:

| Name            | ID  | Works? | Description                                                                       | Extra Data         |
| --------------- | --- | ------ | --------------------------------------------------------------------------------- | ------------------ |
| Enter           | 8   |   N    | Used by dungeons/interiors to determine where the entrance/exit is                |                    |
| Start           | 10  |   N    | Where the player will start if they start a new playthrough in this dungeon       |                    |
| Quest           | 11  |   N    | Where NPCs will appear if spawned in a dungeon or interior from a quest           |                    |
| Random Monster  | 15  |   N    | Random monster chosen from dungeon spawn tables will appear                       |                    |
| Monster         | 16  |   Y    | Monster of the enemyID specified in dataID will appear                            | Enemy ID (integer) |
| Quest Item      | 18  |   N    | Where the quest item will appear if spawned in a dungeon or interior from a quest |                    |
| Random Treasure | 19  |   Y    | A random lootpile will appear there                                               |                    |
| Ladder Bottom   | 21  |   Y    | When a nearby ladder is activated from the top, the player will appear there      |                    |
| Ladder Top      | 22  |   Y    | When a nearby ladder is activated from the bottom, the player will appear there   |                    |

Markers with "Y" under "Works?" can be used in location prefabs and be useful.

## Prefabs

## Instances

# Serialization
