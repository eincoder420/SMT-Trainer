using UnityEngine;

namespace PathCreation.Examples;

public class PathFollower : MonoBehaviour
{
	public PathCreator pathCreator;

	public EndOfPathInstruction endOfPathInstruction;

	public float speed = 5f;

	public float start_speed;

	public float speed_multiplier;

	public float distanceTravelled;

	public float distance_Offset;

	private void Start()
	{
		if (pathCreator != null)
		{
			pathCreator.pathUpdated += OnPathChanged;
			distanceTravelled = distance_Offset;
			speed_multiplier = 1f;
		}
	}

	private void Update()
	{
		if (pathCreator != null)
		{
			speed = (start_speed + Mathf.PerlinNoise(base.transform.position.x * 0.1f, base.transform.position.z * 0.1f) * 5f) * speed_multiplier;
			distanceTravelled += speed * Time.deltaTime;
			base.transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
			base.transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
		}
	}

	private void OnPathChanged()
	{
		distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(base.transform.position);
	}
}
