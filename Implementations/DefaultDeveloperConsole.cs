using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FedoraDev.DeveloperConsole.Implementations
{
	public class DefaultDeveloperConsole : IDeveloperConsole
	{
		#region Fields
		[SerializeField] OverrideRule _overrideRule = OverrideRule.Ignore;
		[SerializeField] SpacingStyle _spacingStyle = SpacingStyle.Spacious;
		[SerializeField] int _indentSize = 8;

		Dictionary<string, IConsoleCommand> _commands = new Dictionary<string, IConsoleCommand>();
		List<IPreProcessCommand> _preProcessCommands = new List<IPreProcessCommand>();
		string _messageLog;
		int _indent = 0;
		#endregion

		#region Duplicate Command Handling
		void ManageDuplicateCommandName(IConsoleCommand command)
		{
			PushMessage($"Command with name '{command.Name}' is already registered.");

			switch (_overrideRule)
			{
				case OverrideRule.Ignore:
					PushMessage($"Command will be ignored.");
					break;

				case OverrideRule.Replace:
					HandleReplaceCommand(command);
					break;

				case OverrideRule.Rename:
					HandleRenameCommand(command);
					break;
			}

			if (_spacingStyle == SpacingStyle.Spacious)
				PushMessage(string.Empty);
		}

		void HandleReplaceCommand(IConsoleCommand command)
		{
			PushMessage($"Command will be overridden");
			_commands[command.Name] = command;
		}

		void HandleRenameCommand(IConsoleCommand command)
		{
			PushMessage($"Command will be renamed.");

			int increment = 1;
			string newCommandName = $"{command.Name}_{increment}";

			while (_commands.ContainsKey(newCommandName))
				newCommandName = $"{command.Name}_{++increment}";

			_commands.Add(newCommandName, command);
			PushMessage($"Command will be named '{newCommandName}'");
		}
		#endregion

		#region Get Arguments and Flags
		ICommandArguments GetArgumentsAndFlags(string input)
		{
			Regex regex = new Regex(@"-[a-xA-Z]=(""([^""\\]*(?:\\.[^""\\]*)*)""|'([^'\\]*(?:\\.[^'\\]*)*)')|[^\s]+", RegexOptions.ExplicitCapture);
			MatchCollection matches = regex.Matches(input);
			List<string> inputParts = new List<string>();

			foreach (Match match in matches)
			{
				GroupCollection groups = match.Groups;

				foreach (Group group in groups)
					inputParts.Add(group.Value);
			}

			string commandName = inputParts[0];
			List<string> arguments = new List<string>();
			Dictionary<char, string> flags = new Dictionary<char, string>();

			for (int i = 1; i < inputParts.Count; i++)
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
		#endregion

		#region Handle Help Request
		void HandleHelpRequest(ICommandArguments commandArguments)
		{
			if (commandArguments.ArgumentQuantity == 0)
			{
				PushMessage("To use a command, use the following syntax:");

				Indent();
				PushMessage("{command name} [arguments|flags]");
				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);
				Dedent();

				PushMessage("Available Commands:");

				Indent();
				foreach (KeyValuePair<string, IConsoleCommand> consoleCommand in _commands)
					PushMessage($"{consoleCommand.Key}: {consoleCommand.Value.Usage}");
				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);
				Dedent();

				PushMessage("Available Preprocessors:");

				Indent();
				foreach (IPreProcessCommand preProcessCommand in _preProcessCommands)
					PushMessage($"{preProcessCommand.Name}: {preProcessCommand.Usage}");
				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);
				Dedent();
				return;
			}
			else
			{
				foreach (KeyValuePair<string, IConsoleCommand> consoleCommand in _commands)
					if (commandArguments.GetArgument(0) == consoleCommand.Key)
						PushMessages(consoleCommand.Value.GetHelp(commandArguments));

				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);

				foreach (IPreProcessCommand preProcessCommand in _preProcessCommands)
				{
					if (commandArguments.GetArgument(0) == preProcessCommand.Name)
					{
						PushMessage("Preprocessor:");
						Indent();
						PushMessages(preProcessCommand.GetHelp(commandArguments));
						Dedent();
					}
				}
			}
		}
		#endregion

		#region Handle Command
		string HandleCommand(ICommandArguments commandArguments)
		{
			foreach (KeyValuePair<string, IConsoleCommand> consoleCommand in _commands)
			{
				if (commandArguments.CommandName == consoleCommand.Key)
				{
					try
					{
						return consoleCommand.Value.Execute(commandArguments);
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
						return string.Empty;
					}
				}
			}

			return string.Empty;
		}
		#endregion

		#region Handle Indention
		void Indent() => _indent += _indentSize;
		void Dedent() => _indent -= _indentSize;
		#endregion

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

		public void RegisterPreProcessCommand(IPreProcessCommand preProcessCommand)
		{
			if (_preProcessCommands == null)
				_preProcessCommands = new List<IPreProcessCommand>();

			_preProcessCommands.Add(preProcessCommand);
			preProcessCommand.DeveloperConsole = this;
		}

		public T GetCommand<T>() where T : class, IConsoleCommand
		{
			foreach (KeyValuePair<string, IConsoleCommand> command in _commands)
				if (command.Value is T)
					return command.Value as T;

			return null;
		}

		public T GetPreProcessCommand<T>() where T : class, IPreProcessCommand
		{
			foreach (IPreProcessCommand preProcessCommand in _preProcessCommands)
				if (preProcessCommand is T)
					return preProcessCommand as T;

			return null;
		}

		public string ProcessCommand(string input)
		{
			string[] commandPipeline = input.Split('|');
			string output = "";

			for (int i = 0; i < commandPipeline.Length; i++)
			{
				string command = commandPipeline[i].Trim();

				PushMessage($"> {command}");
				int indents = 0;
				Indent();
				indents++;

				foreach (IPreProcessCommand preProcessCommand in _preProcessCommands)
				{
					string newInput = preProcessCommand.PreProcess(command);

					if (newInput != command)
					{
						command = newInput;
						PushMessage($"{command}");
						Indent();
						indents++;

						if (string.IsNullOrWhiteSpace(command))
						{
							for (int j = 0; j < indents; j++)
								Dedent();
							return string.Empty;
						}
					}
				}

				ICommandArguments commandArguments = GetArgumentsAndFlags(command);

				output = string.Empty;

				if (commandArguments.CommandName == "help")
					HandleHelpRequest(commandArguments);
				else
					output = HandleCommand(commandArguments);

				if (_spacingStyle == SpacingStyle.Spacious)
					PushMessage(string.Empty);
				for (int j = 0; j < indents; j++)
					Dedent();

				if (commandPipeline.Length > i + 1)
					commandPipeline[i + 1] += $" {output}";
			}

			return output;
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

		public void SetActive(bool active) { }
		#endregion
	}
}