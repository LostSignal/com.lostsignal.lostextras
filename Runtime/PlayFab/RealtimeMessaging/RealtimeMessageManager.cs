//-----------------------------------------------------------------------
// <copyright file="RealtimeMessageManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace Lost
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Lost.PlayFab;
    using UnityEngine;

    public sealed class RealtimeMessageManager : Manager<RealtimeMessageManager>
    {
        private readonly Dictionary<string, Type> messageTypes = new Dictionary<string, Type>();
        private readonly HashSet<string> subscribedChannels = new HashSet<string>();

        #pragma warning disable 0649
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool printDebugOutput;
        #pragma warning restore 0649

        public override void Initialize()
        {
            this.StartCoroutine(InitializeCoroutine());

            IEnumerator InitializeCoroutine()
            {
                if (this.isEnabled)
                {
                    yield return PlayFabManager.WaitForInitialization();
                    yield return PlayFabManager.Instance.GetRealtimeMessagesUrl();
                }

                this.SetInstance(this);
            }
        }

        public void RegisterType<T>()
            where T : RealtimeMessage, new()
        {
            var instance = Activator.CreateInstance(typeof(T)) as RealtimeMessage;
            this.messageTypes.Add(instance.Type, typeof(T));
        }

        public void Subscribe(string channel)
        {
            if (this.subscribedChannels.Contains(channel) == false)
            {
                //// this.ably.Channels.Get(channel).Subscribe(this.MessageReceived);
                
                this.subscribedChannels.Add(channel);
            }
        }

        public void Unsubscribe(string channel)
        {
            if (this.subscribedChannels.Contains(channel))
            {
                //// this.ably.Channels.Get(channel).Unsubscribe(this.MessageReceived);
                
                this.subscribedChannels.Remove(channel);
            }
        }

        //// private void MessageReceived(Message message)
        //// {
        ////     string json = message.Data as string;
        ////     JObject javaObject;
        //// 
        ////     try
        ////     {
        ////         javaObject = JObject.Parse(json);
        ////     }
        ////     catch
        ////     {
        ////         Debug.LogError($"Received RealtimeMessage with invalid Json: {json}");
        ////         return;
        ////     }
        //// 
        ////     string realtimeMessageType = javaObject["Type"]?.ToString();
        //// 
        ////     if (realtimeMessageType == null)
        ////     {
        ////         Debug.LogError($"Received Json that was not a RealtimeMessage: {json}");
        ////     }
        ////     else if (this.messageTypes.TryGetValue(realtimeMessageType, out Type type))
        ////     {
        ////         var realtimeMessage = JsonUtil.Deserialize(json, type);
        //// 
        ////         //// TODO [bgish]: Forward realtimeMessage onto the message subscription system
        //// 
        ////         if (this.printDebugOutput)
        ////         {
        ////             Debug.Log($"Received RealtimeMessage of type {realtimeMessageType} and json: {json}");
        ////         }
        ////     }
        ////     else
        ////     {
        ////         Debug.LogError($"Received RealtimeMessage of unknown type {realtimeMessageType}");
        ////     }
        //// }
        //// 
        //// private void InitilializeAbly(string ablyKey)
        //// {
        ////     this.ably = new AblyRealtime(ablyKey);
        ////     this.Subscribe(PlayFabManager.Instance.User.PlayFabId);
        //// }
    }
}

#endif
