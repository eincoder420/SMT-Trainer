using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Check_Ragdolled : vStateDecision
{
	private Roxanne_Control player;

	public override string categoryName => "MyCustomDecisions/";

	public override string defaultName => "Check_Ragdolled";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (!player)
		{
			player = Object.FindObjectOfType<Roxanne_Control>();
		}
		return player.is_Ragdolled();
	}
}
