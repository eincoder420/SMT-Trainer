using System.Collections;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Invector.Utils;

public static class LoadLevelHelper
{
	public static vThirdPersonInput targetCharacter;

	public static string spawnPointName;

	public static string sceneName;

	public static void LoadScene(string _sceneName, string _spawnPointName, vThirdPersonInput tpInput)
	{
		if (!tpInput)
		{
			return;
		}
		targetCharacter = tpInput;
		spawnPointName = _spawnPointName;
		sceneName = _sceneName;
		if ((bool)targetCharacter.tpCamera)
		{
			targetCharacter.tpCamera.transform.parent = targetCharacter.transform;
		}
		if ((bool)targetCharacter)
		{
			vISceneLoadListener[] components = targetCharacter.GetComponents<vISceneLoadListener>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnStartLoadScene(_sceneName);
			}
			targetCharacter.StartCoroutine(LoadAsyncScene());
		}
	}

	private static IEnumerator LoadAsyncScene()
	{
		Scene currentScene = SceneManager.GetActiveScene();
		if (!currentScene.name.Equals(sceneName))
		{
			SceneManager.sceneUnloaded += OnSceneLoaded;
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			while (!asyncLoad.isDone)
			{
				yield return null;
			}
			SceneManager.MoveGameObjectToScene(targetCharacter.gameObject, SceneManager.GetSceneByName(sceneName));
			SceneManager.UnloadSceneAsync(currentScene);
		}
		else
		{
			MoveCharaterToSpawnPoint();
		}
	}

	private static void OnSceneLoaded(Scene arg0)
	{
		vISceneLoadListener[] components = targetCharacter.GetComponents<vISceneLoadListener>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnFinishLoadScene(arg0.name);
		}
		MoveCharaterToSpawnPoint();
		SceneManager.sceneUnloaded -= OnSceneLoaded;
	}

	private static void MoveCharaterToSpawnPoint()
	{
		GameObject gameObject = GameObject.Find(spawnPointName);
		if ((bool)gameObject && (bool)targetCharacter)
		{
			targetCharacter.lockCameraInput = true;
			if ((bool)targetCharacter.tpCamera)
			{
				targetCharacter.tpCamera.FreezeCamera();
			}
			targetCharacter.transform.position = gameObject.transform.position;
			targetCharacter.transform.rotation = gameObject.transform.rotation;
			if ((bool)targetCharacter.tpCamera)
			{
				targetCharacter.tpCamera.transform.parent = null;
				targetCharacter.tpCamera.UnFreezeCamera();
			}
			targetCharacter.lockCameraInput = false;
		}
	}
}
