using EnvDTE;
using LibraProgramming.VisualStudio.XamlStyler.Commands;

namespace LibraProgramming.VisualStudio.XamlStyler
{
    internal class CommandFactory
    {
        private readonly DTE dte;
        private readonly IOutputProvider output;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public CommandFactory(DTE dte, IOutputProvider output)
        {
            this.dte = dte;
            this.output = output;
        }

        public IToolCommand CreateCommand()
        {
            return new FormatDocumentCommand(dte, output);
        }
    }
}