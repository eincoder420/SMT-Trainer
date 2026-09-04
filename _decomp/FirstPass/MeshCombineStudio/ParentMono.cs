using System;
using UnityEngine;

namespace MeshCombineStudio;

public abstract class ParentMono<T> : MonoBehaviour
{
	[NonSerialized]
	public T parent;
}
