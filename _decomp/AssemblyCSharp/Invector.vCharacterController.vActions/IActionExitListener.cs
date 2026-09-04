using UnityEngine;

namespace Invector.vCharacterController.vActions;

public interface IActionExitListener : IActionController
{
	void OnActionExit(Collider actionCollider);
}
