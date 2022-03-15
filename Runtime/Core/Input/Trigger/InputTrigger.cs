//-----------------------------------------------------------------------
// <copyright file="InputTrigger.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    public class InputTrigger
    {
        public bool IsFired { get; private set; }

        public void Fire()
        {
            this.IsFired = true;
        }

        public void Reset()
        {
            this.IsFired = false;
        }
    }
}

#endif
