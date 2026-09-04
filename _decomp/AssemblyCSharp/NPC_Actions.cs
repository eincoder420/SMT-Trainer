using UnityEngine;

public class NPC_Actions : MonoBehaviour
{
	private Animator anim;

	private NPC_generator generator;

	private void Awake()
	{
		anim = GetComponentInChildren<Animator>(includeInactive: true);
		generator = GetComponentInChildren<NPC_generator>(includeInactive: true);
	}

	public void Open_Door()
	{
		anim.SetTrigger("Pick_Item");
	}

	public void Smoke()
	{
		anim.SetTrigger("Smoke");
	}

	public void Pee()
	{
		anim.SetTrigger("Pee");
	}

	public void Sit(Transform target)
	{
		generator.Before_Sit_Position = generator.transform.position;
		generator.AI.Sitting = true;
		generator.Check_Sitting();
		generator.Sit_Target = target;
		generator.Moving_To_Sit = true;
	}

	public void Stay()
	{
		generator.AI.Sitting = false;
		anim.SetTrigger("Stay");
		generator.Sit_Target = null;
	}
}
