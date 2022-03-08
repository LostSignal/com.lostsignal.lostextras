
* Stub out GameObjectState
  * Instant
    * [x] ActivateGameObject
    * [x] ActivateBehaviour
    * [x] DeactivateGameObject
    * [x] DeactivateBehaviour
    * [x] ExecuteUnityEvent
    * [ ] SetAnimatorBool
    * [ ] SetAnimatorTrigger
    * [ ] SetGameObjectState

  * Over Time
    * [ ] UpdateAnimatorFloat
    * [ ] UpdateFloatMaterialProperty
    * [ ] UpdateColorMaterialProperty
	
    * [ ] UpdateFloatPropertyBlock
    * [ ] UpdateColorPropertyBlock
  
    * [ ] UpdateGraphicColor
    * [ ] UpdateCanvasGroupAlpha
  
    * [ ] UpdateLocalPosition
    * [ ] UpdateLocalRotation
    * [ ] UpdateLocalScale

```csharp

    // https://gist.github.com/deebrol/02f61b7611fd4eca923776077b92dfc2

	TTarget, TValue

	#pragma warning disable 0649
	[SerializeField] private Renderer renderer;
	
	[SerializeField] private bool useCurrentFloatValue;
	[SerializeField] private float startFloatValue;
	[SerializeField] private float endFloatValue;
	
	[SerializeField] private string propertyName;
	[SerializeField] private float floatValue;
	#pragma warning restore 0649
	
	private MaterialPropertyBlock propertyBlock;
	private float startFloatValue;
	private int propertyId;
	private bool isInitialized;
	
	private void Start()
	{
		if (isInitialized == false)
		{
			this.isInitialized = true;
			this.propertyId = Shader.PropertyToID(this.propertyName);
			this.propertyBlock = new MaterialPropertyBlock();
		}
		
		if (this.useCurrentFloatValue)
		{
			this.startFloatValue = this.renderer.sharedMaterial.GetFloat(
		}
		else
		{
			// Use the user specified value
		}

		this.propertyBlock.SetFloat(this.propertyId, this.startFloatValue);
		this.renderer.SetPropertyBlock(this.block);
	}
	
	private void Update(float progress)
	{
		this.renderer.GetPropertyBlock(this.propertyBlock);
        this.propertyBlock.SetFloat(this.propertyId, Mathf.Lerp(this.startFloatValue, this.floatValue, progress));
        this.renderer.SetPropertyBlock(this.propertyBlock);
	}
	
	
	
	
	public class HavenSetMaterialColor : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private Renderer rendererToSet;
        [SerializeField] private Color color;
        #pragma warning restore 0649

        public void SetColor()
        {
            this.rendererToSet.material.color = this.color;
        }
    }
	
	
```
	
	
	
* Make Showable Component that just uses GameObjectState under the hood with Show and Hide states.

* ---------------

* Clean up LoadBalancingManager and UpdateManager to not use Receipts

* Stub out Lost Events (ScriptableObject Based)

* Stub out Flag System (ScriptableObject Based)
* Stub out PlayerProximity
* Stub out OnPlayerEnterExit

* Stub out Scene/Project/Build Validators
  * Make Build Configs
    * RunSceneValidators (bool HeadlessOnlyBuilds)
    * RunProjectValidator (bool HeadlessOnlyBuilds)
    * RunBuildValidator (bool HeadlessOnlyBuilds)

* First Pass Visual Scripting Nodes
  * Cross Scene Reference
  * Periodic Update
  * Lost Events
  * Pooler.Instantiate OR NetworkManager.Insantiate????
  * OnPlayerEnterExit

* ---------------------------------
  
* Stub out Level Manager
  * Fade Down
  * Pause UpdateManager
  * Load Level
    * Load Chuncks
    * Activate Scenes
  * Wait for Awake/Start/Work Managers to finish
  * Fade Up
  * Start Performance Reporting

* Stub out PerfManager
  * Simply waits for level load and then starts collecting stats and writing to a file
  * When exiting game/switching levels, puts that data into the cloud

* Stub out RemoteInspector
  * Tag Functions with the [RemoteInspector("Name")] attribute and you'll be able to query and for these functions and call them
    * Show Nav Mesh

* Stub out Device/Player/GameSave Data Systems
  * Make MenuItems for clearing this data
  * Remove the LostPlayerPrefs code

* Character
* Health/Damage
* Spawner
  
* --------------------------------

* More Systems To Move Over - Make sure to add them to the "Lost Core Managers" Prefab as well
  * Debug Menu
  * Bootloader
  * PlayFab
    * Should PlayFabManager register Dialogs with the DialogManager?
  * Input Manager
    * Update Input Manager to use Update Manager
      * Only register for OnUpdate if there is anyone listening (basically making it free to use)
  * Ads Manager
  * Audio

* Bootloader Needs an Overhaul
  * Make sure it only works if there are Bootloader settings in the RuntimeConfig
  * Can't take dependency on DialogManager
    * Needs it's own Dialog and MessageBox functionality
	* Needs to handle xr and non-xr... not sure how
  * Move Back to "Lost Core"
  
* Remove LostPlayerPrefs and replace with DeviceDataManager
  * It can use PlayerPrefs under the hood for now.
  
* Update 3rd Party code to just have managers say "Blah" 3rd party package not found, this manager will be ignored.
  * Look at XR Manager for an exmpale
  * Tiinoo
  * Dissonance
  * Add these Managers to Lost Core Managers Prefab

* Have the Pool class flaten the hierachy using IAwake

* Check in new Lost Core / Lost Extras to GitHub
* Make sure Package Mapper works with new source
* Make Lost LBE Package and put in GitHub
  
* Where should Unity Purchasing Manager live?

* Make Bad GUID helper tools (most settings / file list to lost library settings)
  * Can i make a script to fix bad guids?
  * Make file with list of asset files that are affected, and put in the true guids.  unity then clears the cache, sets the metas.  and reimports the files. it can check it's plastic commit id to know when to run again.

* Make sure these tools run often
  * Remove Empty Directories (Run in background thread?)
  * Guid Fixer               (Run in background thread?)
  * Can I test current plastic revision and whether or not run these tools (only if it changes)
  * Only run every hour, check after DomainReload?

* Add Components to "Lost Core Managers" prefab
  * Pooling Manager
  * Add Analytics / Logging Providers?

* ---------------------

* Add 3rd Party SearchableEnum property
* Update 3rd Party GuidManager to Clear it's guids on a Bootloader.Reset (Platform.Reset)
  * Make Visuals Scripting nodes to use these Guid components
  
* Make MessageBox a property of DialogManager
* Move XR Over, but say that XR Interaction Toolkit not found, this manager will be ignored
* Should have Lost Scene / Build Validation
* Move MessageBox and StringInputBox to live under the DialogManager?
* Move as much code as possible out of _LostCore and back into Lost Library
  * Remove Bootloader from LostCore
  * Move Bootloader.Reset to Platform.Reset
  * Remove OnManagersReady (will have whole class devoted to this)

* Move Audio Over?
* Move Gameplay Over?
* Move XR Over?
* Move PlayFab Over?
* How will Bootloader/Releases work in this new paradigm

* Remove all code from the _DEPRICATED folder
  * This mainly involves Removing CoroutineRunner and replacing LostPlayerPrefs with new DeviceDataManager



