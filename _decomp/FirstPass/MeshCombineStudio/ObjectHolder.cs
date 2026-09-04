namespace MeshCombineStudio;

public class ObjectHolder<T> : FastIndex
{
	public T item;

	public ObjectHolder()
	{
	}

	public ObjectHolder(T item)
	{
		this.item = item;
	}
}
