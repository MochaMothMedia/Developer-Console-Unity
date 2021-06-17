namespace FedoraDev.DeveloperConsole
{
    public interface IPreProcessCommand
    {
        string Name { get; }
        string Usage { get; }
        IDeveloperConsole DeveloperConsole { get; set; }
        string PreProcess(string input);
        string[] GetHelp(ICommandArguments arguments);
    }
}