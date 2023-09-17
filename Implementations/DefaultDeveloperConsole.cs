using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MochaMoth.DeveloperConsole.Implementations
{
	public class DefaultDeveloperConsole : IDeveloperConsole
	{
		public LoggingLevel LoggingLevel { get => _loggingLevel; set => _loggingLevel = value; }
		public int BufferCount => _commandBuffer == null ? 0 : _commandBuffer.Count;
		StreamWriter LogFileWriter
		{
			get
			{
				if (_logFileWriter == null)
				{
					DateTime now = DateTime.UtcNow;

					string directoryName = $"{Application.persistentDataPath}/Logging Output";
					if (!Directory.Exists(directoryName))
						Directory.CreateDirectory(directoryName);

					string filename = $"{directoryName}/{now.Year:0000}-{now.Month:00}-{now.Day:00}-{now.Hour:00}-{now.Minute:00}-{now.Second:00}-{now.Millisecond:000}.txt";
					if (!File.Exists(filename))
					{
						FileStream newFileStream = File.Create(filename);
						newFileStream.Close();
						newFileStream.Dispose();
					}

					_logFileWriter = new StreamWriter(filename, true);
				}

				return _logFileWriter;
			}
		}

		#region Fields
		[SerializeField] OverrideRule _overrideRule = OverrideRule.Ignore;
		[SerializeField] SpacingStyle _spacingStyle = SpacingStyle.Spacious;
		[SerializeField] LoggingLevel _loggingLevel = LoggingLevel.Message | LoggingLevel.Warning | LoggingLevel.Error | LoggingLevel.Exception;
		[SerializeField] int _indentSize = 8;
		[SerializeField] int _maxVisibleCharacters = 8000;

		StreamWriter _logFileWriter;
		Dictionary<string, IConsoleCommand> _commands = new Dictionary<string, IConsoleCommand>();
		List<IPreProcessCommand> _preProcessCommands = new List<IPreProcessCommand>();
		string _messageLog;
		int _indent = 0;
		List<string> _commandBuffer = new List<string>();
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
							if (flags.ContainsKey(flagParts[0].ToCharArray()[j]))
								flags[flagParts[0].ToCharArray()[j]] = "true";
							else
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
			try
			{
				IConsoleCommand command = _commands[commandArguments.CommandName];
				return command.Execute(commandArguments);
			}
			catch (KeyNotFoundException keyNotFoundException)
			{
				Debug.LogError(keyNotFoundException.Message);
				PushMessage($"Command '{commandArguments.CommandName}' not found. Use command 'help' for available commands.");
				return string.Empty;
			}
			catch (Exception exception)
			{
				string[] messages = new string[]
				{
					$"There was an error while running the command.",
					$"'{commandArguments.CommandName}'",
					$"Produced:",
					$"{exception.Message}",
					$"{exception.StackTrace}"
				};

				PushMessages(messages);
				return string.Empty;
			}
		}
		#endregion

		#region Handle Indention
		void Indent(int indentLevel = 1) => _indent += _indentSize * indentLevel;
		void Dedent(int indentLevel = 1) => _indent -= _indentSize * indentLevel;
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
			if (_commandBuffer == null)
				_commandBuffer = new List<string>();
			_commandBuffer.Insert(0, input);
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
			string log = $"\n{new string(' ', _indent)}{message}";
			_messageLog += log;
			if (_messageLog.Length > _maxVisibleCharacters)
				_messageLog = _messageLog.Substring(_messageLog.Length - _maxVisibleCharacters);
			LogFileWriter.Write(log);
			_logFileWriter.Flush();
		}

		public void PushMessages(string[] messages)
		{
			string logs = "\n" + string.Join($"\n{new string(' ', _indent)}", messages);
			_messageLog += logs;
			if (_messageLog.Length > _maxVisibleCharacters)
				_messageLog = _messageLog.Substring(_messageLog.Length - _maxVisibleCharacters);
			LogFileWriter.Write(logs);
			_logFileWriter.Flush();
		}

		public void PushMessageIndented(string message, int indentLevel = 1)
		{
			Indent(indentLevel);
			PushMessage(message);
			Dedent(indentLevel);
		}

		public void PushMessagesIndented(string[] messages, int indentLevel = 1)
		{
			Indent(indentLevel);
			PushMessages(messages);
			Dedent(indentLevel);
		}

		public void ClearLog()
		{
			_messageLog = string.Empty;
		}

		public string GetCommandFromBuffer(int index)
		{
			if (_commandBuffer.Count > index)
				return _commandBuffer[index];
			else
				return _commandBuffer[_commandBuffer.Count - 1];
		}

		public void SetActive(bool active) { }
		#endregion

		#region IDisposable Implementation
		public void Dispose()
		{
			if (_logFileWriter != null)
			{
				_logFileWriter.Close();
				_logFileWriter.Dispose();
			}
		}
		#endregion
	}
}