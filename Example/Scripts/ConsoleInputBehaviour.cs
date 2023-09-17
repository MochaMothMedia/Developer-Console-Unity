using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MochaMoth.DeveloperConsole.Examples
{
	[RequireComponent(typeof(TMP_InputField))]
	public class ConsoleInputBehaviour : SerializedMonoBehaviour
	{
		[SerializeField] IDeveloperConsole _developerConsole;

		TMP_InputField _inputField;
		bool _isFocused = false;
		int _commandBufferIndex = 0;

		private void Awake()
		{
			_inputField = GetComponent<TMP_InputField>();
		}

		private void Update()
		{
			if (_developerConsole == null)
				return;

			if (_isFocused && Input.GetKeyDown(KeyCode.Return))
			{
				_developerConsole.ProcessCommand(_inputField.text);
				_inputField.text = string.Empty;
				_inputField.OnPointerClick(new PointerEventData(FindObjectOfType<EventSystem>()));
				_commandBufferIndex = 0;
			}
			else if (_isFocused && Input.GetKeyDown(KeyCode.UpArrow))
			{
				_commandBufferIndex++;
				if (_commandBufferIndex > _developerConsole.BufferCount)
					_commandBufferIndex--;
				if (_commandBufferIndex == 0)
					_inputField.text = string.Empty;
				else
					_inputField.text = _developerConsole.GetCommandFromBuffer(_commandBufferIndex - 1);
			}
			else if (_isFocused && Input.GetKeyDown(KeyCode.DownArrow))
			{
				_commandBufferIndex--;
				if (_commandBufferIndex <= 0)
				{
					_commandBufferIndex = 0;
					_inputField.text = string.Empty;
				}
				else
					_inputField.text = _developerConsole.GetCommandFromBuffer(_commandBufferIndex - 1);
			}
			else
				_isFocused = _inputField.isFocused;
		}
	}
}