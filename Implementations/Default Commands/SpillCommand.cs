using System.Collections.Generic;

namespace MochaMoth.DeveloperConsole.Implementations.Commands
{
	public class SpillCommand : IConsoleCommand
	{
		public string Name => "spill";
		public string Usage => "spill {command}";

		IDeveloperConsole _developerConsole;
		public IDeveloperConsole DeveloperConsole { get => _developerConsole; set => _developerConsole = value; }

		public string Execute(ICommandArguments arguments)
		{
			string commandOutput = string.Empty;

			if (arguments.ArgumentQuantity == 0)
			{
				_developerConsole.PushMessages(GetHelp(arguments));
				return commandOutput;
			}

			char[] alphabet = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

			commandOutput += $"Text Entered: {arguments.TextEntered.Substring(arguments.CommandName.Length + 1)}\n";
			commandOutput += $"Command: {arguments.GetArgument(0)}\n";
			commandOutput += $"Arguments: {arguments.ArgumentQuantity - 1}\n";

			for (int i = 1; i < arguments.ArgumentQuantity; i++)
				commandOutput += $"        {i - 1}: {arguments.GetArgument(i)}\n";

			List<char> assignedFlags = new List<char>();

			foreach (char flag in alphabet)
			{
				string flagValue = arguments.GetFlag(flag);
				if (flagValue != null)
					assignedFlags.Add(flag);
			}

			commandOutput += $"Flags: {assignedFlags.Count}\n";

			for (int i = 0; i < assignedFlags.Count; i++)
				commandOutput += $"        {assignedFlags[i]}: {arguments.GetFlag(assignedFlags[i])}\n";

			commandOutput = commandOutput.Substring(0, commandOutput.Length - 1);
			DeveloperConsole.PushMessages(commandOutput.Split('\n'));
			return commandOutput;
		}

		public string[] GetHelp(ICommandArguments arguments)
		{
			return new string[] {
				"Spills the command argument object contents of the given command.",
				$"Usage: {Usage}"
			};
		}
	}
}