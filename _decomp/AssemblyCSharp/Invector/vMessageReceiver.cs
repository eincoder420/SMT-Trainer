using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("MESSAGE RECEIVER", "Use this component with the vMessageSender to call Events.")]
public class vMessageReceiver : vMonoBehaviour
{
	[Serializable]
	public delegate void OnReceiveMessage(string name, string message = null);

	[Serializable]
	public class OnReceiveMessageEvent : UnityEvent<string>
	{
	}

	[Serializable]
	public class vMessageListener
	{
		public string Name;

		public bool receiveFromGlobal;

		public OnReceiveMessageEvent onReceiveMessage;

		public void OnReceiveMessage(string name, string message = null)
		{
			if (Name.Equals(name))
			{
				onReceiveMessage.Invoke(string.IsNullOrEmpty(message) ? string.Empty : message);
			}
		}

		public vMessageListener(string name)
		{
			Name = name;
		}

		public vMessageListener(string name, UnityAction<string> listener)
		{
			Name = name;
			onReceiveMessage.AddListener(listener);
		}
	}

	public List<vMessageListener> messagesListeners;

	public static event OnReceiveMessage onReceiveGlobalMessage;

	public event OnReceiveMessage onReceiveMessage;

	private void Start()
	{
		for (int i = 0; i < messagesListeners.Count; i++)
		{
			vMessageListener vMessageListener = messagesListeners[i];
			if (vMessageListener.receiveFromGlobal)
			{
				onReceiveGlobalMessage -= vMessageListener.OnReceiveMessage;
				onReceiveGlobalMessage += vMessageListener.OnReceiveMessage;
			}
			else
			{
				onReceiveMessage -= vMessageListener.OnReceiveMessage;
				onReceiveMessage += vMessageListener.OnReceiveMessage;
			}
		}
	}

	public void AddListener(string name, UnityAction<string> listener)
	{
		if (messagesListeners.Exists((vMessageListener l) => l.Name.Equals(name)))
		{
			messagesListeners.Find((vMessageListener l) => l.Name.Equals(name)).onReceiveMessage.AddListener(listener);
		}
		else
		{
			messagesListeners.Add(new vMessageListener(name, listener));
		}
	}

	public void RemoveListener(string name, UnityAction<string> listener)
	{
		if (messagesListeners.Exists((vMessageListener l) => l.Name.Equals(name)))
		{
			messagesListeners.Find((vMessageListener l) => l.Name.Equals(name)).onReceiveMessage.RemoveListener(listener);
		}
	}

	public void Send(string name, string message)
	{
		if (base.enabled)
		{
			this.onReceiveMessage?.Invoke(name, message);
		}
	}

	public void Send(string name)
	{
		if (base.enabled)
		{
			this.onReceiveMessage?.Invoke(name, string.Empty);
		}
	}

	public static void SendGlobal(string name, string message = null)
	{
		vMessageReceiver.onReceiveGlobalMessage?.Invoke(name, message);
	}
}
