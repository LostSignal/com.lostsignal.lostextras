//-----------------------------------------------------------------------
// <copyright file="GameEvent.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Game Event", menuName = "Lost/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<IGameEventListener> eventListeners = new List<IGameEventListener>();

        public void Raise()
        {
            for (int i = this.eventListeners.Count - 1; i >= 0; i--)
            {
                this.eventListeners[i].OnEventRaised();
            }   
        }

        public void RegisterListener(IGameEventListener listener)
        {
            if (this.eventListeners.Contains(listener) == false)
            {
                this.eventListeners.Add(listener);
            }   
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            if (this.eventListeners.Contains(listener))
            {
                this.eventListeners.Remove(listener);
            }   
        }
    }
}

#endif
