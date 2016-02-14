using System;

namespace LibraProgramming.Xaml.Globalization
{
    public abstract class LocalizationManager
    {
        /*public static ResourceMap GlobalResourceMap
        {
            get;
            set;
        }

        public ResourceMap DefaultResourceMap
        {
            get;
            protected set;
        }

        public ResourceMap UserResourceMap
        {
            get;
            set;
        }

        public IStringResourceLoader StringLoader
        {
            get;
            set;
        }

        public string GetString(string key)
        {
            var str = StringLoader?.GetString(key);

            if (!String.IsNullOrEmpty(str))
            {
                return str;
            }

            string value;

            if (null != UserResourceMap && TryGetString(UserResourceMap, key, out value))
            {
                return value;
            }

            if (null != GlobalResourceMap && TryGetString(GlobalResourceMap, key, out value))
            {
                return value;
            }

            if (null != DefaultResourceMap && TryGetString(DefaultResourceMap, key, out value))
            {
                return value;
            }

            return String.Empty;
        }

        private static bool TryGetString(ResourceMap map, string key, out string @string)
        {
            var candidate = map.GetValue(key, ResourceContext.GetForCurrentView());

            if (null != candidate)
            {
                @string = candidate.ValueAsString;
                
                if (!String.IsNullOrEmpty(@string))
                {
                    return true;
                }
            }

            @string = null;

            return false;
        }*/
    }
}
