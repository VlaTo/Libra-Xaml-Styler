using System;

namespace LibraProgramming.Xaml.Parsing.StateMachine
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    public interface IParsingStateConfigurator<TState, in TTrigger>
        where TState : struct
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        IParsingStateConfigurator<TState, TTrigger> Permit(TTrigger trigger, TState target);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        IParsingStateConfigurator<TState, TTrigger> Ignore(TTrigger trigger);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IParsingStateConfigurator<TState, TTrigger> OnEnter(Action<TState> action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IParsingStateConfigurator<TState, TTrigger> OnExit(Action<TState> action);
    }
}