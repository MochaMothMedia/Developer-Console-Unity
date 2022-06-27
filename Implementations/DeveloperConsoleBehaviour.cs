using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FedoraDev.DeveloperConsole.Implementations
{
	public class DeveloperConsoleBehaviour : SerializedMonoBehaviour, IDeveloperConsole
	{
		public LoggingLevel LoggingLevel { get => _console.LoggingLevel; set => _console.LoggingLevel = value; }
		public int BufferCount => _console.BufferCount;

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

		private void OnDestroy()
		{
			_console.Dispose();
		}

		void HandleLog(string logString, string stackTrace, LogType logType)
		{
			if (!_catchEditorLogs)
				return;

			if ((logType == LogType.Log       && !LoggingLevel.HasFlag(LoggingLevel.Message))   ||
			    (logType == LogType.Warning   && !LoggingLevel.HasFlag(LoggingLevel.Warning))   ||
			    (logType == LogType.Error     && !LoggingLevel.HasFlag(LoggingLevel.Error))     ||
			    (logType == LogType.Exception && !LoggingLevel.HasFlag(LoggingLevel.Exception)) ||
			    (logType == LogType.Assert    && !LoggingLevel.HasFlag(LoggingLevel.Assertion)))
			   return;

			string richTextColor = string.Empty;

			switch (logType)
			{
				case LogType.Log:
					richTextColor = "<color=\"white\">";
					break;
				case LogType.Warning:
					richTextColor = "<color=\"yellow\">";
					break;
				case LogType.Error:
					richTextColor = "<color=\"red\">";
					break;
				case LogType.Exception:
					richTextColor = "<color=#AA0000>";
					break;
				case LogType.Assert:
					richTextColor = "<color=\"green\">";
					break;
				default:
					break;
			}

			PushMessage($"{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}:{DateTime.Now.Millisecond:000} - {Time.frameCount} - {richTextColor}{logType}</color> - {logString}");

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

		public void PushMessageIndented(string message, int indentLevel = 1)
		{
			_console.PushMessageIndented(message, indentLevel);
			UpdateConsoleWindow();
		}

		public void PushMessagesIndented(string[] messages, int indentLevel = 1)
		{
			_console.PushMessagesIndented(messages, indentLevel);
			UpdateConsoleWindow();
		}

		public void ClearLog()
		{
			_console.ClearLog();
			UpdateConsoleWindow();
		}

		public string GetCommandFromBuffer(int index)
		{
			return _console.GetCommandFromBuffer(index);
		}

		public void Dispose()
		{
			_console.Dispose();
		}

		public void SetActive(bool active) => gameObject.SetActive(active);
	}
}