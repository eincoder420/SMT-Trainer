using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Invector;

public static class vExtensions
{
	public static string InsertSpaceBeforeUpperCase(this string input)
	{
		string text = "";
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (char.IsUpper(c) && !string.IsNullOrEmpty(text))
			{
				text += " ";
			}
			text += c;
		}
		return text;
	}

	public static string RemoveUnderline(this string input)
	{
		return input.Replace("_", "");
	}

	public static string ToClearUpper(this string target)
	{
		return target.Replace(" ", string.Empty).ToUpper();
	}

	public static bool IsInSideRange(this float value, float min, float max)
	{
		if (value >= min)
		{
			return value <= max;
		}
		return false;
	}

	public static bool IsInSideRange(this float value, Vector2 minMaxRange)
	{
		if (value >= minMaxRange.x)
		{
			return value <= minMaxRange.y;
		}
		return false;
	}

	public static bool IsVectorNaN(this Vector3 vector)
	{
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y))
		{
			return float.IsNaN(vector.z);
		}
		return true;
	}

	public static Vector3[] MakeSmoothCurve(this Vector3[] pts, float smoothFactor = 0.25f)
	{
		smoothFactor = Mathf.Clamp(smoothFactor, 0.1f, 0.9f);
		Vector3[] array = new Vector3[(pts.Length - 2) * 2 + 2];
		try
		{
			array[0] = pts[0];
			array[array.Length - 1] = pts[pts.Length - 1];
			int num = 1;
			for (int i = 0; i < pts.Length - 2; i++)
			{
				array[num] = pts[i] + (pts[i + 1] - pts[i]) * (1f - smoothFactor);
				array[num + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * smoothFactor;
				num += 2;
			}
		}
		catch
		{
			array = pts;
		}
		return array;
	}

	public static float GetLenght(this NavMeshPath path)
	{
		float num = 0f;
		if (path != null && path.corners.Length > 1)
		{
			Vector3 a = path.corners[0];
			for (int i = 1; i < path.corners.Length; i++)
			{
				num += Vector3.Distance(a, path.corners[i]);
				a = path.corners[i];
			}
		}
		return num;
	}

	public static List<Vector3> MakeSmoothCurve(this List<Vector3> pts, float smoothFactor = 0.25f)
	{
		smoothFactor = Mathf.Clamp(smoothFactor, 0.1f, 0.9f);
		List<Vector3> list = new List<Vector3>((pts.Count - 2) * 2 + 2);
		try
		{
			list[0] = pts[0];
			list[list.Count - 1] = pts[pts.Count - 1];
			int num = 1;
			for (int i = 0; i < pts.Count - 2; i++)
			{
				list[num] = pts[i] + (pts[i + 1] - pts[i]) * (1f - smoothFactor);
				list[num + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * smoothFactor;
				num += 2;
			}
		}
		catch
		{
			list = pts;
		}
		return list;
	}

	public static Vector3[] MakeSmoothCurveArray(this List<Vector3> pts, float smoothFactor = 0.25f)
	{
		smoothFactor = Mathf.Clamp(smoothFactor, 0.1f, 0.9f);
		Vector3[] array = new Vector3[(pts.Count - 2) * 2 + 2];
		try
		{
			array[0] = pts[0];
			array[array.Length - 1] = pts[pts.Count - 1];
			int num = 1;
			for (int i = 0; i < pts.Count - 2; i++)
			{
				array[num] = pts[i] + (pts[i + 1] - pts[i]) * (1f - smoothFactor);
				array[num + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * smoothFactor;
				num += 2;
			}
		}
		catch
		{
			array = pts.vToArray();
		}
		return array;
	}

	public static void SetLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform item in obj.transform)
		{
			item.gameObject.SetLayerRecursively(layer);
		}
	}

	public static bool ContainsLayer(this LayerMask layermask, int layer)
	{
		return (int)layermask == ((int)layermask | (1 << layer));
	}

	public static void SetActiveChildren(this GameObject gameObjet, bool value)
	{
		foreach (Transform item in gameObjet.transform)
		{
			item.gameObject.SetActive(value);
		}
	}

	public static bool isChild(this Transform me, Transform target)
	{
		if (!target)
		{
			return false;
		}
		string name = target.gameObject.name;
		Transform transform = me.FindChildByNameRecursive(name);
		if (transform == null)
		{
			return false;
		}
		return transform.Equals(target);
	}

	private static Transform FindChildByNameRecursive(this Transform me, string name)
	{
		if (me.name == name)
		{
			return me;
		}
		for (int i = 0; i < me.childCount; i++)
		{
			Transform transform = me.GetChild(i).FindChildByNameRecursive(name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
	{
		Vector3 vector = eulerAngle;
		if (vector.x > 180f)
		{
			vector.x -= 360f;
		}
		else if (vector.x < -180f)
		{
			vector.x += 360f;
		}
		if (vector.y > 180f)
		{
			vector.y -= 360f;
		}
		else if (vector.y < -180f)
		{
			vector.y += 360f;
		}
		if (vector.z > 180f)
		{
			vector.z -= 360f;
		}
		else if (vector.z < -180f)
		{
			vector.z += 360f;
		}
		return new Vector3(vector.x, vector.y, vector.z);
	}

	public static Vector3 Difference(this Vector3 vector, Vector3 otherVector)
	{
		return otherVector - vector;
	}

	public static Vector3 AngleFormOtherDirection(this Vector3 directionA, Vector3 directionB)
	{
		return Quaternion.LookRotation(directionA).eulerAngles.AngleFormOtherEuler(Quaternion.LookRotation(directionB).eulerAngles);
	}

	public static Vector3 AngleFormOtherDirection(this Vector3 directionA, Vector3 directionB, Vector3 up)
	{
		return Quaternion.LookRotation(directionA, up).eulerAngles.AngleFormOtherEuler(Quaternion.LookRotation(directionB, up).eulerAngles);
	}

	public static Vector3 AngleFormOtherEuler(this Vector3 eulerA, Vector3 eulerB)
	{
		return eulerA.NormalizeAngle().Difference(eulerB.NormalizeAngle()).NormalizeAngle();
	}

	public static string ToStringColor(this bool value)
	{
		if (value)
		{
			return "<color=green>YES</color>";
		}
		return "<color=red>NO</color>";
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		do
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
		}
		while (angle < -360f || angle > 360f);
		return Mathf.Clamp(angle, min, max);
	}

	public static T[] Append<T>(this T[] arrayInitial, T[] arrayToAppend)
	{
		if (arrayToAppend == null)
		{
			throw new ArgumentNullException("The appended object cannot be null");
		}
		if (arrayInitial is string || arrayToAppend is string)
		{
			throw new ArgumentException("The argument must be an enumerable");
		}
		T[] array = new T[arrayInitial.Length + arrayToAppend.Length];
		arrayInitial.CopyTo(array, 0);
		arrayToAppend.CopyTo(array, arrayInitial.Length);
		return array;
	}

	public static List<T> vCopy<T>(this List<T> list)
	{
		List<T> list2 = new List<T>();
		if (list == null || list.Count == 0)
		{
			return list;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static List<T> vToList<T>(this T[] array)
	{
		List<T> list = new List<T>();
		if (array == null || array.Length == 0)
		{
			return list;
		}
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(array[i]);
		}
		return list;
	}

	public static T[] vToArray<T>(this List<T> list)
	{
		T[] array = new T[list.Count];
		if (list == null || list.Count == 0)
		{
			return array;
		}
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = list[i];
		}
		return array;
	}

	public static Vector3 BoxSize(this BoxCollider boxCollider)
	{
		float x = boxCollider.transform.lossyScale.x * boxCollider.size.x;
		float z = boxCollider.transform.lossyScale.z * boxCollider.size.z;
		float y = boxCollider.transform.lossyScale.y * boxCollider.size.y;
		return new Vector3(x, y, z);
	}

	public static bool IsClosed(this BoxCollider boxCollider, Vector3 position, Vector3 margin, Vector3 centerOffset)
	{
		Vector3 vector = boxCollider.BoxSize();
		float x = margin.x;
		float y = margin.y;
		float z = margin.z;
		Vector3 vector2 = boxCollider.center + centerOffset;
		Vector2 minMaxRange = new Vector2(vector2.x - vector.x * 0.5f - x, vector2.x + vector.x * 0.5f + x);
		Vector2 minMaxRange2 = new Vector2(vector2.y - vector.y * 0.5f - y, vector2.y + vector.y * 0.5f + y);
		Vector2 minMaxRange3 = new Vector2(vector2.z - vector.z * 0.5f - z, vector2.z + vector.z * 0.5f + z);
		position = boxCollider.transform.InverseTransformPoint(position);
		bool num = (position.x * boxCollider.transform.lossyScale.x).IsInSideRange(minMaxRange);
		bool flag = (position.y * boxCollider.transform.lossyScale.y).IsInSideRange(minMaxRange2);
		bool flag2 = (position.z * boxCollider.transform.lossyScale.z).IsInSideRange(minMaxRange3);
		return num && flag && flag2;
	}

	public static T ToEnum<T>(this string value, bool ignoreCase = true)
	{
		return (T)Enum.Parse(typeof(T), value, ignoreCase);
	}

	public static bool Contains<T>(this Enum value, Enum lookingForFlag) where T : struct
	{
		int num = (int)(object)value;
		int num2 = (int)(object)lookingForFlag;
		return (num & num2) == num2;
	}
}
