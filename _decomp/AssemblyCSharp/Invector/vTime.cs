using System;
using UnityEngine;

namespace Invector;

public static class vTime
{
	public static bool useUnscaledTime;

	private static bool unscaledTime
	{
		get
		{
			if (Time.timeScale <= 0f)
			{
				return useUnscaledTime;
			}
			return false;
		}
	}

	public static float deltaTime
	{
		get
		{
			if (unscaledTime)
			{
				return Time.unscaledDeltaTime;
			}
			return Time.deltaTime;
		}
	}

	public static float fixedDeltaTime
	{
		get
		{
			if (unscaledTime)
			{
				return Time.fixedUnscaledDeltaTime;
			}
			return Time.fixedDeltaTime;
		}
	}

	public static float time
	{
		get
		{
			if (unscaledTime)
			{
				return Time.unscaledTime;
			}
			return Time.time;
		}
	}

	public static float GetNormalizedTime(this Animator animator, int layer, int round = 2)
	{
		return (float)Math.Round((animator.IsInTransition(layer) ? animator.GetNextAnimatorStateInfo(layer).normalizedTime : animator.GetCurrentAnimatorStateInfo(layer).normalizedTime) % 1f, round);
	}
}
