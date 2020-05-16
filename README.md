# BannerlordMinorClanTroopRecruitment

Currently Working with 1.4.0 and tested on 1.4.1

Adds the ability to Recruit Minor Clan Troops as mercenaries. They act like existing mercenaries and refresh every day. I found it silly that the only way to access these troops was through prisoner rercuitment. So this is an alternative.

Link to Nexus: [here](https://www.nexusmods.com/mountandblade2bannerlord/mods/1520/)

# Features
Ability recruit Minor Clans either inside of the tavern itself or from the town's tavern game menu.

On new campaign start the clan mercenaries will be populated in every town.
On loading a game it will take a day to populate every town with mercenaries as this mod does not the current minor clan mercs to your save.
This also means this mod can be removed at anytime and not affect current saves.

# Future Features
Limit the spawn to be more culture dependent.
Add scaling for the number of mercs available as time goes on
Make the spawning more rare, so that maybe it can spawn every day.
Or make the spawning be weekly instead of daily.
 
 Add mod menu for options to control the above possible features.
 
 # Manual Installing
 
 - Extract the zip file to ﻿C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules.
    - Make sure that SubModule.xml and the bin folder are now in ﻿C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\MinorClanTroopRecruitment
- Navigate to "Modules > MinorClanTroopRecruitment> bin > Win64_Shipping_Client" in your game files.
- Right click the "MinorClanTroopRecruitment.dll" and click properties
- If you see an unblock at the bottom, click it. (Visual reference: https://www.limilabs.com/blog/unblock-dll-file)
- Start the Bannerlord launcher and then tick MinorClanTroopRecruitment in the Singleplayer > Mods tab.
 
 # Development
 
 The folder structure inside of the Bannerlord module folder would be as follows 
 ```text
- MinorClanTroopRecruitment
	- bin
		- Win64_Shipping_Client
			-- MinorClanTroopRecruitment.dll
    - SubModule.xml
```

the SubModule.xml being a copy of the one inside of this projects SubModuleXML folder and the MinorClanTroopRecruitment.dll being from the build output of the project

You can build a new dll with your changes and move it into the Win64_Shipping_Client folder and run the Bannerlord launcher (just make sure on the mod tab the mod checkbox is ticked)
