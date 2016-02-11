using System.Collections.Generic;

namespace LibraProgramming.Xaml.Core
{
    public interface IXamlNode
    {
        IXamlNode Parent
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

        string Name
        {
            get;
        }

        IReadOnlyCollection<string> NameSegments
        {
            get;
        }
    }
}