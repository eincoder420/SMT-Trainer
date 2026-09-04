using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Check_Arrested : vStateDecision
{
	private Roxanne_Control rox_control;

	public override string categoryName => "MyCustomDecisions/";

	public override string defaultName => "Check_Arrested";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (rox_control == null)
		{
			rox_control = Object.FindObjectOfType<Roxanne_Control>();
		}
		return rox_control.Arrested;
	}
}
