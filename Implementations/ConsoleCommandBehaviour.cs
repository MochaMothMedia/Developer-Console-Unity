using Sirenix.OdinInspector;
using UnityEngine;

namespace MochaMoth.DeveloperConsole.Implementations
{
	public class ConsoleCommandBehaviour : SerializedMonoBehaviour
	{
		[SerializeField, HideLabel, BoxGroup("Developer Console")] IDeveloperConsole _developerConsole;
		[SerializeField, HideLabel, BoxGroup("Commands")] IConsoleCommand[] _commands = new IConsoleCommand[0];

		private void Awake()
		{
			if (_developerConsole == null)
				return;

			foreach (IConsoleCommand command in _commands)
				_developerConsole.RegisterCommand(command);
		}
	}
}