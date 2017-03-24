using System.Diagnostics;

namespace LibraProgramming.Parsing.Xaml.Tokens
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Terminal, Term = {Term}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    internal sealed class XamlTerminalToken : XamlToken
    {
        /// <summary>
        /// 
        /// </summary>
        public XamlTerminal Term
        {
            get;
        }

        public XamlTerminalToken(XamlTerminal term)
            : base(XamlTokenType.Terminal)
        {
            Term = term;
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly XamlTerminalToken token;

            public XamlTerminal Terminal => token.Term;

            public DebugView(XamlTerminalToken token)
            {
                this.token = token;
            }
        }
    }
}