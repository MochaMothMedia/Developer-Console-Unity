namespace FedoraDev.DeveloperConsole
{
    public interface IPreProcessCommand
    {
        IDeveloperConsole DeveloperConsole { get; set; }
        string PreProcess(string input);
    }
}