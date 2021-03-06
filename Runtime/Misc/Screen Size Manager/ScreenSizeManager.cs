//-----------------------------------------------------------------------
// <copyright file="ScreenSizeManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections;
    using UnityEngine;

    public sealed class ScreenSizeManager : Manager<ScreenSizeManager>
    {
        #pragma warning disable 0649
        [SerializeField] private bool limitMobileScreenSize;
        [SerializeField] private int maxScreenSize = 1920;
        #pragma warning restore 0649

        public override void Initialize()
        {
            this.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                if (this.limitMobileScreenSize)
                {
                    yield return this.LimitScreenSize(this.maxScreenSize);
                }

                this.SetInstance(this);
            }
        }

        private IEnumerator LimitScreenSize(int maxScreenSize)
        {
            // Wait a tick for all managers to be registered
            yield return null;

            bool isMobilePlatform = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
            bool isMobileVrDevice = isMobilePlatform && (SystemInfo.deviceName == "Oculus Quest" || SystemInfo.deviceName == "Oculus Quest 2");
            
            if (isMobilePlatform && isMobileVrDevice == false)
            {
                bool isLandscape = Screen.width > Screen.height;

                if (isLandscape && Screen.width > maxScreenSize)
                {
                    float aspectRatio = Screen.height / (float)Screen.width;
                    int newHeight = (int)(maxScreenSize * aspectRatio);
                    int newWidth = maxScreenSize;

                    Debug.LogFormat("Resizing Screen From {0}x{1} To {2}x{3}", Screen.width, Screen.height, newWidth, newHeight);
                    Screen.SetResolution(newWidth, newHeight, true);
                }
                else if (isLandscape == false && Screen.height > maxScreenSize)
                {
                    float aspectRatio = Screen.width / (float)Screen.height;
                    int newHeight = maxScreenSize;
                    int newWidth = (int)(maxScreenSize * aspectRatio);

                    Debug.LogFormat("Resizing Screen From {0}x{1} To {2}x{3}", Screen.width, Screen.height, newWidth, newHeight);
                    Screen.SetResolution(newWidth, newHeight, true);
                }
            }
        }
    }
}

#endif
