using System;
using System.Collections.Generic;

namespace LibraProgramming.Xaml.Parsing.StateMachine
{
    internal class ParsingStateDescriptor<TState, TTrigger>
    {
        public IDictionary<TTrigger, ParsingStateTransition> Triggers
        {
            get;
        }

        public Action<TState> OnExit
        {
            get;
            set;
        }

        public Action<TState> OnEnter
        {
            get;
            set;
        }

        public ParsingStateDescriptor()
        {
            Triggers = new Dictionary<TTrigger, ParsingStateTransition>(EqualityComparer<TTrigger>.Default);
        }

        public bool CanFire(TTrigger trigger)
        {
            ParsingStateTransition transition;

            if (!Triggers.TryGetValue(trigger, out transition))
            {
                throw new ArgumentException(nameof(trigger));
            }

            return transition.CanTransit;
        }

        public TTransition GetTransition<TTransition>(TTrigger trigger)
            where TTransition : ParsingStateTransition
        {
            ParsingStateTransition transition;
            return Triggers.TryGetValue(trigger, out transition) ? transition as TTransition : null;
        }
    }
}