using System;
using System.Collections.Generic;
using Invector;
using Invector.vCharacterController.vActions;
using UnityEngine.Events;

[vClassHeader("Trigger Action Event", true, "icon_v2", false, "", helpBoxText = "Use this to filter a specific TriggerAction so you can use Events with the Controller or components attached to the Controller", useHelpBox = true)]
public class vTriggerActionEvent : vMonoBehaviour
{
	[Serializable]
	public class ActionEvent
	{
		public string actionName;

		public UnityEvent onTriggerEvent;
	}

	public List<ActionEvent> actionFinders;

	public void TriggerEvent(vTriggerGenericAction action)
	{
		actionFinders.Find((ActionEvent a) => a.actionName.Equals(action.gameObject.name))?.onTriggerEvent.Invoke();
	}
}
