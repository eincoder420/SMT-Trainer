using UnityEngine;

[ExecuteInEditMode]
public class EnableChildrenMeshRenderers : MonoBehaviour
{
	public bool execute;

	private void Update()
	{
		if (execute)
		{
			execute = false;
			Execute();
		}
	}

	private void Execute()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}
}
