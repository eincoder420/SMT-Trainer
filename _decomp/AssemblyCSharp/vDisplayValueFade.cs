using UnityEngine;

public class vDisplayValueFade : MonoBehaviour
{
	public CanvasGroup group;

	public AnimationCurve groupAlphaCurve;

	public float upSpeed;

	public float timeToDestroy = 4f;

	private float currentTime;

	private Transform rotateTransform;

	private void Awake()
	{
		group.alpha = 0f;
	}

	public void Update()
	{
		if (rotateTransform == null)
		{
			if ((bool)Camera.current)
			{
				rotateTransform = Camera.current.transform;
				base.transform.forward = rotateTransform.position - base.transform.position;
				group.alpha = 1f;
			}
			else
			{
				group.alpha = 0f;
			}
			return;
		}
		base.transform.Translate(Vector3.up * upSpeed * Time.deltaTime);
		base.transform.forward = rotateTransform.position - base.transform.position;
		currentTime += Time.deltaTime;
		float num = currentTime / timeToDestroy;
		if ((bool)group)
		{
			group.alpha = groupAlphaCurve.Evaluate(1f - num);
		}
		if (currentTime >= timeToDestroy)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
