using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController.AI.FSMBehaviour;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI Spawn System", true, "icon_v2", false, "", openClose = false)]
public class vAISpawn : vMonoBehaviour
{
	[Serializable]
	public class vAISpawnProperties
	{
		public delegate void OnSpawnAI(vAIMotor ai, vAISpawnProperties spawnProperties);

		[Header("Spawn Properties")]
		public string spawnName;

		public vAIMotor prefab;

		public vAIMotor[] randomPrefab;

		public List<vSpawnPoint> spawnPoints;

		[Tooltip("Check this option to Pause the Spawn routine")]
		public bool pauseSpawning;

		public float timeToFirstSpawn = 1f;

		[Tooltip("Enable or Disable the FSM Controller when Spawn")]
		public bool enableFSMOnSpawn = true;

		[Tooltip("Delay to Enable the FSM Controller")]
		public float delayToEnableFSM = 2f;

		public bool randomTimeToSpawn = true;

		[vHideInInspector("randomTimeToSpawn", false)]
		public float minTimeBetweenSpawn = 1f;

		[vHideInInspector("randomTimeToSpawn", false)]
		public float maxTimeBetweenSpawn = 10f;

		public int maxQuantity = 10;

		public bool keepMaxQuantity = true;

		[Header("AI Detection Settings")]
		public bool overrideDetectionSettings;

		public vTagMask detectionTags;

		public LayerMask detectionLayer;

		public vTagMask damageTags;

		public LayerMask damageLayer;

		[Header("Spawn Destination")]
		public List<Transform> spawnDestinations;

		public vAIMovementSpeed destinationSpeed = vAIMovementSpeed.Running;

		public float setWaypointAreaDelay;

		public vWaypointArea waypointArea;

		public bool randomDestination;

		[Header("AIs Spawned")]
		public List<vAIMotor> aiSpawnedList;

		[Header("Spawn Events")]
		public UnityEvent onStartSpawn;

		public UnityEvent onSpawn;

		public UnityEvent onDead;

		private bool firstSpawnDone;

		private int spawned;

		private int indexOfDestination;

		private bool inSpawn;

		public bool canSpawn
		{
			get
			{
				if ((keepMaxQuantity ? aiSpawnedList.Count : spawned) < maxQuantity && spawnPoints.Count > 0)
				{
					if (!prefab)
					{
						return randomPrefab.Length != 0;
					}
					return true;
				}
				return false;
			}
		}

		public IEnumerator Spawn(MonoBehaviour mono, OnSpawnAI callBack = null, bool forceSpawn = false)
		{
			aiSpawnedList.RemoveAll((vAIMotor ai) => ai == null || ai.isDead);
			spawnPoints.RemoveAll((vSpawnPoint sp) => sp == null);
			spawnDestinations.RemoveAll((Transform sd) => sd == null);
			vAIMotor _ai = null;
			if (forceSpawn)
			{
				spawned = aiSpawnedList.Count;
			}
			if (canSpawn && (!pauseSpawning || forceSpawn) && !inSpawn)
			{
				inSpawn = true;
				yield return new WaitForEndOfFrame();
				List<vSpawnPoint> _spawnPoints = spawnPoints.FindAll((vSpawnPoint sp) => sp.isValid);
				if (_spawnPoints.Count > 0)
				{
					onStartSpawn.Invoke();
					yield return new WaitForSeconds((!firstSpawnDone) ? timeToFirstSpawn : (randomTimeToSpawn ? UnityEngine.Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn) : maxTimeBetweenSpawn));
					int index = Mathf.Clamp(UnityEngine.Random.Range(-1, _spawnPoints.Count), 0, _spawnPoints.Count - 1);
					vSpawnPoint vSpawnPoint2 = _spawnPoints[index];
					if (randomPrefab.Length != 0)
					{
						vAIMotor vAIMotor2 = randomPrefab[UnityEngine.Random.Range(0, randomPrefab.Length - 1)];
						if ((bool)vAIMotor2)
						{
							_ai = UnityEngine.Object.Instantiate(vAIMotor2, vSpawnPoint2.transform.position, vSpawnPoint2.transform.rotation);
						}
					}
					else
					{
						_ai = UnityEngine.Object.Instantiate(prefab, vSpawnPoint2.transform.position, vSpawnPoint2.transform.rotation);
					}
					firstSpawnDone = true;
					if ((bool)_ai)
					{
						_ai.onDead.AddListener(OnDead);
						onSpawn.Invoke();
						aiSpawnedList.Add(_ai);
						vIControlAI aiController = _ai.GetComponent<vIControlAI>();
						yield return new WaitForSeconds(0.1f);
						if (enableFSMOnSpawn)
						{
							vIFSMBehaviourController component = _ai.GetComponent<vIFSMBehaviourController>();
							if (component != null && enableFSMOnSpawn)
							{
								component.isStopped = true;
								if (delayToEnableFSM <= 0f)
								{
									component.isStopped = false;
								}
								else
								{
									mono.StartCoroutine(EnableFSM(component));
								}
							}
						}
						if (aiController != null)
						{
							Vector3 destination = (aiController.selfStartPosition = ((spawnDestinations.Count == 0) ? aiController.transform.position : GetSpawnDestination()));
							if (spawnDestinations.Count > 0)
							{
								aiController.MoveTo(destination, destinationSpeed);
							}
							if ((bool)waypointArea)
							{
								if (setWaypointAreaDelay <= 0f)
								{
									aiController.waypointArea = waypointArea;
								}
								else
								{
									mono.StartCoroutine(SetWaypointAreaToAI(aiController));
								}
							}
							if (overrideDetectionSettings)
							{
								aiController.SetDetectionLayer(detectionLayer);
								aiController.SetDetectionTags(detectionTags);
								if (aiController is vIControlAIMelee)
								{
									(aiController as vIControlAIMelee).SetMeleeHitTags(damageTags);
								}
								if (aiController is vIControlAIShooter)
								{
									(aiController as vIControlAIShooter).SetShooterHitLayer(damageLayer);
								}
							}
						}
						spawned++;
					}
				}
			}
			else
			{
				yield return new WaitForSeconds((!firstSpawnDone) ? timeToFirstSpawn : (randomTimeToSpawn ? UnityEngine.Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn) : maxTimeBetweenSpawn));
			}
			inSpawn = false;
			if (!forceSpawn)
			{
				callBack?.Invoke(_ai, this);
			}
		}

		private Vector3 GetRandomPoint()
		{
			int num = UnityEngine.Random.Range(-20, 20);
			int num2 = UnityEngine.Random.Range(-20, 20);
			return new Vector3(num, 0f, num2);
		}

		protected Vector3 GetSpawnDestination()
		{
			Vector3 zero = Vector3.zero;
			if (randomDestination)
			{
				indexOfDestination = Mathf.Clamp(UnityEngine.Random.Range(-1, spawnDestinations.Count), 0, spawnDestinations.Count - 1);
				zero = spawnDestinations[indexOfDestination].transform.position;
			}
			else
			{
				if (indexOfDestination >= spawnDestinations.Count)
				{
					indexOfDestination = 0;
				}
				zero = spawnDestinations[indexOfDestination].transform.position;
				indexOfDestination++;
			}
			return zero;
		}

		private IEnumerator EnableFSM(vIFSMBehaviourController vIFSM)
		{
			if (vIFSM != null)
			{
				yield return new WaitForSeconds(delayToEnableFSM);
				vIFSM.isStopped = false;
			}
		}

		private IEnumerator SetWaypointAreaToAI(vIControlAI controller)
		{
			yield return new WaitForSeconds(setWaypointAreaDelay);
			controller.waypointArea = waypointArea;
		}

		protected void OnDead(GameObject obj)
		{
			onDead.Invoke();
		}
	}

	public List<vAISpawnProperties> spawnPropertiesList;

	private readonly WaitForSeconds waitBetweenSpawnProps = new WaitForSeconds(0.1f);

	private IEnumerator Start()
	{
		for (int i = 0; i < spawnPropertiesList.Count; i++)
		{
			yield return waitBetweenSpawnProps;
			StartCoroutine(spawnPropertiesList[i].Spawn(this, OnAISpawned));
		}
	}

	public void Spawn(string spawnName)
	{
		vAISpawnProperties vAISpawnProperties = spawnPropertiesList.Find((vAISpawnProperties sp) => sp.spawnName.Equals(spawnName));
		if (vAISpawnProperties != null)
		{
			StartCoroutine(vAISpawnProperties.Spawn(this, OnAISpawned, forceSpawn: true));
		}
	}

	public void Spawn(int index)
	{
		if (spawnPropertiesList.Count > 0 && index < spawnPropertiesList.Count)
		{
			StartCoroutine(spawnPropertiesList[index].Spawn(this, null, forceSpawn: true));
		}
	}

	public void SpawnOneOfAll()
	{
		StartCoroutine(SpawnOneOfAllRoutine());
	}

	public void StartSpawn(string spawnName)
	{
		vAISpawnProperties vAISpawnProperties = spawnPropertiesList.Find((vAISpawnProperties sp) => sp.spawnName.Equals(spawnName));
		if (vAISpawnProperties != null)
		{
			vAISpawnProperties.pauseSpawning = false;
		}
	}

	public void StartSpawn(int index)
	{
		if (spawnPropertiesList.Count > 0 && index < spawnPropertiesList.Count)
		{
			spawnPropertiesList[index].pauseSpawning = false;
		}
	}

	public void StartSpawnAll()
	{
		StartCoroutine(StartAllRoutine());
	}

	public void PauseSpawn(string spawnName)
	{
		vAISpawnProperties vAISpawnProperties = spawnPropertiesList.Find((vAISpawnProperties sp) => sp.spawnName.Equals(spawnName));
		if (vAISpawnProperties != null)
		{
			vAISpawnProperties.pauseSpawning = true;
		}
	}

	public void PauseSpawn(int index)
	{
		if (spawnPropertiesList.Count > 0 && index < spawnPropertiesList.Count)
		{
			spawnPropertiesList[index].pauseSpawning = true;
		}
	}

	public void PauseSpawnAll()
	{
		StartCoroutine(PauseAllRoutine());
	}

	private IEnumerator SpawnOneOfAllRoutine()
	{
		for (int i = 0; i < spawnPropertiesList.Count; i++)
		{
			yield return waitBetweenSpawnProps;
			StartCoroutine(spawnPropertiesList[i].Spawn(this, null, forceSpawn: true));
		}
	}

	private IEnumerator StartAllRoutine()
	{
		for (int i = 0; i < spawnPropertiesList.Count; i++)
		{
			yield return waitBetweenSpawnProps;
			spawnPropertiesList[i].pauseSpawning = false;
		}
	}

	private IEnumerator PauseAllRoutine()
	{
		for (int i = 0; i < spawnPropertiesList.Count; i++)
		{
			yield return waitBetweenSpawnProps;
			spawnPropertiesList[i].pauseSpawning = true;
		}
	}

	private void OnAISpawned(vAIMotor ai, vAISpawnProperties spawnProperties)
	{
		StartCoroutine(spawnProperties.Spawn(this, OnAISpawned));
	}
}
