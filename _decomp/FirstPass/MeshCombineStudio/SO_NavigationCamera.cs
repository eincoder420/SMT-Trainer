using UnityEngine;

namespace MeshCombineStudio;

public class SO_NavigationCamera : ScriptableObject
{
	public float mouseSensitity = 1f;

	public float speedUpLerpMulti = 1f;

	public float speedDownLerpMulti = 15f;

	public float speedSlow = 1f;

	public float speedNormal = 10f;

	public float speedFast = 25f;

	public float mouseScrollWheelMulti = 25f;
}
