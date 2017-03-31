using System;
using System.Text;

namespace LibraProgramming.Parsing.Xaml
{
    internal class XamlNameBuilder
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

        public void AccumulateName(string name)
        {
            if (String.IsNullOrEmpty(Name))
            {
                Name = name;
                return;
            }

            Name = new StringBuilder(Name).Append('.').Append(name).ToString();
        }

        public void Clear()
        {
            Prefix = null;
            Name = null;
        }

        public XamlName ToXamlName()
        {
            return XamlName.Create(Prefix, Name, null);
        }
    }
}