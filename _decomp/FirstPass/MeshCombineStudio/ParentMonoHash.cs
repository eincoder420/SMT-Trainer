using System;

namespace MeshCombineStudio;

public abstract class ParentMonoHash<T> : MonoBehaviourFastIndex
{
	[NonSerialized]
	public T parent;
}
