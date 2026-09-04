using UnityEngine;

public class ShiftAtRuntime : MonoBehaviour
{
	private DayNight dayNight;

	private void Start()
	{
		dayNight = Object.FindObjectOfType<DayNight>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.N) && (bool)dayNight)
		{
			dayNight.isNight = !dayNight.isNight;
			dayNight.ChangeMaterial();
		}
	}
}
