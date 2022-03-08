//-----------------------------------------------------------------------
// <copyright file="GameEventListener.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class GameEventListener : MonoBehaviour, IValidate, IGameEventListener
    {
#pragma warning disable 0649
        [Tooltip("Event to register with.")]
        [SerializeField] private GameEvent gameEvent;

        [Tooltip("Response to invoke when Event is raised.")]
        [SerializeField] private UnityEvent response;
#pragma warning restore 0649

        public void OnEventRaised()
        {
            this.response.SafeInvoke();
        }

        public void Validate(List<ValidationError> errors)
        {
            if (this.gameEvent == null)
            {
                errors.Add(new ValidationError
                {
                    Name = "Null Game Event",
                    Description = "",
                    AffectedObject = this,
                });
            }
        }

        private void OnEnable()
        {
            if (this.gameEvent)
            {
                this.gameEvent.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (this.gameEvent)
            {
                this.gameEvent.UnregisterListener(this);
            }
        }
    }
}

#endif
