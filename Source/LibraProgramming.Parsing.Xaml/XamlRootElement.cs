using System;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlRootElement : XamlElement
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Name => LocalName;

        /// <summary>
        /// 
        /// </summary>
        public override string Prefix => String.Empty;

        /// <summary>
        /// 
        /// </summary>
        public override string LocalName => "#root";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        public XamlRootElement(XamlDocument document)
            : base(XamlNodeType.Root, document)
        {
        }
    }
}