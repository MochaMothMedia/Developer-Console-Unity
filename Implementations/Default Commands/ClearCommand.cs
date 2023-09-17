namespace MochaMoth.DeveloperConsole.Implementations.Commands
{
	public class ClearCommand : IConsoleCommand
	{
		IDeveloperConsole _developerConsole;

		public string Name => "clear";
		public string Usage => "clear";

		public IDeveloperConsole DeveloperConsole { get => _developerConsole; set => _developerConsole = value; }

		public string Execute(ICommandArguments arguments)
		{
			_developerConsole.ClearLog();
			return string.Empty;
		}

		public string[] GetHelp(ICommandArguments arguments) => new string[] { "Clears the console's log." };
	}
}