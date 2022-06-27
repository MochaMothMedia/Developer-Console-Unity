using System;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.DeveloperConsole.Implementations
{
	public class ConsoleCommand : IConsoleCommand
	{
		public string Name => "console";
		public string Usage => "console {command} {operands}";

		public IDeveloperConsole DeveloperConsole { get; set; }

		Dictionary<string, Func<ICommandArguments, string>> _subCommands;

		public string Execute(ICommandArguments arguments)
		{
			_subCommands = new Dictionary<string, Func<ICommandArguments, string>>();

			_subCommands["logging"] = Logging;

			if (arguments.ArgumentQuantity == 0)
			{
				Debug.LogWarning("Please pass the sub-command you are attempting to use.");
				Debug.LogWarning(Usage);
				return string.Empty;
			}

			if (_subCommands.ContainsKey(arguments.GetArgument(0)))
				return _subCommands[arguments.GetArgument(0)](arguments);

			Debug.LogError($"No sub command found matching {arguments.GetArgument(0)}");
			return string.Empty;
		}

		public string[] GetHelp(ICommandArguments arguments)
		{
			List<string> strings = new List<string>();
			strings.Add("Enables modifying the console's behaviour mid-game.");
			strings.Add($"Usage: {Usage}");
			strings.Add("Available commands:");
			strings.Add("        logging - Print the state of all logging levels.");
			strings.Add("        logging [message|warning|error|exception|assertion] - Check the status of the desired logging level.");
			strings.Add("        logging [set|unset] [message|warning|error|exception|assertion] - Modify the logging level of the console.");

			return strings.ToArray();
		}

		string Logging(ICommandArguments arguments)
		{
			if (arguments.ArgumentQuantity == 1)
			{
				LogCurrentLoggingLevels();
				return string.Empty;
			}

			LoggingLevel level;

			switch (arguments.GetArgument(1))
			{
				case "set":
					if (arguments.ArgumentQuantity == 1)
					{
						Debug.LogWarning($"Please pass in one of [message|warning|error|exception|assertion].");
						return string.Empty;
					}
					level = GetLoggingLevelFromArgument(arguments.GetArgument(2));
					if (level == LoggingLevel.None)
						return string.Empty;
					DeveloperConsole.LoggingLevel |= level;
					LogCurrentLoggingLevels();
					break;

				case "unset":
					if (arguments.ArgumentQuantity == 1)
					{
						Debug.LogWarning($"Please pass in one of [message|warning|error|exception|assertion].");
						return string.Empty;
					}
					level = GetLoggingLevelFromArgument(arguments.GetArgument(2));
					if (level == LoggingLevel.None)
						return string.Empty;
					DeveloperConsole.LoggingLevel ^= level;
					LogCurrentLoggingLevels();
					break;

				default:
					level = GetLoggingLevelFromArgument(arguments.GetArgument(1));
					if (level == LoggingLevel.None)
						return string.Empty;
					DeveloperConsole.PushMessage($"{Enum.GetName(typeof(LoggingLevel), level)}: {DeveloperConsole.LoggingLevel.HasFlag(level)}");
					break;
			}

			return string.Empty;
		}

		LoggingLevel GetLoggingLevelFromArgument(string argument)
		{
			try
			{
				string levelString = argument.Substring(0, 1).ToUpper() + argument.Substring(1).ToLower();
				return (LoggingLevel)Enum.Parse(typeof(LoggingLevel), levelString);
			}
			catch (ArgumentException)
			{
				Debug.LogError($"The provided argument '{argument}' is not a valid Logging Level. Please choose one of [{LoggingLevelHelpers.LoggingLevelAll}].");
				return LoggingLevel.None;
			}
		}

		void LogCurrentLoggingLevels()
		{
			DeveloperConsole.PushMessage($"Current Logging Flags: {DeveloperConsole.LoggingLevel}");
			DeveloperConsole.PushMessage($"Other Logging Flags: {LoggingLevelHelpers.LoggingLevelAll ^ DeveloperConsole.LoggingLevel}");
		}
	}
}
