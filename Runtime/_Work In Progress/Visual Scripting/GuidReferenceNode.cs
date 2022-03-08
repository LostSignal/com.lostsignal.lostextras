//-----------------------------------------------------------------------
// <copyright file="GuidReferenceUnit.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_VISUAL_SCRIPTING

namespace Lost
{
    using UnityEngine;
    using global::Unity.VisualScripting;

    [UnitCategory("Lost")]
    [UnitTitle("Guid Reference")]
    public class GuidReferenceNode : Unit
    {
        [Inspectable]
        [UnitHeaderInspectable]
        [Serialize]
        private GuidReference guidReference;
        
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput Output { get; private set; }

        protected override void Definition()
        {
            this.Output = this.ValueOutput(typeof(GameObject), nameof(this.Output), (flow) =>
            {
                return this.guidReference.GameObject;
            });
        }
    }
}

#endif
