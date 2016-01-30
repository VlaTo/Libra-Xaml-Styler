using System;
using System.Diagnostics;
using EnvDTE;

namespace LibraProgramming.VisualStudio.XamlStyler.Commands
{
    public class FormatDocumentCommand : IToolCommand
    {
        private readonly DTE dte;
        private readonly IOutputProvider output;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public FormatDocumentCommand(DTE dte, IOutputProvider output)
        {
            this.dte = dte;
            this.output = output;
        }

        public bool CanExecute(Document document)
        {
            if (null == document)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if ("xaml" != document.Language.ToLowerInvariant())
            {
                return false;
            }

            if (document.ReadOnly)
            {
                Debug.WriteLine("ReadOnly");
                return false;
            }

            return true;
        }

        public void Execute(Document document)
        {
            if (null == document)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var textDocument = document.Object("TextDocument") as TextDocument;

            if (null == textDocument)
            {
                return;
            }

            output.WriteLine($"[XamlStyler] formatting document: {document.FullName}");

            var start = textDocument.StartPoint.CreateEditPoint();
            var end = textDocument.EndPoint.CreateEditPoint();

            var current = textDocument.Selection.ActivePoint;
            var linenumber = current.Line;
            var charoffset = current.LineCharOffset;

            var text = start.GetText(end);


        }
    }
}