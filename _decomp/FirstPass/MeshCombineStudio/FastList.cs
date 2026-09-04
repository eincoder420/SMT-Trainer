using System;
using UnityEngine;

namespace MeshCombineStudio;

[Serializable]
public class FastList<T> : FastListBase<T>
{
	public FastList()
	{
		items = new T[4];
		arraySize = 4;
	}

	public FastList(bool reserve, int reserved)
	{
		int num = Mathf.Max(reserved, 4);
		items = new T[num];
		arraySize = num;
		Count = (_count = reserved);
	}

	public FastList(int capacity)
	{
		if (capacity < 1)
		{
			capacity = 1;
		}
		items = new T[capacity];
		arraySize = capacity;
	}

	public FastList(FastList<T> list)
	{
		if (list == null)
		{
			items = new T[4];
			arraySize = 4;
			return;
		}
		items = new T[list.Count];
		Array.Copy(list.items, items, list.Count);
		arraySize = items.Length;
		Count = (_count = items.Length);
	}

	public FastList(T[] items)
	{
		base.items = items;
		_count = (Count = (arraySize = items.Length));
	}

	protected void SetCapacity(int capacity)
	{
		arraySize = capacity;
		T[] destinationArray = new T[arraySize];
		if (_count > 0)
		{
			Array.Copy(items, destinationArray, _count);
		}
		items = destinationArray;
	}

	public void SetCount(int count)
	{
		if (count > arraySize)
		{
			SetCapacity(count);
		}
		Count = (_count = count);
	}

	public void EnsureCount(int count)
	{
		if (count > _count)
		{
			if (count > arraySize)
			{
				SetCapacity(count);
			}
			Count = (_count = count);
		}
	}

	public virtual void SetArray(T[] items)
	{
		base.items = items;
		_count = (Count = (arraySize = items.Length));
	}

	public int AddUnique(T item)
	{
		if (!Contains(item))
		{
			return Add(item);
		}
		return -1;
	}

	public bool Contains(T item)
	{
		return Array.IndexOf(items, item, 0, _count) != -1;
	}

	public int IndexOf(T item)
	{
		return Array.IndexOf(items, item, 0, _count);
	}

	public T GetIndex(T item)
	{
		int num = Array.IndexOf(items, item, 0, _count);
		if (num == -1)
		{
			return default(T);
		}
		return items[num];
	}

	public virtual int Add(T item)
	{
		if (_count == arraySize)
		{
			DoubleCapacity();
		}
		items[_count] = item;
		Count = ++_count;
		return _count - 1;
	}

	public virtual int AddThreadSafe(T item)
	{
		lock (this)
		{
			if (_count == arraySize)
			{
				DoubleCapacity();
			}
			items[_count] = item;
			Count = ++_count;
			return _count - 1;
		}
	}

	public virtual void Add(T item, T item2)
	{
		if (_count + 1 >= arraySize)
		{
			DoubleCapacity();
		}
		items[_count++] = item;
		items[_count++] = item2;
		Count = _count;
	}

	public virtual void Add(T item, T item2, T item3)
	{
		if (_count + 2 >= arraySize)
		{
			DoubleCapacity();
		}
		items[_count++] = item;
		items[_count++] = item2;
		items[_count++] = item3;
		Count = _count;
	}

	public virtual void Add(T item, T item2, T item3, T item4)
	{
		if (_count + 3 >= arraySize)
		{
			DoubleCapacity();
		}
		items[_count++] = item;
		items[_count++] = item2;
		items[_count++] = item3;
		items[_count++] = item4;
		Count = _count;
	}

	public virtual void Add(T item, T item2, T item3, T item4, T item5)
	{
		if (_count + 4 >= arraySize)
		{
			DoubleCapacity();
		}
		items[_count++] = item;
		items[_count++] = item2;
		items[_count++] = item3;
		items[_count++] = item4;
		items[_count++] = item5;
		Count = _count;
	}

	public virtual void Insert(int index, T item)
	{
		if (index > _count)
		{
			Debug.LogError("Index " + index + " is out of range " + _count);
		}
		if (_count == arraySize)
		{
			DoubleCapacity();
		}
		if (index < _count)
		{
			Array.Copy(items, index, items, index + 1, _count - index);
		}
		items[index] = item;
		Count = ++_count;
	}

	public virtual void AddRange(T[] arrayItems)
	{
		if (arrayItems != null)
		{
			int num = arrayItems.Length;
			int num2 = _count + num;
			if (num2 >= arraySize)
			{
				SetCapacity(num2 * 2);
			}
			Array.Copy(arrayItems, 0, items, _count, num);
			Count = (_count = num2);
		}
	}

	public virtual void AddRange(T[] arrayItems, int startIndex, int length)
	{
		int num = _count + length;
		if (num >= arraySize)
		{
			SetCapacity(num * 2);
		}
		Array.Copy(arrayItems, startIndex, items, _count, length);
		Count = (_count = num);
	}

	public virtual void AddRange(FastList<T> list)
	{
		if (list.Count != 0)
		{
			int num = _count + list.Count;
			if (num >= arraySize)
			{
				SetCapacity(num * 2);
			}
			Array.Copy(list.items, 0, items, _count, list.Count);
			Count = (_count = num);
		}
	}

	public virtual int GrabListThreadSafe(FastList<T> threadList, bool fastClear = false)
	{
		lock (threadList)
		{
			int count = _count;
			AddRange(threadList);
			if (fastClear)
			{
				threadList.FastClear();
			}
			else
			{
				threadList.Clear();
			}
			return count;
		}
	}

	public virtual void ChangeRange(int startIndex, T[] arrayItems)
	{
		for (int i = 0; i < arrayItems.Length; i++)
		{
			items[startIndex + i] = arrayItems[i];
		}
	}

	public virtual bool Remove(T item, bool weakReference = false)
	{
		int num = Array.IndexOf(items, item, 0, _count);
		if (num >= 0)
		{
			items[num] = items[--_count];
			items[_count] = default(T);
			Count = _count;
			return true;
		}
		return false;
	}

	public virtual void RemoveAt(int index)
	{
		if (index >= _count)
		{
			Debug.LogError("Index " + index + " is out of range. List count is " + _count);
		}
		else
		{
			items[index] = items[--_count];
			items[_count] = default(T);
			Count = _count;
		}
	}

	public virtual void RemoveLast()
	{
		if (_count != 0)
		{
			_count--;
			items[_count] = default(T);
			Count = _count;
		}
	}

	public virtual void RemoveRange(int index, int length)
	{
		if (_count - index < length)
		{
			Debug.LogError("Invalid length!");
		}
		if (length > 0)
		{
			_count -= length;
			if (index < _count)
			{
				Array.Copy(items, index + length, items, index, _count - index);
			}
			Array.Clear(items, _count, length);
			Count = _count;
		}
	}

	public virtual T Dequeue()
	{
		if (_count == 0)
		{
			Debug.LogError("List is empty!");
			return default(T);
		}
		T result = items[--_count];
		items[_count] = default(T);
		Count = _count;
		return result;
	}

	public virtual T Dequeue(int index)
	{
		T result = items[index];
		items[index] = items[--_count];
		items[_count] = default(T);
		Count = _count;
		return result;
	}

	public virtual void Clear()
	{
		Array.Clear(items, 0, _count);
		Count = (_count = 0);
	}

	public virtual void ClearThreadSafe()
	{
		lock (this)
		{
			Array.Clear(items, 0, _count);
			Count = (_count = 0);
		}
	}

	public virtual void ClearRange(int startIndex)
	{
		Array.Clear(items, startIndex, _count - startIndex);
		Count = (_count = startIndex);
	}

	public virtual void FastClear()
	{
		Count = (_count = 0);
	}

	public virtual void FastClear(int newCount)
	{
		if (newCount < Count)
		{
			Count = (_count = newCount);
		}
	}

	public virtual T[] ToArray()
	{
		T[] array = new T[_count];
		Array.Copy(items, 0, array, 0, _count);
		return array;
	}
}
