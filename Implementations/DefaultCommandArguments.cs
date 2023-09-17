using System.Collections.Generic;

namespace MochaMoth.DeveloperConsole.Implementations
{
	public struct DefaultCommandArguments : ICommandArguments
	{
		string _textEntered;
		string _commandName;
		string[] _arguments;
		Dictionary<char, string> _flags;

		public DefaultCommandArguments(string textEntered, string commandName, string[] arguments, Dictionary<char, string> flags)
		{
			_textEntered = textEntered;
			_commandName = commandName;
			_arguments = arguments;
			_flags = flags;
		}

		public string TextEntered => _textEntered;
		public string CommandName => _commandName;
		public int ArgumentQuantity => _arguments.Length;

		public string GetArgument(int index)
		{
			if (index < ArgumentQuantity)
				return _arguments[index];
			return null;
		}

		public string GetFlag(char flagName)
		{
			if (_flags.ContainsKey(flagName))
				return _flags[flagName];
			return null;
		}
	}
}