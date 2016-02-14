using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibraProgramming.Xaml.Core
{
    internal class NodeName
    {
        public string Prefix
        {
            get;
        }

        public string Name
        {
            get;
        }

        public NodeName(string prefix, string name)
        {
            if (null == prefix)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Prefix = prefix;
            Name = name;
        }
    }
}