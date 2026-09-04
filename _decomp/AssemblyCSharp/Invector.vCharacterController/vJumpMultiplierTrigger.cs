using UnityEngine;

namespace Invector.vCharacterController;

public class vJumpMultiplierTrigger : MonoBehaviour
{
	public float multiplier = 5f;

	public float timeToReset = 0.5f;

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			vThirdPersonController component = other.GetComponent<vThirdPersonController>();
			if ((bool)component && (component.isJumping || !component.isGrounded) && component._rigidbody.velocity.y <= 0f)
			{
				component.SetJumpMultiplier(multiplier, timeToReset);
				component.isJumping = false;
				component.verticalVelocity = 0f;
				component.heightReached = base.transform.position.y;
				component.isGrounded = true;
				component.Jump();
			}
		}
	}
}
