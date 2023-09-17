using Sirenix.OdinInspector;
using UnityEngine;

namespace MochaMoth.DeveloperConsole.Examples
{
	public class DebugTest : MonoBehaviour
	{
		string _lorem = "Varius morbi enim nunc faucibus a. Elit ut aliquam purus sit amet luctus venenatis lectus magna. At augue eget arcu dictum varius. Dignissim cras tincidunt lobortis feugiat vivamus at augue eget arcu. Semper eget duis at tellus. Nunc mi ipsum faucibus vitae aliquet nec ullamcorper sit amet. Elementum integer enim neque volutpat ac tincidunt vitae. Faucibus nisl tincidunt eget nullam non. Dictum non consectetur a erat nam at lectus urna. Viverra orci sagittis eu volutpat odio facilisis mauris. Non curabitur gravida arcu ac tortor dignissim convallis aenean et. Bibendum arcu vitae elementum curabitur vitae.";

		bool _logEveryFrame;

		[Button]
		void LogMessage() => Debug.Log(CreateOutputMessage());

		[Button]
		void LogWarning() => Debug.LogWarning(CreateOutputMessage());

		[Button]
		void LogError() => Debug.LogError(CreateOutputMessage());

		[Button]
		void LogException() => Debug.LogException(new System.Exception(CreateOutputMessage()), this);

		[Button]
		void LogAssertion() => Debug.LogAssertion(CreateOutputMessage());

		[Button]
		void StartLogging() => _logEveryFrame = true;

		[Button]
		void StopLogging() => _logEveryFrame = false;

		void Update()
		{
			if (_logEveryFrame)
			{
				int sel = Random.Range(0, 100);

				if (sel < 80)
					LogMessage();
				else if (sel < 90)
					LogWarning();
				else if (sel < 95)
					LogError();
				else if (sel < 99)
					LogAssertion();
				else
					LogException();
			}
		}

		string CreateOutputMessage()
		{
			int loremStart = Random.Range(0, _lorem.Length - 100);
			int loremLength = Random.Range(50, 100);
			return _lorem.Substring(loremStart, loremLength);
		}
	}
}