# CrewQ
(KSP Mod) Crew Rotation and Variety

This mod is focused around the idea of encouraging the player to keep a deeper roster of crew, and to increase the value of individual experienced crew. 

Features
--------
* Crew go on Vacation after completing a mission, between 7 and 28 days in length, depending on the length of their mission __(configurable)__.
* While on Vacation, crew cannot be sent on missions. Optionally, crew may simply be not automatically selected for missions but can be sent manually. _A penalty for crew pulled off their missions early is planned._
* The assignation of crew to the first crewed part may be disabled, or tweaked to provide a role composition congruent with that part's function. At least one of each role specified by the part (if available), will be chosen before randomly selecting other roles (requested by the part) to fill additional spaces. The first part so crewed will be chosen randomly from the highest available rank of crew members, subsequent automatic 'filling' will be chosen from the least experienced quarter of your roster.
  * 'Mission command', such as the Mk1, Mk1-2 pods will request: Pilot, Engineer, Scientist
  * 'Lander cans', such as the Mk1, Mk2 lander-cans will request: Pilot, Engineer, Scientist
  * 'Science Labs', such as the Mobile Processing Lab will request: Scientist
  * 'Orbital Habitats', such as the Hitchhiker Storage Container will request: Engineer, Scientist
  * Mod-added parts will be treated as 'Mission command' parts, unless otherwise configured.
 
Installation
------------
[Download](https://github.com/fingerboxes/CrewQ/releases) the latest Release or Pre-release. Pre-releases should be used knowing that they contain bugs, and the newest Release will generally be identical to the newest Pre-release.

Once you've downloaded this file, simply unzip it and merge the GameData directory with the one located in your Kerbal Space Program installation directory, overwriting any files if prompted.

Uninstallation
--------------
To uninstall all of my mods, simply remove the Fingerboxes directory located in your GameData directory. To remove a particular mod, remove the relevant sub-directory located in the Fingerboxes directory, you should leave the 'Common' directory unless you are removing all of my mods.

Building
--------
If you would like to build this mod from source, for whatever reason, it has the following dependencies.

###### From Kerbal Space Program:
* Assembly-CSharp.dll
* Assembly-CSharp-firstpass.dll
* UnityEngine.dll

You will need to clone both this repository, and [FingerboxLib](https://github.com/fingerboxes/FingerboxLib). If both are cloned into adjacent directories (../SomeDirectory/CrewQ, ../SomeDirectory/FingerboxLib) and the required dlls from KSP are copied or linked into the common root (../Somedirectory/), you should be able to build this project with no further configuration. The required output is the *combined* contents of both projects 'GameData' directories.

Future Plans
------------
This mod is only the first in a planned series with the goal of making _time_ a resource which you must manage in the same way that you currently manage FUNds, Science!, and Reputation. There are no further plans _for this mod_, but many of the plans talked about previously are likely to appear in their own, seperate, mod, as I feel that they are too far removed from the initial design goals of this mod. I prefer a modular design to a monolithic one, and I would rather that _you, the user_ only need install the features which you want to use. 

The only currently-unimplemented goals for this mod are an API, so that other mods may interact with it, a penalty for ending a crew's vacation early (likely a longer vacation next time), and a 'lite' version that does not contain crew reassignment or vacations, simply removes crew from the VAB and Launch dialogs.

Special Thanks To
-----------------
* TriggerAu - Lots of the GUI code was built using his KSPPluginFramework, an altered version of which is included in FingerboxLib
* nebuchadnezzar - made the applauncher icon
* Darren Thomas - AVC file

License
-------
I am fully committed to free software, and I feel that the concept of imposing licensing requirements on software in general, and mods in particular, beyond simple attribution is absurd. As such, this mod is released under the terms of the MIT License.

###### TL;DR
* You may not hold me liable for any defects in the software or damage caused by the software.
* If you _reuse_ or _modify_ my code, you must include the copyright and license with that code, but it does not need to apply to your entire work - just the components which are based on my work.
* You may _reuse_ or _modify_ my code.
* You may _distribute_ my code.
* You may _change the license_ of my code, so long as you include the original attribution and license text. In other words, you may publish a modified version of my code under a more restrictive license, but you may not publish it under a license which removes my attribution.
* You may do any of the above for _private_, or _commercial_ purposes.

As an aside, I would love to know if you find my work useful enough to modify for your own purposes. That would make my day.
