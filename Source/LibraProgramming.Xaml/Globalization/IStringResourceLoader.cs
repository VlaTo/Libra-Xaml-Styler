namespace LibraProgramming.Xaml.Globalization
{
    /// <summary>
    /// Represents a custom loader that may be used to lookup string resouces from the code.
    /// </summary>
    public interface IStringResourceLoader
    {
        /// <summary>
        /// Gets the localized string resource associated with key specified.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>Localized string resource.</returns>
        string GetString(string key);
    }
}