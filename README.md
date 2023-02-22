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
- [Thunderstore](https://valheim.thunderstore.io/package/MSchmoecker/ItemHopper/)
- [Github](https://github.com/MSchmoecker/ValheimHopper)
- [Nexus](https://www.nexusmods.com/valheim/mods/1974)
- Discord: Margmas#9562. Feel free to DM or ping me, for example in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Credits
Big thanks to Bento#5066 for the hopper models and icons!


## Development
See [contributing](https://github.com/MSchmoecker/ValheimHopper/blob/master/CONTRIBUTING.md).


## Changelog
See [changelog](https://github.com/MSchmoecker/ValheimHopper/blob/master/CHANGELOG.md).
