//-----------------------------------------------------------------------
// <copyright file="BootloaderDialog.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BootloaderDialog : MonoBehaviour
    {
        #pragma warning disable 0649
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressBar;
        #pragma warning disable 0649

        private Coroutine showCoroutine;

        public bool IsShown { get; private set; }

        public void Show()
        {
            this.StopShow();
            this.showCoroutine = this.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                this.IsShown = true;
                Bootloader.ProgressTextUpdate += this.OnProgressTextUpdate;
                Bootloader.ProgressUpdated += this.OnProgressUpdated;

                yield return WaitForUtil.Seconds(0.5f);

                this.DisableAspectRatioFitter();

                
            }
        }

        public void Hide()
        {
            this.StopShow();

            this.IsShown = false;
            Bootloader.ProgressTextUpdate -= this.OnProgressTextUpdate;
            Bootloader.ProgressUpdated -= this.OnProgressUpdated;
        }

        private void StopShow()
        {
            if (this.showCoroutine != null)
            {
                this.StopCoroutine(this.showCoroutine);
                this.showCoroutine = null;
            }
        }

        private void OnProgressTextUpdate(string newText)
        {
            if (this.progressText != null)
            {
                this.progressText.text = newText;
            }
        }

        private void OnProgressUpdated(float newProgress)
        {
            if (this.progressBar != null)
            {
                this.progressBar.fillAmount = newProgress;
            }
        }

        private void DisableAspectRatioFitter()
        {
            if (this.aspectRatioFitter != null && Application.isEditor == false)
            {
                this.aspectRatioFitter.enabled = false;
            }
        }
    }
}

#endif
