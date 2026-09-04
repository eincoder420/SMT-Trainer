using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class vHelpBoxAttribute : PropertyAttribute
{
	public enum MessageType
	{
		None,
		Info,
		Warning
	}

	public string text;

	public int lineSpace;

	public MessageType messageType;

	public vHelpBoxAttribute(string text, MessageType messageType = MessageType.None)
	{
		this.text = text;
		this.messageType = messageType;
	}
}
