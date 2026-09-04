using UnityEngine;

namespace Invector.Utils;

public class vScaleControl : MonoBehaviour
{
	private Vector3 targetScale;

	private Vector3 defaultScale;

	public float scaleX
	{
		set
		{
			targetScale.x = defaultScale.x * value;
			base.transform.localScale = targetScale;
		}
	}

	public float scaleY
	{
		set
		{
			targetScale.y = defaultScale.y * value;
			base.transform.localScale = targetScale;
		}
	}

	public float scaleZ
	{
		set
		{
			targetScale.z = defaultScale.z * value;
			base.transform.localScale = targetScale;
		}
	}

	private void Awake()
	{
		defaultScale = base.transform.localScale;
		targetScale = defaultScale;
	}
}
