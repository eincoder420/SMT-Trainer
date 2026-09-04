using FIMSpace.FOptimizing;
using UnityEngine;
using UnityEngine.AI;

public sealed class FLOD_NavMeshAgent : FLOD_Base
{
	[Space(4f)]
	[Range(0f, 1f)]
	public float Priority = 1f;

	public ObstacleAvoidanceType Quality = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

	public FLOD_NavMeshAgent()
	{
		SupportingTransitions = true;
		HeaderText = "NavMeshAgent LOD Settings";
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<FLOD_NavMeshAgent>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_NavMeshAgent fLOD_NavMeshAgent = ScriptableObject.CreateInstance<FLOD_NavMeshAgent>();
		fLOD_NavMeshAgent.CopyBase(this);
		fLOD_NavMeshAgent.Priority = Priority;
		fLOD_NavMeshAgent.Quality = Quality;
		return fLOD_NavMeshAgent;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[Custom OPTIMIZERS] Given component is null instead of NavMeshAgent!");
		}
		NavMeshAgent navMeshAgent = component as NavMeshAgent;
		if (navMeshAgent != null)
		{
			Priority = navMeshAgent.avoidancePriority;
			Quality = navMeshAgent.obstacleAvoidanceType;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		FLOD_NavMeshAgent fLOD_NavMeshAgent = lodA as FLOD_NavMeshAgent;
		FLOD_NavMeshAgent fLOD_NavMeshAgent2 = lodB as FLOD_NavMeshAgent;
		Priority = Mathf.Lerp(fLOD_NavMeshAgent.Priority, fLOD_NavMeshAgent2.Priority, transitionToB);
		ObstacleAvoidanceType quality = fLOD_NavMeshAgent.Quality;
		int quality2 = (int)fLOD_NavMeshAgent2.Quality;
		int quality3 = (int)Mathf.Lerp((float)quality, quality2, transitionToB);
		Quality = (ObstacleAvoidanceType)quality3;
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		FLOD_NavMeshAgent fLOD_NavMeshAgent = initialSettingsReference as FLOD_NavMeshAgent;
		if (fLOD_NavMeshAgent == null)
		{
			Debug.Log("[Custom OPTIMIZERS] Target LOD is not NavMeshAgent LOD or is null");
			return;
		}
		NavMeshAgent obj = component as NavMeshAgent;
		obj.avoidancePriority = (int)Mathf.Clamp(fLOD_NavMeshAgent.Priority * Priority, 0f, 99f);
		obj.obstacleAvoidanceType = Quality;
		base.ApplySettingsToComponent(component, initialSettingsReference);
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		if (source as NavMeshAgent == null)
		{
			Debug.LogError("[Custom OPTIMIZERS] Given component for reference values is null or is not NavMeshAgent Component!");
		}
		float num = (Priority = GetValueForLODLevel(1f, 0f, lodIndex, lodCount));
		int quality = (int)Quality;
		quality = (int)((float)quality * num);
		Quality = (ObstacleAvoidanceType)quality;
		base.name = "LOD" + (lodIndex + 2);
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		Priority = 0f;
		Quality = ObstacleAvoidanceType.NoObstacleAvoidance;
	}

	public override void SetSettingsAsForNearest(Component component)
	{
		base.SetSettingsAsForNearest(component);
		Priority = 1f;
		Quality = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		Priority = 0.2f;
		Quality = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		NavMeshAgent navMeshAgent = target as NavMeshAgent;
		if (!navMeshAgent)
		{
			navMeshAgent = target.GetComponentInChildren<NavMeshAgent>();
		}
		if ((bool)navMeshAgent && !optimizer.ContainsComponent(navMeshAgent))
		{
			return new FComponentLODsController(optimizer, navMeshAgent, "NavMeshAgent", this);
		}
		return null;
	}
}
