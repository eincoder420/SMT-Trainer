using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[DisallowMultipleComponent]
[vClassHeader("AI Noise Listener", true, "icon_v2", false, "")]
public class vAINoiseListener : vMonoBehaviour, vIAIComponent
{
	[vHelpBox("The noise has a radius effect and the noise volume decreases depending on the distance, 'Listener Power'  will applify the distance of the noise to listener", vHelpBoxAttribute.MessageType.None)]
	[Range(0f, 10f)]
	public float listenerPower = 1f;

	public bool debugMode;

	public List<string> ignoreNoiseType;

	protected List<vNoise> _ListenerdNoises;

	public virtual Type ComponentType => typeof(vAINoiseListener);

	public virtual List<vNoise> ListenerdNoises
	{
		get
		{
			if (_ListenerdNoises == null)
			{
				_ListenerdNoises = new List<vNoise>();
			}
			return _ListenerdNoises;
		}
		protected set
		{
			_ListenerdNoises = value;
		}
	}

	public virtual vNoise LastListenedNoise { get; protected set; }

	protected virtual void OnEnable()
	{
		vAINoiseManager.Instance.AddListener(this);
	}

	protected virtual void OnDisable()
	{
		vAINoiseManager.Instance.RemoveListener(this);
	}

	protected virtual void OnDestroy()
	{
		try
		{
			vAINoiseManager.Instance.RemoveListener(this);
		}
		catch
		{
		}
	}

	protected virtual bool IsInListenerPower(vNoise noise)
	{
		if (ignoreNoiseType.Contains(noise.noiseType))
		{
			return false;
		}
		return NoiseVolume(noise) > 0f;
	}

	protected virtual List<vNoise> SortByDistance()
	{
		if (ListenerdNoises.Count > 1)
		{
			ListenerdNoises.Sort((vNoise noiseA, vNoise noiseB) => Vector3.Distance(base.transform.position, noiseA.position).CompareTo(Vector3.Distance(base.transform.position, noiseB.position)));
		}
		if (ListenerdNoises.Count > 0)
		{
			LastListenedNoise = ListenerdNoises[0];
		}
		return ListenerdNoises;
	}

	protected virtual List<vNoise> GetNoiseByType(string noiseType)
	{
		List<vNoise> list = ListenerdNoises.FindAll((vNoise n) => n.noiseType.Equals(noiseType));
		if (list.Count > 0)
		{
			LastListenedNoise = list[0];
		}
		return list;
	}

	protected virtual List<vNoise> GetNoiseByType(List<string> noiseTypes)
	{
		List<vNoise> list = ListenerdNoises.FindAll((vNoise n) => noiseTypes.Contains(n.noiseType));
		if (list.Count > 0)
		{
			LastListenedNoise = list[0];
		}
		return list;
	}

	public virtual float NoiseVolume(vNoise noise)
	{
		float num = 0f;
		if (listenerPower > 0f)
		{
			float num2 = noise.minDistance * listenerPower;
			float num3 = noise.maxDistance * listenerPower;
			float num4 = Vector3.Distance(noise.position, base.transform.position) - num2;
			num = 1f - num4 / ((num2 == num3) ? num3 : ((num2 > num3) ? (num2 - num3) : (num3 - num2)));
		}
		return noise.volume * num;
	}

	public virtual bool IsListeningNoise()
	{
		if (ListenerdNoises == null)
		{
			ListenerdNoises = new List<vNoise>();
		}
		return ListenerdNoises.Count > 0;
	}

	public virtual bool IsListeningNoise(out vNoise noise)
	{
		List<vNoise> list = SortByDistance();
		if (list.Count > 0)
		{
			noise = list[0];
			return true;
		}
		noise = null;
		return false;
	}

	public virtual bool IsListeningSpecificNoises(List<string> noiseTypes)
	{
		return GetNoiseByType(noiseTypes).Count > 0;
	}

	public virtual bool IsListeningSpecificNoises(List<string> noiseTypes, out vNoise noise)
	{
		List<vNoise> noiseByType = GetNoiseByType(noiseTypes);
		if (noiseByType.Count > 0)
		{
			noise = noiseByType[0];
			return true;
		}
		noise = null;
		return false;
	}

	public virtual vNoise GetNearNoise()
	{
		if (SortByDistance().Count > 0)
		{
			return ListenerdNoises[0];
		}
		return null;
	}

	public virtual vNoise GetNearNoiseByType(string noiseType)
	{
		if (GetNoiseByType(noiseType).Count > 0)
		{
			return ListenerdNoises[0];
		}
		return null;
	}

	public virtual vNoise GetNearNoiseByTypes(List<string> noiseTypes)
	{
		if (GetNoiseByType(noiseTypes).Count > 0)
		{
			return ListenerdNoises[0];
		}
		return null;
	}

	public virtual List<vNoise> GetNoiseByTypes(List<string> noiseTypes)
	{
		if (GetNoiseByType(noiseTypes).Count > 0)
		{
			return ListenerdNoises;
		}
		return null;
	}

	public virtual bool IsClosestListenerToNoise(vNoise noise)
	{
		return (from l in vAINoiseManager.Instance.noiseListeners.FindAll((vAINoiseListener l) => l.ListenerdNoises.Contains(noise))
			orderby (l.transform.position - noise.position).magnitude
			select l).ToList()[0].Equals(this);
	}

	public virtual void AddNoise(vNoise noise)
	{
		if (ListenerdNoises == null)
		{
			ListenerdNoises = new List<vNoise>();
		}
		if (IsInListenerPower(noise))
		{
			if (ListenerdNoises.Contains(noise))
			{
				ListenerdNoises[ListenerdNoises.IndexOf(noise)].AddDuration(noise.duration);
				ListenerdNoises = SortByDistance();
			}
			else
			{
				noise.onFinishNoise.AddListener(RemoveNoise);
				ListenerdNoises.Add(noise);
				ListenerdNoises = SortByDistance();
			}
		}
	}

	public virtual void RemoveNoise(vNoise noise)
	{
		if (ListenerdNoises == null)
		{
			ListenerdNoises = new List<vNoise>();
		}
		if (ListenerdNoises.Contains(noise))
		{
			ListenerdNoises.Remove(noise);
			ListenerdNoises = SortByDistance();
		}
	}
}
