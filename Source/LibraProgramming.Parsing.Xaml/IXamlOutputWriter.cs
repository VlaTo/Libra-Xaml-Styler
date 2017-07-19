using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    public interface IXamlOutputWriter
    {
        void WriteElementBegin(bool forceClosing);

        void WriteElementName(string name);

        void WriteElementEnd(bool forceInline);

        void WriteElementValue(string value);

        void WriteAttributeBegin();

        void WriteAttributeName(string name);

        void WriteAttributeValue(string value);

        void WriteAttributeEnd();
    }
}