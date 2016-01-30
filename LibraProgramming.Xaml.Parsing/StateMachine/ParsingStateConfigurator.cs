using System;

namespace LibraProgramming.Xaml.Parsing.StateMachine
{
    internal class ParsingStateConfigurator<TState, TTrigger> : IParsingStateConfigurator<TState, TTrigger>
        where TState : struct
    {
        private readonly ParsingStateDescriptor<TState, TTrigger> descriptor;
        private TState state;

        public ParsingStateConfigurator(TState state, ParsingStateDescriptor<TState, TTrigger> descriptor)
        {
            this.state = state;
            this.descriptor = descriptor;
        }

        public IParsingStateConfigurator<TState, TTrigger> Permit(TTrigger trigger, TState target)
        {
            EnsureNoTrigger(trigger);

            descriptor.Triggers.Add(trigger, new PermittedStateTransition<TState>(target));

            return this;
        }

        public IParsingStateConfigurator<TState, TTrigger> Ignore(TTrigger trigger)
        {
            EnsureNoTrigger(trigger);

            descriptor.Triggers.Add(trigger, new IgnoredTransition());

            return this;
        }

        public IParsingStateConfigurator<TState, TTrigger> OnEnter(Action<TState> action)
        {
            if (null != descriptor.OnEnter)
            {
                throw new ArgumentException(nameof(action));
            }

            descriptor.OnEnter = action;

            return this;
        }

        public IParsingStateConfigurator<TState, TTrigger> OnExit(Action<TState> action)
        {
            if (null != descriptor.OnExit)
            {
                throw new ArgumentException(nameof(action));
            }

            descriptor.OnExit = action;

            return this;
        }

        private void EnsureNoTrigger(TTrigger trigger)
        {
            if (descriptor.Triggers.ContainsKey(trigger))
            {
                throw new ArgumentException(nameof(trigger));
            }
        }
    }
}