using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using LibraProgramming.Parsing.Xaml;
using LibraProgramming.Parsing.Xaml.Visitors;

namespace LibraProgramming.VS2015.Package.Commands
{
    public class FormatDocumentCommand : IToolCommand
    {
        private readonly DTE dte;
        private readonly IOutputProvider output;
        private readonly DocumentReformatSettings settings;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public FormatDocumentCommand(DTE dte, IOutputProvider output, DocumentReformatSettings settings)
        {
            this.dte = dte;
            this.output = output;
            this.settings = settings;
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

            output.WriteLine($"[XamlStyler] Source document: \'{document.FullName}\'");

            var start = textDocument.StartPoint.CreateEditPoint();
            var end = textDocument.EndPoint.CreateEditPoint();

            var current = textDocument.Selection.ActivePoint;
            var linenumber = current.Line;
            var charoffset = current.LineCharOffset;

            ReformatSourceAsync(start, end).Wait(TimeSpan.FromMinutes(1.0d));

            if (linenumber <= textDocument.EndPoint.Line)
            {
                var position = start.CreateEditPoint();

                if (linenumber > 0)
                {
                    position.LineDown(linenumber - 1);
                }

                charoffset = Math.Min(charoffset, position.LineLength + 1);

                if (charoffset > 0)
                {
                    textDocument.Selection.MoveToLineAndOffset(linenumber, charoffset);
                }
                else
                {
                    textDocument.Selection.GotoLine(linenumber);
                }
            }
            else
            {
                var position = start.CreateEditPoint();

                position.EndOfDocument();
                textDocument.Selection.MoveToPoint(position);
            }
        }

        private async Task ReformatSourceAsync(EditPoint start, EditPoint end)
        {
            using (var reader = new StringReader(start.GetText(end)))
            {
                var document = await XamlDocument.ParseAsync(reader);
                var text = new StringBuilder();

                ReformatDocument(document, text, settings);

                var flag = (int) vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers;

                start.ReplaceText(end, text.ToString(), flag);
            }
        }

        private static void ReformatDocument(XamlDocument document, StringBuilder text,
            DocumentReformatSettings reformatSettings)
        {
            var textWriter = new StringWriter(text);

            using (var writer = new XamlWriter(textWriter))
            {
                var visitor = new ReformatXamlVisitor(writer, reformatSettings);
                visitor.Visit(document);
            }
        }
    }
}