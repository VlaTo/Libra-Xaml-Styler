using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public enum FormattingMode
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// 
        /// </summary>
        Indent
    }

    // http://referencesource.microsoft.com/#System.Xml/System/Xml/Core/XmlTextWriter.cs,789e9b2b28f9b93e
    public sealed partial class XamlWriter : IDisposable
    {
        private const int MaxNamespacesWalkCount = 16;
        private const string nsPrefix = "xmlns";
        private const string xmlPrefix = "xml";

        private readonly TextWriter writer;
        private State currentState;
        private State[] stateTransitions;
        private Token lastToken;
        private TagInfo[] stack;
        private Namespace[] namespaces;
        private int stackIndex;
        private int namespaceIndex;
        private bool hasNamespaces;
        private bool indented;
        private bool endTrailing;
        private int indentation;
        private char quoteChar;
        private IDictionary<string, int> nsHashtable;
        private bool useNsHashtable;
        private FormattingMode formattingMode;
        private readonly XamlEncoder encoder;

        public FormattingMode FormattingMode
        {
            get
            {
                return formattingMode;
            }
            set
            {
                formattingMode = value;
                indented = FormattingMode.Indent == value;
            }
        }

        public int Indentation
        {
            get
            {
                return indentation;
            }
            set
            {
                if (0 > value)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                indentation = value;
            }
        }

        public bool NewLineBeforeAttribute
        {
            get
            {
                return endTrailing;
            }
            set
            {
                endTrailing = value;
            }
        }

        public char IndentChar
        {
            get;
            set;
        }

        public bool HasNamespaces
        {
            get
            {
                return hasNamespaces;
            }
            set
            {
                if (State.Start != currentState)
                {
                    throw new InvalidOperationException();
                }

                hasNamespaces = value;
            }
        }

        public char QuoteChar
        {
            get
            {
                return quoteChar;
            }
            set
            {
                if ('\"' != value && '\'' != value)
                {
                    throw new ArgumentException("");
                }

                quoteChar = value;
            }
        }

        public XamlWriter(TextWriter writer)
            : this()
        {
            if (null == writer)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.writer = writer;
            encoder = new XamlEncoder(writer);
        }

        public XamlWriter(Stream stream)
            : this()
        {
            if (null == stream)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            writer = new StreamWriter(stream);
            encoder = new XamlEncoder(writer);
        }

        private XamlWriter()
        {
            stack = new TagInfo[10];
            stackIndex = 0;
            stack[stackIndex].Init();
            stateTransitions = stateTransitionsDefault;
            namespaces = new Namespace[20];
            namespaceIndex = -1;
            hasNamespaces = true;
            currentState = State.Start;
            lastToken = Token.Empty;
            quoteChar = '\"';
            IndentChar = ' ';
            indentation = 2;
        }

        /// <summary>
        /// 
        /// </summary>
        public void WriteStartDocument()
        {
            WriteStartDocumentInternal(-1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="standalone"></param>
        public void WriteStartDocument(bool standalone)
        {
            WriteStartDocumentInternal(standalone ? 1 : 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void WriteEndDocument()
        {
            try
            {
                AutoCompleteAll();

                if (State.Epilog != currentState)
                {
                    if (State.Closed == currentState)
                    {
                        throw new ArgumentException();
                    }

                    throw new ArgumentException();
                }

                stateTransitions = stateTransitionsDefault;
                currentState = State.Start;
                lastToken = Token.Empty;
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        public void WriteStartElement(string prefix, string localName, string ns)
        {
            try
            {
                AutoComplete(Token.StartElement);
                PushStack();

                writer.Write('<');

                if (hasNamespaces)
                {
                    stack[stackIndex].DefaultNs = stack[stackIndex - 1].DefaultNs;

                    if (NamespaceState.Uninitialized != stack[stackIndex - 1].DefaultNamespaceState)
                    {
                        stack[stackIndex - 1].DefaultNamespaceState = NamespaceState.NotDeclaredButInScope;
                    }

                    stack[stackIndex].Mixed = stack[stackIndex - 1].Mixed;

                    if (null == ns)
                    {
                        if (false == String.IsNullOrEmpty(prefix) && (-1 == LookupNamespace(prefix)))
                        {
                            throw new ArgumentException("", nameof(prefix));
                        }
                    }
                    else
                    {
                        if (null == prefix)
                        {
                            var str = FindPrefix(ns);

                            if (null != str)
                            {
                                prefix = str;
                            }
                            else
                            {
                                PushNamespace(null, ns, false);
                            }
                        }
                        else if (0 == prefix.Length)
                        {
                            PushNamespace(null, ns, false);
                        }
                        else
                        {
                            if (0 == ns.Length)
                            {
                                prefix = null;
                            }

                            VerifyPrefixXml(prefix, ns);
                            PushNamespace(prefix, ns, false);
                        }
                    }

                    stack[stackIndex].Prefix = null;

                    if (false == String.IsNullOrEmpty(prefix))
                    {
                        stack[stackIndex].Prefix = prefix;
                        writer.Write(prefix);
                        writer.Write(':');
                    }
                }
                else
                {
                    if (false == String.IsNullOrEmpty(ns) || false == String.IsNullOrEmpty(prefix))
                    {
                        throw new ArgumentException("", nameof(ns));
                    }
                }

                stack[stackIndex].Name = localName;
                writer.Write(localName);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void WriteEndElement()
        {
            WriteEndElementInternal(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void WriteLongEndElement()
        {
            WriteEndElementInternal(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        public void WriteStartAttribute(string prefix, string localName, string ns)
        {
            try
            {
                AutoComplete(Token.StartAttribute);

                if (HasNamespaces)
                {
                    var comparer = StringComparer.Ordinal;

                    if (null != prefix && 0 == prefix.Length)
                    {
                        prefix = null;
                    }

                    if (comparer.Equals("xmlns", ns) && null == prefix && false == comparer.Equals(nsPrefix, localName))
                    {
                        prefix = nsPrefix;
                    }

                    if (comparer.Equals(xmlPrefix, prefix))
                    {
                        if ("lang" == localName)
                        {

                        }
                        else if ("space" == localName)
                        {

                        }
                    }
                    else if (comparer.Equals(nsPrefix, prefix))
                    {
                        if (String.IsNullOrEmpty(localName))
                        {
                            localName = prefix;
                            prefix = null;
//                            this.prefixForXmlNs = null;
                        }
                        else
                        {
//                            this.prefixForXmlNs = localName;
                        }
                    }
                    else if (null == prefix && comparer.Equals(xmlPrefix, localName))
                    {
                    }
                    else
                    {
                        if (null == ns)
                        {
                            if (null != prefix && (LookupNamespace(prefix) == -1))
                            {
                                throw new ArgumentException();
                            }
                        }
                        else if (0 == ns.Length)
                        {
                            prefix = String.Empty;
                        }
                        else
                        {
                            VerifyPrefixXml(prefix, null);

                            if (null != prefix && -1 < LookupNamespaceInCurrentScope(prefix))
                            {
                                prefix = null;
                            }

                            var definedPrefix = FindPrefix(ns);

                            if (null != definedPrefix && (null == prefix || prefix == definedPrefix))
                            {
                                prefix = definedPrefix;
                            }
                            else
                            {
                                if (null == prefix)
                                {
                                    prefix = GeneratePrefix();
                                }

                                PushNamespace(prefix, ns, false);
                            }
                        }
                    }

                    if (false == String.IsNullOrEmpty(prefix))
                    {
                        writer.Write(prefix);
                        writer.Write(':');
                    }
                }
                else
                {
                    if (false == String.IsNullOrEmpty(ns) || false == String.IsNullOrEmpty(prefix))
                    {
                        throw new ArgumentException();
                    }

                    if ("xml:lang" == localName)
                    {

                    }
                    else if ("xml:space" == localName)
                    {

                    }
                }

                writer.Write(localName);
                writer.Write('=');
                writer.Write(QuoteChar);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void WriteEndAttribute()
        {
            try
            {
                AutoComplete(Token.EndAttribute);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        public void WriteCharEntity(char ch)
        {
            try
            {
                AutoComplete(Token.Content);
                encoder.WriteCharEntity(ch);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        /*public void WriteAttributeContent(XamlContent content)
        {
            var buffer = new StringBuilder();

            using (var temp = new StringWriter(buffer))
            {
                content.WriteTo(temp);
            }

            writer.WriteAttributeValue(buffer.ToString());
        }*/

        public void WriteString(string text)
        {
            try
            {
                if (false == String.IsNullOrEmpty(text))
                {
                    AutoComplete(Token.Content);
                    writer.Write(text);
                }
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        public void WriteName(string name)
        {
            try
            {
                AutoComplete(Token.Content);
                WriteNameInternal(name, false);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        /*public void WriteElementContent(XamlContent content)
        {
            LastInfo.IsEmpty = false;

            var buffer = new StringBuilder();

            using (var temp = new StringWriter(buffer))
            {
                content.WriteTo(temp);
            }

            writer.WriteElementValue(buffer.ToString());
        }*/

        public void Close()
        {
            try
            {
                AutoCompleteAll();
            }
            catch
            {
            }
            finally
            {
                currentState = State.Closed;
                writer.Close();
            }
        }

        public void Flush()
        {
            writer.Flush();
        }

        public string LookupPrefix(string @namespace)
        {
            if (String.IsNullOrEmpty(@namespace))
            {
//                throw new ArgumentException(Res.GetString(Res.Xml_EmptyName));
                throw new ArgumentException();
            }

            var prefix = FindPrefix(@namespace);

            if (prefix == null && @namespace == stack[stackIndex].DefaultNs)
            {
                prefix = String.Empty;
            }

            return prefix;
        }

        public void Dispose()
        {
            Flush();
            Close();
        }

        private void WriteStartDocumentInternal(int standalone)
        {
            try
            {
                if (State.Start != currentState)
                {
                    throw new InvalidOperationException();
                }

                stateTransitions = stateTransitionsDocument;
                currentState = State.Prolog;

            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        private void AutoComplete(Token token)
        {
            if (State.Closed == currentState)
            {
                throw new InvalidOperationException();
            }

            if (State.Failed == currentState)
            {
                throw new InvalidOperationException();
            }

            var state = stateTransitions[(int) token * 6 + (int) currentState];

            if (State.Failed == state)
            {
                throw new InvalidOperationException();
            }

            switch (token)
            {
                case Token.StartElement:
                {
                    if (State.Attribute == currentState)
                    {
                        WriteEndAttributeQuote();
                        WriteEndStartTag(false);
                    }
                    else if (State.Element == currentState)
                    {
                        WriteEndStartTag(false);
                    }

                    if (indented && State.Start != currentState)
                    {
                        Ident(false);
                    }

                    break;
                }

                case Token.EndElement:
                case Token.LongEndElement:
                {
                    if (State.Attribute == currentState)
                    {
                        WriteEndAttributeQuote();
                    }

                    if (State.Content == currentState)
                    {
                        token = Token.LongEndElement;
                    }
                    else
                    {
                        WriteEndStartTag(Token.EndElement == token);
                    }

                    if (stateTransitionsDocument == stateTransitions && 1 == stackIndex)
                    {
                        state = State.Epilog;
                    }

                    break;
                }

                case Token.StartAttribute:
                {
                    if (State.Attribute == currentState)
                    {
                        WriteEndAttributeQuote();
                        WriteAttributePreamble();
                    }
                    else if (State.Element == currentState)
                    {
                        WriteAttributePreamble();
                    }

                    break;
                }

                case Token.EndAttribute:
                {
                    WriteEndAttributeQuote();
                    break;
                }

                case Token.Content:
                {
                    if (State.Element == currentState && Token.Content != lastToken)
                    {
                        WriteEndStartTag(false);
                    }

                    if (State.Content == state)
                    {
                        stack[stackIndex].Mixed = true;
                    }

                    break;
                }
            }

            currentState = state;
            lastToken = token;
        }

        private void AutoCompleteAll()
        {
            while (0 < stackIndex)
            {
                WriteEndElement();
            }
        }

        private void WriteEndElementInternal(bool forceLong)
        {
            try
            {
                if (0 >= stackIndex)
                {
                    throw new InvalidOperationException();
                }

                AutoComplete(forceLong ? Token.LongEndElement : Token.EndElement);

                if (Token.LongEndElement == lastToken)
                {
                    if (indented)
                    {
                        Ident(true);
                    }

                    writer.Write('<');
                    writer.Write('/');

                    if (HasNamespaces && null != stack[stackIndex].Prefix)
                    {
                        writer.Write(stack[stackIndex].Prefix);
                        writer.Write(':');
                    }

                    writer.Write(stack[stackIndex].Name);
                    writer.Write('>');
                }

                var prevNsIndex = stack[stackIndex].PreviosNsIndex;

                if (useNsHashtable && prevNsIndex < namespaceIndex)
                {
                    PopNamespaces(prevNsIndex + 1, namespaceIndex);
                }

                namespaceIndex = prevNsIndex;

                stackIndex--;
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        private void Ident(bool beforeEndElement)
        {
            if (stackIndex == 0)
            {
                writer.WriteLine();
            }
            else if (false == stack[stackIndex].Mixed)
            {
                writer.WriteLine();

                var ident = beforeEndElement ? stackIndex - 1 : stackIndex;

                for (ident *= Indentation; ident > 0; ident--)
                {
                    writer.Write(IndentChar);
                }
            }
        }

        private void PushNamespace(string prefix, string ns, bool declared)
        {
            /*if (XmlReservedNs.NsXmlNs == ns)
            {
                throw new ArgumentException(Res.GetString(Res.Xml_CanNotBindToReservedNamespace));
            }*/

            if (null == prefix)
            {
                switch (stack[stackIndex].DefaultNamespaceState)
                {
                    case NamespaceState.DeclaredButNotWrittenOut:
                    {
                        //Debug.Assert(declared == true, "Unexpected situation!!");
                        // the first namespace that the user gave us is what we
                        // like to keep. 
                        break;
                    }

                    case NamespaceState.Uninitialized:
                    case NamespaceState.NotDeclaredButInScope:
                    {
                        // we now got a brand new namespace that we need to remember
                        stack[stackIndex].DefaultNs = ns;
                        break;
                    }

                    default:
                    {
//                        Debug.Assert(false, "Should have never come here");
                        return;
                    }
                }

                stack[stackIndex].DefaultNamespaceState = declared
                    ? NamespaceState.DeclaredAndWrittenOut
                    : NamespaceState.DeclaredButNotWrittenOut;
            }
            else
            {
                if (prefix.Length != 0 && ns.Length == 0)
                {
//                    throw new ArgumentException(Res.GetString(Res.Xml_PrefixForEmptyNs));
                    throw new ArgumentException();
                }

                var existingNsIndex = LookupNamespace(prefix);

                if (existingNsIndex != -1 && namespaces[existingNsIndex].Name == ns)
                {
                    // it is already in scope.
                    if (declared)
                    {
                        namespaces[existingNsIndex].IsDeclared = true;
                    }
                }
                else
                {
                    // see if prefix conflicts for the current element
                    if (declared)
                    {
                        if (existingNsIndex != -1 && existingNsIndex > stack[stackIndex].PreviosNsIndex)
                        {
                            namespaces[existingNsIndex].IsDeclared = true; // old one is silenced now
                        }
                    }

                    AddNamespace(prefix, ns, declared);
                }
            }
        }

        private void AddNamespace(string prefix, string ns, bool declared)
        {
            var position = ++namespaceIndex;

            if (position == namespaces.Length)
            {
                var temp = new Namespace[position * 2];

                Array.Copy(namespaces, temp, position);
                namespaces = temp;
            }

            namespaces[position].Set(prefix, ns, declared);

            if (useNsHashtable)
            {
                AddToNamespaceHashtable(position);
            }
            else if (position == MaxNamespacesWalkCount)
            {
                // add all
//                nsHashtable = new Dictionary<string, int>(new SecureStringHasher());
                nsHashtable = new Dictionary<string, int>(StringComparer.InvariantCulture);

                for (var index = 0; index <= position; index++)
                {
                    AddToNamespaceHashtable(index);
                }

                useNsHashtable = true;
            }
        }

        private void AddToNamespaceHashtable(int index)
        {
            var prefix = namespaces[index].Prefix;

            if (nsHashtable.TryGetValue(prefix, out int existingNsIndex))
            {
                namespaces[index].PrevNsIndex = existingNsIndex;
            }

            nsHashtable[prefix] = index;
        }

        private void PopNamespaces(int indexFrom, int indexTo)
        {
//            Debug.Assert(useNsHashtable);
            for (var index = indexTo; index >= indexFrom; index--)
            {
//                Debug.Assert(nsHashtable.ContainsKey(nsStack[index].prefix));
                if (-1 == namespaces[index].PrevNsIndex)
                {
                    nsHashtable.Remove(namespaces[index].Prefix);
                }
                else
                {
                    nsHashtable[namespaces[index].Prefix] = namespaces[index].PrevNsIndex;
                }
            }
        }

        private string GeneratePrefix()
        {
            var temp = 1 + stack[stackIndex].PrefixCount++;
            return String.Format(CultureInfo.InvariantCulture, "d{0:d}p{1:d}", stackIndex, temp);

            /*return "d" + stackIndex.ToString(CultureInfo.InvariantCulture)
                + "p" + temp.ToString(CultureInfo.InvariantCulture);*/
        }

        private int LookupNamespace(string prefix)
        {
            if (useNsHashtable)
            {
                if (nsHashtable.TryGetValue(prefix, out int nsIndex))
                {
                    return nsIndex;
                }
            }

            for (var index = namespaceIndex; index >= 0; index--)
            {
                if (String.Equals(namespaces[index].Prefix, prefix, StringComparison.InvariantCulture))
                {
                    return index;
                }
            }

            return -1;
        }

        private int LookupNamespaceInCurrentScope(string prefix)
        {
            if (useNsHashtable)
            {
                if (nsHashtable.TryGetValue(prefix, out int nsIndex))
                {
                    if (nsIndex > stack[stackIndex].PreviosNsIndex)
                    {
                        return nsIndex;
                    }
                }
            }

            for (var index = namespaceIndex; index > stack[stackIndex].PreviosNsIndex; index--)
            {
                if (String.Equals(namespaces[index].Prefix, prefix, StringComparison.InvariantCulture))
                {
                    return index;
                }
            }

            return -1;
        }

        private string FindPrefix(string ns)
        {
            for (var index = namespaceIndex; index >= 0; index--)
            {
                if (false == String.Equals(namespaces[index].Name, ns, StringComparison.InvariantCulture))
                {
                    continue;
                }

                if (index == LookupNamespace(namespaces[index].Prefix))
                {
                    return namespaces[index].Prefix;
                }
            }

            return null;
        }

        private void WriteNameInternal(string name, bool isNCName)
        {
            ValidateName(name, isNCName);
            writer.Write(name);
        }

        private void WriteAttributePreamble()
        {
            if (NewLineBeforeAttribute)
            {
                Ident(false);
            }
            else
            {
                writer.Write(' ');
            }
        }

        private void ValidateName(string name, bool isNCName)
        {
            if (String.IsNullOrEmpty(name))
            {
//                throw new ArgumentException(Res.GetString(Res.Xml_EmptyName));
                throw new ArgumentException();
            }

            /*var nameLength = name.Length;

            // Namespaces supported
            if (hasNamespaces)
            {
                var colonPosition = -1;
                var position = ValidateNames.ParseNCName(name);

                Continue:
                if (position == nameLength)
                {
                    return;
                }

                // we have prefix:localName
                if (name[position] == ':')
                {
                    if (!isNCName)
                    {
                        // first colon in qname
                        if (colonPosition == -1)
                        {
                            // make sure it is not the first or last characters
                            if (position > 0 && position + 1 < nameLength)
                            {
                                colonPosition = position;
                                // Because of the back-compat bug (described above) parse the rest as Nmtoken
                                position++;
                                position += ValidateNames.ParseNmtoken(name, position);
                                goto Continue;
                            }
                        }
                    }
                }
            }
            // Namespaces not supported
            else
            {
                if (ValidateNames.IsNameNoNamespaces(name))
                {
                    return;
                }
            }
            throw new ArgumentException(Res.GetString(Res.Xml_InvalidNameChars, name));*/
        }

        private void WriteEndAttributeQuote()
        {
            /*if (this.specialAttr != SpecialAttr.None)
            {
                // Ok, now to handle xmlspace, etc.
                HandleSpecialAttribute();
            }*/

            encoder.EndAttribute();
            writer.Write(quoteChar);
        }


        private void WriteEndStartTag(bool empty)
        {
            encoder.StartAttribute(false);

            for (var index = namespaceIndex; index > stack[stackIndex].PreviosNsIndex; index--)
            {
                if (namespaces[index].IsDeclared)
                {
                    continue;
                }

                writer.Write(nsPrefix);
                writer.Write(':');
                writer.Write(namespaces[index].Prefix);
                writer.Write('=');
                writer.Write(quoteChar);
                encoder.Write(namespaces[index].Name);
                writer.Write(quoteChar);
            }

            if (false == String.Equals(stack[stackIndex].DefaultNs, stack[stackIndex - 1].DefaultNs) &&
                (NamespaceState.DeclaredButNotWrittenOut == stack[stackIndex].DefaultNamespaceState))
            {
                writer.Write(nsPrefix);
                writer.Write('=');
                writer.Write(quoteChar);
                encoder.Write(stack[stackIndex].DefaultNs);
                writer.Write(quoteChar);
                stack[stackIndex].DefaultNamespaceState = NamespaceState.DeclaredAndWrittenOut;
            }

            encoder.EndAttribute();

            if (empty)
            {
                if (NewLineBeforeAttribute)
                {
                    Ident(true);
                }
                else
                {
                    writer.Write(' ');
                }

                writer.Write('/');
            }

            writer.Write('>');
        }

        /*private static string CreateTagName(string prefix, string localName, string namespaceURI)
        {
            var name = new StringBuilder();

            if (false == String.IsNullOrEmpty(prefix))
            {
                name.Append(prefix).Append(':');
            }

            name.Append(localName);

            return name.ToString();
        }*/

        private void PushStack()
        {
            if (stackIndex == (stack.Length - 1))
            {
                var temp = new TagInfo[stack.Length + 10];

                if (stackIndex > 0)
                {
                    Array.Copy(stack, temp, stackIndex + 1);
                }

                stack = temp;
            }

            stackIndex++;

            stack[stackIndex].Init();
        }

        private static void VerifyPrefixXml(string prefix, string ns)
        {
            if (prefix == null || prefix.Length != 3)
            {
                return;
            }

            if ((prefix[0] == 'x' || prefix[0] == 'X') &&
                (prefix[1] == 'm' || prefix[1] == 'M') &&
                (prefix[2] == 'l' || prefix[2] == 'L'))
            {
                //                    if (XmlReservedNs.NsXml != ns)
                if (false == String.Equals(nsPrefix, ns, StringComparison.InvariantCulture))
                {
                    //                        throw new ArgumentException(Res.GetString(Res.Xml_InvalidPrefix));
                    throw new ArgumentException();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum NamespaceState
        {
            Uninitialized,
            NotDeclaredButInScope,
            DeclaredButNotWrittenOut,
            DeclaredAndWrittenOut
        }

        /// <summary>
        /// 
        /// </summary>
        private struct TagInfo
        {
            public string Name
            {
                get;
                set;
            }

            public string Prefix
            {
                get;
                set;
            }

            public int PrefixCount
            {
                get;
                set;
            }

            public NamespaceState DefaultNamespaceState
            {
                get;
                set;
            }

            public string DefaultNs
            {
                get;
                set;
            }

            public bool Mixed
            {
                get;
                set;
            }

            public int PreviosNsIndex
            {
                get;
                set;
            }

            public void Init()
            {
                Name = null;
                Prefix = null;
                DefaultNs = String.Empty;
                Mixed = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct Namespace
        {
            public string Prefix
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public bool IsDeclared
            {
                get;
                set;
            }

            public int PrevNsIndex
            {
                get;
                set;
            }

            internal void Set(string prefix, string ns, bool declared)
            {
                Prefix = prefix;
                Name = ns;
                IsDeclared = declared;
                PrevNsIndex = -1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private enum Token
        {
            StartElement,
            EndElement,
            LongEndElement,
            StartAttribute,
            EndAttribute,
            Content,
            Empty
        }

        /// <summary>
        /// 
        /// </summary>
        private enum State
        {
            Start,
            Prolog,
            Element,
            Attribute,
            Content,
            Epilog,
            Closed,
            Failed
        }

        private static readonly State[] stateTransitionsDefault =
        {
            // Token / State
            //                   Start          Prolog         Element        Attribute      Content        Epilog
            /* StartElement   */ State.Element, State.Element, State.Element, State.Element, State.Element, State.Element,
            /* EndElement     */ State.Failed, State.Failed, State.Content, State.Content, State.Content, State.Failed,
            /* LongEndElement */ State.Failed, State.Failed, State.Content, State.Content, State.Content, State.Failed,
            /* StartAttribute */ State.Attribute, State.Failed, State.Attribute, State.Attribute, State.Failed, State.Failed,
            /* EndAttribute   */ State.Failed, State.Failed, State.Failed, State.Element, State.Failed, State.Failed,
            /* Content        */ State.Content, State.Content, State.Content, State.Attribute, State.Content, State.Epilog
        };

        private static readonly State[] stateTransitionsDocument =
        {
            //                   Start         Prolog        Element        Attribute     Content       Epilog       
            /* StartElement   */ State.Failed, State.Element, State.Element, State.Element, State.Element, State.Failed,
            /* EndElement     */ State.Failed, State.Failed, State.Content, State.Content, State.Content, State.Failed,
            /* LongEndElement */ State.Failed, State.Failed, State.Content, State.Content, State.Content, State.Failed,
            /* StartAttribute */ State.Failed, State.Failed, State.Attribute, State.Attribute, State.Failed, State.Failed,
            /* EndAttribute   */ State.Failed, State.Failed, State.Failed, State.Element, State.Failed, State.Failed,
            /* Content        */ State.Failed, State.Failed, State.Content, State.Attribute, State.Content, State.Failed
        };
    }
}