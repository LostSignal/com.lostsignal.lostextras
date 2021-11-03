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

        public IEnumerator LoadScene()
        {
            if (IsSceneLoaded(this.sceneName) == false)
            {
                if (string.IsNullOrWhiteSpace(this.SceneAddressablesPath) == false)
                {
                    yield return UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(this.SceneAddressablesPath, LoadSceneMode.Additive);
                }
                else
                {
                    yield return SceneManager.LoadSceneAsync(this.sceneName, LoadSceneMode.Additive);
                }
            }
        }

        public bool IsLoaded()
        {
            return SceneManager.GetSceneByName(this.sceneName).isLoaded;
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }

            return false;
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
        // Constants
        public const string DefaultBootloaderResourcePath = "Lost/Bootloader";

        // RuntimeConfig Settings Keys
        public const string BootloaderResourcePath = "Bootloader.ResourcePath";
        public const string BootloaderConfigLocation = "Bootloader.ConfigLocation";
        public const string BootloaderConfig = "Bootloader.Config";
        public const string BootloaderIgnoreSceneNames = "Bootloader.IgnoreSceneNames";

        private static Bootloader bootloaderInstance;

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
            if (bootloaderInstance == null)
            {
                Debug.LogError("Reboot failed, bootloader never booted up correctly.");
                return;
            }

            bootloaderInstance.StartCoroutine(Coroutine());

            static IEnumerator Coroutine()
            {
                bootloaderInstance.loadingCamera.gameObject.SetActive(true);
                DialogManager.ForceUpdateDialogCameras(bootloaderInstance.loadingCamera);
                bootloaderInstance.bootloaderDialog.Dialog.Show();

                // TODO [bgish]: Update text to say Shutting Down

                while (SceneManager.sceneCount > 0)
                {
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(0));
                }

                // TODO [bgish]: Once Bootloader has moved back into Core, move this back into Bootloader
                Platform.Reset();

                bootloaderInstance.StartBootupSequence();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBootloaderObject()
        {
            string bootloaderResourcesPath = RuntimeBuildConfig.Instance.GetString(BootloaderResourcePath);
            string bootloaderConfigLocation = RuntimeBuildConfig.Instance.GetString(BootloaderConfigLocation);

            if (string.IsNullOrWhiteSpace(bootloaderResourcesPath) ||
                string.IsNullOrWhiteSpace(bootloaderConfigLocation) || 
                ShouldRunBootloader() == false)
            {
                return;
            }

            // Parsing the bootloader location
            if (int.TryParse(bootloaderConfigLocation, out int bootloaderLocationIntValue) == false)
            {
                Debug.LogError($"Unable to startup Bootloader.  BootloaderConfigLocation was not a valid int \"{bootloaderConfigLocation}\"");
                return;
            }

            // Making sure bootloader location is a valid value
            if (bootloaderLocationIntValue != 0 && bootloaderLocationIntValue != 1)
            {
                Debug.LogError($"Unable to startup Bootloader.  BootloaderConfigLocation was not a valid value \"{bootloaderLocationIntValue}\"");
                return;
            }

            // Loading the Bootloader object
            var bootloaderPrefab = Resources.Load<Bootloader>(bootloaderResourcesPath);

            // Making sure Bootloader Prefab exits
            if (bootloaderPrefab == null)
            {
                Debug.LogError($"Unable to startup Bootloader.  Unable to load Bootlaoder prefab at bootloaderResourcesPath {bootloaderResourcesPath}");
                return;
            }

            // Creating the Bootloader instance
            bootloaderInstance = GameObject.Instantiate(bootloaderPrefab);
            bootloaderInstance.name = bootloaderInstance.name.Replace("(Clone)", string.Empty);
            GameObject.DontDestroyOnLoad(bootloaderInstance);

            bootloaderInstance.StartBootupSequence();

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
        }

        private Coroutine StartBootupSequence()
        {
            return this.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                float startTime = Time.realtimeSinceStartup;
                this.bootloaderDialog.Dialog.Show();

                yield return DialogManager.WaitForInitialization();
                yield return ReleasesManager.WaitForInitialization();
                yield return ReleasesManager.Instance.ShowForceUpdateDialog();
                yield return AddressablesManager.WaitForInitialization();

                // Getting the Bootloader Config
                string bootloaderConfigLocation = RuntimeBuildConfig.Instance.GetString(BootloaderConfigLocation);
                int bootloaderCongigLocationInt = int.Parse(bootloaderConfigLocation);
                var bootloaderLocation = (BootloaderConfigLocation)bootloaderCongigLocationInt;

                if (bootloaderLocation == Lost.BootloaderConfigLocation.RuntimeConfigSettings)
                {
                    string bootloaderConfigJson = RuntimeBuildConfig.Instance.GetString(BootloaderConfig);
                    this.bootloaderConfig = JsonUtil.Deserialize<BootloaderConfig>(bootloaderConfigJson);
                }
                else if (bootloaderLocation == Lost.BootloaderConfigLocation.Releases)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    Debug.LogError($"Unknown BootloaderConfigLocation encountered {bootloaderLocation}");
                    yield break;
                }

                // Loading all Required Scenes
                foreach (var requiredScene in this.bootloaderConfig.RequiredScenes)
                {
                    yield return requiredScene.LoadScene();

                    while (requiredScene.IsLoaded() == false)
                    {
                        yield return null;
                    }
                }

                yield return null;
                yield return null;
                yield return null;

                // Waiting for all managers to finish loading
                yield return WaitForManagersToInitialize();

                // Disabling the Loading camera now that all required scenes are loaded
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

            IEnumerator WaitForManagersToInitialize()
            {
                ManagersReady.Instance.WaitForManagers();

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
}

#endif
