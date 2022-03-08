
* Prints Warning if no Scene Validator Found
* All Errors will be logged to the Console

### Future Validators To Implement

* Physics
  * Find all MeshColliders with transforms, or parent transforms with negative scale (these are baked at runtime)
  * Maybe even listen for Debug.LogWarning "Couldn't create a Convex Mesh from source mesh "Blah""
  * Print warning if object is on the Default layer (should really make specific layers and stick to them, Default
    as a catch all leads to issues.
  
* UI Checks
  * Find All Invalid Raycast Targets (Image, Text, TMP_Text, Etc)
  * CanvasRenderer CullTransparentMesh != true
  * Make sure Dont Render Objects with Alpha 0 is checked
  * Make tool that goes over every Canvas in scene and makes sure every RectTransform has scale 1,1,1 and the Z < 100
  * Give warnings about odd scaling
  * Unecessary Graphics Raycasters
  * RectTransforms scale are 1,1,1
  * Button has no Raycast Targets
  
* General Checks
  * Warnings
    * Give warning about any object with non-uniform scaling
    * Objects with disabled mesh filters (for what seems to be no reason)
    * Objects with textures that have no compression
	* Objects with Bad Components that don't exist anymore
  * Errors
    * Objects with normal textures, but textures aren't marked as normal, mip maps
	
	