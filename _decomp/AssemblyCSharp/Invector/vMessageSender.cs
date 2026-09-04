using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector;

[vClassHeader("Message Sender", "Requires a vMessageReceiver to receive messages.", openClose = false)]
public class vMessageSender : vMonoBehaviour
{
	[Serializable]
	public class vMessage
	{
		public string name;

		public string message;

		[vHelpBox("- sendByTrigger (You can use vSimpleTrigger to verify the Player and Send messages using Events by calling the 'OnTrigger' method", vHelpBoxAttribute.MessageType.None)]
		public bool sendByTrigger;

		public List<vMessageReceiver> defaultReceivers;
	}

	[Serializable]
	public class vGlobalMessage
	{
		public string name;

		public string message;
	}

	public List<vMessage> messages;

	public List<vGlobalMessage> globalMessages;

	public virtual void SendToDefaultReceiver(int messageIndex)
	{
		vMessage vMessage = ((messages.Count > 0 && messageIndex < messages.Count) ? messages[messageIndex] : null);
		if (vMessage == null)
		{
			return;
		}
		for (int i = 0; i < vMessage.defaultReceivers.Count; i++)
		{
			if ((bool)vMessage.defaultReceivers[i])
			{
				vMessage.defaultReceivers[i].Send(vMessage.name, vMessage.message);
			}
		}
	}

	public virtual void SendToDefaultReceiver(string messageName)
	{
		List<vMessage> list = messages.FindAll((vMessage m) => m.name.Equals(messageName));
		if (list == null || list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].defaultReceivers.Count; j++)
			{
				if ((bool)list[i].defaultReceivers[j])
				{
					list[i].defaultReceivers[j].Send(list[i].name, list[i].message);
				}
			}
		}
	}

	public virtual void SendToParentReceiver(int messageIndex)
	{
		vMessageReceiver componentInParent = GetComponentInParent<vMessageReceiver>();
		if ((bool)componentInParent)
		{
			vMessage vMessage = ((messages.Count > 0 && messageIndex < messages.Count) ? messages[messageIndex] : null);
			if (vMessage != null)
			{
				componentInParent.Send(vMessage.name, vMessage.message);
			}
		}
	}

	public virtual void SendToParentReceiver(string messageName)
	{
		vMessageReceiver componentInParent = GetComponentInParent<vMessageReceiver>();
		if (!componentInParent)
		{
			return;
		}
		List<vMessage> list = messages.FindAll((vMessage m) => m.name.Equals(messageName));
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				componentInParent.Send(list[i].name, list[i].message);
			}
		}
	}

	public virtual void Send(GameObject target, int messageIndex)
	{
		if ((bool)target)
		{
			return;
		}
		vMessageReceiver component = target.GetComponent<vMessageReceiver>();
		if ((bool)component)
		{
			vMessage vMessage = ((messages.Count > 0 && messageIndex < messages.Count) ? messages[messageIndex] : null);
			if (vMessage != null)
			{
				component.Send(vMessage.name, vMessage.message);
			}
		}
	}

	public virtual void Send(Collider target, int messageIndex)
	{
		if (!target)
		{
			Send(target.gameObject, messageIndex);
		}
	}

	public virtual void Send(Transform target, int messageIndex)
	{
		if (!target)
		{
			Send(target.gameObject, messageIndex);
		}
	}

	public virtual void Send(GameObject target, string messageName)
	{
		if ((bool)target)
		{
			return;
		}
		vMessageReceiver component = target.GetComponent<vMessageReceiver>();
		if (!component)
		{
			return;
		}
		List<vMessage> list = messages.FindAll((vMessage m) => m.name.Equals(messageName));
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				component.Send(list[i].name, list[i].message);
			}
		}
	}

	public virtual void Send(Collider target, string messageName)
	{
		if ((bool)target)
		{
			Send(target.gameObject, messageName);
		}
	}

	public virtual void Send(Transform target, string messageName)
	{
		if ((bool)target)
		{
			Send(target.gameObject, messageName);
		}
	}

	public virtual void SendAllToDefaultReceiver()
	{
		if (messages == null || messages.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < messages.Count; i++)
		{
			for (int j = 0; j < messages[i].defaultReceivers.Count; j++)
			{
				if ((bool)messages[i].defaultReceivers[j])
				{
					messages[i].defaultReceivers[j].Send(messages[i].name, messages[i].message);
				}
			}
		}
	}

	public virtual void SendAll(GameObject target)
	{
		if (!target)
		{
			return;
		}
		vMessageReceiver component = target.GetComponent<vMessageReceiver>();
		if ((bool)component)
		{
			for (int i = 0; i < messages.Count; i++)
			{
				component.Send(messages[i].name, messages[i].message);
			}
		}
	}

	public virtual void OnTrigger(Collider target)
	{
		if (!target)
		{
			return;
		}
		vMessageReceiver component = target.gameObject.GetComponent<vMessageReceiver>();
		if (!component)
		{
			return;
		}
		for (int i = 0; i < messages.Count; i++)
		{
			if (messages[i].sendByTrigger)
			{
				component.Send(messages[i].name, messages[i].message);
			}
		}
	}

	public virtual void SendGlobal(string messageName)
	{
		List<vGlobalMessage> list = globalMessages.FindAll((vGlobalMessage m) => m.name.Equals(messageName));
		for (int i = 0; i < list.Count; i++)
		{
			vMessageReceiver.SendGlobal(list[i].name, list[i].message);
		}
	}

	public virtual void SendGlobal(int messageIndex)
	{
		vGlobalMessage vGlobalMessage = ((globalMessages.Count > 0 && messageIndex < globalMessages.Count) ? globalMessages[messageIndex] : null);
		if (vGlobalMessage != null)
		{
			vMessageReceiver.SendGlobal(vGlobalMessage.name, vGlobalMessage.message);
		}
	}
}
