# BannerlordMinorClanTroopRecruitment

Adds the ability to Recruit Minor Clan Troops as mercenaries. They act like existing mercenaries and refresh every day.

Link to Nexus: [here](https://www.nexusmods.com/mountandblade2bannerlord/mods/1520/)

# Features
Ability recruit Minor Clans either inside of the tavern itself or from the town's tavern game menu.

On new campaign start or load the new mercenaries will be populated in every town.\
Changes the "Mercenary Spawn Towns" options in the mod menu will need a reload to take affect.\
This also means this mod can be removed at anytime and not affect current saves.

# Manual Installing
 
 - Extract the zip file to 
 ```text 
 C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules.
 ```
- Navigate to "Modules > MinorClanTroopRecruitment> bin > Win64_Shipping_Client" in your game files.
- Right click the "MinorClanTroopRecruitment.dll" and click properties
- If you see an unblock at the bottom, click it. (Visual reference: https://www.limilabs.com/blog/unblock-dll-file)
- This Mod needs MCM, Mod Configuration Menu to run. It handles the options screen. It can be downloaded [here](https://www.nexusmods.com/mountandblade2bannerlord/mods/612)
- Start the Bannerlord launcher and then tick MinorClanTroopRecruitment in the Singleplayer > Mods tab.

# Creating your own list of Available Mercs:
On first time loading post 1.1.3 the mod creates a folder located at 
```text
C:\Users\*currentUser\Documents\Mount and Blade II Bannerlord\Configs\ModSettings\MinorClanTroopRecruitment\CustomOptions
```
Inside of which will be the default example custom json files.\
Creating your own json file and placing it in this folder will make it appear as a selectable option.\
Beware that anytime you add or remove a customfile that you should verify your selected option is correct.\
Check out the wiki page [here](https://github.com/sweatty1/BannerlordMinorClanTroopRecruitment/wiki/Bannerlord-Minor-Clan-Troop-Recruitment-Wiki). If interested in modifying an existing one or creating your own.

# Development
 
 The folder structure inside of the Bannerlord module folder would be as follows 
 ```text
- MinorClanTroopRecruitment
	- bin
		- Win64_Shipping_Client
			-- MinorClanTroopRecruitment.dll
	-ModuleData
		- CustomA.json
		- CustomB.json
		- CustomC.json
		- Json Same Culture.json
    - SubModule.xml
```

the SubModule.xml being a copy of the one inside of this projects SubModuleXML folder and the MinorClanTroopRecruitment.dll being from the build output of the project

Easier way todo the above is to follow the install instructions.

The Refernces will be missing. Right click refernces select add then browse and navigate and select the taleworlds.*.dll from
```text
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client
```

Note: some dlls are found in base game Module folders like
```text
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\StoryMode\bin\Win64_Shipping_Client\SotryMode.dll which is not present in the base bin\Win64_Shipping_Client
```

Build a new dll with your changes and move it into the Win64_Shipping_Client folder and run the Bannerlord launcher (just make sure on the mod tab the mod checkbox is ticked)

To remove the manual copy/paste change the build output path to the Win64_Shipping_Client.
