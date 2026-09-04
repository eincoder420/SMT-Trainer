using UnityEngine;

namespace MeshCombineStudio;

public class FastIndexList<T> : FastList<T>, IFastIndexList where T : IFastIndex
{
	public FastIndexList()
	{
		items = new T[4];
	}

	public FastIndexList(int capacity)
	{
		items = new T[capacity];
	}

	public new void Clear()
	{
		for (int i = 0; i < _count; i++)
		{
			items[i].ListIndex = -1;
			items[i].List = null;
			items[i] = default(T);
		}
		Count = (_count = 0);
	}

	public void SetItem(int index, T item)
	{
		if (item.List != null)
		{
			Debug.LogError("Is already in another list!");
			return;
		}
		if (index >= items.Length)
		{
			SetCapacity(index * 2);
		}
		else if (index >= _count)
		{
			_count = (Count = index + 1);
		}
		items[index] = item;
		item.ListIndex = index;
		item.List = this;
	}

	public new int Add(T item)
	{
		IFastIndexList list = item.List;
		if (list == this)
		{
			Debug.LogError("Item is already in this list");
			return item.ListIndex;
		}
		if (list != null)
		{
			Debug.LogError("Is already in another list!");
			return -1;
		}
		if (item.ListIndex != -1)
		{
			Debug.Log("Item already added");
			return -1;
		}
		if (_count == items.Length)
		{
			DoubleCapacity();
		}
		items[_count] = item;
		item.ListIndex = _count++;
		item.List = this;
		Count = _count;
		return _count - 1;
	}

	public new void AddRange(T[] newItems)
	{
		int num = _count + newItems.Length;
		if (num >= items.Length)
		{
			SetCapacity(num * 2);
		}
		for (int i = 0; i < newItems.Length; i++)
		{
			if (newItems[i].List != null)
			{
				Debug.LogError("Is already in another list!");
				continue;
			}
			if (newItems[i].ListIndex != -1)
			{
				Debug.Log("Item already added");
				continue;
			}
			items[_count] = newItems[i];
			newItems[i].ListIndex = _count++;
			newItems[i].List = this;
		}
		Count = _count;
	}

	public new bool RemoveAt(int index)
	{
		if (index >= _count)
		{
			Debug.LogError("Index " + index + " is out of range. List count is " + _count);
			return false;
		}
		T val = items[index];
		if (val.ListIndex == -1)
		{
			Debug.Log("Item already removed");
			return false;
		}
		items[index] = items[--_count];
		items[index].ListIndex = index;
		items[_count] = default(T);
		val.ListIndex = -1;
		val.List = null;
		Count = _count;
		return true;
	}

	public override T Dequeue()
	{
		if (_count == 0)
		{
			return default(T);
		}
		T result = items[--_count];
		items[_count] = default(T);
		result.ListIndex = -1;
		result.List = null;
		Count = _count;
		return result;
	}

	public bool Remove(IFastIndex item)
	{
		if (item == null || item.List != this)
		{
			return false;
		}
		int listIndex = item.ListIndex;
		if (listIndex == -1)
		{
			Debug.Log("Item already removed");
			return false;
		}
		items[listIndex] = items[--_count];
		items[listIndex].ListIndex = listIndex;
		items[_count] = default(T);
		item.ListIndex = -1;
		item.List = null;
		Count = _count;
		return true;
	}
}
