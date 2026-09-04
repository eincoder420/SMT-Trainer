using System.Collections.Generic;
using UnityEngine;

namespace Invector;

public class vSpikeControl : MonoBehaviour
{
	[HideInInspector]
	public List<Transform> attachColliders;

	private void Start()
	{
		attachColliders = new List<Transform>();
		vSpike[] componentsInChildren = GetComponentsInChildren<vSpike>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].control = this;
		}
	}
}
