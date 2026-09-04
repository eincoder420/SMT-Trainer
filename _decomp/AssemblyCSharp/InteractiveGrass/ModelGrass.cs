using System.Collections.Generic;
using UnityEngine;

namespace InteractiveGrass;

public class ModelGrass : MonoBehaviour
{
	public class Force
	{
		public float m_Time;

		public Vector3 m_Force;

		public Force(Vector3 force)
		{
			m_Force = force;
		}
	}

	[Range(0f, 8f)]
	public float m_Amplitude = 3f;

	[Range(0f, 1f)]
	public float m_BurnProgress;

	public float m_ColliderRadius = 1f;

	public float m_ColliderStrength = 2f;

	public float m_MaxForceMagnitude = 8f;

	public float m_WaveFrequency = 6f;

	public float m_Resistance = 0.25f;

	private List<Force> m_ForceList = new List<Force>();

	private Vector3 m_AccForce = Vector3.zero;

	private Renderer m_Rd;

	private bool m_IsBurnOuting;

	private float m_BurnOutStartTime;

	private float m_BurnOutDurationTime;

	private float m_BurnOutPassTime;

	public void Initialize()
	{
		m_Rd = GetComponent<Renderer>();
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		float num = 0f;
		float num2 = 0f;
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			float y = vertices[i].y;
			num2 = Mathf.Min(num2, y);
			num = Mathf.Max(num, y);
		}
		Color[] array = new Color[vertices.Length];
		for (int j = 0; j < vertices.Length; j++)
		{
			float num3 = vertices[j].y / (num - num2);
			array[j] = new Color(num3, num3, num3, 1f);
		}
		mesh.colors = array;
	}

	public void DoUpdate()
	{
		UpdateForce();
		BurnOutUpdate();
		m_Rd.material.SetFloat("_Amplitude", m_Amplitude);
		m_Rd.material.SetFloat("_BurnAmount", m_BurnProgress);
	}

	private void AddForce(Vector3 force)
	{
		Vector3 force2 = new Vector3(force.x, force.y, force.z);
		if (force2.magnitude > m_MaxForceMagnitude)
		{
			force2 = force2.normalized * m_MaxForceMagnitude;
		}
		m_ForceList.Add(new Force(force2));
	}

	private void UpdateForce()
	{
		if (m_ForceList.Count == 0)
		{
			return;
		}
		m_AccForce = Vector3.zero;
		for (int num = m_ForceList.Count - 1; num >= 0; num--)
		{
			if (m_ForceList[num].m_Force.magnitude > 0.01f)
			{
				float num2 = Mathf.Sin(m_ForceList[num].m_Time * m_WaveFrequency);
				float num3 = easeOutExpo(1f, 0f, m_Resistance * Time.deltaTime);
				m_ForceList[num].m_Force *= num3;
				m_ForceList[num].m_Time += Time.deltaTime;
				m_AccForce += m_ForceList[num].m_Force * num2;
			}
			else
			{
				m_ForceList.RemoveAt(num);
			}
		}
		m_AccForce = base.transform.InverseTransformVector(m_AccForce);
		m_Rd.material.SetVector("_MoveVec", m_AccForce);
	}

	private float easeOutExpo(float start, float end, float value)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * value) + 1f) + start;
	}

	private void OnTriggerEnter(Collider other)
	{
		Transform component = other.gameObject.GetComponent<Transform>();
		Vector3 vector = GetComponent<Transform>().position - component.position;
		vector.y = 0f;
		float num = 0.25f + Mathf.Clamp01((m_ColliderRadius - vector.magnitude) / m_ColliderRadius) * 0.75f;
		AddForce(vector.normalized * num * m_ColliderStrength);
	}

	public void BurnOutStart(float delay, float duration)
	{
		m_BurnOutStartTime = Time.time + delay;
		m_BurnOutDurationTime = duration;
	}

	public void BurnOutStop()
	{
		m_BurnProgress = 0f;
		m_IsBurnOuting = false;
		m_BurnOutStartTime = (m_BurnOutDurationTime = (m_BurnOutPassTime = 0f));
	}

	private void BurnOutUpdate()
	{
		if (m_BurnOutStartTime != 0f && Time.time > m_BurnOutStartTime)
		{
			m_IsBurnOuting = true;
			m_BurnOutStartTime = 0f;
			m_BurnOutPassTime = 0f;
		}
		if (m_IsBurnOuting)
		{
			m_BurnOutPassTime += Time.deltaTime;
			float num = m_BurnOutPassTime / m_BurnOutDurationTime;
			m_BurnProgress = Mathf.Lerp(0f, 1f, num);
			if (num > 1f)
			{
				m_IsBurnOuting = false;
				m_BurnOutStartTime = (m_BurnOutPassTime = 0f);
			}
		}
	}
}
