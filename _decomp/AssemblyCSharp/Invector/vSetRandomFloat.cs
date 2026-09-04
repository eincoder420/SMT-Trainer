using UnityEngine;
using UnityEngine.UI;

namespace Invector;

public class vSetRandomFloat : MonoBehaviour
{
	public bool randomValue = true;

	[vHideInInspector("randomValue", false)]
	public float min;

	public float max;

	public bool setOnStart;

	public Slider.SliderEvent onSet;

	private void Start()
	{
		if (setOnStart)
		{
			Set();
		}
	}

	public void Set()
	{
		if (randomValue)
		{
			onSet.Invoke(Random.Range(min, max));
		}
		else
		{
			onSet.Invoke(max);
		}
	}
}
