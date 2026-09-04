namespace MeshCombineStudio;

public class FastIndex : IFastIndex
{
	public IFastIndexList List { get; set; }

	public int ListIndex { get; set; }

	public FastIndex()
	{
		ListIndex = -1;
	}

	public void RemoveFromList()
	{
		if (List != null)
		{
			List.Remove(this);
		}
	}
}
