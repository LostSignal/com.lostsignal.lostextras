
* Scans the Build Logs for all Scene Validator and Project Validator Errors as well as any Unity errors that occurs.
  * Has settings for posting these finding to Slack

* Scans Logs for Scene Validator Errors

* Scans Logs for Project Validor Errors

* Scans Logs for General Errors
  * Bad Colliders
    * MeshCollider baking errors - Sometimes it creates colliders with more than 256 polygons and fails to bake
  * Bad File GUIDs
  * Base File Casing
  * Materials referencing normal map textures that aren't marked as normal maps in import settings
  * Code Warnings

* Track Unused Assets List

* Track Unused Assets Size

  
### Future Build Validation Thoughts
----------------------------------
* Check out this validator https://github.com/DarrenTsung/DTValidator
* Is OnValidate called at build time?  If so, if I throw a build exception will it fail the build?

  