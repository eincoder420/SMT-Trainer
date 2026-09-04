using System;
using System.Collections;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Invector;

[vClassHeader("Simple GameController Example", true, "icon_v2", false, "", openClose = false)]
public class vGameController : vMonoBehaviour
{
	[Serializable]
	public class OnRealoadGame : UnityEvent
	{
	}

	[vHelpBox("Assign your Character Prefab to be instantiate at the SpawnPoint, leave it unassigned to Restart the Scene instead", vHelpBoxAttribute.MessageType.None)]
	public GameObject playerPrefab;

	[vHelpBox("Assign a empty transform to spawn the Player to a specific location", vHelpBoxAttribute.MessageType.None)]
	public Transform spawnPoint;

	[vHelpBox("Time to wait until the scene restart or the player will be spawned again", vHelpBoxAttribute.MessageType.None)]
	public float respawnTimer = 4f;

	[vHelpBox("Check this if you want to destroy the dead body after the respawn", vHelpBoxAttribute.MessageType.None)]
	public bool destroyBodyAfterDead;

	[vHelpBox("Display a message using the FadeText UI", vHelpBoxAttribute.MessageType.None)]
	public bool displayInfoInFadeText = true;

	[HideInInspector]
	public OnRealoadGame OnReloadGame = new OnRealoadGame();

	[HideInInspector]
	public GameObject currentPlayer;

	private vThirdPersonController currentController;

	public static vGameController instance;

	private GameObject oldPlayer;

	public UnityEvent onSpawn;

	public bool dontDestroyOnLoad = true;

	protected virtual void Start()
	{
		if (instance == null)
		{
			instance = this;
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			base.gameObject.name = base.gameObject.name + " Instance";
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Init Scene");
			}
			FindPlayer();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public virtual void ShowCursor(bool value)
	{
		Cursor.visible = value;
	}

	public virtual void LockCursor(bool value)
	{
		if (value)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	protected virtual void OnCharacterDead(GameObject _gameObject)
	{
		oldPlayer = _gameObject;
		if (playerPrefab != null)
		{
			StartCoroutine(RespawnRoutine());
			return;
		}
		if (displayInfoInFadeText && (bool)vHUDController.instance)
		{
			vHUDController.instance.ShowText("Restarting Scene...");
		}
		Invoke("ResetScene", respawnTimer);
	}

	protected virtual IEnumerator RespawnRoutine()
	{
		yield return new WaitForSeconds(respawnTimer);
		if (!(playerPrefab != null) || !(spawnPoint != null))
		{
			yield break;
		}
		if (oldPlayer != null && destroyBodyAfterDead)
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Player destroyed: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
			UnityEngine.Object.Destroy(oldPlayer);
		}
		else
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Remove Player Components: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
			DestroyPlayerComponents(oldPlayer);
		}
		yield return new WaitForEndOfFrame();
		currentPlayer = UnityEngine.Object.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
		currentController = currentPlayer.GetComponent<vThirdPersonController>();
		currentController.onDead.AddListener(OnCharacterDead);
		if (displayInfoInFadeText && (bool)vHUDController.instance)
		{
			vHUDController.instance.ShowText("Respawn player: " + currentPlayer.name.Replace("(Clone)", ""));
		}
		OnReloadGame.Invoke();
		onSpawn.Invoke();
	}

	protected virtual void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (currentController == null)
		{
			return;
		}
		if (currentController.currentHealth > 0f)
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Load Scene: " + scene.name);
			}
			return;
		}
		if (displayInfoInFadeText && (bool)vHUDController.instance)
		{
			vHUDController.instance.ShowText("Reload Scene");
		}
		OnReloadGame.Invoke();
		FindPlayer();
	}

	protected virtual void FindPlayer()
	{
		vThirdPersonController vThirdPersonController = UnityEngine.Object.FindObjectOfType<vThirdPersonController>();
		if ((bool)vThirdPersonController)
		{
			currentPlayer = vThirdPersonController.gameObject;
			currentController = vThirdPersonController;
			vThirdPersonController.onDead.AddListener(OnCharacterDead);
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Found player: " + currentPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
		}
		else if (currentPlayer == null && playerPrefab != null && spawnPoint != null)
		{
			SpawnAtPoint(spawnPoint);
		}
	}

	protected virtual void DestroyPlayerComponents(GameObject target)
	{
		if ((bool)target)
		{
			MonoBehaviour[] componentsInChildren = target.GetComponentsInChildren<MonoBehaviour>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
			Collider component = target.GetComponent<Collider>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			Rigidbody component2 = target.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				UnityEngine.Object.Destroy(component2);
			}
			Animator component3 = target.GetComponent<Animator>();
			if (component3 != null)
			{
				UnityEngine.Object.Destroy(component3);
			}
		}
	}

	public virtual void SetSpawnSpoint(Transform newSpawnPoint)
	{
		spawnPoint = newSpawnPoint;
	}

	public void SetPlayerPrefab(GameObject prefab)
	{
		playerPrefab = prefab;
	}

	public virtual void SpawnAtPoint(Transform targetPoint)
	{
		if (!(playerPrefab != null))
		{
			return;
		}
		if (oldPlayer != null && destroyBodyAfterDead)
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Player destroyed: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
			UnityEngine.Object.Destroy(oldPlayer);
		}
		else if (oldPlayer != null)
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Remove Player Components: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
			DestroyPlayerComponents(oldPlayer);
		}
		currentPlayer = UnityEngine.Object.Instantiate(playerPrefab, targetPoint.position, targetPoint.rotation);
		currentController = currentPlayer.GetComponent<vThirdPersonController>();
		currentController.onDead.AddListener(OnCharacterDead);
		OnReloadGame.Invoke();
		if (displayInfoInFadeText && (bool)vHUDController.instance)
		{
			vHUDController.instance.ShowText("Spawn player: " + currentPlayer.name.Replace("(Clone)", ""));
		}
	}

	public virtual void SpawnPlayer(GameObject prefab)
	{
		if (!(prefab != null) || !(spawnPoint != null))
		{
			return;
		}
		Transform transform = spawnPoint;
		if (oldPlayer != null && destroyBodyAfterDead)
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Player destroyed: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
			UnityEngine.Object.Destroy(oldPlayer);
		}
		else if (oldPlayer != null)
		{
			if (displayInfoInFadeText && (bool)vHUDController.instance)
			{
				vHUDController.instance.ShowText("Remove Player Components: " + oldPlayer.name.Replace("(Clone)", "").Replace("Instance", ""));
			}
			DestroyPlayerComponents(oldPlayer);
		}
		currentPlayer = UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation);
		currentController = currentPlayer.GetComponent<vThirdPersonController>();
		currentController.onDead.AddListener(OnCharacterDead);
		OnReloadGame.Invoke();
		if (displayInfoInFadeText && (bool)vHUDController.instance)
		{
			vHUDController.instance.ShowText("Spawn player: " + currentPlayer.name.Replace("(Clone)", ""));
		}
	}

	public virtual void ResetScene()
	{
		if ((bool)oldPlayer)
		{
			DestroyPlayerComponents(oldPlayer);
		}
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		if ((bool)oldPlayer && destroyBodyAfterDead)
		{
			UnityEngine.Object.Destroy(oldPlayer);
		}
	}
}
