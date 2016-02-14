using System.Collections.Generic;
using System.Reflection;

namespace LibraProgramming.Xaml.Core
{
    public interface IXamlAttribute
    {
        IXamlNode Node
        {
            get;
        }

        string Prefix
        {
            get;
        }

        string Name
        {
            get;
        }

        string Value
        {
            get;
        }
    }
}