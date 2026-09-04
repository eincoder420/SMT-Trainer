using UnityEngine;

namespace Invector.vCharacterController.vActions;

public interface IActionStayListener : IActionController
{
	void OnActionStay(Collider actionCollider);
}
