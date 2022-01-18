<Instruction about prefabs>

There are following 2 prefab folders:

BasicTornado
FireTornado

And each folder have "LowPoly" folder. There are LowPoly prefabs.
LowPoly prefab has LowPoly 3D mesh in its "BodyInner", "BodyOuter gameobject. Check Mesh element in Particle's Renderer tab in inspector. 
If you will use ths asset in low spec device or mobile device, it is recommended to use LowPoly prefab to reduce verts and tris. 

<Instruction about CleanStart.cs script>
"LongTornado", "LongTornado_RoughBase" prefabs have CleanStart.cs script.
Without this script, LongTornado's "TopCloud" object will create some unexpected small clouds after particle started. 
So you need this script for "LongTornado" and "LongTornado_RoughBase" prefabs.

<How to resize prefab>
1. Click a game object
2. Expand game object in hierarchy.
3. Select all child objects by shift + click. But deselect parent object.
4. Change scale of X, Y, Z axis on Transform component by inspector

<Order in layer>
Usually every particle system object's "order in layer" option in Renderer component is 0. But some cloud particle system's "order in layer" option is 1. Because the object having more small digit is located more back side of another objects.
All particle systems have basically "0" or "1" for "order in layer" option.

Thank you for using our asset.

technical support:
oharinth@gmail.com

-------------
Release Note
-------------

Ver 1.0.1
6 Fire Tornado added.
LowPoly Model added for low spec device and mobile device.

Ver 1.0.0
First Release


