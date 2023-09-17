using UnityEngine;

namespace MochaMoth.DeveloperConsole.Implementations
{
	public class DebugCommand : IConsoleCommand
	{
		public string Name => "debug";
		public string Usage => "debug [type] {message}";
		public IDeveloperConsole DeveloperConsole { get; set; }

		public string Execute(ICommandArguments arguments)
		{
			string message = "";
			for (int i = 0; i < arguments.ArgumentQuantity; i++)
				message += $" {arguments.GetArgument(i)}";

			if (arguments.GetFlag('w') == "true")
				Debug.LogWarning(message);

			else if (arguments.GetFlag('e') == "true")
				Debug.LogError(message);

			else if (arguments.GetFlag('x') == "true")
				Debug.LogException(new System.Exception(message));

			else if (arguments.GetFlag('a') == "true")
				Debug.LogAssertion(message);

			else
				Debug.Log(message);

			return message;
		}

		public string[] GetHelp(ICommandArguments arguments)
		{
			return new string[]
			{
				"Logs Unity messages to the Editor Console.",
				$"Usage: {Usage}",
				"types: -w: warning, -e: error, -x: exception, -a: assertion",
				"If no type is specified, a regular log is used."
			};
		}
	}
}
