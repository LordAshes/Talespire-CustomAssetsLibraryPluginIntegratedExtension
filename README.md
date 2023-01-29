## Custom Assets Library Plugin Integrated Extension
The Custom Assets Library Plugin Integrated Extension adds additional functionality such as auras, effects, filters, transformations,
animations and sound to custom assets. Previously this was part of CALP but it has been moved to it own plugin so that if this more
complex plugin fails due to a BR update, the core CALP code can still function.

## Installing With R2ModMan
This package is designed specifically for R2ModMan and Talespire. 
You can install them via clicking on "Install with Mod Manager" or using the r2modman directly.

## Player Usage
Once enabled, this plugin provides additional functionality as outlined below. 

### Default Keyboard Shortcuts

CALP has a bunch of keyboard shortcuts for triggering additional functionality. The default keys can be reconfigured to other
keys by editing the R2ModMan configuration for the CALP plugin. Keyboard shortcuts are divided into two sub-sections: functionality
triggers and asset spawn modifeirs. 

#### Functionality Triggers

Press these keys while a mini is selected to trigger the corresponding effect if supported.

```
+-----------------+------------------------------------------------------------------+
| Modifier Keys   | Results                                                          |
+-----------------+------------------------------------------------------------------+
+-----------------+------------------------------------------------------------------+
| LControl + 1    | Trigger Animation "Anim01"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 2    | Trigger Animation "Anim02"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 3    | Trigger Animation "Anim03"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 4    | Trigger Animation "Anim04"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 5    | Trigger Animation "Anim05"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 6    | Trigger Animation "Anim06"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 7    | Trigger Animation "Anim07"                                       |
+-----------------+------------------------------------------------------------------+
| LControl + 8    | Prompt for animation name and play it                            |
+-----------------+------------------------------------------------------------------+
| LControl + 9    | Start playing associated audio                                   |
+-----------------+------------------------------------------------------------------+
| LControl + 0    | Stop all (animation, audio and blend shape sequences)            |
+-----------------+------------------------------------------------------------------+
| LAlt + 1        | Toggle Blend Shape Sequence 1                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 2        | Toggle Blend Shape Sequence 2                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 3        | Toggle Blend Shape Sequence 3                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 4        | Toggle Blend Shape Sequence 4                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 5        | Toggle Blend Shape Sequence 5                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 6        | Toggle Blend Shape Sequence 6                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 7        | Toggle Blend Shape Sequence 7                                    |
+-----------------+------------------------------------------------------------------+
| LAlt + 0        | Alternate Stop all (animation, audio and blend shape sequences)* |
+-----------------+------------------------------------------------------------------+
+-----------------+------------------------------------------------------------------+
| RControl + A    | Analyze and log selected minis' renderer information             |
+-----------------+------------------------------------------------------------------+
``` 
* = Same functionality as Stop All just on a different keyboard shortcut

#### Asset Spawn Modifiers

When selecting an asset from the Talespire Library the asset will be loaded based on the kind specified in the asset bundle or as a
creature if the asset does not specify the kind in its info.txt file. However, it is possible to override the kind indicated in the
asset bundle with a different kind by holding down the corresponding modifier key while selecting it from the library and when placing
it down (if applicable). The following tables show what modifier keys select what kind:

```
+-----------------+------------------------------------------------------------------+
| Modifier Keys   | Resulting Kind                                                   |
+-----------------+------------------------------------------------------------------+
+-----------------+------------------------------------------------------------------+
| None            | Uses kind specified in asset bundle or creature if not specified |
+-----------------+------------------------------------------------------------------+
| Left Shift      | Processes spawn as if the kind was effect*                       |
+-----------------+------------------------------------------------------------------+
| Right Shift     | Processes spawn as if the kind was transform*                    |
+-----------------+------------------------------------------------------------------+
| Left Control    | Processes spawn as if the kind was aura                          |
+-----------------+------------------------------------------------------------------+
| Right Control   | Processes spawn as if the kind was creature                      |
+-----------------+------------------------------------------------------------------+
| Left Alt        | Processes spawn as if the kind was filter*                       |
+-----------------+------------------------------------------------------------------+
| Right Alt       | Processes spawn as if the kind was audio                         |
+-----------------+------------------------------------------------------------------+
```

See Custom Assets Library Plugin (CALP) for more details on the meaning of the different kinds.

Note 1: Effect types are currently treated the same a creature types but may be different in the future.
Note 2: Transforms are not yet supported and may be removed completely since core TS has a similar function.
Note 3: Filters are not yet supported but will be soon.
Note 4: When using modifier keys ensure that the modifier key is held down for the entire duration, after
        library selection click, until the desired result is visible. For example, with Auras ensure that the
		key is held down until the aura appears and snaps to the selected mini.

### Asset Types

To avoid duplication, see the CALP documentation for the various differences between the different kind of assets.

## Configuration

The R2ModMan configuration can be edited to tweak settings. The following settings can be changed:

1. Diagnostic mode level. Set to Ultra when getting logs for trouble shooting.
2. Default keys for most CALPIE functions.
3. Show Hide Update Delay: Delay in seconds after an asset loads before Non-TS content is synced. If this value is
   too low, content may not hide on board load. If this value is too high there will be a long delay before content
   is hidden on board load.
   
## Blend Shape Sequencer

To use blend shapes, add the blend shapes property to the info.txt file similar to the following:
```
{
    "name": "Star Gate",
    "kind": "Creature",
    "groupName": "Portals",
    "description": "Star Gate Atlantis",
    "tags": "Star Gate, Atlantis",
    "author": "Lord Ashes",
    "version": "1.0",
    "comment": "Blender 2.9",
    "size": 1.0,
    "assetBase": "Default",
    "blendshapes":
    [
        {
            "elements": 
            [ 
                { "style": 1, "blendShapeIndex": 1, "start": 0.0, "end": 100.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 2, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 3, "start": 0.0, "end": 100.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 4, "start": 0.0, "end": 100.0, "step": 100.0 }
            ]
        },
        {
            "elements": 
            [ 
                { "style": 1, "blendShapeIndex": 1, "start": 100.0, "end": 0.0, "step": -2.0 },
                { "style": 1, "blendShapeIndex": 2, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 3, "start": 0.0, "end": 100.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 4, "start": 0.0, "end": 100.0, "step": 100.0 }
            ]
        },
        {
            "elements": 
            [ 
                { "style": 1, "blendShapeIndex": 1, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 2, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 3, "start": 0.0, "end": 100.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 4, "start": 100.0, "end": 0.0, "step": -5.0 }
            ]
        },
        {
            "elements": 
            [ 
                { "style": 1, "blendShapeIndex": 1, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 2, "blendShapeIndex": 2, "start": 0.0, "end": 100.0, "step": 5.0 },
                { "style": 1, "blendShapeIndex": 3, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 4, "start": 0.0, "end": 0.0, "step": 100.0 }
            ]
        },
        {
            "elements": 
            [ 
                { "style": 1, "blendShapeIndex": 1, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 2, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 3, "start": 0.0, "end": 100.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 4, "start": 0.0, "end": 100.0, "step": 5.0 }
            ]
        },
        {
            "elements": 
            [ 
                { "style": 1, "blendShapeIndex": 1, "start": 0.0, "end": 100.0, "step": 2.0 },
                { "style": 1, "blendShapeIndex": 2, "start": 0.0, "end": 0.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 3, "start": 0.0, "end": 100.0, "step": 100.0 },
                { "style": 1, "blendShapeIndex": 4, "start": 0.0, "end": 100.0, "step": 100 }
            ]
        }
    ]
}
```
Each set of elemets is a Blend Shape Sequence (triggered using LALT+#). The sequence can set one
or more blend shapes. In this case, all blend shapes are being set each time to ensure that the
asset is in a correct state but that is not required. Each entry in the elements section has:

``style`` = 1: Single (from start to end), 2: PingPong (from start to end to start), 3: Loop
``blendShapeIndex`` = The number of the actual blend shape (starting at 1)*
``start`` = The starting value of the blend shape (0 to 100)
``end`` = The ending value of the blend shape (0 to 100)
``step`` = The rate at which the value is changed** 

``*`` = With log set to "ultra", run a blend shape sequence to see the order of the blend shapes
``**`` = Set step to 100 in order to set a value without transitioning from start to end
``***`` = To be able blend shape sequence from 0 to 100 and 100 to 0, two blend shape sequences
          are needed. The Blend Shape Sequence is *not* a toggle.

## Multi Slabs

Paste content with the following format using the Multi Slab Paste:
```
[
	{
		"position": {"x": 0, "y": 0, "z": 0},
		"code": "H4s...QkIgLpAQAAA=="
	}
	,	
	{
		"position": {"x": 20, "y": 20, "z": 20},
		"code": "H4s...TJVTAOAAA"
	}
]
```
Note: Code above has been abridged for documentation purpose. Note that code should not include
the 3 starting and/or ending apostrophes.

## Changelog
```
2.0.0: Implemented proper Aura spawning
1.9.6: Added missing dependency for RadialUI
1.9.5: Bug fix for Multi-Slab paste functionality
1.9.4: Added support to trigger (rigged) animation by name instead of index 
1.9.3: Added work-around implementation of auras. 
1.9.2: Catch AssetLoader exceptions
1.9.1: Partial fix for LOS issues
1.9.0: Re-implemented varaiants. If a mini has variants they will be added as morphs. 
1.8.1: Fix bug with custom multi material assets hiding 
1.8.0: Fix of Cyberpunk Update
1.8.0: Improved GetBaseLoader and GetAssetLoader functions to work with more different assets
1.7.0: Replaced Preliminary Blend Shape functionality with Blend Shape Sequencer
1.6.0: Preliminary Blend Shape functionality
1.6.0: Fix Height Bat hide for Shiv Transparency Trick content
1.5.0: Shiv Transparency Trick content is hidden by hide/reveal, hight bar and hide volumes.
1.4.0: Kind Effect works exactly like Creature. Use the Shiv Transparency Trick to add transparency to minis.
1.3.0: Unified on-load and on-demand code to handle shaders the same way.
1.2.0: Reworked shader setting code to fix "already loaded bug" issue.
1.1.0: Improved modifier key detection
1.1.0: Added more "already loaded" protection
1.1.0: Bug fix with prefab property
1.0.1: Bug fix guess for "already loaded" issue
1.0.1: Bug fix for laggy effects
1.0.1: Bug fix for older registrations without PREFAB key
1.0.0: Supports partial implemntation of Multi Slabs. As of now, Multi-Slabs don't respect the Height Bar. 
1.0.0: Supports Animations and Sound.
1.0.0: Supports Effects including hiding effects by Hide function and Height Bar.
1.0.0: Initial release.
```

## Limitations

Auras (and eventually filters) have been re-implemented using a more proper solution. However, there may be issues
using auras when the base or the aura uses morphs (or CALPIE Variants). This issue is being worked on but there is
not current work around except to avoid using Morphs if they cause problems with your auras.

## Notes

1. Kind Effect is still a legal kind. While it does the same as Kind Creature, it is being supported for backwards
   compatibility and for possible differences in the future.
   
2. Blend Shape speed uses Update cycles as opposed to actual time and thus sequences may be faster or slower depending
   on CPU. Currently there is no compensation for this. In future version a configurable parameter will be added to
   adjust sequencing speed.