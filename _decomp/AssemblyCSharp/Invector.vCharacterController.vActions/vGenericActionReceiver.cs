using System.Collections.Generic;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions;

[vClassHeader("Action Receiver", true, "icon_v2", false, "")]
public class vGenericActionReceiver : vMonoBehaviour
{
	public List<string> supportedActionNames = new List<string> { "Action" };

	public UnityEvent onEnterTriggerAction;

	public UnityEvent onExitTriggerAction;

	public UnityEvent onStartAction;

	public UnityEvent onCancelAction;

	public UnityEvent onEndAction;

	private void Start()
	{
		vGenericAction componentInParent = base.gameObject.GetComponentInParent<vGenericAction>();
		if ((bool)componentInParent)
		{
			componentInParent.OnEnterTriggerAction.AddListener(OnEnterTriggerAction);
			componentInParent.OnExitTriggerAction.AddListener(OnExitTriggerAction);
			componentInParent.OnStartAction.AddListener(OnStartAction);
			componentInParent.OnCancelAction.AddListener(OnCancelAction);
			componentInParent.OnEndAction.AddListener(OnEndAction);
		}
	}

	private void OnDestroy()
	{
		vGenericAction componentInParent = GetComponentInParent<vGenericAction>();
		if ((bool)componentInParent)
		{
			componentInParent.OnEnterTriggerAction.RemoveListener(OnEnterTriggerAction);
			componentInParent.OnExitTriggerAction.RemoveListener(OnExitTriggerAction);
			componentInParent.OnStartAction.RemoveListener(OnStartAction);
			componentInParent.OnCancelAction.RemoveListener(OnCancelAction);
			componentInParent.OnEndAction.RemoveListener(OnEndAction);
		}
	}

	protected virtual bool IsValidAction(vTriggerGenericAction actionInfo)
	{
		if (base.enabled && base.gameObject.activeInHierarchy && actionInfo != null)
		{
			return supportedActionNames.Contains(actionInfo.actionName);
		}
		return false;
	}

	public virtual void OnEnterTriggerAction(vTriggerGenericAction actionInfo)
	{
		if (IsValidAction(actionInfo))
		{
			onEnterTriggerAction.Invoke();
		}
	}

	public virtual void OnExitTriggerAction(vTriggerGenericAction actionInfo)
	{
		if (IsValidAction(actionInfo))
		{
			onExitTriggerAction.Invoke();
		}
	}

	public virtual void OnStartAction(vTriggerGenericAction actionInfo)
	{
		if (IsValidAction(actionInfo))
		{
			onStartAction.Invoke();
		}
	}

	public virtual void OnCancelAction(vTriggerGenericAction actionInfo)
	{
		if (IsValidAction(actionInfo))
		{
			onCancelAction.Invoke();
		}
	}

	public virtual void OnEndAction(vTriggerGenericAction actionInfo)
	{
		if (IsValidAction(actionInfo))
		{
			onEndAction.Invoke();
		}
	}
}
