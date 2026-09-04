using Invector.Utils;
using Invector.vCharacterController.AI;
using UnityEngine;

public class vSpawnRangeAttack : MonoBehaviour
{
	public vControlAI controlAI;

	public GameObject rangeEffectPrefab;

	[vHelpBox("If true, the rangeEffectPrefab need a component vTargetLookAt", vHelpBoxAttribute.MessageType.None)]
	public bool followTarget = true;

	protected virtual void Start()
	{
		if (controlAI == null)
		{
			controlAI = GetComponentInParent<vControlAI>();
		}
		if ((bool)controlAI)
		{
			controlAI.OnUpdateAI.AddListener(UpdateForward);
		}
	}

	public void Spawn()
	{
		GameObject gameObject = Object.Instantiate(rangeEffectPrefab, base.transform.position, base.transform.rotation);
		if (followTarget)
		{
			vTargetLookAt component = gameObject.GetComponent<vTargetLookAt>();
			if ((bool)component)
			{
				component.target = controlAI.currentTarget;
			}
		}
	}

	protected virtual void UpdateForward()
	{
		if (controlAI.currentTarget.transform != null && controlAI.targetInLineOfSight)
		{
			base.transform.forward = controlAI.currentTarget.collider.bounds.center - base.transform.position;
		}
		else
		{
			base.transform.forward = controlAI.transform.forward;
		}
	}
}
