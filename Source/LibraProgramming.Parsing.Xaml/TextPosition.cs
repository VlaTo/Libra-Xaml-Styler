namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TextPosition
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly TextPosition Empty;

        /// <summary>
        /// 
        /// </summary>
        public int Line
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Position
        {
            get;
        }

        internal TextPosition(int line, int position)
        {
            Line = line;
            Position = position;
        }

        static TextPosition()
        {
            Empty = new TextPosition(0, 0);
        }
    }
}