namespace LibraProgramming.Parsing.Xaml
{
    internal class NodeName
    {
        public string Namespace
        {
            get;
        }

        public string Tag
        {
            get;
        }

        public NodeName(string ns, string tag)
        {
            Namespace = ns;
            Tag = tag;
        }
    }
}