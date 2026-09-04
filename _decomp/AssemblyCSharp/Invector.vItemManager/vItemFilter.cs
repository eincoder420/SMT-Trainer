using System;
using System.Collections.Generic;

namespace Invector.vItemManager;

[Serializable]
public class vItemFilter
{
	public bool invertFilterResult;

	public List<vItemType> filter;

	public vItemFilter()
	{
		filter = new List<vItemType>();
	}

	public vItemFilter(bool invertFilterResult = false, params vItemType[] itemTypesToFilter)
	{
		if (itemTypesToFilter != null && itemTypesToFilter.Length != 0)
		{
			filter = itemTypesToFilter.vToList();
		}
		else
		{
			filter = new List<vItemType>();
		}
		this.invertFilterResult = invertFilterResult;
	}

	public bool Validate(vItem item)
	{
		if (item == null)
		{
			return false;
		}
		if (!invertFilterResult)
		{
			return filter.Contains(item.type);
		}
		return !filter.Contains(item.type);
	}
}
