using MochaMoth.DeveloperConsole.Implementations.Commands;
using System.Text.RegularExpressions;

namespace MochaMoth.DeveloperConsole.Implementations.PreProcessors
{
	public class ClipPreProcessor : IPreProcessCommand
	{
		public string Name => "clip";
		public string Usage => "Inserts clipped text into the given command.";
		public IDeveloperConsole DeveloperConsole { get; set; }

		public string PreProcess(string input)
		{
			ClipCommand clipCommand = DeveloperConsole.GetCommand<ClipCommand>();
			Regex regex = new Regex(@"\[[0-9]+\]", RegexOptions.ExplicitCapture);
			MatchCollection matches = regex.Matches(input);

			foreach (Match match in matches)
			{
				GroupCollection groups = match.Groups;

				foreach (Group group in groups)
				{
					string intString = group.Value.Substring(1, group.Value.Length - 2);
					if (int.TryParse(intString, out int intValue))
					{
						string clip = clipCommand.GetBuffer(intValue);
						if (!string.IsNullOrWhiteSpace(clip))
							input = input.Replace(group.Value, clip);
						else
							input = input.Replace(group.Value, "NULL");
					}
				}
			}

			return input;
		}

		public string[] GetHelp(ICommandArguments arguments)
		{
			return new string[]
			{
				$"{Name}: {Usage}",
				"To see what clips are available, use 'clip -l'",
				"To insert a clip into a command, take the index of the clip and surround it with brackets.",
				"'echo [0]'"
			};
		}
	}
}