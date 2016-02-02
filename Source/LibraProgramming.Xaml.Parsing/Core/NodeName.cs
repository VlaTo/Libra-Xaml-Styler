using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibraProgramming.Xaml.Parsing.Core
{
    internal class NodeName
    {
        public string Namespace
        {
            get;
        }

        public IReadOnlyCollection<string> Parts
        {
            get;
        }

        public NodeName(string ns, IList<string> parts)
        {
            if (null == ns)
            {
                throw new ArgumentNullException(nameof(ns));
            }

            if (null == parts)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            Namespace = ns;
            Parts = new ReadOnlyCollection<string>(parts);
        }
    }
}