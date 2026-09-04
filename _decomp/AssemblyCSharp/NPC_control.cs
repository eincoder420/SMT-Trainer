using Invector.vCharacterController.AI;
using UnityEngine;

public class NPC_control : MonoBehaviour
{
	public int District;

	public bool Mom;

	public bool Neighbor_Usual;

	public bool Seller;

	public bool Naked;

	public bool Swimmer;

	public bool Speaker;

	public bool Presser;

	public bool Drinker;

	public bool Smoker;

	public AudioClip[] speeches;

	private void Awake()
	{
		vControlAI[] componentsInChildren = GetComponentsInChildren<vControlAI>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			NPC_generator component = componentsInChildren[i].GetComponent<NPC_generator>();
			if (Mom)
			{
				Set_Mom_Params(component);
			}
			if (Seller)
			{
				component.Bage.gameObject.SetActive(value: true);
				component.Seller = true;
			}
			if (Naked)
			{
				component.Make_Naked();
			}
			if (Swimmer)
			{
				component.Make_Swimmer();
			}
			component.Neighbor_Usual = Neighbor_Usual;
			component.Speaker = Speaker;
			component.Presser = Presser;
			component.Drinker = Drinker;
			component.Smoker = Smoker;
		}
	}

	private void Set_Mom_Params(NPC_generator generator)
	{
		Inventory_Script inventory_Script = Object.FindObjectOfType<Inventory_Script>();
		if (inventory_Script.data.Language == 0)
		{
			generator.Name = "Мачеха " + inventory_Script.data.Name2;
		}
		if (inventory_Script.data.Language == 1)
		{
			generator.Name = inventory_Script.data.Name + "'s Stepmom";
		}
		generator.Name_text.text = generator.Name;
		for (int i = 0; i < generator.Hairs.childCount; i++)
		{
			generator.Hairs.GetChild(i).gameObject.SetActive(i == 0);
		}
		generator.armature.transform.localScale = new Vector3(1f, 1f, 1f);
		generator.Make_Mom();
		if (inventory_Script.data.progress_data.Mom_Progress >= 1)
		{
			generator.Speech_Collider.gameObject.SetActive(value: false);
		}
		generator.Speeches = speeches;
		generator.Mom = true;
		generator.gameObject.SetActive(value: true);
	}
}
