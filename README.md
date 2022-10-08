# Item Hopper
## About
Adds hoppers that can transfer and pickup items.

<img src="https://raw.githubusercontent.com/MSchmoecker/ValheimHopper/master/Docs/ShowcaseSmelter.png" height="400" alt="Smelter"/> <img src="https://raw.githubusercontent.com/MSchmoecker/ValheimHopper/master/Docs/ShowcaseMiniSorter.png" height="400" alt="MiniSorter"/>


## Features
### Real ingame hopper
Different hopper types are available: bronze and iron.
The pieces are found in the hammer crafting tab, both types cost 3 wood and 1 nail of their respective type.

They have the same transfer speed but the bronze hopper has only one slot while the iron hopper has three.

### Individual hopper settings
Every hopper can have it's own setting. They appear in a custom UI when the hopper is opened.
- Filter Items: this can be used for automate item routing.
  The last item will be remembered with a "ghost" item and only this item type will be moved to the hopper.
- Enable Item Dropping: if enabled and the hopper has no target inventory they will dropped like the smelter does for example.
- Enable Item Pickup: if disabled the hopper will not pickup items from the ground.

### Seamless multiplayer
The mod aims to work without interruption or major behavior differences of hoppers in multiplayer.

## Manual Installation
This mod requires BepInEx, Jötunn and MultiUserChest.\
Extract the content of `ValheimHopper` into the `BepInEx/plugins` folder.

The mod must be installed on all clients and the server, otherwise the connection will fail.


## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/ItemHopper/
- Github: https://github.com/MSchmoecker/ValheimHopper
- Nexus: https://www.nexusmods.com/valheim/mods/1974
- Discord: Margmas#9562. Feel free to DM or ping me in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Credits
Big thanks to Bento#5066 for the hopper models and icons!


## Development
See [contributing](https://github.com/MSchmoecker/ValheimHopper/blob/master/CONTRIBUTING.md).


## Changelog
0.3.2
- Fixed a pushing hopper did not respected the filter order of a filter hopper it pushed into. This could lead to an existing filter being overwritten

0.3.1
- Fixed smelter snappoints were slightly too far away, causing placed hoppers to not being supported and break

0.3.0
- Changed filter to a "ghost" item instead of holding on to the last item.
  This also fixed the issue that stacked filter hoppers could not be used for filtering and allows for tool/weapon filtering
- Added option to disable item pickup
- Added more snappoint positions
- Added German localization, improved English localization
- Fixed item loop if hopper are too close together

0.2.1
- Fixed errors when destroying a smelter while a hopper is still attached

0.2.0

**Attention: This update changes how structural integrity and filter hopper work.
Old filter and structural unsupported hoppers in a world will be destroyed when loading the area.
All items will be dropped and will not be lost. Please make a backup of your world before updating, regardless.**

- Added UI to configure hoppers individually, removed extra pieces for filter hoppers
- Added item dropping for hoppers that have no target smelter/chest. Default off, has its own config inside the UI
- Added structural integrity to hopper, this fixes item placement issues with PlanBuild
- Added enforcement of mod presence on the server and clients
- Added Vulkan support
- Added workbench build requirement for hoppers
- Improved snappoints slightly

0.1.1
- Added missing MultiUserChest dependency to Thunderstore

0.1.0
- Release
