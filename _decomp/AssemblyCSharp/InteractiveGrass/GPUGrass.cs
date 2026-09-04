using UnityEngine;

namespace InteractiveGrass;

public class GPUGrass : MonoBehaviour
{
	[Header("Trail")]
	public GameObject[] m_InteractiveObjs;

	public Texture2D m_Trail;

	public int m_TrailMapSize = 256;

	public float m_Radius = 1f;

	public Vector3 m_Offset = Vector3.up;

	public float m_MaxDistance = 1f;

	public LayerMask m_GrassLayer;

	private Transform m_GrassTsf;

	private Renderer m_GrassRdr;

	private void Start()
	{
		m_GrassTsf = GetComponent<Transform>();
		m_GrassRdr = GetComponent<Renderer>();
		Color[] array = new Color[m_TrailMapSize * m_TrailMapSize];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color(0.5f, 0.5f, 1f, 1f);
		}
		m_Trail = new Texture2D(m_TrailMapSize, m_TrailMapSize, TextureFormat.ARGB32, mipChain: false, linear: true);
		m_Trail.name = "GrassTrail";
		m_Trail.SetPixels(array);
		m_Trail.Apply();
	}

	private void Update()
	{
		for (int i = 0; i < m_InteractiveObjs.Length; i++)
		{
			RoundDisplacement(i);
		}
		m_Trail.Apply();
		if ((bool)m_GrassRdr)
		{
			m_GrassRdr.material.SetTexture("_TrailTex", m_Trail);
		}
	}

	private void RoundDisplacement(int ind)
	{
		Transform component = m_InteractiveObjs[ind].GetComponent<Transform>();
		string text = $"_ForceCenter{ind + 1}";
		m_GrassRdr.material.SetVector(text, component.position);
		Vector2 texCoord;
		Vector2 forward;
		Vector2 right;
		Transform worldToTextureSpaceMatrix = GetWorldToTextureSpaceMatrix(component.TransformPoint(m_Offset), Vector3.down, m_MaxDistance, m_GrassLayer, out texCoord, out forward, out right);
		if (worldToTextureSpaceMatrix == null || worldToTextureSpaceMatrix != m_GrassTsf)
		{
			return;
		}
		Invert2x2Matrix(forward, right, out var inverseForward, out var inverseRight);
		inverseForward.Normalize();
		inverseRight.Normalize();
		int num = (int)(m_Radius * forward.magnitude * (float)m_Trail.width);
		Vector2 vector = new Vector2(texCoord.x * (float)m_Trail.width, texCoord.y * (float)m_Trail.height);
		int num2 = (int)(vector.x - (float)num);
		int num3 = (int)(vector.y - (float)num);
		int x = Mathf.Clamp(num2, 0, m_Trail.width);
		int y = Mathf.Clamp(num3, 0, m_Trail.height);
		int num4 = Mathf.Min(num2 + num * 2, m_Trail.width) - num2;
		int num5 = Mathf.Min(num3 + num * 2, m_Trail.height) - num3;
		vector -= new Vector2(num2, num3);
		Color[] pixels = m_Trail.GetPixels(x, y, num4, num5);
		Vector3 forward2 = component.forward;
		for (int i = 0; i < num5; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				Color color = pixels[j + i * num4];
				Vector2 vector2 = (new Vector2(j, i) - vector) / num;
				vector2 = vector2.x * inverseRight + vector2.y * inverseForward;
				float b = 1f - CalcFalloff(vector2.magnitude, 1f);
				Vector2 to = new Vector2(forward2.x, forward2.z);
				if (color.b > 0.5f && Vector2.Angle(vector2, to) < 180f)
				{
					Vector2 vector3 = vector2;
					float num6 = CalcFalloff(vector2.magnitude, 0.5f);
					vector2 = vector3.normalized * num6;
					vector2 = VectorToColorSpace(vector2);
					pixels[j + i * num4] = new Color(vector2.x, vector2.y, b, 1f);
				}
			}
		}
		m_Trail.SetPixels(x, y, num4, num5, pixels);
	}

	private static Vector2 VectorToColorSpace(Vector2 v)
	{
		return (v + Vector2.one) * 0.5f;
	}

	private static Vector2 GetTexCoordDifference(Vector3 pos, Vector2 texCoords, Vector3 offset, float maxDistance, LayerMask layerMask)
	{
		Ray ray = new Ray(pos + offset, Vector3.down);
		if (Physics.Raycast(ray, out var hitInfo, maxDistance, layerMask))
		{
			return (hitInfo.textureCoord - texCoords) / offset.magnitude;
		}
		ray.direction = -ray.direction;
		if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
		{
			return (texCoords - hitInfo.textureCoord) / offset.magnitude;
		}
		return Vector2.zero;
	}

	private static Transform GetWorldToTextureSpaceMatrix(Vector3 pos, Vector3 rayDir, float maxDistance, LayerMask layerMask, out Vector2 texCoord, out Vector2 forward, out Vector2 right)
	{
		if (Physics.Raycast(new Ray(pos, rayDir.normalized), out var hitInfo, maxDistance, layerMask))
		{
			texCoord = hitInfo.textureCoord;
			forward = GetTexCoordDifference(pos, texCoord, Vector3.forward * 0.1f, maxDistance, layerMask);
			right = GetTexCoordDifference(pos, texCoord, Vector3.right * 0.1f, maxDistance, layerMask);
			return hitInfo.transform;
		}
		texCoord = Vector2.zero;
		forward = Vector2.zero;
		right = Vector2.zero;
		return null;
	}

	private static float CalcFalloff(float distance, float falloff)
	{
		return Mathf.Pow(Mathf.Max(1f - distance, 0f), falloff);
	}

	private static void Invert2x2Matrix(Vector2 forward, Vector2 right, out Vector2 inverseForward, out Vector2 inverseRight)
	{
		float num = right.x * forward.y - right.y * forward.x;
		inverseRight = new Vector2(forward.y, 0f - right.y) / num;
		inverseForward = new Vector2(0f - forward.x, right.x) / num;
	}
}
