# Item Hopper

## About
Adds hoppers that can transfer and pickup items.

<img src="https://raw.githubusercontent.com/MSchmoecker/ValheimHopper/master/Docs/ShowcaseSmelter.png" height="400" alt="Smelter"/> <img src="https://raw.githubusercontent.com/MSchmoecker/ValheimHopper/master/Docs/ShowcaseMiniSorter.png" height="400" alt="MiniSorter"/>

Different hopper types are available: bronze and iron.
The pieces are found in the hammer crafting tab,
bronze hoppers cost 3 Wood + 1 Bronze Nail, iron hoppers cost 3 Wood + 1 Iron Nail.
Both have the same transfer speed but the bronze hopper has only one slot while the iron hopper has three.

Filter hopper: these can be used to automate item routing.
They will always hold on to the last item in every slot.


## Installation
This mod requires BepInEx, Jötunn and MultiUserChest.\
Extract the content of `ValheimHopper` into the `BepInEx/plugins` folder.

## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/ItemHopper/
- Github: https://github.com/MSchmoecker/ValheimHopper
- Nexus: https://www.nexusmods.com/valheim/mods/1974
- Discord: Margmas#9562

## Credits
Big thanks to Bento#5066 for the hopper models and icons!

## Development
BepInEx must be setup at manual or with r2modman/Thunderstore Mod Manager.
Jötunn must be installed.

Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman/Tunderstore Mod Manager you can set the path too, but this is optional.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>
        <!-- Optional, needs to be the path to a r2modmanPlus profile folder -->
        <R2MODMAN_INSTALL>C:\Users\[user]\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>
        <USE_R2MODMAN_AS_DEPLOY_FOLDER>false</USE_R2MODMAN_AS_DEPLOY_FOLDER>
    </PropertyGroup>
</Project>
```

## Changelog
0.1.0
- Release
