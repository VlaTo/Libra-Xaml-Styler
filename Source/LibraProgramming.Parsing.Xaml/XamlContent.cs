using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class XamlContent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public abstract void WriteTo(XamlWriter writer);
    }
}