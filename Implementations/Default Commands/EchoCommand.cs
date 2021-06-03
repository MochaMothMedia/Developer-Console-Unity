namespace FedoraDev.DeveloperConsole.Implementations.Commands
{
	public class EchoCommand : IConsoleCommand
	{
		public string Name => "echo";
		public string Usage => "echo {text to display}";
		public IDeveloperConsole DeveloperConsole { get => _developerConsole; set => _developerConsole = value; }

		IDeveloperConsole _developerConsole;

		public string Execute(ICommandArguments arguments)
		{
			string commandOutput = arguments.TextEntered.Substring(arguments.CommandName.Length);
			DeveloperConsole.PushMessage(commandOutput);

			return commandOutput;
		}

		public string[] GetHelp(ICommandArguments arguments)
		{
			return new string[] { "Prints the text that follows the command to the console." };
		}
	}
}