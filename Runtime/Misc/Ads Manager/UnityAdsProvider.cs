//-----------------------------------------------------------------------
// <copyright file="UnityAdsProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using UnityEngine;

#if USING_UNITY_ADS
    using UnityEngine.Advertisements;
#endif

    //// TODO [bgish]: Possible add warnings/errors if they want to use Unity Ads but don't specify a proper Store Id
    //// TODO [bgish]: Investigate the removal of the USING_UNITY_ADS define

#if USING_UNITY_ADS && (UNITY_IOS || UNITY_ANDROID)
    public class UnityAdsProvider : MonoBehaviour, IAdProvider, IUnityAdsListener
#else
    public class UnityAdsProvider : MonoBehaviour, IAdProvider
#endif
    {
#pragma warning disable 0649, 0414
        [SerializeField] private string appleAppStoreId = null;
        [SerializeField] private string googlePlayAppStoreId = null;
#pragma warning restore 0649, 0414

        private System.Action<AdWatchedResult> watchResultCallback;

        string IAdProvider.ProviderName => "UnityAds";

#if USING_UNITY_ADS && (UNITY_IOS || UNITY_ANDROID)

        bool IAdProvider.AreAdsSupported => Advertisement.isSupported;

        bool IAdProvider.AreAdsInitialized => Advertisement.isSupported && Advertisement.isInitialized;

        bool IAdProvider.IsAdReady(string placementId) => this.IsPlacementIdAdReady(placementId);

        void IAdProvider.ShowAd(string placementId, bool isRewarded, System.Action<AdWatchedResult> watchResultCallback)
        {
            if (this.watchResultCallback != null)
            {
                Debug.LogError("Calling ShowAd while ad already in progress!");
                watchResultCallback?.Invoke(AdWatchedResult.AdFailed);
                return;
            }

            this.watchResultCallback = watchResultCallback;

            var options = new ShowOptions()
            {
                gamerSid = null,  // TODO [bgish]: Not sure what to put here...
            };

            Advertisement.Show(placementId, options);
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log($"OnUnityAdsReady({placementId})");
        }

        public void OnUnityAdsDidError(string message)
        {
            Debug.LogError($"OnUnityAdsDidError({message})");
        }

        public void OnUnityAdsDidStart(string placementId)
        {
            Debug.Log($"OnUnityAdsDidStart({placementId})");
        }

        public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        {
            if (this.watchResultCallback != null)
            {
                switch (showResult)
                {
                    case ShowResult.Failed:
                        this.watchResultCallback(AdWatchedResult.AdFailed);
                        break;
                    case ShowResult.Skipped:
                        this.watchResultCallback(AdWatchedResult.AdSkipped);
                        break;
                    case ShowResult.Finished:
                        this.watchResultCallback(AdWatchedResult.AdFinished);
                        break;
                    default:
                        Debug.LogErrorFormat("UnityAdProvider.ShowAd() encountered unknown ShowResult {0}", showResult);
                        break;
                }
            }

            this.watchResultCallback = null;
        }

        private bool IsPlacementIdAdReady(string placementId) => Advertisement.isSupported && Advertisement.isInitialized && Advertisement.IsReady(placementId);

#if UNITY_EDITOR && !USING_UNITY_ADS

        [ShowEditorError("This provider will not work unless you the Unity Ads package added through the Package Manager.")]
        [ExposeInEditor("Add Unity Ads Package")]
        private void AddUsingUsingUnityAdsDefine()
        {
            PackageManagerUtil.Add("com.unity.ads");
        }

#elif UNITY_EDITOR && USING_UNITY_ADS

        [ExposeInEditor("Open Unity Ads Dashboard")]
        private void OpenUnityAdsDashboard()
        {
            string projectId = UnityEditor.CloudProjectSettings.projectId;
            string organizationId = UnityEditor.CloudProjectSettings.organizationId;
            string url = $"https://operate.dashboard.unity3d.com/organizations/{organizationId}/projects/{projectId}/operate-settings";
            Application.OpenURL(url);
        }

#endif

        private void OnEnable()
        {
#if USING_UNITY_ADS
            AdsManager.OnInitialized += () =>
            {
                Advertisement.AddListener(this);

#if UNITY_IOS
                
                if (string.IsNullOrWhiteSpace(this.appleAppStoreId) == false && UnityEngine.Advertisements.Advertisement.isInitialized == false)
                {
                    UnityEngine.Advertisements.Advertisement.Initialize(this.appleAppStoreId);
                }

#elif UNITY_ANDROID

                if (string.IsNullOrWhiteSpace(this.googlePlayAppStoreId) == false && UnityEngine.Advertisements.Advertisement.isInitialized == false)
                {
                    UnityEngine.Advertisements.Advertisement.Initialize(this.googlePlayAppStoreId);
                }

#endif
                AdsManager.Instance.SetAdProvider(this);
            };
#endif
        }

#else

        bool IAdProvider.AreAdsSupported => false;

        bool IAdProvider.AreAdsInitialized => false;

        bool IAdProvider.IsAdReady(string placementId) => false;

        void IAdProvider.ShowAd(string placementId, bool isRewarded, System.Action<AdWatchedResult> watchResultCallback) => watchResultCallback?.Invoke(AdWatchedResult.AdsNotSupported);

#endif
    }
}

#endif
