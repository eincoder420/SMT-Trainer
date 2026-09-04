namespace FIMSpace.FOptimizing;

public class FOptimizers_LODTransition
{
	public FComponentLODsController LODsController;

	public FLOD_Base From;

	public FLOD_Base To;

	public bool done;

	private readonly FLOD_Base tempLOD;

	private FLOD_Base breakLOD;

	public FOptimizers_LODTransition(FComponentLODsController lodsController, FLOD_Base to)
	{
		LODsController = lodsController;
		From = LODsController.LODSet.LevelOfDetailSets[LODsController.CurrentLODLevel];
		tempLOD = From.CreateNewCopy();
		To = to;
		if (!LODsController.RootReference.SupportingTransitions)
		{
			LODsController.ApplyLODLevelSettings(To);
			To = null;
			done = true;
		}
	}

	public void BreakCurrentTransition(int targetLODLevel)
	{
		if (breakLOD == null)
		{
			breakLOD = LODsController.RootReference.GetLODInstance();
		}
		done = false;
		breakLOD = tempLOD.CreateNewCopy();
		From = breakLOD;
		To = LODsController.LODSet.LevelOfDetailSets[targetLODLevel];
	}

	public void Update(float progress, float secondsAfter = 0f)
	{
		if (To == null)
		{
			return;
		}
		tempLOD.InterpolateBetween(From, To, progress);
		LODsController.ApplyLODLevelSettings(tempLOD);
		if (!(progress >= 1f))
		{
			return;
		}
		if (To.Disable)
		{
			if (To.ToCullDelay <= 0f)
			{
				done = true;
			}
			else if (secondsAfter >= To.ToCullDelay)
			{
				done = true;
			}
		}
		else
		{
			done = true;
		}
	}

	public void Finish()
	{
		if (!(To == null))
		{
			done = true;
			LODsController.ApplyLODLevelSettings(To);
		}
	}
}
