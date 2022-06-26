namespace FedoraDev.DeveloperConsole
{
    public interface IDeveloperConsole
    {
        string MessageLog { get; }

        void SetActive(bool active);
        void RegisterCommand(IConsoleCommand command);
		void RegisterPreProcessCommand(IPreProcessCommand preProcessCommand);
        T GetCommand<T>() where T : class, IConsoleCommand;
        T GetPreProcessCommand<T>() where T : class, IPreProcessCommand;
        string ProcessCommand(string input);
        void PushMessage(string message);
        void PushMessages(string[] messages);
        void PushMessageIndented(string message);
        void PushMessagesIndented(string[] messages);
        void ClearLog();
	}
}