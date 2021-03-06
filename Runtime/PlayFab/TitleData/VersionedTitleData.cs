//-----------------------------------------------------------------------
// <copyright file="VersionedTitleData.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace Lost.PlayFab
{
    using System.Collections.Generic;
    using UnityEngine;

    public class VersionedTitleData<T> : ScriptableObject
        where T : new()
    {
        #if UNITY_EDITOR
        [SerializeField] private List<T> data = new List<T> { new T() };
        #endif

        [SerializeField] private List<string> versions = new List<string> { "1.0" };
        [SerializeField] private string titleDataKeyName = string.Empty;
        [SerializeField] private bool serializeWithUnity = true;
        [SerializeField] private bool compressData = false;

        #if UNITY_EDITOR
        public List<T> Data
        {
            get { return this.data; }
        }
        #endif

        public List<string> Versions
        {
            get { return this.versions; }
        }

        public string TitleDataKeyName
        {
            get { return this.titleDataKeyName; }
            set { this.titleDataKeyName = value; }
        }

        public bool SerializeWithUnity
        {
            get { return this.serializeWithUnity; }
            set { this.serializeWithUnity = value; }
        }

        public bool CompressData
        {
            get { return this.compressData; }
            set { this.compressData = value; }
        }

        public UnityTask<T> Load(string version)
        {
            // Take key name and the verison and load from title data using PF class
            // If it's compressed, then decompress it
            return null;
        }
    }
}

#endif
