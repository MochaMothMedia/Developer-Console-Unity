using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MochaMoth.DeveloperConsole.Examples
{
	[RequireComponent(typeof(TMP_InputField))]
	public class ConsoleInputBehaviour : SerializedMonoBehaviour
	{
		[SerializeField] IDeveloperConsole _developerConsole;
		[SerializeField] InputActionReference _enterAction;
		[SerializeField] InputActionReference _upAction;
		[SerializeField] InputActionReference _downAction;

		TMP_InputField _inputField;
		bool _isFocused = false;
		int _commandBufferIndex = 0;

		private void Awake()
		{
			_inputField = GetComponent<TMP_InputField>();
		}

		private void OnEnable()
		{
			_enterAction.action.performed += ReturnAction;
			_upAction.action.performed += UpAction;
			_downAction.action.performed += DownAction;
		}

		private void OnDisable()
		{
			_enterAction.action.performed -= ReturnAction;
			_upAction.action.performed -= UpAction;
			_downAction.action.performed -= DownAction;
		}

		private void Update()
		{
			if (_developerConsole == null)
				return;

			_isFocused = _inputField.isFocused;
		}

		private void ReturnAction(InputAction.CallbackContext _)
		{
			if (!_isFocused || _inputField.text.Length == 0) return;

			_developerConsole.ProcessCommand(_inputField.text);
			_inputField.text = string.Empty;
			_inputField.OnPointerClick(new PointerEventData(FindAnyObjectByType<EventSystem>()));
			_commandBufferIndex = 0;
		}

		private void UpAction(InputAction.CallbackContext _)
		{
			if (!_isFocused) return;
			
			_commandBufferIndex++;
			if (_commandBufferIndex > _developerConsole.BufferCount)
				_commandBufferIndex--;
			if (_commandBufferIndex == 0)
				_inputField.text = string.Empty;
			else
				_inputField.text = _developerConsole.GetCommandFromBuffer(_commandBufferIndex - 1);
		}

		private void DownAction(InputAction.CallbackContext _)
		{
			if (!_isFocused) return;
			
			_commandBufferIndex--;
			if (_commandBufferIndex <= 0)
			{
				_commandBufferIndex = 0;
				_inputField.text = string.Empty;
			}
			else
				_inputField.text = _developerConsole.GetCommandFromBuffer(_commandBufferIndex - 1);
		}
	}
}