using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI Throw Object", true, "icon_v2", false, "")]
public class vAIThrowObject : vMonoBehaviour
{
	[Serializable]
	public class ThrowableObject
	{
		public string name;

		public Rigidbody prefab;

		public bool customStartingPoint;

		[vHideInInspector("customStartingPoint", false)]
		public Transform startingPoint;

		public float throwObjectDelayTime = 0.5f;

		public float activeCollisionDelayTime = 0.1f;

		[Tooltip("Time to break Thrown Routine after dead\nIf Time To Throw(see Debug Toolbar) time is greater than this time and Character is dead, the Object will be instantiated but not Launched\nElse Instantiate will be canceled")]
		public float minTimeToThrowAfterDead = 0.25f;

		public float throwAngle = 20f;

		public UnityEvent onStartThrow;

		public UnityEvent onFinishThrow;

		public ThrowableObject()
		{
			throwObjectDelayTime = 0.5f;
			activeCollisionDelayTime = 0.1f;
			throwAngle = 20f;
		}
	}

	[vEditorToolbar("Settings", false, "", false, false)]
	public vControlAI controlAI;

	public Transform defaultThrowStartPoint;

	[vEditorToolbar("Throwables", false, "", false, false)]
	public List<ThrowableObject> throwableObjects;

	[vEditorToolbar("Debug", false, "", false, false)]
	[vReadOnly(false)]
	public bool inThrow;

	[vReadOnly(false)]
	[SerializeField]
	protected float timeToThrow;

	public virtual Vector3 aimPoint => controlAI.currentTarget.transform.position;

	public virtual Vector3 aimDirection => aimPoint - defaultThrowStartPoint.position;

	private Vector3 targetDirection
	{
		get
		{
			if (!controlAI.currentTarget.transform)
			{
				return base.transform.forward;
			}
			return controlAI.lastTargetPosition - base.transform.position;
		}
	}

	private Vector3 targetPosition => controlAI.lastTargetPosition;

	private void Awake()
	{
		controlAI = GetComponent<vControlAI>();
	}

	private Vector3 StartVelocity(Transform startPTransform, Vector3 targetP, float angle)
	{
		float num = Vector3.Distance(startPTransform.position, targetP);
		startPTransform.LookAt(targetP);
		float num2 = Mathf.Sqrt(num * (0f - Physics.gravity.y) / Mathf.Sin((float)Math.PI / 180f * angle * 2f));
		float y = num2 * Mathf.Sin((float)Math.PI / 180f * angle);
		float z = num2 * Mathf.Cos((float)Math.PI / 180f * angle);
		Vector3 vector = new Vector3(0f, y, z);
		return startPTransform.TransformVector(vector);
	}

	private void LaunchObject(Rigidbody projectily, Transform startPTransform, Vector3 targetP, float angle)
	{
		projectily.AddForce(StartVelocity(startPTransform, targetP, angle), ForceMode.VelocityChange);
	}

	public void Throw(string throwableObjectName)
	{
		if (!controlAI.ragdolled && !controlAI.isDead && !controlAI.customAction && !inThrow)
		{
			ThrowableObject throwableObject = throwableObjects.Find((ThrowableObject t) => t.name.Equals(throwableObjectName));
			if (throwableObject != null)
			{
				StartCoroutine(Launch(throwableObject));
			}
		}
	}

	private IEnumerator Launch(ThrowableObject objectToThrow)
	{
		objectToThrow.onStartThrow.Invoke();
		inThrow = true;
		controlAI.RotateTo(targetDirection);
		controlAI.StrafeMoveTo(base.transform.position, targetDirection);
		timeToThrow = 0f;
		Transform startingPoint = (objectToThrow.customStartingPoint ? objectToThrow.startingPoint : defaultThrowStartPoint);
		bool canInstantiate = true;
		bool canThrow = true;
		while (timeToThrow < objectToThrow.throwObjectDelayTime)
		{
			timeToThrow += Time.deltaTime;
			if (controlAI == null || controlAI.isDead)
			{
				canInstantiate = timeToThrow >= objectToThrow.minTimeToThrowAfterDead;
				canThrow = false;
				break;
			}
			yield return null;
		}
		timeToThrow = 0f;
		if (canInstantiate)
		{
			Rigidbody obj = UnityEngine.Object.Instantiate(objectToThrow.prefab, startingPoint.position, startingPoint.rotation);
			obj.isKinematic = false;
			if (canThrow)
			{
				LaunchObject(obj, startingPoint, targetPosition, objectToThrow.throwAngle);
			}
			objectToThrow.onFinishThrow.Invoke();
			if (canThrow)
			{
				yield return new WaitForSeconds(2f * objectToThrow.activeCollisionDelayTime);
			}
			else
			{
				yield return new WaitForSeconds(1f);
			}
			Collider component = obj.GetComponent<Collider>();
			if ((bool)component)
			{
				component.isTrigger = false;
			}
		}
		else
		{
			objectToThrow.onFinishThrow.Invoke();
		}
		inThrow = false;
	}
}
