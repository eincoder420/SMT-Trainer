using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI COMPANION CONTROL", true, "icon_v2", false, "")]
public class vAICompanionControl : vMonoBehaviour
{
	public List<vAICompanion> aICompanions;

	public KeyCode followInput = KeyCode.F;

	private void Update()
	{
		if (Input.GetKeyDown(followInput))
		{
			for (int i = 0; i < aICompanions.Count; i++)
			{
				aICompanions[i].forceFollow = !aICompanions[i].forceFollow;
			}
		}
	}

	public void ReceiveDamage(vDamage damage)
	{
		if (!damage.sender)
		{
			return;
		}
		for (int i = 0; i < aICompanions.Count; i++)
		{
			if ((bool)aICompanions[i].controlAI && aICompanions[i].controlAI.currentTarget.transform == null)
			{
				aICompanions[i].controlAI.SetCurrentTarget(damage.sender, overrideCanseTarget: true);
			}
		}
	}
}
