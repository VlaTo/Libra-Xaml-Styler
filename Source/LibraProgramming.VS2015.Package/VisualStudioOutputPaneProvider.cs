using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace LibraProgramming.VS2015.Package
{
    internal class VisualStudioOutputPaneProvider : IOutputProvider
    {
        private readonly IVsOutputWindowPane pane;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public VisualStudioOutputPaneProvider(IVsOutputWindowPane pane)
        {
            this.pane = pane;
        }

        public void WriteLine(string str)
        {
            var result = pane.OutputString(str + Environment.NewLine);

            if (Microsoft.VisualStudio.ErrorHandler.Failed(result))
            {
                Debug.WriteLine("Error: [XamlStyler] Failed to write to the Output window");
            }
        }
    }
}