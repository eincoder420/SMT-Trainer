using System;
using UnityEngine;

namespace MeshCombineStudio;

[Serializable]
public class SortedFastList<T> : FastList<T>
{
	public new void RemoveAt(int index)
	{
		if (index >= _count)
		{
			Debug.LogError("Index " + index + " is out of range " + _count);
		}
		_count--;
		if (index < _count)
		{
			Array.Copy(items, index + 1, items, index, _count - index);
		}
		items[_count] = default(T);
		Count = _count;
	}

	public new void RemoveRange(int index, int endIndex)
	{
		int num = endIndex - index + 1;
		if (index < 0)
		{
			Debug.LogError("Index needs to be bigger than 0 -> " + index);
		}
		else if (num < 0)
		{
			Debug.LogError("Length needs to be bigger than 0 -> " + num);
		}
		else if (_count - index >= num)
		{
			_count -= num;
			if (index < _count)
			{
				Array.Copy(items, index + num, items, index, _count - index);
			}
			Array.Clear(items, _count, num);
		}
	}
}
