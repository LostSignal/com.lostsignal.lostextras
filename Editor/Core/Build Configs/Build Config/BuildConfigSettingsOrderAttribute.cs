//-----------------------------------------------------------------------
// <copyright file="BuildConfigSettingsOrderAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.BuildConfig
{
    using System;

    public sealed class BuildConfigSettingsOrderAttribute : Attribute
    {
        public BuildConfigSettingsOrderAttribute(int order)
        {
            this.Order = order;
        }

        public int Order { get; private set; }
    }
}
