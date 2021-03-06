//-----------------------------------------------------------------------
// <copyright file="OneParamEvent.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;

    public class OneParamEvent<T>
    {
        private readonly List<Action<T>> actions = new List<Action<T>>();

        public void Subscribe(Action<T> action)
        {
            this.actions.AddIfNotNullAndUnique(action);
        }

        public void Unsubscribe(Action<T> action)
        {
            this.actions.Remove(action);
        }

        public void Raise(T eventObject)
        {
            for (int i = this.actions.Count - 1; i >= 0; i--)
            {
                this.actions[i].Invoke(eventObject);
            }
        }
    }
}
