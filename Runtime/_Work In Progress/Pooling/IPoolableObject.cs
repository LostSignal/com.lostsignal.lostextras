//-----------------------------------------------------------------------
// <copyright file="IPooledObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    public interface IPoolableObject
    {
        void TakenFromPool();

        void ReturnedToPool();
    }
}

#endif
