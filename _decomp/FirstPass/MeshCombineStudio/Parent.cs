using System;

namespace MeshCombineStudio;

public abstract class Parent<T>
{
	[NonSerialized]
	public T parent;
}
