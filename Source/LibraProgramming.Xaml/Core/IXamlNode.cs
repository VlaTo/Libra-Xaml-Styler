using System.Collections.Generic;

namespace LibraProgramming.Xaml.Core
{
    public interface IXamlNode
    {
        string BaseURI
        {
            get;
        }

        string Name
        {
            get;
        }

        IXamlNode Parent
        {
            get;
        }

        string Prefix
        {
            get;
        }

        bool IsInline
        {
            get;
        }

        IReadOnlyCollection<IXamlAttribute> Attributes
        {
            get;
        }

        IReadOnlyCollection<IXamlNode> Children
        {
            get;
        }
    }
}