namespace Invector.vCharacterController.AI.FSMBehaviour;

public class AICheckHealth : vStateDecision
{
	public enum vCheckValue
	{
		Equals,
		Less,
		Greater,
		NoEqual
	}

	public vCheckValue checkValue = vCheckValue.NoEqual;

	public float value;

	public override string categoryName => "Health/";

	public override string defaultName => "Check Health";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return CheckValue(fsmBehaviour);
	}

	protected virtual bool CheckValue(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour == null)
		{
			return false;
		}
		float num = fsmBehaviour.aiController.currentHealth / (float)fsmBehaviour.aiController.MaxHealth * 100f;
		return checkValue switch
		{
			vCheckValue.Equals => num == value, 
			vCheckValue.Less => num < value, 
			vCheckValue.Greater => num > value, 
			vCheckValue.NoEqual => num != value, 
			_ => false, 
		};
	}
}
