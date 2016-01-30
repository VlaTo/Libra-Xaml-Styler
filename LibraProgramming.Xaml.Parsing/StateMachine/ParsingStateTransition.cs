namespace LibraProgramming.Xaml.Parsing.StateMachine
{
    internal abstract class ParsingStateTransition
    {
        public abstract bool CanTransit
        {
            get;
        }
    }

    internal sealed class PermittedStateTransition<TState> : ParsingStateTransition
        where TState : struct
    {
        public TState TargetState
        {
            get;
        }

        public override bool CanTransit => true;

        public PermittedStateTransition(TState targetState)
        {
            TargetState = targetState;
        }
    }

    internal sealed class IgnoredTransition : ParsingStateTransition
    {
        public override bool CanTransit => false;
    }
}