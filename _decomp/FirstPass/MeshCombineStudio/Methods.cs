using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeshCombineStudio;

public static class Methods
{
	public static int GetFirstLayerOfLayerMask(LayerMask layerMask)
	{
		for (int i = 0; i < 32; i++)
		{
			int result = 1 << i;
			if ((i & (int)layerMask) != 0)
			{
				return result;
			}
		}
		return -1;
	}

	public static bool IsLayerInLayerMask(LayerMask layerMask, int layer)
	{
		return (int)layerMask == ((int)layerMask | (1 << layer));
	}

	public static void SetMeshRenderersActive(FastList<MeshRenderer> mrs, bool active)
	{
		for (int i = 0; i < mrs.Count; i++)
		{
			mrs.items[i].enabled = active;
		}
	}

	public static void SetCachedGOSActive(FastList<CachedGameObject> cachedGOS, bool active)
	{
		for (int i = 0; i < cachedGOS.Count; i++)
		{
			cachedGOS.items[i].mr.enabled = active;
		}
	}

	public static void SetTag(GameObject go, string tag)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tag = tag;
		}
	}

	public static void SetTagWhenCollider(GameObject go, string tag)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponent<Collider>() != null)
			{
				componentsInChildren[i].tag = tag;
			}
		}
	}

	public static void SetTagAndLayer(GameObject go, string tag, int layer)
	{
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tag = tag;
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	public static void SetLayer(GameObject go, int layer)
	{
		go.layer = layer;
		Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	public static bool LayerMaskContainsLayer(int layerMask, int layer)
	{
		return ((1 << layer) & layerMask) != 0;
	}

	public static int GetFirstLayerInLayerMask(int layerMask)
	{
		for (int i = 0; i < 32; i++)
		{
			if ((layerMask & Mathw.bits[i]) != 0)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains(string compare, string name)
	{
		List<string> list = new List<string>();
		int num;
		do
		{
			num = name.IndexOf("*");
			if (num != -1)
			{
				if (num != 0)
				{
					list.Add(name.Substring(0, num));
				}
				if (num == name.Length - 1)
				{
					break;
				}
				name = name.Substring(num + 1);
			}
		}
		while (num != -1);
		list.Add(name);
		for (int i = 0; i < list.Count; i++)
		{
			if (!compare.Contains(list[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static T[] Search<T>(GameObject parentGO = null)
	{
		GameObject[] array = null;
		array = ((!(parentGO == null)) ? new GameObject[1] { parentGO } : SceneManager.GetActiveScene().GetRootGameObjects());
		if (array == null)
		{
			return null;
		}
		if (typeof(T) == typeof(GameObject))
		{
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				Transform[] componentsInChildren = array[i].GetComponentsInChildren<Transform>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					list.Add(componentsInChildren[j].gameObject);
				}
			}
			return list.ToArray() as T[];
		}
		if (parentGO == null)
		{
			List<T> list2 = new List<T>();
			for (int k = 0; k < array.Length; k++)
			{
				list2.AddRange(array[k].GetComponentsInChildren<T>(includeInactive: true));
			}
			return list2.ToArray();
		}
		return parentGO.GetComponentsInChildren<T>(includeInactive: true);
	}

	public static FastList<GameObject> GetAllRootGameObjects()
	{
		FastList<GameObject> fastList = new FastList<GameObject>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			fastList.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
		}
		return fastList;
	}

	public static T[] SearchParent<T>(GameObject parentGO, bool searchInActiveGameObjects) where T : Component
	{
		if (parentGO == null)
		{
			return SearchAllScenes<T>(searchInActiveGameObjects).ToArray();
		}
		if (!searchInActiveGameObjects && !parentGO.activeInHierarchy)
		{
			return null;
		}
		if (typeof(T) == typeof(GameObject))
		{
			Transform[] componentsInChildren = parentGO.GetComponentsInChildren<Transform>(searchInActiveGameObjects);
			GameObject[] array = new GameObject[componentsInChildren.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = componentsInChildren[i].gameObject;
			}
			return array as T[];
		}
		return parentGO.GetComponentsInChildren<T>(searchInActiveGameObjects);
	}

	public static T[] SearchScene<T>(Scene scene, bool searchInActiveGameObjects) where T : Component
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		FastList<T> fastList = new FastList<T>();
		GameObject[] array = rootGameObjects;
		foreach (GameObject parentGO in array)
		{
			fastList.AddRange(SearchParent<T>(parentGO, searchInActiveGameObjects));
		}
		return fastList.ToArray();
	}

	public static FastList<T> SearchAllScenes<T>(bool searchInActiveGameObjects) where T : Component
	{
		FastList<T> fastList = new FastList<T>();
		FastList<GameObject> allRootGameObjects = GetAllRootGameObjects();
		for (int i = 0; i < allRootGameObjects.Count; i++)
		{
			T[] arrayItems = SearchParent<T>(allRootGameObjects.items[i], searchInActiveGameObjects);
			fastList.AddRange(arrayItems);
		}
		return fastList;
	}

	public static T Find<T>(GameObject parentGO, string name) where T : Component
	{
		T[] array = SearchParent<T>(parentGO, searchInActiveGameObjects: true);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == name)
			{
				return array[i];
			}
		}
		return null;
	}

	public static void SetCollidersActive(Collider[] colliders, bool active, string[] nameList)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			for (int j = 0; j < nameList.Length; j++)
			{
				if (colliders[i].name.Contains(nameList[j]))
				{
					colliders[i].enabled = active;
				}
			}
		}
	}

	public static void SelectChildrenWithMeshRenderer(GameObject[] parentGOs)
	{
	}

	public static void SelectChildrenWithMeshRenderer(Transform t)
	{
	}

	public static void DestroyChildren(Transform t)
	{
		while (t.childCount > 0)
		{
			Transform child = t.GetChild(0);
			child.parent = null;
			UnityEngine.Object.DestroyImmediate(child.gameObject);
		}
	}

	public static void Destroy(GameObject go)
	{
		if (!(go == null))
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	public static void Destroy(Component c)
	{
		if (!(c == null))
		{
			UnityEngine.Object.Destroy(c);
		}
	}

	public static void SetChildrenActive(Transform t, bool active)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			t.GetChild(i).gameObject.SetActive(active);
		}
	}

	public static void SnapBoundsAndPreserveArea(ref Bounds bounds, float snapSize, Vector3 offset)
	{
		Vector3 vector = Mathw.Snap(bounds.center, snapSize) + offset;
		bounds.size += Mathw.Abs(vector - bounds.center) * 2f;
		bounds.center = vector;
	}

	public static void ListRemoveAt<T>(List<T> list, int index)
	{
		list[index] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
	}

	public static void CopyComponent(Component component, GameObject target)
	{
		Type type = component.GetType();
		target.AddComponent(type);
		PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			propertyInfo.SetValue(target.GetComponent(type), propertyInfo.GetValue(component, null), null);
		}
	}

	public static Transform GetChildRootTransform(Transform t, Transform rootT)
	{
		MCSDynamicObject componentInParent = t.GetComponentInParent<MCSDynamicObject>();
		if ((bool)componentInParent)
		{
			return componentInParent.transform;
		}
		return rootT;
	}
}
