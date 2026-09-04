using UnityEngine;

public class Move_By_Mouse : MonoBehaviour
{
	public Vector3 hit_point;

	private void Update()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray))
		{
			hit_point = ray.GetPoint(1f);
			base.transform.position = new Vector3(hit_point.x, hit_point.y, base.transform.position.z);
		}
	}
}
