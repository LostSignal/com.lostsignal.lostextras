//-----------------------------------------------------------------------
// <copyright file="PeriodicUpdateUnit.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_VISUAL_SCRIPTING

namespace Lost
{
    using global::Unity.VisualScripting;

    [UnitCategory("Lost")]
    [UnitTitle("Periodic Update")]
    public sealed class PeriodicUpdateUnit : Unit, IOnManagersReady
    {
        private GraphReference graphReference;
        private float deltaTime;

        [DoNotSerialize] public ValueInput CallPerSecond { get; private set; }

        [DoNotSerialize] public ValueOutput DeltaTime { get; private set; }

        [DoNotSerialize] public ControlOutput Update { get; private set; }

        public void OnManagersReady()
        {
            // UpdateManager.Instance.RegisterFunction(this.UpdateUnit, Flow.New(this.graphReference).GetValue<int>(this.CallPerSecond));
            PeriodicUpdateManager.Instance.Add(this);
        }

        public override void Instantiate(GraphReference instance)
        {
            base.Instantiate(instance);
            this.graphReference = instance;
            ManagersReady.Register(this);
        }

        public override void Uninstantiate(GraphReference instance)
        {
            base.Uninstantiate(instance);

            if (PeriodicUpdateManager.Instance)
            {
                PeriodicUpdateManager.Instance.Remove(this);
            }
        }

        protected override void Definition()
        {
            this.isControlRoot = true;
            this.CallPerSecond = this.ValueInput<int>(nameof(this.CallPerSecond));
            this.DeltaTime = this.ValueOutput(nameof(this.DeltaTime), this.GetDeltaTime).Predictable();
            this.Update = this.ControlOutput(nameof(this.Update));
        }

        private void UpdateUnit(float deltaTime)
        {
            this.deltaTime = deltaTime;
            Flow.New(this.graphReference).Run(this.Update);
        }

        private float GetDeltaTime(Flow flow) => this.deltaTime;
    }
}

#endif
