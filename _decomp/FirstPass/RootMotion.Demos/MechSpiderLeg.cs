using System;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class MechSpiderLeg : MonoBehaviour
{
	public MechSpider mechSpider;

	public MechSpiderLeg unSync;

	public Vector3 offset;

	public float minDelay = 0.2f;

	public float maxOffset = 1f;

	public float stepSpeed = 5f;

	public float footHeight = 0.15f;

	public float velocityPrediction = 0.2f;

	public float raycastFocus = 0.1f;

	public AnimationCurve yOffset;

	public Transform foot;

	public Vector3 footUpAxis;

	public float footRotationSpeed = 10f;

	public ParticleSystem sand;

	private IK ik;

	private float stepProgress = 1f;

	private float lastStepTime;

	private Vector3 defaultPosition;

	private RaycastHit hit;

	private Quaternion lastFootLocalRotation;

	private Vector3 smoothHitNormal = Vector3.up;

	private Vector3 lastStepPosition;

	public bool isStepping => stepProgress < 1f;

	public Vector3 position
	{
		get
		{
			return ik.GetIKSolver().GetIKPosition();
		}
		set
		{
			ik.GetIKSolver().SetIKPosition(value);
		}
	}

	private void Awake()
	{
		ik = GetComponent<IK>();
		if (foot != null)
		{
			if (footUpAxis == Vector3.zero)
			{
				footUpAxis = Quaternion.Inverse(foot.rotation) * Vector3.up;
			}
			lastFootLocalRotation = foot.localRotation;
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iKSolver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterIK));
		}
	}

	private void AfterIK()
	{
		if (!(foot == null))
		{
			foot.localRotation = lastFootLocalRotation;
			smoothHitNormal = Vector3.Slerp(smoothHitNormal, hit.normal, Time.deltaTime * footRotationSpeed);
			Quaternion quaternion = Quaternion.FromToRotation(foot.rotation * footUpAxis, smoothHitNormal);
			foot.rotation = quaternion * foot.rotation;
		}
	}

	private void Start()
	{
		stepProgress = 1f;
		hit = default(RaycastHit);
		IKSolver.Point[] points = ik.GetIKSolver().GetPoints();
		position = points[points.Length - 1].transform.position;
		lastStepPosition = position;
		hit.point = position;
		defaultPosition = mechSpider.transform.InverseTransformPoint(position + offset * mechSpider.scale);
		StartCoroutine(Step(position, position));
	}

	private Vector3 GetStepTarget(out bool stepFound, float focus, float distance)
	{
		stepFound = false;
		Vector3 vector = mechSpider.transform.TransformPoint(defaultPosition) + mechSpider.velocity * velocityPrediction;
		Vector3 up = mechSpider.transform.up;
		Vector3 rhs = mechSpider.body.position - position;
		Vector3 axis = Vector3.Cross(up, rhs);
		up = Quaternion.AngleAxis(focus, axis) * up;
		if (Physics.Raycast(vector + up * mechSpider.raycastHeight * mechSpider.scale, -up, out hit, mechSpider.raycastHeight * mechSpider.scale + distance, mechSpider.raycastLayers))
		{
			stepFound = true;
		}
		return hit.point + hit.normal * footHeight * mechSpider.scale;
	}

	private void UpdatePosition(float distance)
	{
		Vector3 up = mechSpider.transform.up;
		if (Physics.Raycast(lastStepPosition + up * mechSpider.raycastHeight * mechSpider.scale, -up, out hit, mechSpider.raycastHeight * mechSpider.scale + distance, mechSpider.raycastLayers))
		{
			position = hit.point + hit.normal * footHeight * mechSpider.scale;
		}
	}

	private void Update()
	{
		UpdatePosition(mechSpider.raycastDistance * mechSpider.scale);
		if (!isStepping && !(Time.time < lastStepTime + minDelay) && (!(unSync != null) || !unSync.isStepping))
		{
			bool stepFound = false;
			Vector3 stepTarget = GetStepTarget(out stepFound, raycastFocus, mechSpider.raycastDistance * mechSpider.scale);
			if (!stepFound)
			{
				stepTarget = GetStepTarget(out stepFound, 0f - raycastFocus, mechSpider.raycastDistance * 3f * mechSpider.scale);
			}
			if (stepFound && !(Vector3.Distance(position, stepTarget) < maxOffset * mechSpider.scale * UnityEngine.Random.Range(0.9f, 1.2f)))
			{
				StopAllCoroutines();
				StartCoroutine(Step(position, stepTarget));
			}
		}
	}

	private IEnumerator Step(Vector3 stepStartPosition, Vector3 targetPosition)
	{
		stepProgress = 0f;
		while (stepProgress < 1f)
		{
			stepProgress += Time.deltaTime * stepSpeed;
			position = Vector3.Lerp(stepStartPosition, targetPosition, stepProgress);
			position += mechSpider.transform.up * yOffset.Evaluate(stepProgress) * mechSpider.scale;
			lastStepPosition = position;
			yield return null;
		}
		position = targetPosition;
		lastStepPosition = position;
		if (sand != null)
		{
			sand.transform.position = position - mechSpider.transform.up * footHeight * mechSpider.scale;
			sand.Emit(20);
		}
		lastStepTime = Time.time;
	}
}
