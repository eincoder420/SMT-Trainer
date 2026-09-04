using UnityEngine;

public struct TriangleTest
{
	public Vector3 a;

	public Vector3 b;

	public Vector3 c;

	public Vector3 dirAb;

	public Vector3 dirAc;

	public Vector3 dirBc;

	public Vector3 h1;

	public float ab;

	public float ac;

	public float bc;

	public float area;

	public float h;

	public float ah;

	public float hb;

	public void Calc()
	{
		Vector3 vector = a;
		Vector3 vector2 = b;
		Vector3 vector3 = c;
		Vector3 vector4 = b - a;
		Vector3 vector5 = c - a;
		Vector3 vector6 = c - b;
		float magnitude = vector4.magnitude;
		float magnitude2 = vector5.magnitude;
		float magnitude3 = vector6.magnitude;
		if (magnitude2 > magnitude && magnitude2 > magnitude3)
		{
			a = vector;
			b = vector3;
			c = vector2;
		}
		else if (magnitude3 > magnitude)
		{
			a = vector3;
			b = vector2;
			c = vector;
		}
		dirAb = b - a;
		dirAc = c - a;
		dirBc = c - b;
		ab = dirAb.magnitude;
		ac = dirAc.magnitude;
		bc = dirBc.magnitude;
		float num = (ab + ac + bc) * 0.5f;
		area = Mathf.Sqrt(num * (num - ab) * (num - ac) * (num - bc));
		h = 2f * area / ab;
		ah = Mathf.Sqrt(ac * ac - h * h);
		hb = ab - ah;
		h1 = a + dirAb * (1f / ab * ah);
	}

	private void Swap<T>(ref T v1, ref T v2)
	{
		T val = v1;
		v1 = v2;
		v2 = val;
	}
}
