# Location Loader
Fork of Uncanny_Valley's "Location Loader" for Daggerfall, with extra contributions from Kamer.

Location Loader is a mod that doesn't do anything on its own. What it allows is for other mods containing "location prefabs" and "location instances" to get loaded by this mod, and be loaded into the world of Daggerfall while exploring the outdoors.

# Background

"Location Loader" was first released in May 2018 by dfworkshop.net user Uncanny_Valley, author of Taverns Redone, Convenient Clock, Uncanny Interface, and many others. The original thread can be found here: http://forums.dfworkshop.net/viewtopic.php?f=27&t=1046. It wasn't too impressive for regular Daggerfall players at first, but seasoned modders could immediately tell the potential of this loader.  

![image](https://user-images.githubusercontent.com/5789925/140196140-101ba8dc-29a7-445f-ae2f-fd65c1fe76db.png)

The mod also came with scripts for creating locations directly from the Unity Editor. Still, extensive mods making use of this loader were taking their time to come out. Being able to create locations in one thing, but placing them out there in a game the size of Daggerfall? You'd have to place thousands of them for a player to even find one, maybe. And placing thousands of locations by hand isn't an easy task to do, much less maintain.

It is only in June 2021 that someone released a solution: carademono's GIS Construction Set. It was first announced in this thread: http://forums.dfworkshop.net/viewtopic.php?t=4877. GIS (Geographic Information System) software are tools that specialize in managing extensive geographic data, such as maps of our planet. Building on the open source QGIS, carademono brought this technology to the undisputably extensive geographic data of Daggerfall. With this, a modder can not only view the landscape of the Iliac Bay and its bodies of water, cities, villages, dungeons, even Hazelnut's Basic Roads: they can even place new locations anywhere on the map. With a satellite's view of the landscape and a grid system, users can easily place and keep track of hundreds or thousands of locations added to the world. If Location Loader could create and load new locations, and GISCS could place them, then what stood between a motivated modder and a whole new world?

It is only a month later that Kamer released a new version of World of Daggerfall on Nexus, using this technology. Bandit camps, mountains, shrines, docks... All scattered over Daggerfall - well, the region of Daggerfall anyway, and its latest hegemonic acquisition, Betony. It showed what kind of things would be possible for the future of Daggerfall, at least if you have Kamer's determination and diversity of skills. The results were impressive, from a player's perspective. But it had its issues. 

The mod expanded on UV's Location Loader to add new features, fix certain issues. But Location Loader had always been mostly a proof of concept, not a full polished product. It was missing the features a game modder would need, such as the work that was done to support bandit camps with actual bandits you can fight, and loot you can pilfer. In addition, there were major performance issues on many machines. For Kamer, these issues were hard to fix. He's always had multiple projects to work on, and not nearly enough time to spare. New Daggerfall Unity updates eventually broke the mod, and it had to be removed from Nexus. The Location Loader, at least, needed to be taken by other hands.

This project is the continuation of this effort. To load new locations for the world of Daggerfall, efficiently, and then allow expansion on these locations.
