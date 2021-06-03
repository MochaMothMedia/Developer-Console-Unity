using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace FedoraDev.DeveloperConsole.Implementations
{
	public class PreProcessCommandBehaviour : SerializedMonoBehaviour
	{
		[SerializeField, HideLabel, BoxGroup("Developer Console")] IDeveloperConsole _developerConsole;
		[SerializeField, HideLabel, BoxGroup("Pre Process Commands")] IPreProcessCommand[] _preProcessCommands = new IPreProcessCommand[0];

		private void Awake()
		{
			if (_developerConsole == null)
				return;

			foreach (IPreProcessCommand preProcessCommand in _preProcessCommands)
				_developerConsole.RegisterPreProcessCommand(preProcessCommand);
		}
	}
}