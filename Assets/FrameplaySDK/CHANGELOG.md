# Frameplay Unity SDK Changelog

## [1.23.0] (17-08-2021)

Features:

- Ad Channels: New Synchronized Ad Space structure
- Ad Space Inspector multi-select support
- Camera render-to-texture support

## [1.21.1] (27-05-2021)

Bug Fixes:

- Fixed namespace conflict in StartSession example script
- Fixed PowerVR Rogue GE8320 GPU compatibility issues on Android devices

## [1.20.3] (04-03-2021)

Bug Fixes:

- Static Ad Metrics gathering
- OpenGLES2 Metrics gathering
- Ad texture filtering

## [1.19.0] (14-01-2021)

Features:

- Debug option: Show loading icon on default placeholder when an Ad Space is loading an Ad
- Debug option: Toggle between our custom image loader and Unitys default Image Web Request

Bug Fixes:

- Fixed StartSession language parse issue
- Fixed StartSession & RegisterCamera initialisation issues
- Fixed issue in SDK cleanup
- Improved image decoding stability
- Fixed metrics capture issue for static Ad Spaces
- Fixed SDK event logging issue
- Removed requirement for Mesh Colliders on Ad Spaces

## [1.14.1] (25-11-2020)

Features:

- 60x9 Ratio
- Updated Placeholder Texture
- Universal Windows Platform Support

Bug Fixes:

- Developer Settings display issue on OSX
- Fixed issue with Android devices with arm7 CPUs running Android 6.0 and lower
- Replace .dylib with .bundle extension for Unity 2018 compatibility on OSX

## [1.13.4] (14-10-2020)

Features:

- Orthographic camera support
- Removed default AdSpace RigidBody component

Bug Fixes:

- Fixed bug with Ad display in Unity 2018 with GLES2

## [1.12.0] (22-09-2020)

Features:

- Added Frameplay file menu

Bug Fixes:

- Fixed iOS 14 issues

## [1.11.0] (22-09-2020)

Bug Fixes:

- Fixed metrics collection issue

## [1.10.1] (2020-09-16)

Features:

- .NET 3.5 Support
- Removed Ad Space Instance Type

Bug Fixes:

- StartSession namespace fix

Optimisations:

- Image processing optimisations

## [1.9.0] (2020-09-02)

Features:

- Simplified Unity integration and the Frameplay Start Session API call

Bug Fixes:

- Fixed index out of range bug

## [1.8.1] (2020-08-17)

Features:

- Unity 2020 support

Optimisations:

- Performance optimisations in Metrics processing system

## [1.8.0] (2020-07-29)

Features:

- StartSession API update and added Player class
- Added Player Privacy Toggle

Bug Fixes:

- Fixed bug when propagating Ad Space property values from Prefabs to Scene instances
- Fixed bug prematurely hitting the max Ad Space limit

## [1.7.1] (2020-07-15)

Features:

- Introduced Synchronized Ad Spaces
- Minimised GC allocation of metrics web requests
- Reduced the the amount of uploaded data in metrics web requests

## [1.6.2] (2020-04-27)

Bug Fixes:

- Fixed bug building android ARM64

## [1.6.1] (2020-04-24)

Bug Fixes:

- Fixed bug building IL2CPP

## [1.5.2] (2020-04-01)

Features:

- Performance upgrade to WebGL platforms
- Ad textures are now decompressed & uploaded to the GPU asynchronously

Bug Fixes:

- Data Asset Display Bug fix in Unity 2018

## [1.4.3] (2020-03-18)

Features:

- Unity Asset Store compatibility

## [1.4.2] (2020-03-18)

Features:

- Added Ad Loaded Unity Event to Ad Space class

## [1.4.1] (2020-03-18)

Features:

- Performance upgrade to Windows, OSX, iOS and Android platforms
- Ad textures are now decompressed & uploaded to the GPU asynchronously

## [1.3.2] (2020-03-02)

Features:

- Data Asset Inspector UI updated to improve visibility of data and speed up the SDK workflow

Optimisations:

- Network download optimisation, advertisements are now down-sampled on the Frameplay server to match the required resolution at run-time

## [1.2.0] (2020-02-17)

Features:

- Global texture quality setting added to Data Asset
- Texture quality setting added to Ad Space

## [1.1.1] (2020-01-07)

Features:

- Ad Space material support for Lightweight Render Pipeline

## [1.1.0] (2020-01-06)

Features:

- Pause functionality added to Ad Spaces
- Updated Ad Space placeholder texture

Optimisations:

- Performance optimisations to metrics gathering system

## [1.0.0] (2019-11-07)

- Initial commit
