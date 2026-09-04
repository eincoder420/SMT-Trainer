using System.Collections;
using UnityEngine;

public class Level_Load_Trigger : MonoBehaviour
{
	public int Level;

	public int Position_id;

	private Menu_Level_Loader Loader;

	private void Start()
	{
		if (!Loader)
		{
			Loader = Object.FindObjectOfType<Menu_Level_Loader>();
		}
	}

	public void Set_Spawn_Data()
	{
		Loader.data.saved_data.Spawn_position_id = Position_id;
		StartCoroutine(Wait_For_Load());
	}

	private IEnumerator Wait_For_Load()
	{
		yield return new WaitForSeconds(0.5f);
		if (!Loader.menu.hidden_data.Demo)
		{
			Loader.LoadScene(Level);
			yield break;
		}
		Loader.anim.Play("Thanks_Demo");
		Loader.Set_Audio(Loader.Win_Music_Demo);
		yield return new WaitForSeconds(8f);
		Loader.LoadScene(0);
	}
}
