
### Future Thoughts

* Add ability to fail the build 
* Update to check every ScriptableObject and Prefab


### Validators To Create

* Very big audio files that aren't marked as Streaming
* Finding PSDs in Project (using PNGs isntead)
* Finding Resource Files in project (They should not exist)
* Scan all code bad line endings that don't match the project line ending settings

* Mesh 
  * Blend Shapes are off (faster import time)
  * Rig -> Optimize Game Object (true)
  * Optimize Game Objects (True)
  * Read/Write = false
  
* Turn off Rig if not needed
  * Turn off Animator if not needed
  * Animation Comppression
  * Model Compression
  * Texture Compression
  * Optimize Rig
  * Model has Animator
  * Model not set to Optimize Animation (which doesn't expose bones)
  * Model has Expose Bones = true

* Textures
  * Read/Write = false
  * Are Compressed
  * Are less than X MB

* ParticleSystems have a "withChildren" attribute that defaults to true, should be false
  * Look up why ```https://www.youtube.com/watch?v=_wxitgdx-UI``` 4:00ish minutes in

* LazyAssets, Localization Ids, Settings Strings point to real data
