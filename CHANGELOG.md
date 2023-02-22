# Changelog

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
