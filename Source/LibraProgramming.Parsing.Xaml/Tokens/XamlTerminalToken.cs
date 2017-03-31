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
        public char Term
        {
            get;
        }

        public XamlTerminalToken(char term)
            : base(XamlTokenType.Terminal)
        {
            Term = term;
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly XamlTerminalToken token;

            public char Terminal => token.Term;

            public DebugView(XamlTerminalToken token)
            {
                this.token = token;
            }
        }
    }
}