using System;

namespace MeshCombineStudio;

public class FastListBase
{
	protected const int defaultCapacity = 4;

	public int Count;

	protected int _count;

	protected int arraySize;
}
public class FastListBase<T> : FastListBase
{
	public T[] items;

	protected void DoubleCapacity()
	{
		arraySize *= 2;
		T[] destinationArray = new T[arraySize];
		Array.Copy(items, destinationArray, _count);
		items = destinationArray;
	}
}
