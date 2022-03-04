using Sirenix.OdinInspector;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
	[Button]
	void LogMessage() => Debug.Log("Test Log");

	[Button]
	void LogWarning() => Debug.LogWarning("Test Warning");

	[Button]
	void LogError() => Debug.LogError("Test Error");

	[Button]
	void LogException() => Debug.LogException(new System.Exception("Test Exception"), this);

	[Button]
	void LogAssertion() => Debug.LogAssertion("Test Assertion");
}
