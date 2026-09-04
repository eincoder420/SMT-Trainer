namespace FIMSpace.FOptimizing;

public class FOptimizers_Transitioning
{
	public FOptimizer_Base Optimizer;

	public int Index = -1;

	private float elapsed;

	private float transitionDuration;

	private int targetLODLevel;

	private bool allDone;

	private FOptimizers_LODTransition[] lodTypes;

	public int Id { get; private set; }

	public bool Finished { get; private set; }

	public FOptimizers_Transitioning(int optimizerId, FOptimizer_Base optimizer, int targetLODLevel, float duration, int index = -1)
	{
		Id = optimizerId;
		Finished = false;
		Optimizer = optimizer;
		this.targetLODLevel = targetLODLevel;
		transitionDuration = duration;
		elapsed = 0f;
		Index = index;
		InitTransitioning();
	}

	private void InitTransitioning()
	{
		lodTypes = new FOptimizers_LODTransition[Optimizer.ToOptimize.Count];
		for (int i = 0; i < lodTypes.Length; i++)
		{
			lodTypes[i] = new FOptimizers_LODTransition(Optimizer.ToOptimize[i], Optimizer.ToOptimize[i].LODSet.LevelOfDetailSets[targetLODLevel]);
		}
		Optimizer.TransitionNextLOD = targetLODLevel;
		Optimizer.TransitionPercent = 0f;
	}

	internal void BreakCurrentTransition(float newDuration, int targetLODLevel)
	{
		transitionDuration = newDuration;
		this.targetLODLevel = targetLODLevel;
		elapsed = 0f;
		BreakTransitioning();
	}

	private void BreakTransitioning()
	{
		for (int i = 0; i < lodTypes.Length; i++)
		{
			lodTypes[i].BreakCurrentTransition(targetLODLevel);
		}
		Optimizer.TransitionNextLOD = targetLODLevel;
		Optimizer.TransitionPercent = -1f;
	}

	public void Finish()
	{
		Optimizer.SetLODLevel(targetLODLevel);
		for (int i = 0; i < lodTypes.Length; i++)
		{
			lodTypes[i].Finish();
		}
		Finished = true;
		Optimizer.TransitionNextLOD = 0;
		Optimizer.TransitionPercent = -1f;
	}

	public void Update(float deltaTime)
	{
		elapsed += deltaTime;
		if (allDone)
		{
			Finish();
			return;
		}
		float num = elapsed / transitionDuration;
		Optimizer.TransitionPercent = num;
		float secondsAfter = 0f;
		if (elapsed > transitionDuration)
		{
			secondsAfter = elapsed - transitionDuration;
		}
		if (!Optimizer.gameObject.activeInHierarchy)
		{
			Optimizer.gameObject.SetActive(value: true);
		}
		bool flag = true;
		for (int i = 0; i < lodTypes.Length; i++)
		{
			if (!lodTypes[i].done)
			{
				lodTypes[i].Update(num, secondsAfter);
				flag = false;
			}
		}
		if (num >= 1f)
		{
			allDone = flag;
		}
		else
		{
			allDone = false;
		}
	}
}
