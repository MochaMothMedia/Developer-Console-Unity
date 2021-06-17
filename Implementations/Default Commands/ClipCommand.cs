using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FedoraDev.DeveloperConsole.Implementations.Commands
{
	public class ClipCommand : IConsoleCommand
	{
		public string Name => "clip";
		public string Usage => "clip {clip text}";
		public IDeveloperConsole DeveloperConsole { get; set; }

		Dictionary<int, string> _buffer = new Dictionary<int, string>();

		public string GetBuffer(int index)
		{
			if (_buffer == null)
				_buffer = new Dictionary<int, string>();

			if (_buffer.ContainsKey(index))
				return _buffer[index];
			return string.Empty;
		}

		void CopyToBuffer(string input, int bufferIndex)
		{
			if (_buffer == null)
				_buffer = new Dictionary<int, string>();

			if (_buffer.ContainsKey(bufferIndex))
				_buffer[bufferIndex] = input;
			else
				_buffer.Add(bufferIndex, input);
		}

		void ListBuffer()
		{
			foreach (KeyValuePair<int, string> bufferItem in _buffer)
				DeveloperConsole.PushMessage($"{bufferItem.Key}: {bufferItem.Value}");
		}

		public string Execute(ICommandArguments arguments)
		{
			List<string> commandOutput = new List<string>();
			string flagI = arguments.GetFlag('i');
			string flagL = arguments.GetFlag('l');
			int index = 0;

			if (!string.IsNullOrEmpty(flagL))
			{
				ListBuffer();
				return string.Join("\n", commandOutput);
			}

			if (!string.IsNullOrEmpty(flagI))
				if (!int.TryParse(flagI, out index))
					commandOutput.Add($"-i={flagI} is not a valid integer and will be placed at -i=0 instead.");

			if (string.IsNullOrWhiteSpace(arguments.GetFlag('c')))
			{
				List<string> copyText = new List<string>();
				for (int i = 0; i < arguments.ArgumentQuantity; i++)
					copyText.Add(arguments.GetArgument(i));

				string textToCopy = string.Join(" ", copyText);
				commandOutput.Add(textToCopy);
				CopyToBuffer(textToCopy, index);
			}
			else
			{
				Regex reg = new Regex(@"""([^""\\]*(?:\\.[^""\\]*)*)""", RegexOptions.ExplicitCapture);

				MatchCollection matches = reg.Matches(arguments.TextEntered);
				List<string> commands = new List<string>();

				foreach (Match match in matches)
				{
					GroupCollection groups = match.Groups;

					foreach (Group group in groups)
						commands.Add(group.Value.Substring(1, group.Value.Length - 2));
				}

				foreach (string command in commands)
				{
					string output = DeveloperConsole.ProcessCommand(command);
					commandOutput.Add(output);
					CopyToBuffer(output, index);
				}
			}

			return string.Join("\n", commandOutput);
		}

		public string[] GetHelp(ICommandArguments arguments)
		{
			return new string[] {
				"Copies the following text into a buffer.",
				$"Usage: {Usage}",
				"Flags:",
				"    -c=\"[command]\":\tCopies the output of the command stored in quotes.",
				"    -i=[index]:\t\tSpecifies an index to store the clip for multiple clip storage.",
				"    -l: Displays a list of the current clip buffer."
			};
		}
	}
}