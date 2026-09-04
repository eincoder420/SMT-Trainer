using UnityEngine;
using UnityEngine.SceneManagement;

namespace Invector.vCharacterController;

public class vFindSpawnPoint : MonoBehaviour
{
	public Transform spawnPoint;

	public string spawnPointName;

	public GameObject target;

	public void AlighObjetToSpawnPoint(GameObject target, string spawnPointName)
	{
		this.target = target;
		this.spawnPointName = spawnPointName;
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		GameObject gameObject = GameObject.Find(spawnPointName);
		if ((bool)gameObject && (bool)target)
		{
			target.transform.position = gameObject.transform.position;
			target.transform.rotation = gameObject.transform.rotation;
			return;
		}
		try
		{
			Object.Destroy(base.gameObject);
		}
		catch
		{
		}
	}
}
