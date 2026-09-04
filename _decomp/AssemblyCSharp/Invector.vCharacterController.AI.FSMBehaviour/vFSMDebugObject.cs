using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vFSMDebugObject
{
	public string message = string.Empty;

	public Object sender;

	public vFSMDebugObject(string message, Object sender = null)
	{
		if (!string.IsNullOrEmpty(message))
		{
			this.message = message;
		}
		this.sender = sender;
	}
}
