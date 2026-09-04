using System;
using System.Linq;
using UnityEngine;

namespace FCG;

public class TrafficCar : MonoBehaviour
{
	public enum StatusCar
	{
		transitingNormally,
		waitingForAnotherVehicleToPass,
		stoppedAtTrafficLights,
		bloked,
		Undefined,
		crashed
	}

	[Serializable]
	public class CarWheelsTransform
	{
		public Transform frontRight;

		public Transform frontLeft;

		public Transform backRight;

		public Transform backLeft;

		public Transform backRight2;

		public Transform backLeft2;
	}

	[Serializable]
	public class CarSetting
	{
		public Transform carSteer;

		[Range(10000f, 60000f)]
		public float springs = 25000f;

		[Range(1000f, 6000f)]
		public float dampers = 1500f;

		[Range(60f, 200f)]
		public float carPower = 120f;

		[Range(5f, 10f)]
		public float brakePower = 8f;

		[Range(20f, 30f)]
		public float limitSpeed = 30f;

		[Range(30f, 72f)]
		public float maxSteerAngle = 40f;

		[Range(-1f, 1f)]
		public float curveAdjustment;
	}

	[HideInInspector]
	public StatusCar status;

	public GameObject BreakLight;

	public GameObject LightLeft;

	public GameObject LightRight;

	private bool lightDirection;

	[HideInInspector]
	public Transform mRayC1;

	[HideInInspector]
	public Transform mRayC2;

	private Vector3 mRayCenter;

	[HideInInspector]
	public Transform[] wheel;

	public WheelCollider[] wCollider;

	private int countWays;

	private Transform[] nodes;

	[HideInInspector]
	public int currentNode;

	[HideInInspector]
	public float distanceToNode;

	private float steer;

	private float speed;

	private float brake;

	private float motorTorque;

	private Vector3 steerCurAngle = Vector3.zero;

	private Rigidbody myRigidbody;

	private Vector3 relativeVector;

	public CarWheelsTransform wheelsTransforms;

	private float timeStoped;

	private Transform myReference;

	private float iRC;

	private float brake2;

	[HideInInspector]
	public Transform atualWay;

	[HideInInspector]
	public int sideAtual;

	[HideInInspector]
	public FCGWaypointsContainer atualWayScript;

	[HideInInspector]
	public bool nodeSteerCarefully;

	[HideInInspector]
	public bool nodeSteerCarefully2;

	[HideInInspector]
	public Transform myOldWay;

	[HideInInspector]
	public int myOldSideAtual;

	[HideInInspector]
	public FCGWaypointsContainer myOldWayScript;

	private Vector3 _avanceNode = Vector3.zero;

	private float countTimeToSignal;

	private bool toSignal;

	private bool toSignalLeft;

	private bool toSignalRight;

	private Transform behind;

	public Transform player;

	public TrafficSystem tSystem;

	public float distanceToSelfDestroy;

	public CarSetting carSetting;

	private Vector3 shiftCentre = new Vector3(0f, -0.05f, 0f);

	private int countC;

	private Transform GetTransformWheel(string wheelName)
	{
		GameObject[] array = (from g in UnityEngine.Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name.Equals(wheelName) && g.transform.parent.root == base.transform
			select g).ToArray();
		if (array.Length != 0)
		{
			return array[0].transform;
		}
		return null;
	}

	public void Configure()
	{
		if (!wheelsTransforms.frontRight)
		{
			wheelsTransforms.frontRight = GetTransformWheel("FR");
		}
		if (!wheelsTransforms.frontLeft)
		{
			wheelsTransforms.frontLeft = GetTransformWheel("FL");
		}
		if (!wheelsTransforms.backRight)
		{
			wheelsTransforms.backRight = GetTransformWheel("BR");
		}
		if (!wheelsTransforms.backLeft)
		{
			wheelsTransforms.backLeft = GetTransformWheel("BL");
		}
		if (!wheelsTransforms.backRight2)
		{
			wheelsTransforms.backRight2 = base.transform.Find("BR2");
		}
		if (!wheelsTransforms.backLeft2)
		{
			wheelsTransforms.backLeft2 = base.transform.Find("BL2");
		}
		if (!base.transform.GetComponent<Rigidbody>())
		{
			base.transform.gameObject.AddComponent<Rigidbody>();
		}
		if (base.transform.gameObject.GetComponent<Rigidbody>().mass < 4000f)
		{
			base.transform.gameObject.GetComponent<Rigidbody>().mass = 4000f;
		}
		base.transform.gameObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		if (!wheelsTransforms.frontLeft || !wheelsTransforms.frontRight || !wheelsTransforms.backRight || !wheelsTransforms.backLeft)
		{
			Debug.LogError("wheelsTransforms absent in inspector");
			return;
		}
		float z = wheelsTransforms.frontRight.localPosition.z + 0.6f;
		float x = wheelsTransforms.frontRight.localPosition.x;
		Transform transform = new GameObject("RayTest").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0f, wheelsTransforms.frontRight.localPosition.z + 4f);
		transform.LookAt(base.transform);
		transform.position += new Vector3(0f, 0.8f, 0f);
		if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 4f))
		{
			Debug.DrawRay(transform.position, transform.forward * 4f, Color.red);
			z = wheelsTransforms.frontRight.localPosition.z + 4f - hitInfo.distance - 0.15f;
		}
		else
		{
			Debug.LogError("Adicione um collider e então tente novamente");
		}
		UnityEngine.Object.DestroyImmediate(transform.gameObject);
		if (!base.transform.Find("RayC1"))
		{
			mRayC1 = new GameObject("RayC1").transform;
			mRayC1.SetParent(base.transform);
		}
		else if (!mRayC1)
		{
			mRayC1 = base.transform.Find("RayC1");
		}
		mRayC1.localRotation = Quaternion.identity;
		mRayC1.localPosition = new Vector3(0f - x, 0.8f, z);
		if (!base.transform.Find("RayC2"))
		{
			mRayC2 = new GameObject("RayC2").transform;
			mRayC2.SetParent(base.transform);
		}
		else if (!mRayC1)
		{
			mRayC2 = base.transform.Find("RayC2");
		}
		mRayC2.localRotation = Quaternion.identity;
		mRayC2.localPosition = new Vector3(x, 0.8f, z);
		carSetting.maxSteerAngle = (int)Mathf.Clamp(Vector3.Distance(wheelsTransforms.frontRight.transform.position, wheelsTransforms.backRight.transform.position) * 12f, 35f, 72f);
		wheel = new Transform[4];
		wCollider = new WheelCollider[4];
		GameObject gameObject = new GameObject("Center");
		Vector3[] array = new Vector3[4];
		Vector3 vector = new Vector3(0f, 0f, 0f);
		wheel[0] = wheelsTransforms.frontRight;
		wheel[1] = wheelsTransforms.frontLeft;
		wheel[2] = wheelsTransforms.backRight;
		wheel[3] = wheelsTransforms.backLeft;
		for (int i = 0; i < 4; i++)
		{
			wCollider[i] = SetWheelComponent(i);
			gameObject.transform.SetParent(wheel[i].transform);
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			gameObject.transform.SetParent(base.transform);
			array[i] = (gameObject.transform.localPosition -= new Vector3(0f, wCollider[i].radius, 0f));
			vector += array[i];
		}
		shiftCentre = vector / 4f;
		UnityEngine.Object.DestroyImmediate(gameObject);
	}

	private WheelCollider SetWheelComponent(int w)
	{
		Transform transform = base.transform.Find(wheel[w].name + " - WheelCollider");
		if ((bool)transform)
		{
			try
			{
				UnityEngine.Object.DestroyImmediate(transform.gameObject);
			}
			catch
			{
			}
		}
		if ((bool)transform)
		{
			return transform.GetComponent<WheelCollider>();
		}
		transform = new GameObject(wheel[w].name + " - WheelCollider").transform;
		transform.transform.SetParent(base.transform);
		transform.transform.position = wheel[w].position;
		transform.transform.eulerAngles = base.transform.eulerAngles;
		WheelCollider obj2 = (WheelCollider)transform.gameObject.AddComponent(typeof(WheelCollider));
		WheelCollider component = transform.GetComponent<WheelCollider>();
		JointSpring suspensionSpring = obj2.suspensionSpring;
		suspensionSpring.spring = carSetting.springs;
		suspensionSpring.damper = carSetting.dampers;
		obj2.suspensionSpring = suspensionSpring;
		obj2.suspensionDistance = 0.05f;
		obj2.radius = wheel[w].GetComponent<MeshFilter>().sharedMesh.bounds.size.z * wheel[w].transform.localScale.z * 0.5f * 0.92f;
		obj2.mass = 2000f;
		return component;
	}

	private void Start()
	{
		timeStoped = Time.time;
		float z = wheelsTransforms.frontRight.localPosition.z + 0.6f;
		if (!base.transform.Find("RayC1"))
		{
			mRayC1 = new GameObject("RayC1").transform;
			mRayC1.SetParent(base.transform);
			mRayC1.localRotation = Quaternion.identity;
			mRayC1.localPosition = new Vector3(-0.6f, 0.5f, z);
		}
		else if (!mRayC1)
		{
			mRayC1 = base.transform.Find("RayC1");
		}
		if (!base.transform.Find("RayC2"))
		{
			mRayC2 = new GameObject("RayC2").transform;
			mRayC2.SetParent(base.transform);
			mRayC2.localRotation = Quaternion.identity;
			mRayC2.localPosition = new Vector3(0.6f, 0.5f, z);
		}
		else if (!mRayC1)
		{
			mRayC2 = base.transform.Find("RayC2");
		}
		myReference = new GameObject("myReference").transform;
		myReference.SetParent(base.transform);
		myReference.localPosition = new Vector3(0f, 0f, wheelsTransforms.frontRight.localPosition.z * 0.6f);
		myReference.localRotation = Quaternion.identity;
		if ((bool)player && !UnityEngine.Object.FindObjectOfType<TrafficSystem>())
		{
			Debug.LogError("The Traffic System.prefab not found in the Hierarchy");
		}
		if ((bool)atualWay)
		{
			Init();
		}
	}

	public void Init()
	{
		atualWayScript = atualWay.GetComponent<FCGWaypointsContainer>();
		myRigidbody = base.transform.GetComponent<Rigidbody>();
		myRigidbody.centerOfMass = shiftCentre;
		DefineNewPath();
		if (currentNode == 0)
		{
			currentNode = 1;
		}
		distanceToNode = Vector3.Distance(atualWayScript.Node(sideAtual, currentNode), myReference.position + myReference.forward * (carSetting.curveAdjustment * 0.5f));
		InvokeRepeating("MoveCar", 0.02f, 0.02f);
		status = StatusCar.transitingNormally;
		lightDirection = (bool)LightLeft && (bool)LightRight;
		if ((bool)BreakLight)
		{
			BreakLight.SetActive(value: false);
		}
		if ((bool)LightLeft)
		{
			LightLeft.SetActive(value: false);
		}
		if ((bool)LightRight)
		{
			LightRight.SetActive(value: false);
		}
	}

	public void ActivateSelfDestructWhenAwayFromThePlayer()
	{
		if ((bool)tSystem && (bool)player)
		{
			if (distanceToSelfDestroy == 0f)
			{
				distanceToSelfDestroy = 200f;
			}
			InvokeRepeating("SelfDestructWhenAwayFromThePlayer", 5f, 5f);
		}
	}

	public float GetSpeed()
	{
		return speed;
	}

	public bool Get_avanceNode()
	{
		if (currentNode == 0 && nodeSteerCarefully)
		{
			return _avanceNode != Vector3.zero;
		}
		return false;
	}

	public Vector3 GetNodePosition()
	{
		return atualWayScript.Node(sideAtual, currentNode);
	}

	private bool CheckBookAllPathOptions(FCGWaypointsContainer wayScript, int side)
	{
		int num = ((side == 0) ? wayScript.nextWay0.Length : wayScript.nextWay1.Length);
		for (int i = 0; i < num; i++)
		{
			FCGWaypointsContainer fCGWaypointsContainer;
			int side2;
			if (side == 0)
			{
				fCGWaypointsContainer = wayScript.nextWay0[i];
				side2 = wayScript.nextWaySide0[i];
			}
			else
			{
				fCGWaypointsContainer = wayScript.nextWay1[i];
				side2 = wayScript.nextWaySide1[i];
			}
			if ((bool)fCGWaypointsContainer)
			{
				if (fCGWaypointsContainer.GetNodeZeroCar(side2) != null && fCGWaypointsContainer.GetNodeZeroCar(side2) != base.transform && fCGWaypointsContainer.GetNodeZeroOldWay(side2) != myOldWay && (!Get_avanceNode() || !fCGWaypointsContainer.GetNodeZeroCar(side2).GetComponent<TrafficCar>().Get_avanceNode()))
				{
					return false;
				}
			}
			else
			{
				Debug.LogWarning("wScript Error");
			}
		}
		return true;
	}

	private bool BookAllPathOptions(FCGWaypointsContainer wayScript, int side, bool book = true)
	{
		int num = ((side == 0) ? wayScript.nextWay0.Length : wayScript.nextWay1.Length);
		for (int i = 0; i < num; i++)
		{
			FCGWaypointsContainer fCGWaypointsContainer;
			int side2;
			if (side == 0)
			{
				fCGWaypointsContainer = wayScript.nextWay0[i];
				side2 = wayScript.nextWaySide0[i];
			}
			else
			{
				fCGWaypointsContainer = wayScript.nextWay1[i];
				side2 = wayScript.nextWaySide1[i];
			}
			if (book)
			{
				bool force = (bool)fCGWaypointsContainer.GetNodeZeroCar(side2) && fCGWaypointsContainer.GetNodeZeroCar(side2).GetComponent<TrafficCar>().Get_avanceNode();
				if (!fCGWaypointsContainer.SetNodeZero(side2, wayScript.transform, base.transform, force))
				{
					return false;
				}
			}
			else
			{
				fCGWaypointsContainer.UnSetNodeZero(side2, base.transform);
			}
		}
		return true;
	}

	private void MoveCar()
	{
		if (status == StatusCar.bloked)
		{
			return;
		}
		if (lightDirection)
		{
			countTimeToSignal += 1f;
			if (countTimeToSignal > 16f)
			{
				countTimeToSignal = 0f;
				toSignal = !toSignal;
				if (toSignalLeft)
				{
					LightLeft.SetActive(toSignal);
				}
				else if (toSignalRight)
				{
					LightRight.SetActive(toSignal);
				}
				else
				{
					LightLeft.SetActive(value: false);
					LightRight.SetActive(value: false);
				}
			}
		}
		speed = myRigidbody.velocity.magnitude * 3.6f;
		VerificaPoints();
		distanceToNode = Vector3.Distance(atualWayScript.Node(sideAtual, currentNode), myReference.position + myReference.forward * (carSetting.curveAdjustment * 0.5f));
		if (_avanceNode != Vector3.zero)
		{
			relativeVector = base.transform.InverseTransformPoint(_avanceNode);
			if (Vector3.Distance(_avanceNode, myReference.position) < 4f)
			{
				_avanceNode = Vector3.zero;
			}
		}
		else
		{
			relativeVector = base.transform.InverseTransformPoint(atualWayScript.Node(sideAtual, currentNode, (currentNode == 0 && nodeSteerCarefully) ? 3 : 0));
		}
		steer = relativeVector.x / relativeVector.magnitude * carSetting.maxSteerAngle;
		bool flag = true;
		iRC += 1f;
		if (iRC >= 4f)
		{
			iRC = 0f;
			if (currentNode == 0)
			{
				if (behind == null && atualWayScript.BookNodeZero(this))
				{
					if ((nodeSteerCarefully && !myOldWayScript.oneway) || nodeSteerCarefully2)
					{
						if (!nodeSteerCarefully2)
						{
							flag = myOldWayScript.SetNodeZero((myOldSideAtual != 1) ? 1 : 0, myOldWay, base.transform);
						}
						bool flag2 = CheckBookAllPathOptions(myOldWayScript, myOldSideAtual) && BookAllPathOptions(myOldWayScript, myOldSideAtual);
						brake2 = ((!(flag && flag2)) ? 4000 : 0);
					}
					else
					{
						brake2 = 0f;
					}
				}
				else
				{
					brake2 = 4000f;
				}
			}
			else
			{
				brake2 = 0f;
			}
			if (speed > 2f)
			{
				status = StatusCar.transitingNormally;
			}
			if (brake2 <= 0f || (bool)behind)
			{
				brake = FixedRaycasts();
			}
			else
			{
				status = StatusCar.waitingForAnotherVehicleToPass;
			}
			if (speed < 2f && (status != StatusCar.stoppedAtTrafficLights || status != StatusCar.waitingForAnotherVehicleToPass))
			{
				if (Time.time > timeStoped + 50f)
				{
					UnityEngine.Object.Destroy(base.transform.gameObject);
					return;
				}
			}
			else
			{
				timeStoped = Time.time;
			}
		}
		brake = ((brake2 > brake) ? brake2 : brake);
		if ((bool)BreakLight)
		{
			BreakLight.SetActive(brake > 200f);
		}
		float num = 0f;
		if (speed > carSetting.limitSpeed)
		{
			num = Mathf.Lerp(100f, 1000f, (speed - carSetting.limitSpeed) / 10f);
		}
		if (num > brake)
		{
			brake = num;
		}
		for (int i = 0; i < 4; i++)
		{
			if (brake == 0f)
			{
				wCollider[i].brakeTorque = 0f;
			}
			else
			{
				wCollider[i].motorTorque = 0f;
				wCollider[i].brakeTorque = carSetting.brakePower * brake;
			}
			if (i < 2)
			{
				motorTorque = Mathf.Lerp(carSetting.carPower * 30f, 0f, speed / carSetting.limitSpeed);
				wCollider[i].motorTorque = motorTorque;
				wCollider[i].steerAngle = steer;
			}
			wCollider[i].GetWorldPose(out var pos, out var quat);
			wheel[i].position = pos;
			wheel[i].rotation = quat;
		}
		if ((bool)wheelsTransforms.backRight2)
		{
			wheelsTransforms.backRight2.rotation = wheelsTransforms.backRight.rotation;
			wheelsTransforms.backLeft2.rotation = wheelsTransforms.backRight.rotation;
		}
		if ((bool)carSetting.carSteer)
		{
			carSetting.carSteer.localEulerAngles = new Vector3(steerCurAngle.x, steerCurAngle.y, steerCurAngle.z - steer);
		}
	}

	private void VerificaPoints()
	{
		if (!(distanceToNode < 5f))
		{
			return;
		}
		if (currentNode < countWays - 1)
		{
			currentNode++;
			if (currentNode != 1)
			{
				return;
			}
			atualWayScript.UnSetNodeZero(sideAtual, base.transform);
			status = StatusCar.transitingNormally;
			if (nodeSteerCarefully || nodeSteerCarefully2)
			{
				myOldWayScript.UnSetNodeZero((myOldSideAtual != 1) ? 1 : 0, base.transform);
				BookAllPathOptions(myOldWayScript, myOldSideAtual, book: false);
			}
			nodeSteerCarefully = false;
			nodeSteerCarefully2 = false;
			if (lightDirection)
			{
				toSignalRight = false;
				toSignalLeft = false;
				if ((bool)LightLeft)
				{
					LightLeft.SetActive(value: false);
				}
				if ((bool)LightRight)
				{
					LightRight.SetActive(value: false);
				}
			}
			return;
		}
		int num = TestWay();
		bool num2 = ((sideAtual == 0) ? (atualWayScript.nextWay0.Length == 1) : (atualWayScript.nextWay1.Length == 1));
		myOldWay = atualWay;
		myOldSideAtual = sideAtual;
		myOldWayScript = atualWayScript;
		if (sideAtual == 0 && (!atualWayScript.oneway || atualWayScript.doubleLine))
		{
			sideAtual = atualWayScript.nextWaySide0[num];
			atualWayScript = atualWayScript.nextWay0[num];
		}
		else
		{
			sideAtual = atualWayScript.nextWaySide1[num];
			atualWayScript = atualWayScript.nextWay1[num];
		}
		atualWay = atualWayScript.transform;
		if (num2 && atualWayScript.bloked)
		{
			myOldWayScript.bloked = true;
			brake2 = 6000f;
			status = StatusCar.bloked;
		}
		DefineNewPath();
		currentNode = 0;
		float angulo = GetAngulo(base.transform, atualWayScript.Node(sideAtual, 0));
		if (myOldWayScript.oneway && !myOldWayScript.doubleLine)
		{
			if (myOldWayScript.doubleLine)
			{
				nodeSteerCarefully2 = (myOldSideAtual == 0 && angulo > 20f && angulo < 90f) || (myOldSideAtual == 1 && angulo < 340f && angulo > 270f);
			}
			else
			{
				nodeSteerCarefully = false;
			}
		}
		else
		{
			nodeSteerCarefully = (atualWayScript.rightHand == 0 && angulo < 340f && angulo > 270f) || (atualWayScript.rightHand != 0 && angulo > 20f && angulo < 90f);
		}
		if (lightDirection)
		{
			toSignalLeft = angulo < 340f && angulo > 270f;
			toSignalRight = angulo > 20f && angulo < 90f;
		}
		if (nodeSteerCarefully)
		{
			_avanceNode = myOldWayScript.AvanceNode(myOldSideAtual, myOldWayScript.waypoints.Count - 1, 7f);
		}
	}

	public Transform GetBehind()
	{
		return behind;
	}

	private float FixedRaycasts()
	{
		float num = 6f;
		float num2 = ((speed < 3f) ? (num / 1.5f) : num);
		mRayC1.localRotation = Quaternion.Euler(0f, steer, 0f);
		mRayC2.localRotation = mRayC1.localRotation;
		Debug.DrawRay((mRayC1.position + mRayC2.position) * 0.5f, mRayC1.forward * num2, Color.yellow);
		Debug.DrawRay(mRayC1.position, mRayC1.forward * num2, Color.yellow);
		Debug.DrawRay(mRayC2.position, mRayC2.forward * num2, Color.yellow);
		float num3;
		if (Physics.Raycast((mRayC1.position + mRayC2.position) * 0.5f, mRayC1.forward, out var hitInfo, num2))
		{
			Debug.DrawRay((mRayC1.position + mRayC2.position) * 0.5f, mRayC1.forward * num2, Color.red);
			num3 = hitInfo.distance;
		}
		else if (Physics.Raycast(mRayC1.position, mRayC1.forward, out hitInfo, num2))
		{
			Debug.DrawRay(mRayC1.position, mRayC1.forward * num2, Color.red);
			num3 = hitInfo.distance;
		}
		else if (Physics.Raycast(mRayC2.position, mRayC2.forward, out hitInfo, num2))
		{
			Debug.DrawRay(mRayC2.position, mRayC2.forward * num2, Color.red);
			num3 = hitInfo.distance;
		}
		else
		{
			num3 = 0f;
		}
		behind = ((num3 == 0f) ? null : hitInfo.transform);
		if (num3 > 0f && speed < 2f && status != StatusCar.stoppedAtTrafficLights && status != StatusCar.waitingForAnotherVehicleToPass && status != StatusCar.Undefined)
		{
			if (hitInfo.transform.name == "Stop")
			{
				status = StatusCar.stoppedAtTrafficLights;
			}
			else if ((bool)hitInfo.transform.GetComponent<TrafficCar>())
			{
				StatusCar statusCar = hitInfo.transform.GetComponent<TrafficCar>().status;
				status = ((statusCar == StatusCar.stoppedAtTrafficLights || statusCar == StatusCar.waitingForAnotherVehicleToPass) ? statusCar : StatusCar.Undefined);
			}
		}
		if (num3 == 0f)
		{
			return 0f;
		}
		if (!(num3 < 1f) && !(speed < 0.5f))
		{
			return speed * 6f * (num / num3 * 6f);
		}
		return 20000f;
	}

	private void DefineNewPath()
	{
		nodes = new Transform[atualWay.childCount];
		int num = 0;
		foreach (Transform item in atualWay)
		{
			nodes[num++] = item;
		}
		countWays = nodes.Length;
	}

	private int TestWay()
	{
		int num = 0;
		num = ((sideAtual != 0) ? atualWayScript.nextWay1.Length : atualWayScript.nextWay0.Length);
		int num2 = UnityEngine.Random.Range(0, num);
		if (num > 1 && (CheckStoped(num2) || VerifyTraffic(num2, 30f) < 30f || VerifyNodeSteerCarefully2(num2)))
		{
			float num3 = 0f;
			for (int i = 0; i < num; i++)
			{
				if (!CheckStoped(i))
				{
					float num4 = VerifyTraffic(i, 30f);
					if (num4 == 30f)
					{
						return i;
					}
					if (num4 < 30f && num4 > num3)
					{
						num3 = num4;
						num2 = i;
					}
				}
			}
		}
		return num2;
	}

	private Vector3 GetNodeNextWay(int way, int node = 0)
	{
		if (sideAtual == 0)
		{
			return atualWayScript.nextWay0[way].Node(atualWayScript.nextWaySide0[way], node);
		}
		return atualWayScript.nextWay1[way].Node(atualWayScript.nextWaySide1[way], node);
	}

	private bool CheckStoped(int way)
	{
		if (sideAtual == 0)
		{
			return atualWayScript.nextWay0[way].bloked;
		}
		return atualWayScript.nextWay1[way].bloked;
	}

	private bool VerifyNodeSteerCarefully2(int t)
	{
		if (atualWayScript.oneway && atualWayScript.doubleLine)
		{
			float angulo = GetAngulo(base.transform, GetNodeNextWay(t));
			if (sideAtual != 0 || !(angulo > 20f) || !(angulo < 90f))
			{
				if (sideAtual == 1 && angulo < 340f)
				{
					return angulo > 270f;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private float VerifyTraffic(int t, float mts = 12f)
	{
		Vector3 vector = GetNodeNextWay(t) + new Vector3(0f, 0.5f, 0f);
		Vector3 vector2 = GetNodeNextWay(t, 1) + new Vector3(0f, 0.5f, 0f);
		if (Physics.Raycast(vector, vector2 - vector, out var hitInfo, mts) && (bool)hitInfo.transform.GetComponent<TrafficCar>())
		{
			if (hitInfo.transform.GetComponent<TrafficCar>().GetSpeed() < 8f)
			{
				return hitInfo.distance;
			}
			return mts - 1f;
		}
		return mts;
	}

	private void Pause(Vector3 position)
	{
	}

	private void SelfDestructWhenAwayFromThePlayer()
	{
		if (speed < 2f && status != StatusCar.stoppedAtTrafficLights && ((Time.time > timeStoped + 30f) & InTheFieldOfVision(base.transform.position, player)))
		{
			tSystem.nVehicles--;
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (Vector3.Distance(base.transform.position, player.position) < distanceToSelfDestroy || InTheFieldOfVision(base.transform.position, player))
		{
			countC = 0;
			return;
		}
		countC++;
		if (countC >= 2)
		{
			tSystem.nVehicles--;
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void SelfDestructWhenAwayFromThePlayerInit()
	{
		if ((bool)tSystem && (bool)player)
		{
			if (Vector3.Distance(base.transform.position, player.position) > distanceToSelfDestroy && !InTheFieldOfVision(base.transform.position, player))
			{
				tSystem.nVehicles--;
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				ActivateSelfDestructWhenAwayFromThePlayer();
			}
		}
	}

	private bool InTheFieldOfVision(Vector3 source, Transform target)
	{
		if (Physics.Linecast(source + Vector3.up * 1f, target.position + Vector3.up * 1f, out var hitInfo))
		{
			if (hitInfo.transform == target || hitInfo.transform.root == target)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private float GetAngulo(Transform origem, Vector3 target)
	{
		GameObject obj = new GameObject("Compass");
		obj.transform.parent = origem;
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
		obj.transform.LookAt(target);
		float y = obj.transform.localEulerAngles.y;
		UnityEngine.Object.DestroyImmediate(obj);
		return y;
	}
}
