using System.Collections.Generic;

namespace FIMSpace.FOptimizing;

public class FOptimizers_CullingContainersList : List<FOptimizers_CullingContainer>
{
	public int ID { get; private set; }

	public FOptimizers_CullingContainersList(int id)
	{
		ID = id;
	}

	public void Dispose()
	{
		for (int i = 0; i < base.Count; i++)
		{
			base[i].Dispose();
		}
		Clear();
	}
}
