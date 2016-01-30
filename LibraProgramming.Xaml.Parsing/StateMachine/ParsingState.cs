using System;
using System.Collections.Generic;

namespace LibraProgramming.Xaml.Parsing.StateMachine
{
    public sealed class ParsingState<TState, TTrigger>
        where TState : struct
    {
        private readonly IDictionary<TState, ParsingStateDescriptor<TState, TTrigger>> states;
        private readonly IEqualityComparer<TState> comparer;
        private TState currentState;

        public TState CurrentState
        {
            get
            {
                return currentState;
            }
            private set
            {
                if (comparer.Equals(value, currentState))
                {
                    return;
                }

                DoCurrentStateExit();

                currentState = value;

                DoCurrentStateEnter();
            }
        }

        public ParsingState()
        {
            comparer = EqualityComparer<TState>.Default;
            states = new Dictionary<TState, ParsingStateDescriptor<TState, TTrigger>>(comparer);
        }

        public IParsingStateConfigurator<TState, TTrigger> Configure(TState state)
        {
            ParsingStateDescriptor<TState, TTrigger> descriptor;

            if (!states.TryGetValue(state, out descriptor))
            {
                descriptor = new ParsingStateDescriptor<TState, TTrigger>();
                states.Add(state, descriptor);
            }

            return new ParsingStateConfigurator<TState, TTrigger>(state, descriptor);
        }

        public bool CanFire(TTrigger trigger)
        {
            var descriptor = GetCurrentStateDescriptor();
            return descriptor.CanFire(trigger);
        }

        public void Fire(TTrigger trigger)
        {
            var descriptor = GetCurrentStateDescriptor();
            var transition = descriptor.GetTransition<ParsingStateTransition>(trigger);

            if (null == transition)
            {
                throw new InvalidOperationException();
            }

            var permit = transition as PermittedStateTransition<TState>;

            if (null != permit)
            {
                if (!transition.CanTransit)
                {
                    throw new InvalidOperationException();
                }

                CurrentState = permit.TargetState;

                return;
            }

            var ignore = transition as IgnoredTransition;

            if (null != ignore)
            {
                // simply ignore transition and do nothing
                return;
            }

            throw new InvalidOperationException();
        }

        private void DoCurrentStateExit()
        {
            var descriptor = GetCurrentStateDescriptor();
            descriptor.OnExit?.Invoke(CurrentState);
        }

        private void DoCurrentStateEnter()
        {
            var descriptor = GetCurrentStateDescriptor();
            descriptor.OnEnter?.Invoke(CurrentState);
        }

        private ParsingStateDescriptor<TState, TTrigger> GetCurrentStateDescriptor()
        {
            ParsingStateDescriptor<TState, TTrigger> descriptor;

            if (!states.TryGetValue(CurrentState, out descriptor))
            {
                throw new InvalidOperationException();
            }

            return descriptor;
        }
    }
}