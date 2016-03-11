namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public interface IXamlAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        IXamlNode Node
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        string Prefix
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        string Value
        {
            get;
        }
    }
}