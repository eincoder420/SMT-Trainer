using System;
using UnityEngine;

namespace MeshCombineStudio;

public static class Mathw
{
	public static readonly int[] bits = new int[32]
	{
		1, 2, 4, 8, 16, 32, 64, 128, 256, 512,
		1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288,
		1048576, 2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912,
		1073741824, -2147483648
	};

	public static Vector3 Clamp(Vector3 v, float min, float max)
	{
		if (v.x < min)
		{
			v.x = min;
		}
		else if (v.x > max)
		{
			v.x = max;
		}
		if (v.y < min)
		{
			v.y = min;
		}
		else if (v.y > max)
		{
			v.y = max;
		}
		if (v.z < min)
		{
			v.z = min;
		}
		else if (v.z > max)
		{
			v.z = max;
		}
		return v;
	}

	public static Vector3 FloatToVector3(float v)
	{
		return new Vector3(v, v, v);
	}

	public static float SinDeg(float angle)
	{
		return Mathf.Sin(angle * ((float)Math.PI / 180f));
	}

	public static float GetMax(Vector3 v)
	{
		float num = v.x;
		if (v.y > num)
		{
			num = v.y;
		}
		if (v.z > num)
		{
			num = v.z;
		}
		return num;
	}

	public static Vector3 SetMin(Vector3 v, float min)
	{
		if (v.x < min)
		{
			v.x = min;
		}
		if (v.y < min)
		{
			v.y = min;
		}
		if (v.z < min)
		{
			v.z = min;
		}
		return v;
	}

	public static Vector3 Snap(Vector3 v, float snapSize)
	{
		v.x = Mathf.Floor(v.x / snapSize) * snapSize;
		v.y = Mathf.Floor(v.y / snapSize) * snapSize;
		v.z = Mathf.Floor(v.z / snapSize) * snapSize;
		return v;
	}

	public static Vector3 SnapRound(Vector3 v, float snapSize)
	{
		v.x = Mathf.Round(v.x / snapSize) * snapSize;
		v.y = Mathf.Round(v.y / snapSize) * snapSize;
		v.z = Mathf.Round(v.z / snapSize) * snapSize;
		return v;
	}

	public static Vector3 Divide(Vector3 a, Vector3 b)
	{
		a.x /= b.x;
		a.y /= b.y;
		a.z /= b.z;
		return a;
	}

	public static Vector3 Divide(float a, Vector3 b)
	{
		b.x = a / b.x;
		b.y = a / b.y;
		b.z = a / b.z;
		return b;
	}

	public static Vector3 Scale(Vector3 a, Int3 b)
	{
		a.x *= b.x;
		a.y *= b.y;
		a.z *= b.z;
		return a;
	}

	public static Vector3 Abs(Vector3 v)
	{
		return new Vector3((v.x < 0f) ? (0f - v.x) : v.x, (v.y < 0f) ? (0f - v.y) : v.y, (v.z < 0f) ? (0f - v.z) : v.z);
	}

	public static bool IntersectAABB3Sphere3(AABB3 box, Sphere3 sphere)
	{
		Vector3 center = sphere.center;
		Vector3 min = box.min;
		Vector3 max = box.max;
		float num = 0f;
		if (center.x < min.x)
		{
			float num2 = center.x - min.x;
			num += num2 * num2;
		}
		else if (center.x > max.x)
		{
			float num2 = center.x - max.x;
			num += num2 * num2;
		}
		if (center.y < min.y)
		{
			float num2 = center.y - min.y;
			num += num2 * num2;
		}
		else if (center.y > max.y)
		{
			float num2 = center.y - max.y;
			num += num2 * num2;
		}
		if (center.z < min.z)
		{
			float num2 = center.z - min.z;
			num += num2 * num2;
		}
		else if (center.z > max.z)
		{
			float num2 = center.z - max.z;
			num += num2 * num2;
		}
		return num <= sphere.radius * sphere.radius;
	}

	public static bool IntersectAABB3Triangle3(Vector3 boxCenter, Vector3 boxHalfSize, Triangle3 triangle)
	{
		Vector3 vector = triangle.a - boxCenter;
		Vector3 vector2 = triangle.b - boxCenter;
		Vector3 vector3 = triangle.c - boxCenter;
		Vector3 lhs = vector2 - vector;
		Vector3 rhs = vector3 - vector2;
		Vector3 vector4 = vector - vector3;
		float fb = Abs(lhs[0]);
		float num = Abs(lhs[1]);
		float fa = Abs(lhs[2]);
		if (!AxisTest_X01(vector, vector3, boxHalfSize, lhs[2], lhs[1], fa, num, out var min, out var max))
		{
			return false;
		}
		if (!AxisTest_Y02(vector, vector3, boxHalfSize, lhs[2], lhs[0], fa, fb, out min, out max))
		{
			return false;
		}
		if (!AxisTest_Z12(vector2, vector3, boxHalfSize, lhs[1], lhs[0], num, fb, out min, out max))
		{
			return false;
		}
		fb = Abs(rhs[0]);
		num = Abs(rhs[1]);
		fa = Abs(rhs[2]);
		if (!AxisTest_X01(vector, vector3, boxHalfSize, rhs[2], rhs[1], fa, num, out min, out max))
		{
			return false;
		}
		if (!AxisTest_Y02(vector, vector3, boxHalfSize, rhs[2], rhs[0], fa, fb, out min, out max))
		{
			return false;
		}
		if (!AxisTest_Z0(vector, vector2, boxHalfSize, rhs[1], rhs[0], num, fb, out min, out max))
		{
			return false;
		}
		fb = Abs(vector4[0]);
		num = Abs(vector4[1]);
		fa = Abs(vector4[2]);
		if (!AxisTest_X2(vector, vector2, boxHalfSize, vector4[2], vector4[1], fa, num, out min, out max))
		{
			return false;
		}
		if (!AxisTest_Y1(vector, vector2, boxHalfSize, vector4[2], vector4[0], fa, fb, out min, out max))
		{
			return false;
		}
		if (!AxisTest_Z12(vector2, vector3, boxHalfSize, vector4[1], vector4[0], num, fb, out min, out max))
		{
			return false;
		}
		GetMinMax(vector[0], vector2[0], vector3[0], out min, out max);
		if (min > boxHalfSize[0] || max < 0f - boxHalfSize[0])
		{
			return false;
		}
		GetMinMax(vector[1], vector2[1], vector3[1], out min, out max);
		if (min > boxHalfSize[1] || max < 0f - boxHalfSize[1])
		{
			return false;
		}
		GetMinMax(vector[2], vector2[2], vector3[2], out min, out max);
		if (min > boxHalfSize[2] || max < 0f - boxHalfSize[2])
		{
			return false;
		}
		if (!PlaneBoxOverlap(Vector3.Cross(lhs, rhs), vector, boxHalfSize))
		{
			return false;
		}
		return true;
	}

	private static void GetMinMax(float x0, float x1, float x2, out float min, out float max)
	{
		min = (max = x0);
		if (x1 < min)
		{
			min = x1;
		}
		else if (x1 > max)
		{
			max = x1;
		}
		if (x2 < min)
		{
			min = x2;
		}
		else if (x2 > max)
		{
			max = x2;
		}
	}

	private static bool PlaneBoxOverlap(Vector3 normal, Vector3 vert, Vector3 maxBox)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		for (int i = 0; i <= 2; i++)
		{
			float num = vert[i];
			if (normal[i] > 0f)
			{
				zero[i] = 0f - maxBox[i] - num;
				zero2[i] = maxBox[i] - num;
			}
			else
			{
				zero[i] = maxBox[i] - num;
				zero2[i] = 0f - maxBox[i] - num;
			}
		}
		if (Vector3.Dot(normal, zero) > 0f)
		{
			return false;
		}
		if (Vector3.Dot(normal, zero2) >= 0f)
		{
			return true;
		}
		return false;
	}

	private static float Abs(float v)
	{
		if (!(v < 0f))
		{
			return v;
		}
		return 0f - v;
	}

	private static bool AxisTest_X01(Vector3 v0, Vector3 v2, Vector3 boxHalfSize, float a, float b, float fa, float fb, out float min, out float max)
	{
		float num = a * v0[1] - b * v0[2];
		float num2 = a * v2[1] - b * v2[2];
		if (num < num2)
		{
			min = num;
			max = num2;
		}
		else
		{
			min = num2;
			max = num;
		}
		float num3 = fa * boxHalfSize[1] + fb * boxHalfSize[2];
		if (min > num3 || max < 0f - num3)
		{
			return false;
		}
		return true;
	}

	private static bool AxisTest_X2(Vector3 v0, Vector3 v1, Vector3 boxHalfSize, float a, float b, float fa, float fb, out float min, out float max)
	{
		float num = a * v0[1] - b * v0[2];
		float num2 = a * v1[1] - b * v1[2];
		if (num < num2)
		{
			min = num;
			max = num2;
		}
		else
		{
			min = num2;
			max = num;
		}
		float num3 = fa * boxHalfSize[1] + fb * boxHalfSize[2];
		if (min > num3 || max < 0f - num3)
		{
			return false;
		}
		return true;
	}

	private static bool AxisTest_Y02(Vector3 v0, Vector3 v2, Vector3 boxHalfSize, float a, float b, float fa, float fb, out float min, out float max)
	{
		float num = (0f - a) * v0[0] + b * v0[2];
		float num2 = (0f - a) * v2[0] + b * v2[2];
		if (num < num2)
		{
			min = num;
			max = num2;
		}
		else
		{
			min = num2;
			max = num;
		}
		float num3 = fa * boxHalfSize[0] + fb * boxHalfSize[2];
		if (min > num3 || max < 0f - num3)
		{
			return false;
		}
		return true;
	}

	private static bool AxisTest_Y1(Vector3 v0, Vector3 v1, Vector3 boxHalfSize, float a, float b, float fa, float fb, out float min, out float max)
	{
		float num = (0f - a) * v0[0] + b * v0[2];
		float num2 = (0f - a) * v1[0] + b * v1[2];
		if (num < num2)
		{
			min = num;
			max = num2;
		}
		else
		{
			min = num2;
			max = num;
		}
		float num3 = fa * boxHalfSize[0] + fb * boxHalfSize[2];
		if (min > num3 || max < 0f - num3)
		{
			return false;
		}
		return true;
	}

	private static bool AxisTest_Z12(Vector3 v1, Vector3 v2, Vector3 boxHalfSize, float a, float b, float fa, float fb, out float min, out float max)
	{
		float num = a * v1[0] - b * v1[1];
		float num2 = a * v2[0] - b * v2[1];
		if (num2 < num)
		{
			min = num2;
			max = num;
		}
		else
		{
			min = num;
			max = num2;
		}
		float num3 = fa * boxHalfSize[0] + fb * boxHalfSize[1];
		if (min > num3 || max < 0f - num3)
		{
			return false;
		}
		return true;
	}

	private static bool AxisTest_Z0(Vector3 v0, Vector3 v1, Vector3 boxHalfSize, float a, float b, float fa, float fb, out float min, out float max)
	{
		float num = a * v0[0] - b * v0[1];
		float num2 = a * v1[0] - b * v1[1];
		if (num < num2)
		{
			min = num;
			max = num2;
		}
		else
		{
			min = num2;
			max = num;
		}
		float num3 = fa * boxHalfSize[0] + fb * boxHalfSize[1];
		if (min > num3 || max < 0f - num3)
		{
			return false;
		}
		return true;
	}
}
