using EnvDTE;

namespace LibraProgramming.VisualStudio.XamlStyler.Commands
{
    public interface IToolCommand
    {
        bool CanExecute(Document document);

        void Execute(Document document);
    }
}