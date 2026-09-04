using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.Utils;

public class vSetParentOfController : MonoBehaviour
{
	[vHelpBox("Set this GameObject as parent of the Controller", vHelpBoxAttribute.MessageType.None)]
	private vThirdPersonController cc;

	public UnityEvent onStart;

	private void Start()
	{
		cc = GetComponentInParent<vThirdPersonController>();
		base.transform.parent = cc.transform;
		onStart.Invoke();
	}
}
