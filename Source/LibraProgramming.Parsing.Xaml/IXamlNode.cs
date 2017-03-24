using System.Collections.Generic;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public interface IXamlNode
    {
        /// <summary>
        /// 
        /// </summary>
        string BaseURI
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        XamlNodeName Name
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        IXamlNode Parent
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
/*        string Prefix
        {
            get;
        }*/

        /// <summary>
        /// 
        /// </summary>
        IReadOnlyCollection<IXamlAttribute> Attributes
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        IReadOnlyCollection<IXamlNode> Children
        {
            get;
        }
    }
}