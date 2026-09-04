using UnityEngine;

public class ShadowDistance : MonoBehaviour
{
	public float shadowDistance = 2000f;

	private void Awake()
	{
		QualitySettings.shadowDistance = shadowDistance;
	}
}
