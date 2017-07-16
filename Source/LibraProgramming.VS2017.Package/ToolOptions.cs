using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace LibraProgramming.VS2017.Package
{
    [ComVisible(true)]
    public class ToolOptions : DialogPage
    {
        internal const string GeneralCategoryName = "General";

        [Category(GeneralCategoryName)]
        [DisplayName(@"Enabled"), Description("When enabled, format document before save.")]
        [DefaultValue(true)]
        public bool IsEnabled
        {
            get;
            set;
        }

        [Category(GeneralCategoryName)]
        [DisplayName(@"Format Before Save"), Description("Auto format document before save.")]
        [DefaultValue(false)]
        public bool FormatBeforeSave
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="T:Microsoft.VisualStudio.Shell.DialogPage"/>.
        /// </summary>
        public ToolOptions()
        {
            IsEnabled = true;
            FormatBeforeSave = false;
        }
    }
}