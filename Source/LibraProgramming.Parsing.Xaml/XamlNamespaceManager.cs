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
            
        }

        public bool HasNamespace(string prefix)
        {
            
        }

        public string LookupNamespace(string prefix)
        {
            
        }

        public string LookupPrefix(string uri)
        {
            
        }

        public bool PopScope()
        {
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}