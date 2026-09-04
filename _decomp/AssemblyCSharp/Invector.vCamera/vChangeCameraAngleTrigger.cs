using System.Collections;
using UnityEngine;

namespace Invector.vCamera;

public class vChangeCameraAngleTrigger : MonoBehaviour
{
	public bool applyY;

	public bool applyX;

	public Vector2 angle;

	public vThirdPersonCamera tpCamera;

	public bool useSelfWorldAngle;

	private void OnDrawGizmos()
	{
		if (useSelfWorldAngle)
		{
			angle.x = base.transform.eulerAngles.y;
			angle.y = base.transform.eulerAngles.x;
		}
	}

	private IEnumerator Start()
	{
		tpCamera = Object.FindObjectOfType<vThirdPersonCamera>();
		Collider collider = GetComponent<Collider>();
		if ((bool)collider)
		{
			collider.isTrigger = true;
			collider.enabled = false;
			yield return new WaitForEndOfFrame();
			collider.enabled = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && (bool)tpCamera)
		{
			if (applyX)
			{
				tpCamera.lerpState.fixedAngle.x = angle.x;
			}
			if (applyY)
			{
				tpCamera.lerpState.fixedAngle.y = angle.y;
			}
		}
	}
}
