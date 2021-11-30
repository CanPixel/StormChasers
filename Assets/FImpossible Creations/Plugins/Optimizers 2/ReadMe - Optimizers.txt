__________________________________________________________________________________________

Package "Optimizers"
Version 2.0.1

Made by FImpossible Games - Filip Moeglich
https://www.FilipMoeglich.pl
FImpossibleGames@Gmail.com or Filip.Moeglich@Gmail.com

__________________________________________________________________________________________

Unity Connect: https://connect.unity.com/u/5b2e9407880c6425c117fab1
Youtube: https://www.youtube.com/channel/UCDvDWSr6MAu1Qy9vX4w8jkw
Facebook: https://www.facebook.com/FImpossibleGames
Twitter (@FimpossibleG): https://twitter.com/FImpossibleG
Google+: https://plus.google.com/u/3/115325467674876785237
Discord: https://discord.gg/rdD2Yu

__________________________________________________________________________________________

Package Contests:

Optimizers Converter V1 to V2.unitypackage:
Optimizers V1.0.6 with additional GUI elements and code to easily convert old components to new ones.

Optimizers V2 - Demo Scenes.unitypackage
Package contains example scenes, scripts and assets showing how Optimizers can be used.

Optimizers - Assembly Definitions.unitypackage (Supported since Unity 2017)
Assembly Definition files to speed up compilation time of your project (Fimpossible Directories will have no influence on compilation time of whole project)

__________________________________________________________________________________________
Description:

Cull, deactivate and activate if visible, adjust quality basing on distance (LOD) on anything inside the scene of your project!
You can give your project more FPS which can give you possibility to make your scenes more detailed!

Use it for simple optimization, like deactivating only when target object is far away, or use for much more complex
stuff like many LOD levels, fading between levels, using different settings on certain levels for many components at the same time,
switch settings / Cull when object is behind wall or camera looking away.

Optimizers can optimize almost everything, things like Lights, Particle Systems, Terrains, Renderers and much more.
Package also brings components with which you can cull or L.O.D. your script components! (very useful for example for foot IK scripts)

This system is ussing Unity's CullingGroup api and other smart techniques to keep optimization logics in the most performent way.
Optimize static and dynamic objects, Optimizers manager will intelligentlyand progressively adapt to your game performance and work with thread to
not still any fps from your project only give a lot of them!
With easy to use Optimizers' custom inspector window you will set up your culling settings without need to know much about optimizing.

__________________________________________________________________________________________

Changelog:

V2.0.1
- Many small fixes and upgrades
- Added rigidbody optimization support
- Few small changes for inspector GUI (Added duplicate of MaxDistance bar above 'Lod Levels' count slider and few other small elements)
- Added AssemblyDefinitions package which needs remove of previous version from project and reimport again to work (directories paths have been changed)
- There was bug with MultiShapes detection shape radius, now it's fixed

V2.0.0
!!! IMPORTANT INFO FOR VERSION 2.0.0 UPDATE !!!
All optimizers from version 1 need to be converted to version 2 with converter UI which is available under included "Optimizers Converter V1 to V2.unitypackage"

- New GUI Inspector window
- New "Suggest" button and "Auto" parameter for automatic max distance settings
- Camera buttons over LOD ranges - by clicking on the scene view camera moves to selected distance range to check how will look object when switching to new LOD / culling
- The components "Obstacle detection" and "Multi Shape" are removed and features are put into one component "Essential Optimizer" or "Scriptable Optimizer"
- NEW "ESSENTIAL OPTIMIZER" which can't be expanded with scriptable objects but works very stable with prefabing and is recommended to use
- New "Scriptable Optimizer" which is the same like previous "Simple Optimizer" but implements switchable Obstacle detection / Multi shapes
- Multi Shapes working with culling containers (need to be tested more)
- Language support for inspector titles for 5 languages (English, Polish, Russian (translated), Chinese (t), Japanese (t), Korean (t))
- Updated Manual PDF
- Culling Containers browser tab in Optimizers Manager for debugging generated Culling Groups
- Multiple smaller fixes
- Demo scenes packed into unitypackage

V1.0.4
- Now optimized components will appear with names of owner game objects
- When there is no camera on scene there will pop up only one warning instead of spam of errors (also upgrades for automatic new camera assigning)
- Option to set all optimized components disabled / enabled with one button (Nexy to "Diable" button in first component's foldout when you optimize more than 3 components)
- Improved custom MonoBehaviour support, now not supported variable types will be hidden under foldout because this variables are only for info, also there will appear button "Simplify" to not use any variables from MonoBehaviour except "Disable" option
- Some small fixes

V1.0.3
- Important Change: Now "Deactivate Object" is untoggled by default, deactivating and activating whole game objects in many cases was causing performance peaks during culling object if it wasn't in camera view (if you rotated camera)
- Added "Auto" button next to "Max Distance" it will help you define max distance for object, basing on camera's far plane settings, and size of detection sphere
- Automatic detection shapes dimensions now will fit better to scaled objects
- Dynamic optimization method shape now scales with object
- Warning and explanation of "Deactivate Object" toggle
- Warning message when you copy not prefabed objects and without shared settings with optimizers to another scene (LOD Sets will be lost - because they're managed by unity as scene assets, not gameObject serialized data)

V1.0.2
- Important Change: Implemented CullingContainers logics which are significantly boosting performance for culling detection calculations for tones of objects if they're using same count of LOD levels and same distance values - for 'static' and 'effective' methods (if you were using more than 1000 optimizers you should notice difference, also now you can freely use like 100 000 optimizers at the same time! )
- Added possibility to attach transforms to detection spheres in Complex Shape Optimizer component 
IMPORTANT: If you were using complex shape component, select it in project or game view to let component convert previous spheres to new system
- Disabling optimizer is now fully supported [optimizer.enabled = false] (probably you will not need this but some projects could make use of it)
- Static Game Object automatic detection for "Static" optimization method + dialog
- Now light optimizer "Render Mode" enums will have same names like ones inside Light component's inspector window
- Changed drawing for multipliers in light and particle systems LOD settings from 0-1 to 0%-100% ("FPD_Percentage()" instead of "Range()")
- Added helper buttons to sync camera's clipping planes or scene fog settings with render range etc. inside optimizers manager
- Small bug fixes

V1.0.1.3
- Fixed transform scale detection sphere radius preview in scene view during editor mode
- Light transitioning fixes when not using ("change intensity")

V1.0.1.2
- Added toggle in first LOD level window to unlock disabling and editing parameters (experimental)
- Now disabling optimizer component during playmode will prevent changes on optimized objects

V1.0.1.1
- When using nested prefabs (parent object having optimizer and child objects having optimizers) optimizer will not try
adding component onto "To Optimize" list when the same component is in use by other, child optimizer component.
- Added LOG warning about creating optimizer inside prefab mode - it's not yet supported

V1.0.1
- Improved compatibility with Unity 2019
- Added component Optimizer Cleaner to remove unused sub-assets from prefabs generated because of issue with Unity 2019
- Many small changes improving integration
- Entry integration for Asset Pipeline V2