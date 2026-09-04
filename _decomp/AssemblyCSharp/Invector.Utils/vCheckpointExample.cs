using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.Utils;

[RequireComponent(typeof(BoxCollider))]
public class vCheckpointExample : MonoBehaviour
{
	private vGameController gm;

	public UnityEvent onTriggerEnter;

	private void Start()
	{
		gm = GetComponentInParent<vGameController>();
		GetComponent<BoxCollider>().isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			vHUDController.instance.ShowText("Checkpoint reached!");
			gm.spawnPoint = base.gameObject.transform;
			onTriggerEnter.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}
}
