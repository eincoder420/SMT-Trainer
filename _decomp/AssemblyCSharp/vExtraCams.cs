using Invector;
using Invector.vCharacterController;
using UnityEngine;

[vClassHeader(" Extra Camera views", "adds three third cameras which follows the player", iconName = "FPCameraExtraCIcon")]
public class vExtraCams : vMonoBehaviour
{
	private Transform target;

	private vThirdPersonController player;

	public float rotationSpeed = 4f;

	public float followSpeed = 12f;

	public float height = 0.6f;

	private float capsuleH;

	private Animator animator;

	private Transform headBone;

	private void Start()
	{
		player = (vThirdPersonController)Object.FindObjectOfType(typeof(vThirdPersonController));
		player = Object.FindObjectOfType<vThirdPersonController>();
		target = player.transform;
	}

	private void FixedUpdate()
	{
		capsuleH = player.GetComponent<CapsuleCollider>().height;
		float y = height + capsuleH;
		Vector3 b = target.position + new Vector3(0f, y, 0f);
		base.transform.position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * followSpeed);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(base.transform.eulerAngles.x, target.eulerAngles.y, base.transform.eulerAngles.z), Time.deltaTime * rotationSpeed);
	}
}
