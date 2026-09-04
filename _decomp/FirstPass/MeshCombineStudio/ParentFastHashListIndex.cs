using System;

namespace MeshCombineStudio;

public abstract class ParentFastHashListIndex<T> : FastIndex
{
	[NonSerialized]
	public T parent;
}
