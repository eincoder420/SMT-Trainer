using System;
using UnityEngine;

[Serializable]
public struct Mission_Object
{
	public string Name;

	public Transform[] Objects;

	public Mission Appear;

	public Mission Disappear;

	public Mission Deactivate_Temporary;
}
