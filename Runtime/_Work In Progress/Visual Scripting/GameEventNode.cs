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
    [UnitTitle("Game Event")]
    public sealed class GameEventNode : Unit, IGameEventListener
    {
        private GraphReference graphReference;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput GameEvent { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Fired { get; private set; }

        public void OnEventRaised()
        {
            Flow.New(this.graphReference).Run(this.Fired);
        }

        public override void Instantiate(GraphReference instance)
        {
            base.Instantiate(instance);

            var gameEvent = Flow.New(instance).GetValue<GameEvent>(this.GameEvent);

            if (gameEvent)
            {
                gameEvent.RegisterListener(this);
            }

            this.graphReference = instance;
        }

        public override void Uninstantiate(GraphReference instance)
        {
            base.Uninstantiate(instance);

            var gameEvent = Flow.New(instance).GetValue<GameEvent>(this.GameEvent);

            if (gameEvent)
            {
                gameEvent.UnregisterListener(this);
            }
        }

        protected override void Definition()
        {
            this.isControlRoot = true;
            this.GameEvent = this.ValueInput<GameEvent>(nameof(this.GameEvent), null);
            this.Fired = this.ControlOutput(nameof(this.Fired));
        }
    }
}

#endif
