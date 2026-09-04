using UnityEngine;

public class Restaurant : MonoBehaviour
{
	public bool Market;

	public bool Store;

	public int Type;

	public int price;

	public Transform Seller;

	public Transform Lamp;

	public Transform Trigger;

	public Transform[] Hand_Stuff;

	private void Start()
	{
		if (!Market)
		{
			Usable_Object component = GetComponent<Usable_Object>();
			if (Type == 0)
			{
				int[] array = new int[3] { 60, 65, 70 };
				price = array[Random.Range(0, array.Length)];
				component.Text_Actions[0] = "Купить пиво - " + price;
				component.Text_Actions[1] = "Buy beer - " + price;
			}
			if (Type == 1)
			{
				int[] array2 = new int[3] { 45, 50, 55 };
				price = array2[Random.Range(0, array2.Length)];
				component.Text_Actions[0] = "Купить лапши - " + price;
				component.Text_Actions[1] = "Buy noodles - " + price;
			}
		}
		else
		{
			price = 200;
		}
	}

	public void Eat()
	{
		Roxanne_Control roxanne_Control = Object.FindObjectOfType<Roxanne_Control>();
		if (!roxanne_Control || !Seller)
		{
			return;
		}
		if (roxanne_Control.inventory.data.money.Remain_Money >= price)
		{
			if (!roxanne_Control.Eating)
			{
				Seller.GetComponentInChildren<Animator>().Play("Pick_Mid");
				roxanne_Control.interface_script.Take_Item(Type);
				if (Market || Store)
				{
					roxanne_Control.mission_Explorer.Complete_Buy_Mission();
				}
				roxanne_Control.interface_script.Take_Money(price);
				Trigger.GetComponent<Collider>().enabled = false;
				GetComponent<Usable_Object>().Turn_Button(On: false);
			}
			return;
		}
		roxanne_Control.anim.SetTrigger("Cant");
		roxanne_Control.Speak(roxanne_Control.Not_Enough_Money);
		if (Market)
		{
			Hand_Stuff[0].gameObject.SetActive(value: false);
			Hand_Stuff[1].gameObject.SetActive(value: false);
			roxanne_Control.mission_Explorer.Restore_Mission_Targets();
		}
		if (Store)
		{
			Hand_Stuff[0].gameObject.SetActive(value: false);
			roxanne_Control.mission_Explorer.Restore_Mission_Targets();
		}
	}
}
