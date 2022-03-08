//-----------------------------------------------------------------------
// <copyright file="DissonanceManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost.DissonanceIntegration
{
    using UnityEngine;

    public sealed class DissonanceManager : Manager<DissonanceManager>
    {
#pragma warning disable 0649, 0414
        [SerializeField] private bool requestMicrophonePermissionAtStartup = true;
        [SerializeField] private GameObject dissonanceCommsPrefab;
#pragma warning restore 0649, 0414

#if USING_DISSONANCE
        public Dissonance.DissonanceComms DissonanceComms { get; private set; }
#endif

        public void RequestMicrophonePermissions()
        {
#if PLATFORM_ANDROID
            var microphonePermission = UnityEngine.Android.Permission.Microphone;

            if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(microphonePermission) == false)
            {
                UnityEngine.Android.Permission.RequestUserPermission(microphonePermission);
            }
#endif
        }

#if !USING_DISSONANCE && UNITY_EDITOR

        [ShowEditorInfo]
        public string GetInfoMessage() => "Dissonance Package is not present.  Dissonance Manager will be ignored.";

#endif

        public override void Initialize()
        {
            this.StartCoroutine(Coroutine());

            System.Collections.IEnumerator Coroutine()
            {
                #if USING_DISSONANCE

                if (this.requestMicrophonePermissionAtStartup)
                {
                    this.RequestMicrophonePermissions();
                }

                yield return UserInfoManager.WaitForInitialization();

                if (this.dissonanceCommsPrefab == null)
                {
                    Debug.LogError("DissonanceManager: Unable to locate the DissonanceComms object. Dissonance will not work.", this);
                }
                else
                {
                    var dissonanceCommsObject = GameObject.Instantiate(this.dissonanceCommsPrefab, this.transform);

                    this.DissonanceComms = dissonanceCommsObject.GetComponent<Dissonance.DissonanceComms>();

                    if (this.DissonanceComms == null)
                    {
                        Debug.LogError("DissonanceManager: DissonanceComms Prefab does not have the DissonanceComms Component, Dissonance will not work.", this);
                    }
                    else
                    {
                        this.DissonanceComms.LocalPlayerName = UserInfoManager.Instance.UserHexId;
                        this.DissonanceComms.gameObject.name = "Dissonance Comms";
                        this.DissonanceComms.gameObject.SetActive(true);
                    }
                }

                #endif

                this.SetInstance(this);
                yield break;
            }
        }
    }
}

#endif
