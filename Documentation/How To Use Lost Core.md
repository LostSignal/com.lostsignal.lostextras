
1) When you start app, use the following code to instantiate your managers 
```
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void LoadManagers()
{
	// Instantiate prefab from Resources and mark as DontDestroyOnLoad
	//    var instance = GameObject.Instantiate(Resources.Load<GameObject>(path));
    //    instance.name = instance.name.Replace("(Clone)", string.Empty);
    //    DontDestroyOnLoad(instance);
	// 
	// Or load in a Managers scene
	//    SceneManager.LoadScene("Managers", LoadSceneMode.Additive);  // Make sure it's not already loaded before doing this
}
```

2) This is what your code should look like when making classes
```
public class MyClass : MonoBehaviour, IAwake, IStart, IUpdate, IManagersReady
{
    public void Awake() => ManagersReady.Register(this);
	
	public void OnManagersReady()
	{
		AwakeManger.Instance.Register(this);
		StartManager.Instance.Register(this);
	}
	
	public void OnAwake()
	{
		// Do Awake Things...
	}
	
	public void OnStart()
	{
		// Do Start Things...
		
		UpdateManager.Instance.Register(this, "Update Queue Name");
	}
	
	public void OnUpdate(float deltaTime)
	{
	
	}
}
```

3) You should use static fields very very sparenly.  If you do use then, then you should Register for the Platform.OnReset and you should clear them.  This event is fired when leaving PlayMode or "Rebooting" your app (only applicable if you're using the Bootload class).  By doing this, you ensure that turning off DomainReload in play mode won't break your game.

```
public class NetworkTransformAnchor : MonoBehaviour
{
	private static Transform anchorTransform;

	static NetworkTransformAnchor()
	{
		Bootloader.OnReset += Reset;

		static void Reset()
		{
			anchorTransform = null;
		}
	}
}
```

4) If you want to have a list of a type of object, never use GameObject.FindObjectsOfType, use the ObjectTracker class and have your code register/unregister on OnEnable/OnDisable.
```
public class MyClass : MonoBehaviour
{
	public static readonly ObjectTracker<MyClass> MyClasses = new ObjectTracker<MyClass>(20);
	
	private void OnEnable()
	{
		MyClasses.Add(this);
	}
	
	private void OnDisable()
	{
		MyClasses.Remove(this);
	}
}
```

5) Always use LevelManger.Instance.LoadLevel(string levelName), this will do the following:
  * Fade Down
  * Unload level and any chunks
  * Load New Level
  * Load Chuncks
  * Active Scenes One By One
  * Process Queues
    * AwakeManager 
	* StartManager 
	* WorkManager
  * Once all those are empty, it will clean memory and Fade Up
  * Now UpdateManager will start

6) UI
  * If you doing UI, then you'll want to use my Dialog class
  * If you need to have a button use LostButton instead of standard button
  * If you need to set text on a TMP_Text element use my BetterStringBuilder
  * If you're ever needing to display a number or time use IntText, TimespanText, CountDownTimerText

7) Serialization
  * If you ever need to serialize to Json, use JsonUtil.Serialize / JsonUtil.Deserialize, it uses the Unity Package nuget.newtonsoft-json under the hood, but has a few helpful converters defined for converting Unity Color and Unity Vector classes.
  
8) Never use "yield return new WaitForSeconds(float)" again, use "yield return WaitForUtil.Seconds(float)", this will cache it for you.

9) Use LZString class if you ever need to compress data.

10) If you ever need to expose a funtion so you can run it in the editor use the [ExposeInEditor("Button Name")] Attribute

11) Don't use Addressables directly.  Use the LazyAsset system instead.

12) Networking
```

Implement interfaces IGameServerFactory and IGameClientFactory

GameClient IGameClientFactory.CreateGameClientAndConnect(string ip, int port)
{
	var userInfo = UserInfoManager.Instance.GetMyUserInfo();
	userInfo.CustomData.Add("Platform", XRManager.Instance.CurrentDevice.name);

	var gameClient = new GameClient(new LiteNetLibClientTransport(), userInfo, NetworkingManager.PrintDebugOutput)
		.RegisterSubsystem<UnityGameClientSubsystem>()
		.RegisterSubsystem<DissonanceClientSubsystem>();

	gameClient.Connect(ip + ":" + port);

	return gameClient;
}

GameServer IGameServerFactory.CreateGameServerAndStart(int port)
{
	var gameServer = new GameServer(new LiteNetLibServerTransport())
		.RegisterSubsystem<UnityGameServerSubsystem>()
		.RegisterSubsystem<DissonanceServerSubsystem>()
		.RegisterSubsystem<ColorAssignerServerSubsystem>()
		.RegisterSubsystem<ValidatePlayFabSessionTicketSubsystem>();

	gameServer.Start(port);

	return gameServer;
}

```

13) Lost Core Settings has a lot of nice tools, so check out "Lost Core Settings.md" for more info.
