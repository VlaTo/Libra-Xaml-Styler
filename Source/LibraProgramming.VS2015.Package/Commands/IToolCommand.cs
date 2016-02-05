using EnvDTE;

namespace LibraProgramming.VS2015.Package.Commands
{
    public interface IToolCommand
    {
        bool CanExecute(Document document);

        void Execute(Document document);
    }
}