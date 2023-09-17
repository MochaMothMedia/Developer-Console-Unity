namespace MochaMoth.DeveloperConsole
{
    public interface IConsoleCommand
    {
        string Name { get; }
        string Usage { get; }
        IDeveloperConsole DeveloperConsole { get; set; }
        string Execute(ICommandArguments arguments);
        string[] GetHelp(ICommandArguments arguments);
    }
}