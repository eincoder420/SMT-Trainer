using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("REVIVE AI", true, "icon_v2", false, "", openClose = false)]
public class vReviveAI : vMonoBehaviour
{
	public float reviveDelay = 5f;

	public UnityEvent onRevive;

	private vControlAI controlAI;

	private bool inRevive;

	private void Start()
	{
		controlAI = GetComponent<vControlAI>();
		controlAI.onDead.AddListener(delegate
		{
			ReviveAI();
		});
	}

	public void ReviveAI()
	{
		if (!inRevive)
		{
			StartCoroutine(ReviveCoroutine());
		}
	}

	private IEnumerator ReviveCoroutine()
	{
		inRevive = true;
		yield return new WaitForSeconds(reviveDelay);
		controlAI.ResetHealth();
		controlAI._capsuleCollider.enabled = true;
		controlAI._rigidbody.isKinematic = false;
		controlAI.animator.SetBool("isDead", value: false);
		controlAI.triggerDieBehaviour = false;
		inRevive = false;
	}
}
