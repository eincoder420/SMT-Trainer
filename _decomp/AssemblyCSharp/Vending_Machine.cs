using UnityEngine;

public class Vending_Machine : MonoBehaviour
{
	public int price;

	public AudioClip Vending_Sound;

	private void Start()
	{
		price = 25;
		Usable_Object component = GetComponent<Usable_Object>();
		component.Text_Actions[0] = "Купить энергетик - " + price;
		component.Text_Actions[1] = "Buy energy drink - " + price;
	}

	public void Drink()
	{
		Roxanne_Control roxanne_Control = Object.FindObjectOfType<Roxanne_Control>();
		if (!roxanne_Control)
		{
			return;
		}
		if (roxanne_Control.inventory.data.money.Remain_Money >= price)
		{
			if (!roxanne_Control.Drinking)
			{
				roxanne_Control.anim.SetTrigger("Push");
				GetComponent<AudioSource>().PlayOneShot(Vending_Sound);
				roxanne_Control.interface_script.Take_Item(2);
				roxanne_Control.interface_script.Take_Money(price);
				roxanne_Control.mission_Explorer.Complete_Buy_Energy_Mission();
			}
		}
		else
		{
			roxanne_Control.anim.SetTrigger("Cant");
			roxanne_Control.Speak(roxanne_Control.Not_Enough_Money);
		}
	}
}
