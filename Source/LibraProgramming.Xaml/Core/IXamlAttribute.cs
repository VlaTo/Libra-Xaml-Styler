using System.Collections.Generic;

namespace LibraProgramming.Xaml.Core
{
    public interface IXamlAttribute
    {
        IXamlNode Node
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

        string Value
        {
            get;
        }
    }
}