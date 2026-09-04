using UnityEngine;

namespace Invector.vCharacterController.vActions;

public interface IActionEnterListener : IActionController
{
	void OnActionEnter(Collider actionCollider);
}
