using System;
using UnityEngine;

namespace InteractiveGrass;

public class ModelGrassDemo : MonoBehaviour
{
	public GameObject m_Character;

	public KeyCode m_ForwardButton = KeyCode.UpArrow;

	public KeyCode m_BackwardButton = KeyCode.DownArrow;

	public KeyCode m_RightButton = KeyCode.RightArrow;

	public KeyCode m_LeftButton = KeyCode.LeftArrow;

	private ModelGrass[] m_Grasses;

	private void Start()
	{
		QualitySettings.antiAliasing = 8;
		m_Grasses = UnityEngine.Object.FindObjectsOfType<ModelGrass>();
		for (int i = 0; i < m_Grasses.Length; i++)
		{
			m_Grasses[i].Initialize();
		}
	}

	private void Update()
	{
		Vector3 moveTo = Vector3.zero;
		Move(m_ForwardButton, ref moveTo, m_Character.transform.forward);
		Move(m_BackwardButton, ref moveTo, -m_Character.transform.forward);
		Move(m_RightButton, ref moveTo, m_Character.transform.right);
		Move(m_LeftButton, ref moveTo, -m_Character.transform.right);
		m_Character.transform.position += moveTo * 4f * Time.deltaTime;
		for (int i = 0; i < m_Grasses.Length; i++)
		{
			m_Grasses[i].DoUpdate();
		}
	}

	private void OnGUI()
	{
		GUI.Box(new Rect(10f, 10f, 200f, 25f), "Interactive Grass Demo");
		if (GUI.Button(new Rect(10f, 40f, 80f, 30f), "Burn Out"))
		{
			Array.Sort(m_Grasses, delegate(ModelGrass a, ModelGrass b)
			{
				Vector3 position = a.gameObject.transform.position;
				Vector3 position2 = b.gameObject.transform.position;
				Vector3 position3 = m_Character.transform.position;
				float num = Vector3.Distance(position, position3);
				float value = Vector3.Distance(position2, position3);
				return num.CompareTo(value);
			});
			for (int i = 0; i < m_Grasses.Length; i++)
			{
				m_Grasses[i].BurnOutStart((float)i * 0.1f, 2f);
			}
		}
		if (GUI.Button(new Rect(95f, 40f, 80f, 30f), "Reset"))
		{
			for (int j = 0; j < m_Grasses.Length; j++)
			{
				m_Grasses[j].BurnOutStop();
			}
		}
	}

	private void Move(KeyCode key, ref Vector3 moveTo, Vector3 dir)
	{
		if (Input.GetKey(key))
		{
			moveTo = dir;
		}
	}
}
