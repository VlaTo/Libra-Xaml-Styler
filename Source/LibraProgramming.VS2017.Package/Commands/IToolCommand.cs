using EnvDTE;

namespace LibraProgramming.VS2017.Package.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface IToolCommand

    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        bool CanExecute(Document document);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        void Execute(Document document);
    }
}