# Changelog

1.4.2
- Fixed hopper interaction with beehives

1.4.1
- Updated and compiled for Valheim version 0.217.24, not working with an older version
- Updated and compiled for BepInExPack 5.4.2200 and Jotunn 2.14.4

1.4.0
- Added support for hoppers and pipes to push items into ballistas
- Internal code simplifications

1.3.1
- Updated Russian localization (thanks Biatlonist76Biatlonist)

1.3.0
- Added more bronze pipe types (up/down/diagonal/...) and reworked existing pipe models

1.2.0
- Added Russian localization (thanks Biatlonist76Biatlonist)
- Fixed an error when trying to place a hopper

1.1.1
- Fixed an error where a hopper with a filer that was initially spawned far away from the player would try to move items

1.1.0
- Updated for Valheim 0.216.9
- Added round robin distribution for pipes and hoppers, this means items will be distributed evenly if a hopper has multiple valid targets

1.0.0
- Added bronze pipes
- Added snappoints for windmill and spinning wheel. Note that both pieces don't support other pieces themselves, meaning without further support the hoppers will break
- Added debug files (ValheimHopper.dll.mdb) to the release, this will make finding future issues easier
- Added pulling from beehives
- Changed build cost of hoppers to be slightly more expensive
- Changed snappoints
- Updated MultiUserChest dependent code, a minimum version of 0.4.0 is now required
- Reworked hopper code to be more deterministic and easier to maintain
- Removed old filter hoppers prefabs

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
