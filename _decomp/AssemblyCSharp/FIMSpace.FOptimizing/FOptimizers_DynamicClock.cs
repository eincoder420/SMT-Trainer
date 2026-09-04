using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace FIMSpace.FOptimizing;

public class FOptimizers_DynamicClock
{
	private int[] avgTicks;

	private int avgCounter;

	private Stopwatch watch;

	private readonly float delayTolerance;

	private readonly float updateRatio;

	private readonly float maxDelay;

	public FOptimizers_Manager Manager { get; private set; }

	public List<FOptimizer_Base> Optimizers { get; private set; }

	public FEOptimizingDistance OptimizingDistanceType { get; private set; }

	public long FrameTicksConsumption { get; private set; }

	public long LastMSConsumption { get; private set; }

	public long LastTicksConsumption { get; private set; }

	public int LastTickFrame { get; private set; }

	public int DelaysCount { get; private set; }

	public float AdaptedDelay { get; private set; }

	public FOptimizers_DynamicClock(FOptimizers_Manager manager, FEOptimizingDistance type, List<FOptimizer_Base> optimizers)
	{
		Manager = manager;
		OptimizingDistanceType = type;
		Optimizers = optimizers;
		watch = new Stopwatch();
		AdaptedDelay = 0.01f;
		LastMSConsumption = 0L;
		FrameTicksConsumption = 0L;
		LastTicksConsumption = 0L;
		DelaysCount = 0;
		int num = 10;
		switch ((int)type)
		{
		case 0:
			updateRatio = 0.1f;
			maxDelay = 0.3f;
			num = 10;
			delayTolerance = 3.5f;
			break;
		case 1:
			updateRatio = 0.4f;
			maxDelay = 1.1f;
			num = 7;
			delayTolerance = 1.6f;
			break;
		case 2:
			updateRatio = 0.75f;
			maxDelay = 1.5f;
			num = 5;
			delayTolerance = 1.3f;
			break;
		case 3:
			updateRatio = 1.25f;
			maxDelay = 3f;
			num = 4;
			delayTolerance = 1.15f;
			break;
		case 4:
			updateRatio = 2.25f;
			maxDelay = 6f;
			num = 4;
			delayTolerance = 1f;
			break;
		}
		avgTicks = new int[num];
		for (int i = 0; i < avgTicks.Length; i++)
		{
			avgTicks[i] = 0;
		}
		AdaptedDelay = updateRatio + 0.001f;
	}

	public IEnumerator WatchUpdate()
	{
		yield return null;
		yield return null;
		while (true)
		{
			long totalElapsed = 0L;
			long totalTicks = 0L;
			DelaysCount = 0;
			float num = Mathf.Lerp(1f, 2.375f, Manager.UpdateBoost);
			int ticksLimit = (int)(5000f * num * delayTolerance);
			if (!Manager)
			{
				break;
			}
			watch.Start();
			if ((bool)Manager.TargetCamera)
			{
				for (int i = Optimizers.Count - 1; i >= 0; i--)
				{
					if (Optimizers[i] == null)
					{
						Optimizers.RemoveAt(i);
					}
					else
					{
						Manager.CheckElement(Optimizers[i], i);
						if (watch.ElapsedTicks > ticksLimit)
						{
							watch.Stop();
							yield return null;
							DelaysCount++;
							totalElapsed += watch.ElapsedMilliseconds;
							totalTicks += watch.ElapsedTicks;
							FrameTicksConsumption = watch.ElapsedTicks;
							watch.Reset();
							watch.Start();
						}
					}
				}
			}
			watch.Stop();
			LastMSConsumption = totalElapsed + watch.ElapsedMilliseconds;
			LastTicksConsumption = totalTicks + watch.ElapsedTicks;
			AddAverage((int)LastTicksConsumption);
			UpdateAdaptation();
			yield return new WaitForSeconds(AdaptedDelay);
			FrameTicksConsumption = watch.ElapsedTicks;
			watch.Reset();
			LastTickFrame = Time.frameCount;
		}
		UnityEngine.Debug.LogError(string.Concat("[OPTIMIZERS] Manager is not existing anymore! Stopping dynamic clock! (", OptimizingDistanceType, ")"));
	}

	private void UpdateAdaptation()
	{
		float num = maxDelay;
		float num2 = 1f;
		if (Manager.UpdateBoost > 0f)
		{
			num2 = 1f + Manager.UpdateBoost * 2f;
			num = maxDelay / (1f + Manager.UpdateBoost);
			if (OptimizingDistanceType < FEOptimizingDistance.Far)
			{
				num /= 1f + Manager.UpdateBoost;
				num2 = 1f + Manager.UpdateBoost * 5f;
			}
			else if (OptimizingDistanceType == FEOptimizingDistance.Far)
			{
				num /= 1f + Manager.UpdateBoost / 2f;
				num2 = 1f + Manager.UpdateBoost * 3f;
			}
			else if (OptimizingDistanceType == FEOptimizingDistance.Farthest)
			{
				num /= 1f + Manager.UpdateBoost / 1.5f;
				num2 = 1f + Manager.UpdateBoost * 2.5f;
			}
		}
		AdaptedDelay = (float)GetAverage() / 30000f * updateRatio / num2;
		if (AdaptedDelay > num)
		{
			AdaptedDelay = num;
		}
	}

	private void AddAverage(int ticks)
	{
		avgTicks[avgCounter] = ticks;
		avgCounter++;
		if (avgCounter >= avgTicks.Length)
		{
			avgCounter = 0;
		}
	}

	public int GetAverage()
	{
		int num = 0;
		for (int i = 0; i < avgTicks.Length; i++)
		{
			num += avgTicks[i];
		}
		return num / avgTicks.Length;
	}
}
