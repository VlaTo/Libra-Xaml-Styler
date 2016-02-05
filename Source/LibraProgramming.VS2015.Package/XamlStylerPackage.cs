//------------------------------------------------------------------------------
// <copyright file="XamlStylerPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;
using LibraProgramming.VisualStudio.XamlStyler;
using LibraProgramming.VS2015.Package.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LibraProgramming.VS2015.Package
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [ProvideOptionPage(typeof(ToolOptions), "Libra XAML Styler", ToolOptions.GeneralCategoryName, 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class XamlStylerPackage : Microsoft.VisualStudio.Shell.Package, IDisposable
    {
        /// <summary>
        /// XamlStylerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "8e8ae411-eca9-4002-b8bf-d862c991df8e";

        internal static readonly string VsStd2KCmdIdGuid = typeof(VSConstants.VSStd2KCmdID).GUID.ToString("B");
        internal static readonly string VsStd97CmdIdGuid = typeof(VSConstants.VSStd97CmdID).GUID.ToString("B");

        private DTE dte;
        private CommandEvents formatDocumentEvent;
        private CommandEvents saveProjectItemEvent;
        private IOutputProvider output;
        private CommandFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="XamlStylerPackage"/> class.
        /// </summary>
        public XamlStylerPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            dte = GetService(typeof (DTE)) as DTE;

            if (null == dte)
            {
                throw new ArgumentNullException(nameof(dte));
            }

            var pane = GetService(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;

            if (null == pane)
            {
                throw new ArgumentNullException(nameof(pane));
            }

            output = new VisualStudioOutputPaneProvider(pane);

            formatDocumentEvent = dte.Events.CommandEvents[VsStd2KCmdIdGuid, (int) VSConstants.VSStd2KCmdID.FORMATDOCUMENT];
            saveProjectItemEvent = dte.Events.CommandEvents[VsStd97CmdIdGuid, (int) VSConstants.VSStd97CmdID.SaveProjectItem];

            factory = new CommandFactory(dte, output);

            formatDocumentEvent.BeforeExecute += OnFormatDocumentBeforeExecute;
            saveProjectItemEvent.BeforeExecute += OnSaveProjectItemBeforeExecute;
        }

        protected override void Dispose(bool dispose)
        {
            try
            {
                if (dispose)
                {
                    saveProjectItemEvent.BeforeExecute -= OnSaveProjectItemBeforeExecute;
                    formatDocumentEvent.BeforeExecute -= OnFormatDocumentBeforeExecute;

                    dte = null;
                    factory = null;

                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(dispose);
            }
        }

        private ToolOptions GetToolOptions()
        {
            return (ToolOptions) GetDialogPage(typeof (ToolOptions)).AutomationObject;
        }

        private bool FormatActiveDocument()
        {
            var document = dte.ActiveDocument;
            var command = factory.CreateCommand();

            if (command.CanExecute(document))
            {
                output.WriteLine($"Info : [XamlStyler] : Formating file: {document.FullName}");

                command.Execute(document);

                return true;
            }

            output.WriteLine($"Info : [XamlStyler] : Skipping file: {document.FullName}");

            return false;
        }

        private void OnFormatDocumentBeforeExecute(string guid, int id, object customin, object customout, ref bool canceldefault)
        {
            var options = GetToolOptions();

            if (!options.IsEnabled)
            {
                output.WriteLine("Info : [XamlStyler] : Formating disabled");

                return;
            }

            canceldefault = FormatActiveDocument();
        }

        private void OnSaveProjectItemBeforeExecute(string guid, int id, object customin, object customout, ref bool canceldefault)
        {
            var options = GetToolOptions();

            if (!options.IsEnabled || !options.FormatBeforeSave)
            {
                output.WriteLine("Info : [XamlStyler] : Formating disabled");

                return;
            }

            canceldefault = FormatActiveDocument();
        }
    }
}
