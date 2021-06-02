using System;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.DeveloperConsole.Implementations
{
	public class DefaultDeveloperConsole : IDeveloperConsole
	{
		[SerializeField] OverrideRule _overrideRule = OverrideRule.Ignore;
		[SerializeField] SpacingStyle _spacingStyle = SpacingStyle.Spacious;

		Dictionary<string, IConsoleCommand> _commands = new Dictionary<string, IConsoleCommand>();
		List<IPreProcessCommand> _preProcessCommands = new List<IPreProcessCommand>();

		[SerializeField] int _indentSize = 8;

		string _messageLog;
		int _indent = 0;

		void ManageDuplicateCommandName(IConsoleCommand command)
		{
			PushMessage($"Command with name '{command.Name}' is already registered.");

			switch (_overrideRule)
			{
				case OverrideRule.Ignore:
					PushMessage($"Command will be ignored.");
					break;

				case OverrideRule.Replace:
					PushMessage($"Command will be overridden.");
					_commands[command.Name] = command;
					break;

				case OverrideRule.Rename:
					PushMessage($"Command will be renamed.");

					int increment = 1;
					string newCommandName = $"{command.Name}_{increment}";

					while (_commands.ContainsKey(newCommandName))
						newCommandName = $"{command.Name}_{++increment}";

					_commands.Add(newCommandName, command);
					PushMessage($"Command will be named '{newCommandName}'");
					break;
			}

			if (_spacingStyle == SpacingStyle.Spacious)
				PushMessage(string.Empty);
		}

		ICommandArguments GetArgumentsAndFlags(string input)
		{
			string[] inputParts = input.Split(' ');
			string commandName = inputParts[0];
			List<string> arguments = new List<string>();
			Dictionary<char, string> flags = new Dictionary<char, string>();

			for (int i = 1; i < inputParts.Length; i++)
			{
				string part = inputParts[i];

				if (part.StartsWith("-"))
				{
					//This is a flag
					string[] flagParts = part.Split('=');
					if (flagParts.Length > 1)
					{
						for (int j = 1; j < flagParts[0].Length - 1; j++)
							flags.Add(flagParts[0].ToCharArray()[j], "true");
						flags.Add(flagParts[0].ToCharArray()[flagParts[0].Length - 1], flagParts[1]);
					}
					else
					{
						for (int j = 1; j < flagParts[0].Length; j++)
							flags.Add(flagParts[0].ToCharArray()[j], "true");
					}
				}
				else
				{
					//This is an argument
					arguments.Add(part);
				}
			}

			return new DefaultCommandArguments(input, commandName, arguments.ToArray(), flags);
		}

		void HandleHelpRequest(ICommandArguments commandArguments)
		{
			if (commandArguments.ArgumentQuantity == 0)
			{
				PushMessage("To use a command, use the following syntax:");

				Indent();
				PushMessage("{command name} [arguments|flags]");
				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);
				Deindent();

				PushMessage("Available Commands:");

				Indent();
				foreach (KeyValuePair<string, IConsoleCommand> consoleCommand in _commands)
					PushMessage($"{consoleCommand.Key}: {consoleCommand.Value.Usage}");
				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);
				Deindent();

				Deindent();
				return;
			}
			else
			{
				foreach (KeyValuePair<string, IConsoleCommand> consoleCommand in _commands)
					if (commandArguments.GetArgument(0) == consoleCommand.Key)
						PushMessages(consoleCommand.Value.GetHelp(commandArguments));
			}
		}

		void HandleCommand(ICommandArguments commandArguments)
		{
			foreach (KeyValuePair<string, IConsoleCommand> consoleCommand in _commands)
			{
				if (commandArguments.CommandName == consoleCommand.Key)
				{
					try
					{
						consoleCommand.Value.Execute(commandArguments);
						break;
					}
					catch (Exception e)
					{
						string[] messages = new string[]
						{
						$"There was an error while running the command.",
						$"'{commandArguments.CommandName}'",
						$"Produced:",
						$"{e.Message}",
						$"{e.StackTrace}"
						};

						PushMessages(messages);
						break;
					}
				}
			}
		}

		void Indent() => _indent += _indentSize;
		void Deindent() => _indent -= _indentSize;

		#region IDeveloperConsole Implementation
		public string MessageLog => _messageLog;

		public void RegisterCommand(IConsoleCommand command)
		{
			if (_commands == null)
				_commands = new Dictionary<string, IConsoleCommand>();

			if (_commands.ContainsKey(command.Name))
				ManageDuplicateCommandName(command);
			else
				_commands.Add(command.Name, command);

			command.DeveloperConsole = this;
		}

		public void ProcessCommand(string input)
		{
			PushMessage($"> {input}");
			Indent();

			ICommandArguments commandArguments = GetArgumentsAndFlags(input);

			if (commandArguments.CommandName == "help")
				HandleHelpRequest(commandArguments);
			else
				HandleCommand(commandArguments);

			if (_spacingStyle == SpacingStyle.Spacious)
				PushMessage(string.Empty);
			_indent = 0;
		}

		public void PushMessage(string message)
		{
			_messageLog += $"\n{new string(' ', _indent)}{message}";
		}

		public void PushMessages(string[] messages)
		{
			foreach (string message in messages)
				PushMessage(message);
		}

		public void ClearLog()
		{
			_messageLog = string.Empty;
		}
		#endregion
	}
}