using UnityEngine;

namespace Invector;

public class vUnderWaterTrigger : MonoBehaviour
{
	public GameObject waterEffect;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Water"))
		{
			waterEffect.SetActive(value: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Water"))
		{
			waterEffect.SetActive(value: false);
		}
	}
}
