using FedoraDev.DeveloperConsole.Implementations.Commands;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FedoraDev.DeveloperConsole.Implementations.PreProcessors
{
	public class ClipPreProcessor : IPreProcessCommand
	{
		public IDeveloperConsole DeveloperConsole { get; set; }

		[SerializeField] ClipCommand _clipCommand;

		public string PreProcess(string input)
		{
			ClipCommand spillCommand = DeveloperConsole.GetCommand<ClipCommand>();
			Regex regex = new Regex(@"\[[0-9]+\]", RegexOptions.ExplicitCapture);
			MatchCollection matches = regex.Matches(input);

			foreach (Match match in matches)
			{
				GroupCollection groups = match.Groups;

				foreach (Group group in groups)
				{
					string intString = group.Value.Substring(1, group.Value.Length - 2);
					if (int.TryParse(intString, out int intValue))
						input = input.Replace(group.Value, spillCommand.GetBuffer(intValue));
				}
			}

			return input;
		}
	}
}