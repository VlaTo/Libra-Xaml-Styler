using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace LibraProgramming.Parsing.Xaml
{
    public partial class XamlWriter
    {
        private class XamlEncoder
        {
            // Surrogate constants
            private const int SurHighStart = 0xd800;
//            private const int SurHighEnd = 0xdbff;
            private const int SurLowStart = 0xdc00;
//            private const int SurLowEnd = 0xdfff;
//            private const int SurMask = 0xfc00;

            private readonly TextWriter writer;
            private char quoteChar;
            private StringBuilder attrValue;
            private bool inAttribute;
            private bool cacheAttrValue;

            protected internal char QuoteChar
            {
                set
                {
                    quoteChar = value;
                }
            }

            protected internal string AttributeValue => cacheAttrValue ? attrValue.ToString() : String.Empty;

            public XamlEncoder(TextWriter writer)
            {
                this.writer = writer;
                quoteChar = '\"';
            }

            public void StartAttribute(bool cacheAttributeValue)
            {
                inAttribute = true;
                cacheAttrValue = cacheAttributeValue;

                if (false == cacheAttrValue)
                {
                    return;
                }

                if (null == attrValue)
                {
                    attrValue = new StringBuilder();
                }
                else
                {
                    attrValue.Length = 0;
                }
            }

            public void EndAttribute()
            {
                if (cacheAttrValue)
                {
                    attrValue.Length = 0;
                }

                inAttribute = false;
                cacheAttrValue = false;
            }


            public void WriteCharEntity(char ch)
            {
                if (Char.IsSurrogate(ch))
                {
                    throw new InvalidOperationException();
                }

                var strVal = ((int)ch).ToString("X", NumberFormatInfo.InvariantInfo);

                if (cacheAttrValue)
                {
                    attrValue.Append("&#x");
                    attrValue.Append(strVal);
                    attrValue.Append(';');
                }
                WriteCharEntityImpl(strVal);

            }

            public void Write(char[] array, int offset, int count)
            {
                if (null == array)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (0 > offset)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                if (0 > count)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                if (count > array.Length - offset)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                if (cacheAttrValue)
                {
                    attrValue.Append(array, offset, count);
                }

                var endPos = offset + count;
                var position = offset;
                var ch = '\0';

                while (true)
                {
                    var startPos = position;

                    /*unsafe
                    {
                        while (position < endPos && (xmlCharType.charProperties[ch = array[position]] & XmlCharType.fAttrValue) != 0)
                        { // ( xmlCharType.IsAttributeValueChar( ( ch = array[i] ) ) ) ) {
                            position++;
                        }
                    }*/

                    if (startPos < position)
                    {
                        writer.Write(array, startPos, position - startPos);
                    }

                    if (position == endPos)
                    {
                        break;
                    }

                    switch (ch)
                    {
                        case '\x09':
                        {
                            writer.Write(ch);
                            break;
                        }

                        case '\r':
                        case '\n':
                        {
                            if (inAttribute)
                            {
                                WriteCharEntityImpl(ch);
                            }
                            else
                            {
                                writer.Write(ch);
                            }

                            break;
                        }

                        case '<':
                        {
                            WriteEntityRefImpl("lt");
                            break;
                        }

                        case '>':
                        {
                            WriteEntityRefImpl("gt");
                            break;
                        }

                        case '&':
                        {
                            WriteEntityRefImpl("amp");
                            break;
                        }

                        case '\'':
                        {
                            if (inAttribute && quoteChar == ch)
                            {
                                WriteEntityRefImpl("apos");
                            }
                            else
                            {
                                writer.Write('\'');
                            }

                            break;
                        }

                        case '\"':
                        {
                            if (inAttribute && quoteChar == ch)
                            {
                                WriteEntityRefImpl("quot");
                            }
                            else
                            {
                                writer.Write('"');
                            }

                            break;
                        }

                        default:
                        {
                            if (Char.IsHighSurrogate(ch))
                            {
                                if (position + 1 < endPos)
                                {
                                    WriteSurrogateChar(array[++position], ch);
                                }
                                else
                                {
//                                    throw new ArgumentException(Res.GetString(Res.Xml_SurrogatePairSplit));
                                    throw new ArgumentException();
                                }
                            }
                            else if (Char.IsLowSurrogate(ch))
                            {
//                                throw XmlConvert.CreateInvalidHighSurrogateCharException(ch);
                                throw new Exception();
                            }
                            else
                            {
//                                Debug.Assert((ch < 0x20 && !xmlCharType.IsWhiteSpace(ch)) || (ch > 0xFFFD));
                                WriteCharEntityImpl(ch);
                            }

                            break;
                        }
                    }

                    position++;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            public void Write(string text)
            {
                if (null == text)
                {
                    return;
                }

                if (cacheAttrValue)
                {
                    attrValue.Append(text);
                }

                // scan through the string to see if there are any characters to be escaped
                var length = text.Length;
                var index = 0;
                var startPos = 0;
                var ch = (char)0;

                while (true)
                {
                    /*unsafe
                    {
                        while (i < len && (xmlCharType.charProperties[ch = text[i]] & XmlCharType.fAttrValue) != 0)
                        { // ( xmlCharType.IsAttributeValueChar( ( ch = text[i] ) ) ) ) {
                            i++;
                        }
                    }*/

                    if (index == length)
                    {
                        // reached the end of the string -> write it whole out
                        writer.Write(text);
                        return;
                    }

                    if (inAttribute)
                    {
                        if (ch == '\x09')
                        {
                            index++;
                            continue;
                        }
                    }
                    else if (ch == '\x09' || ch == '\r' || ch == '\n' || ch == '\"' || ch == '\'')
                    {
                        index++;
                        continue;
                    }

                    // some character that needs to be escaped is found:
                    break;
                }

                var helperBuffer = new char[256];

                while(true)
                {
                    if (startPos < index)
                    {
                        WriteStringFragment(text, startPos, index - startPos, helperBuffer);
                    }

                    if (index == length)
                    {
                        break;
                    }

                    switch (ch)
                    {
                        case '\x09':
                        {
                            writer.Write(ch);
                            break;
                        }

                        case '\r':
                        case '\n':
                        {
                            if (inAttribute)
                            {
                                WriteCharEntityImpl(ch);
                            }
                            else
                            {
                                writer.Write(ch);
                            }

                            break;
                        }

                        case '<':
                        {
                            WriteEntityRefImpl("lt");
                            break;
                        }

                        case '>':
                        {
                            WriteEntityRefImpl("gt");
                            break;
                        }

                        case '&':
                        {
                            WriteEntityRefImpl("amp");
                            break;
                        }

                        case '\'':
                        {
                            if (inAttribute && quoteChar == ch)
                            {
                                WriteEntityRefImpl("apos");
                            }
                            else
                            {
                                writer.Write('\'');
                            }

                            break;
                        }

                        case '\"':
                        {
                            if (inAttribute && quoteChar == ch)
                            {
                                WriteEntityRefImpl("quot");
                            }
                            else
                            {
                                writer.Write('\"');
                            }
                            break;
                        }

                        default:
                        {
                            if (Char.IsHighSurrogate(ch))
                            {
                                if (index + 1 < length)
                                {
                                    WriteSurrogateChar(text[++index], ch);
                                }
                                else
                                {
//                                    throw XmlConvert.CreateInvalidSurrogatePairException(text[index], ch);
                                    throw new Exception();
                                }
                            }
                            else if (Char.IsLowSurrogate(ch))
                            {
//                                throw XmlConvert.CreateInvalidHighSurrogateCharException(ch);
                                throw new Exception();
                            }
                            else
                            {
//                                Debug.Assert((ch < 0x20 && !xmlCharType.IsWhiteSpace(ch)) || (ch > 0xFFFD));
                                WriteCharEntityImpl(ch);
                            }

                            break;
                        }
                    }

                    index++;
                    startPos = index;

                    /*unsafe
                    {
                        while (index < length && (xmlCharType.charProperties[ch = text[index]] & XmlCharType.fAttrValue) != 0)
                        { // ( xmlCharType.IsAttributeValueChar( ( text[i] ) ) ) ) {
                            index++;
                        }
                    }*/
                }
            }

            public void WriteSurrogateCharEntity(char lowChar, char highChar)
            {
                if (false == Char.IsSurrogatePair(highChar, lowChar))
                {
//                    throw XmlConvert.CreateInvalidSurrogatePairException(lowChar, highChar);
                    throw new Exception();
                }

                var surrogateChar = CombineSurrogateChar(highChar, lowChar);

                if (cacheAttrValue)
                {
                    attrValue.Append(highChar);
                    attrValue.Append(lowChar);
                }

                writer.Write("&#x");
                writer.Write(surrogateChar.ToString("X", NumberFormatInfo.InvariantInfo));
                writer.Write(';');
            }

            public void WriteSurrogateChar(char lowChar, char highChar)
            {
                if (false == Char.IsLowSurrogate(lowChar) || false == Char.IsHighSurrogate(highChar))
                {
//                    throw XmlConvert.CreateInvalidSurrogatePairException(lowChar, highChar);
                    throw new Exception();
                }

                writer.Write(highChar);
                writer.Write(lowChar);
            }

            public void WriteEntityRef(string name)
            {
                if (cacheAttrValue)
                {
                    attrValue.Append('&');
                    attrValue.Append(name);
                    attrValue.Append(';');
                }

                WriteEntityRefImpl(name);
            }

            public void Flush()
            {
                writer.Flush();
            }

            private void WriteCharEntityImpl(string str)
            {
                writer.Write("&#x");
                writer.Write(str);
                writer.Write(';');
            }

            private void WriteCharEntityImpl(char ch)
            {
                WriteCharEntityImpl(((int) ch).ToString("X", NumberFormatInfo.InvariantInfo));
            }

            private void WriteEntityRefImpl(string name)
            {
                writer.Write('&');
                writer.Write(name);
                writer.Write(';');
            }

            private void WriteStringFragment(string str, int offset, int count, char[] helperBuffer)
            {
                var bufferSize = helperBuffer.Length;

                while (0 < count)
                {
                    var copyCount = count;

                    if (copyCount > bufferSize)
                    {
                        copyCount = bufferSize;
                    }

                    str.CopyTo(offset, helperBuffer, 0, copyCount);
                    writer.Write(helperBuffer, 0, copyCount);

                    offset += copyCount;
                    count -= copyCount;
                }
            }

            private static int CombineSurrogateChar(int highChar, int lowChar)
            {
                return (lowChar - SurLowStart) | ((highChar - SurHighStart) << 10) + 0x10000;
            }
        }
    }
}