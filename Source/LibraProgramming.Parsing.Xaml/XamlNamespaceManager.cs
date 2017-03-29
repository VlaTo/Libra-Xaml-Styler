using System;
using System.Collections;
using System.Collections.Generic;

namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlNamespaceManager : IEnumerable<XamlNamespaceScope>
    {
        public string DefaultNamespace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<XamlNamespaceScope> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public void AddNamespace(string prefix, string uri)
        {
            
        }

        public IDictionary<string, string> GetNamespacesInScope(XamlNamespaceScope scope)
        {
            throw new NotImplementedException();
        }

        public bool HasNamespace(string prefix)
        {
            throw new NotImplementedException();
        }

        public string LookupNamespace(string prefix)
        {
            throw new NotImplementedException();
        }

        public string LookupPrefix(string uri)
        {
            throw new NotImplementedException();
        }

        public bool PopScope()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}