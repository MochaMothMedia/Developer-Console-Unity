using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FedoraDev.DeveloperConsole.Implementations
{
	public class DeveloperConsoleBehaviour : SerializedMonoBehaviour, IDeveloperConsole
	{
		[SerializeField, HideLabel, BoxGroup("Console")] IDeveloperConsole _console;
		[SerializeField] TMP_Text _logField;
		[SerializeField] Scrollbar _scrollbar;

		private void Start()
		{
			UpdateConsoleWindow();
		}

		void UpdateConsoleWindow()
		{
			_logField.text = _console.MessageLog;
			_scrollbar.value = 0;
		}

		public string MessageLog => _console.MessageLog;

		public void RegisterCommand(IConsoleCommand command)
		{
			_console.RegisterCommand(command);
			command.DeveloperConsole = this;
		}

		public void RegisterPreProcessCommand(IPreProcessCommand preProcessCommand)
		{
			_console.RegisterPreProcessCommand(preProcessCommand);
			preProcessCommand.DeveloperConsole = this;
		}

		public T GetCommand<T>() where T : class, IConsoleCommand => _console.GetCommand<T>();
		public T GetPreProcessCommand<T>() where T : class, IPreProcessCommand => _console.GetPreProcessCommand<T>();

		public string ProcessCommand(string input)
		{
			string output = _console.ProcessCommand(input);
			UpdateConsoleWindow();
			return output;
		}

		public void PushMessage(string message)
		{
			_console.PushMessage(message);
			UpdateConsoleWindow();
		}

		public void PushMessages(string[] messages)
		{
			_console.PushMessages(messages);
			UpdateConsoleWindow();
		}

		public void ClearLog()
		{
			_console.ClearLog();
			UpdateConsoleWindow();
		}

		public void SetActive(bool active) => gameObject.SetActive(active);
	}
}