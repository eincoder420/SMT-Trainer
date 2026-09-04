using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

[SerializeField]
public class vFSMState : ScriptableObject
{
	public class FSMComponent
	{
		public List<vStateAction> actionsEnter;

		public List<vStateAction> actionsExit;

		public List<vStateAction> actionsUpdate;

		public FSMComponent(List<vStateAction> actions)
		{
			actionsEnter = actions.FindAll((vStateAction act) => (bool)act && (act.executionType & vFSMComponentExecutionType.OnStateEnter) != 0);
			actionsExit = actions.FindAll((vStateAction act) => (bool)act && (act.executionType & vFSMComponentExecutionType.OnStateExit) != 0);
			actionsUpdate = actions.FindAll((vStateAction act) => (bool)act && (act.executionType & vFSMComponentExecutionType.OnStateUpdate) != 0);
		}

		public void DoActions(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType)
		{
			switch (executionType)
			{
			case vFSMComponentExecutionType.OnStateEnter:
			{
				for (int j = 0; j < actionsEnter.Count; j++)
				{
					actionsEnter[j].DoAction(fsmBehaviour, vFSMComponentExecutionType.OnStateEnter);
				}
				break;
			}
			case vFSMComponentExecutionType.OnStateExit:
			{
				for (int k = 0; k < actionsExit.Count; k++)
				{
					actionsExit[k].DoAction(fsmBehaviour, vFSMComponentExecutionType.OnStateExit);
				}
				break;
			}
			case vFSMComponentExecutionType.OnStateUpdate:
			{
				for (int i = 0; i < actionsUpdate.Count; i++)
				{
					actionsUpdate[i].DoAction(fsmBehaviour);
				}
				break;
			}
			case vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter:
				break;
			}
		}
	}

	public bool resetCurrentDestination = true;

	public List<vStateTransition> transitions = new List<vStateTransition>();

	public List<vStateAction> actions = new List<vStateAction>();

	public FSMComponent components;

	[SerializeField]
	[HideInInspector]
	public bool useActions = true;

	[SerializeField]
	[HideInInspector]
	public bool useDecisions = true;

	public vFSMBehaviour parentGraph;

	public vFSMState defaultTransition;

	public string Name
	{
		get
		{
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	public virtual Type requiredType => typeof(vIControlAI);

	public virtual void OnStateEnter(vIFSMBehaviourController fsmBehaviour)
	{
		if (resetCurrentDestination && fsmBehaviour.aiController != null)
		{
			fsmBehaviour.aiController.Stop();
		}
		if (components == null)
		{
			components = new FSMComponent(actions);
		}
		if (useActions && components != null)
		{
			components.DoActions(fsmBehaviour, vFSMComponentExecutionType.OnStateEnter);
		}
	}

	public virtual void UpdateState(vIFSMBehaviourController fsmBehaviour)
	{
		if (components == null)
		{
			components = new FSMComponent(actions);
		}
		if (useActions && components != null)
		{
			components.DoActions(fsmBehaviour, vFSMComponentExecutionType.OnStateUpdate);
		}
		fsmBehaviour.ChangeState(TransitTo(fsmBehaviour));
	}

	public virtual void OnStateExit(vIFSMBehaviourController fsmBehaviour)
	{
		if (components == null)
		{
			components = new FSMComponent(actions);
		}
		if (useActions && components != null)
		{
			components.DoActions(fsmBehaviour, vFSMComponentExecutionType.OnStateExit);
		}
	}

	public vFSMState TransitTo(vIFSMBehaviourController fsmBehaviour)
	{
		vFSMState vFSMState2 = defaultTransition;
		for (int i = 0; i < transitions.Count; i++)
		{
			vFSMState2 = transitions[i].TransitTo(fsmBehaviour);
			if ((bool)vFSMState2)
			{
				break;
			}
		}
		return vFSMState2;
	}
}
