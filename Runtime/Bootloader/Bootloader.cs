//-----------------------------------------------------------------------
// <copyright file="Bootloader.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Lost.BuildConfig;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public delegate void BootloaderDelegate();

    public delegate void BootloaderProgressUpdatedDelegate(float progress);

    public delegate void BootloaderProgressTextUpdatedDelegate(string text);
    
    public enum BootloaderConfigLocation
    {
        RuntimeConfigSettings,
        Releases,
    }

    public enum AssetType
    {
        SceneName,
        SceneAddressable,
        PrefabResources,
        PrefabAddressable,
    }

    [Serializable]
    public class RequiredScene
    {
        [SerializeField] private string sceneName;
        
        [Tooltip("This is optional, if this set, then the scene will be loaded with Addressables.")]
        [SerializeField] private string sceneAddressablesPath;

        public string SceneName
        {
            get => this.sceneName;
            set => this.sceneName = value;
        }

        public string SceneAddressablesPath
        {
            get => this.sceneAddressablesPath;
            set => this.sceneAddressablesPath = value;
        }

        public void Load()
        {
            if (IsSceneLoaded(this.sceneName) == false)
            {
                if (string.IsNullOrWhiteSpace(this.SceneAddressablesPath))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    SceneManager.LoadScene(this.sceneName, LoadSceneMode.Additive);
                }
            }
        }

        public void Unload()
        {
            // if (bootloaderInstance != null)
            // {
            //     if (Application.isPlaying)
            //     {
            //         GameObject.Destroy(bootloaderInstance.gameObject);
            //     }
            //     else
            //     {
            //         GameObject.DestroyImmediate(bootloaderInstance.gameObject);
            //     }
            // }
            // 
            // if (managersInstance != null)
            // {
            //     if (Application.isPlaying)
            //     {
            //         GameObject.Destroy(managersInstance);
            //     }
            //     else
            //     {
            //         GameObject.DestroyImmediate(managersInstance);
            //     }
            // }
        }

        private static GameObject InstantiateResource(string path)
        {
            var instance = GameObject.Instantiate(Resources.Load<GameObject>(path));
            instance.name = instance.name.Replace("(Clone)", string.Empty);
            SceneManager.MoveGameObjectToScene(instance, SceneManager.GetSceneByName(Bootloader.BootloaderSceneName));
            return instance;
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            bool sceneAlreadyLoaded = false;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                sceneAlreadyLoaded |= SceneManager.GetSceneAt(i).name == sceneName;
            }

            return sceneAlreadyLoaded;
        }
    }

    [Serializable]
    public class BootloaderConfig
    {
        [SerializeField] private List<RequiredScene> requiredScenes;

        public List<RequiredScene> RequiredScenes
        {
            get => this.requiredScenes;
            set => this.requiredScenes = value;
        }
    }

    public class Bootloader : MonoBehaviour
    {
        // RuntimeConfig Settings Keys
        public const string BootloaderSceneName = "Bootloader";
        public const string BootloaderConfigLocation = "Bootloader.ConfigLocation";
        public const string BootloaderConfig = "Bootloader.Config";
        public const string BootloaderIgnoreSceneNames = "Bootloader.IgnoreSceneNames";

#pragma warning disable 0649
        [Header("Loading UI")]
        [SerializeField] private BootloaderDialog bootloaderDialog;
        [SerializeField] private bool dontShowLoadingInEditor = true;
        [SerializeField] private float minimumLoadingDialogTime;
        [SerializeField] private Camera loadingCamera;
#pragma warning restore 0649

        private BootloaderConfig bootloaderConfig;

        public static event BootloaderProgressUpdatedDelegate ProgressUpdated;

        public static event BootloaderProgressTextUpdatedDelegate ProgressTextUpdate;

        //// TODO [bgish]: Bring back the Bootloader Resource Path, it should always be just a prefab in resources
        //// TODO [bgish]: Rename Asset to Initial Scenes, they take a SceneName and SceneGUID

        //// TODO [bgish]: Move the Reset/Reboot event back into here
        //// TODO [bgish]: Make the Bootloader.Finsihed event so the UI can react to it

        private bool ShowLoadingInEditor => Application.isEditor == false || this.dontShowLoadingInEditor == false;

        public static void UpdateLoadingText(string newText)
        {
            ProgressTextUpdate?.Invoke(newText);
        }

        public static void Reboot()
        {
            // this.loadingCamera.gameObject.SetActive(true);
            // DialogManager.ForceUpdateDialogCameras(this.loadingCamera);
            // Show Bootloader Dialog
            // Update text to say Shutting Down

            var reboot = new GameObject("Reboot", typeof(EmptyBehaviour));
            var empty = reboot.GetComponent<EmptyBehaviour>();
            GameObject.DontDestroyOnLoad(reboot);
            empty.StartCoroutine(Coroutine(reboot));

            //// TODO [bgish]: May need to create a Reboot prefab that is instantiated with a Camera that renders nothing but black, since 
            ////               Unloading all scene will remove all cameras which can cause unusual behaviour.

            static IEnumerator Coroutine(GameObject rebootGameObject)
            {
                while (SceneManager.sceneCount > 0)
                {
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(0));
                }

                // TODO [bgish]: Once Bootloader has moved back into Core, move this back into Bootloader
                Platform.Reset();

                GameObject.Destroy(rebootGameObject);

                BootBootloader();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootBootloader()
        {
            string bootloaderConfigLocation = RuntimeBuildConfig.Instance.GetString(BootloaderConfigLocation);

            if (string.IsNullOrWhiteSpace(bootloaderConfigLocation) || ShouldRunBootloader() == false)
            {
                return;
            }

            // Parsing the bootloader location
            if (int.TryParse(bootloaderConfigLocation, out int bootloaderLocationIntValue) == false)
            {
                Debug.LogError($"Unable to startup Bootloader.  BootloaderConfigLocation was not a valid int \"{bootloaderConfigLocation}\"");
                return;
            }

            if (bootloaderLocationIntValue != 0 && bootloaderLocationIntValue != 1)
            {
                Debug.LogError($"Unable to startup Bootloader.  BootloaderConfigLocation was not a valid value \"{bootloaderLocationIntValue}\"");
                return;
            }

            if (IsBootloaderOpen() == false)
            {
                SceneManager.LoadScene(BootloaderSceneName, LoadSceneMode.Additive);
            }

            static bool ShouldRunBootloader()
            {
                var ignoreSceneNamesString = RuntimeBuildConfig.Instance.GetString(BootloaderIgnoreSceneNames);
                var ignoreScenes = string.IsNullOrWhiteSpace(ignoreSceneNamesString) ? Array.Empty<string>() : ignoreSceneNamesString.Split(';');
                var activeSceneName = SceneManager.GetActiveScene().name;

                foreach (var sceneToIgnore in ignoreScenes)
                {
                    if (activeSceneName == sceneToIgnore)
                    {
                        return false;
                    }
                }

                return true;
            }

            static bool IsBootloaderOpen()
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    if (SceneManager.GetSceneAt(i).name == Bootloader.BootloaderSceneName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
      
        private void Start()
        {
            this.StartCoroutine(this.Bootup());
        }

        // NOTE [bgish]: Make sure this gets called when exiting PlayMode
        private void OnDestroy()
        {
            if (this.bootloaderConfig?.RequiredScenes?.Count > 0)
            {
                foreach (var asset in this.bootloaderConfig.RequiredScenes)
                {
                    asset.Unload();
                }
            }
        }

        private IEnumerator Bootup()
        {
            this.bootloaderDialog.Dialog.Show();

            yield return ReleasesManager.WaitForInitialization();

            yield return ReleasesManager.Instance.ShowForceUpdateDialog();

            yield return AddressablesManager.WaitForInitialization();

            // Get the boot config
            // Load all the assets

            float startTime = Time.realtimeSinceStartup;

            yield return this.WaitForManagersToInitialize();

            //// // If the only scene open is the bootloader scene, then lets load the startup scene
            //// if (SceneManager.sceneCount == 1 && this.startupScene != null && this.startupScene.AssetGuid.IsNullOrWhitespace() == false)
            //// {
            ////     var loadScene = this.startupScene.LoadScene(LoadSceneMode.Additive);
            ////     yield return loadScene;
            ////     SceneManager.SetActiveScene(loadScene.Result.Scene);
            //// }

            // Destorying the Loading camera now that the startup scene is loaded (the loading dialog will find the new camera automatically)
            this.loadingCamera.gameObject.SetActive(false);
            DialogManager.ForceUpdateDialogCameras(Camera.main);

            // Making sure we wait the minimum time
            if (this.ShowLoadingInEditor && this.bootloaderDialog)
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;

                if (elapsedTime < this.minimumLoadingDialogTime)
                {
                    yield return WaitForUtil.Seconds(this.minimumLoadingDialogTime - elapsedTime);
                }

                // Making sure we don't say Hide if we're still showing (has a bad pop)
                while (this.bootloaderDialog.Dialog.IsShown == false)
                {
                    yield return null;
                }
            }

            // Doing a little cleanup before giving user control
            System.GC.Collect();
            yield return null;

            // TODO [bgish]:  We're done!  Fire the OnBooted event????
            
            if (this.ShowLoadingInEditor && this.bootloaderDialog)
            {
                this.bootloaderDialog.Dialog.Hide();
            }
        }

        private IEnumerator WaitForManagersToInitialize()
        {
            int initialManagersCount = Lost.ManagersReady.Managers.Count;
            List<IManager> managers = new List<IManager>(initialManagersCount);
            List<IManager> managersToRemove = new List<IManager>(initialManagersCount);

            // Populating the manager list with all known managers
            for (int i = 0; i < initialManagersCount; i++)
            {
                managers.Add(Lost.ManagersReady.Managers[i]);
            }

            while (managers.Count > 0)
            {
                managersToRemove.Clear();

                foreach (var manager in managers)
                {
                    if (manager.IsManagerInitialized())
                    {
                        managersToRemove.Add(manager);
                    }
                }

                foreach (var managerToRemvoe in managersToRemove)
                {
                    managers.Remove(managerToRemvoe);
                }

                ProgressUpdated?.Invoke(1.0f - (managers.Count / (float)initialManagersCount));

                yield return null;
            }
        }
    }
}

#endif
