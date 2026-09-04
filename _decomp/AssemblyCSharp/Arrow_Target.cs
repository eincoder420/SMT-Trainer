using UnityEngine;

public class Arrow_Target : MonoBehaviour
{
	public Transform target;

	public Transform arrow;

	private MeshRenderer rend;

	public Transform player;

	public bool Have_Target;

	private void Start()
	{
		rend = arrow.GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		if (Have_Target)
		{
			Vector3 position = target.transform.position;
			position.y = arrow.transform.position.y;
			arrow.transform.LookAt(position);
			rend.enabled = Vector3.Distance(player.position, target.position) > 3f;
			if (Vector3.Distance(player.position, target.position) < 1f && target.gameObject.activeInHierarchy)
			{
				target.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (target.gameObject.activeInHierarchy)
			{
				target.gameObject.SetActive(value: false);
			}
			if (rend.enabled)
			{
				rend.enabled = false;
			}
		}
	}
}
