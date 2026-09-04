using UnityEngine;

namespace MeshCombineStudio;

[ExecuteInEditMode]
public class ObjectSpawner : MonoBehaviour
{
	public GameObject[] objects;

	public Vector3 spawnArea = new Vector3(512f, 512f, 512f);

	public float density = 0.5f;

	public Vector2 scaleRange = new Vector2(0.5f, 2f);

	public Vector3 rotationRange = new Vector3(5f, 360f, 5f);

	public Vector2 heightRange = new Vector2(0f, 1f);

	public float scaleMulti = 1f;

	public float metersBetweenSpawning = 2f;

	public bool spawnInRuntime;

	public bool spawn;

	public bool deleteChildren;

	private Transform t;

	private void Awake()
	{
		t = base.transform;
		if (spawnInRuntime && Application.isPlaying)
		{
			Spawn();
		}
	}

	private void Update()
	{
		if (spawn)
		{
			spawn = false;
			Spawn();
		}
		if (deleteChildren)
		{
			deleteChildren = false;
			DeleteChildren();
		}
	}

	public void DeleteChildren()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (t != componentsInChildren[i] && componentsInChildren[i] != null)
			{
				Object.DestroyImmediate(componentsInChildren[i].gameObject);
			}
		}
	}

	private void SetObjectsActive(bool active)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(active);
		}
	}

	public void Spawn()
	{
		SetObjectsActive(active: true);
		Bounds bounds = default(Bounds);
		bounds.center = base.transform.position;
		bounds.size = spawnArea;
		float x = bounds.min.x;
		float x2 = bounds.max.x;
		float y = bounds.min.y;
		float y2 = bounds.max.y;
		float z = bounds.min.z;
		float z2 = bounds.max.z;
		int max = objects.Length;
		float num = metersBetweenSpawning * 0.5f;
		float num2 = base.transform.lossyScale.y * 0.5f;
		int num3 = 0;
		for (float num4 = z; num4 < z2; num4 += metersBetweenSpawning)
		{
			for (float num5 = x; num5 < x2; num5 += metersBetweenSpawning)
			{
				for (float num6 = y; num6 < y2; num6 += metersBetweenSpawning)
				{
					int num7 = Random.Range(0, max);
					if (Random.value < density)
					{
						Vector3 position = new Vector3(num5 + Random.Range(0f - num, num), y + Random.Range(0f, bounds.size.y) * Random.Range(heightRange.x, heightRange.y), num4 + Random.Range(0f - num, num));
						if (!(position.x < x) && !(position.x > x2) && !(position.y < y) && !(position.y > y2) && !(position.z < z) && !(position.z > z2))
						{
							position.y += num2;
							GameObject obj = Object.Instantiate(rotation: Quaternion.Euler(new Vector3(Random.Range(0f, rotationRange.x), Random.Range(0f, rotationRange.y), Random.Range(0f, rotationRange.z))), original: objects[num7], position: position);
							float num8 = Random.Range(scaleRange.x, scaleRange.y) * scaleMulti;
							obj.transform.localScale = new Vector3(num8, num8, num8);
							obj.transform.parent = t;
							num3++;
						}
					}
				}
			}
		}
		SetObjectsActive(active: false);
		Debug.Log("Spawned " + num3);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, 0f, 0f), spawnArea);
	}
}
