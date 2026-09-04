using UnityEngine;

public class DynamicBoneDemo1 : MonoBehaviour
{
	public GameObject m_Player;

	private float m_weight = 1f;

	private void Update()
	{
		m_Player.transform.Rotate(new Vector3(0f, Input.GetAxis("Horizontal") * Time.deltaTime * 200f, 0f));
		m_Player.transform.Translate(base.transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 4f);
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(50f, 50f, 200f, 24f), "Press arrow key to move");
		Animation componentInChildren = m_Player.GetComponentInChildren<Animation>();
		componentInChildren.enabled = GUI.Toggle(new Rect(50f, 70f, 200f, 24f), componentInChildren.enabled, "Play Animation");
		DynamicBone[] components = m_Player.GetComponents<DynamicBone>();
		GUI.Label(new Rect(50f, 100f, 200f, 24f), "Choose dynamic bone:");
		DynamicBone obj = components[0];
		bool flag2 = (components[1].enabled = GUI.Toggle(new Rect(50f, 120f, 100f, 24f), components[0].enabled, "Breasts"));
		obj.enabled = flag2;
		components[2].enabled = GUI.Toggle(new Rect(50f, 140f, 100f, 24f), components[2].enabled, "Tail");
		GUI.Label(new Rect(50f, 160f, 200f, 24f), "Weight");
		m_weight = GUI.HorizontalSlider(new Rect(100f, 160f, 100f, 24f), m_weight, 0f, 1f);
		DynamicBone[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetWeight(m_weight);
		}
	}
}
