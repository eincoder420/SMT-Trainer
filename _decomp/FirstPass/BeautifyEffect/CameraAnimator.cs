using UnityEngine;

namespace BeautifyEffect;

public class CameraAnimator : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, Time.deltaTime * 10f));
	}
}
