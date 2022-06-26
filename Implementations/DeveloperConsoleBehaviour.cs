using Sirenix.OdinInspector;
using System;
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
		[SerializeField] bool _catchEditorLogs = true;
		[SerializeField, ShowIf("_catchEditorLogs")] bool _showStackTraceOnError = true;

		private void OnEnable()
		{
			Application.logMessageReceived += HandleLog;
		}

		private void OnDisable()
		{
			Application.logMessageReceived -= HandleLog;
		}

		private void Start()
		{
			UpdateConsoleWindow();
		}

		void HandleLog(string logString, string stackTrace, LogType logType)
		{
			if (!_catchEditorLogs)
				return;

			string richTextColor = string.Empty;

			switch (logType)
			{
				case LogType.Error:
					richTextColor = "<color=\"red\">";
					break;
				case LogType.Assert:
					richTextColor = "<color=\"green\">";
					break;
				case LogType.Warning:
					richTextColor = "<color=\"yellow\">";
					break;
				case LogType.Log:
					richTextColor = "<color=\"white\">";
					break;
				case LogType.Exception:
					richTextColor = "<color=#AA0000>";
					break;
				default:
					break;
			}

			PushMessage($"{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}:{DateTime.Now.Millisecond:000} - {richTextColor}{logType}</color> - {logString}");

			if (logType == LogType.Exception && _showStackTraceOnError)
				PushMessagesIndented(stackTrace.Split('\n'));
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

		public void PushMessageIndented(string message)
		{
			_console.PushMessageIndented(message);
			UpdateConsoleWindow();
		}

		public void PushMessagesIndented(string[] messages)
		{
			_console.PushMessagesIndented(messages);
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