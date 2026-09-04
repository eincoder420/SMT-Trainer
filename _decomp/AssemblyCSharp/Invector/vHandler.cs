using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector;

[Serializable]
public class vHandler
{
	public Transform defaultHandler;

	public List<Transform> customHandlers;

	public vHandler()
	{
		customHandlers = new List<Transform>();
	}
}
