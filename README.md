# No Sleep For Kaname Date "Fixes"

This gives you the ability to override some quality settings of the game. There are no comparison screenshots, idk how to compare this stuff properly.

Forcing anisotropy breaks the game a little bit? Overriding MSAA sadly breaks transparent materials.

This mod forces high quality soft shadows, 8k shadows for main light, 4k for additional lights, unlimited skin weights. SMAA quality override might not be neccessary, but nice to have.

More options to come in the future, maybe?

## Configuration

Example config with defaults:

```ini
## Settings file was created by plugin No Sleep For Kaname Date Fixes v1.0.0
## Plugin GUID: ns4kd

[Quality]

## Override MSAA levels. -1 - Don't override. !!!BREAKS TRANSPARENT MATERIALS!!!
# Setting type: Int32
# Default value: -1
MSAA = -1

## Override SMAA quality. -1 - Don't override, 0 - disable, 1 - low, 2 - medium, 3 - high
# Setting type: Int32
# Default value: -1
SMAA = 3

# Setting type: Boolean
# Default value: false
NoVSync = false

## Force this anisoLevel. Breaks some textures.
# Setting type: Int32
# Default value: -1
ForceAniso = -1
```