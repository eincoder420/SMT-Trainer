using UnityEngine;

namespace Invector;

public static class vHealthControllerHelper
{
	private static vIHealthController GetHealthController(this GameObject gameObject)
	{
		return gameObject.GetComponent<vIHealthController>();
	}

	public static void AddHealth(this GameObject receiver, int health)
	{
		receiver.GetHealthController()?.AddHealth(health);
	}

	public static void ChangeHealth(this GameObject receiver, int health)
	{
		receiver.GetHealthController()?.ChangeHealth(health);
	}

	public static void ChangeMaxHealth(this GameObject receiver, int health)
	{
		receiver.GetHealthController()?.ChangeMaxHealth(health);
	}

	public static bool HasHealth(this GameObject gameObject)
	{
		return gameObject.GetHealthController() != null;
	}

	public static bool IsDead(this GameObject gameObject)
	{
		return gameObject.GetHealthController()?.isDead ?? true;
	}

	public static void ResetHealth(this GameObject receiver, float health)
	{
		receiver.GetHealthController()?.ResetHealth(health);
	}

	public static void ResetHealth(this GameObject receiver)
	{
		receiver.GetHealthController()?.ResetHealth();
	}
}
