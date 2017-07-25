using System;

namespace LibraProgramming.Parsing.Xaml
{
    internal static class TextPositionExtension
    {
        public static TextPosition Begin(this TextPosition position)
        {
            if (null == position)
            {
                throw new ArgumentNullException(nameof(position));
            }

            return new TextPosition(1, 1);
        }

        public static TextPosition NextPosition(this TextPosition position)
        {
            if (null == position)
            {
                throw new ArgumentNullException(nameof(position));
            }

            return new TextPosition(position.Line, position.Position + 1);
        }

        public static TextPosition NewLine(this TextPosition position)
        {
            if (null == position)
            {
                throw new ArgumentNullException(nameof(position));
            }

            return new TextPosition(position.Line + 1, 1);
        }
    }
}