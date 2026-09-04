using PathCreation;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
public class GeneratePathExample : MonoBehaviour
{
	public bool closedLoop = true;

	public Transform way;

	private void Start()
	{
		Transform[] array = new Transform[way.childCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = way.GetChild(i);
		}
		if (array.Length != 0)
		{
			BezierPath bezierPath = new BezierPath(array, closedLoop, PathSpace.xyz);
			GetComponent<PathCreator>().bezierPath = bezierPath;
		}
	}
}
