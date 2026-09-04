namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vPlayAnimationAction : vStateAction
{
	public string _animationState;

	public int _layer;

	public float crossfade = 0.2f;

	public override string categoryName => "Animator/";

	public override string defaultName => "Play Animation";

	public vPlayAnimationAction()
	{
		executionType = vFSMComponentExecutionType.OnStateEnter;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		fsmBehaviour.aiController.animator.CrossFadeInFixedTime(_animationState, crossfade, _layer);
	}
}
