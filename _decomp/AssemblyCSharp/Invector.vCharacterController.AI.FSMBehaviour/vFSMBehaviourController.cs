using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI.FSMBehaviour;

[vClassHeader(" FSM BEHAVIOUR CONTROLLER", true, "icon_v2", false, "", helpBoxText = "Required a AI Controller Component", useHelpBox = true, iconName = "Textures/Editor/FSMIcon2")]
public class vFSMBehaviourController : vMonoBehaviour, vIFSMBehaviourController
{
	[Serializable]
	public class FSMStateEvent : UnityEvent<vFSMState>
	{
	}

	[Serializable]
	public class FSMBehaviourEvent : UnityEvent<vFSMBehaviour>
	{
	}

	[vEditorToolbar("FSM", false, "", false, false)]
	[SerializeField]
	protected vFSMBehaviour _fsmBehaviour;

	[SerializeField]
	protected bool _stop;

	[SerializeField]
	protected bool _debugMode;

	public UnityEvent onStartFSM;

	public UnityEvent onPauseFSM;

	public UnityEvent onResetFSM;

	public FSMStateEvent onStateEnter;

	public FSMStateEvent onStateExit;

	public FSMBehaviourEvent onChangeBehaviour;

	private Dictionary<string, float> _timers = new Dictionary<string, float>();

	private vFSMState _currentState;

	private vFSMState _lastState;

	private bool inChangeState;

	private vMessageReceiver _messageReceiver;

	private bool tryGetMessageReceiver;

	public virtual vFSMBehaviour fsmBehaviour
	{
		get
		{
			return _fsmBehaviour;
		}
		set
		{
			_fsmBehaviour = value;
		}
	}

	public virtual bool debugMode
	{
		get
		{
			return _debugMode;
		}
		set
		{
			_debugMode = value;
		}
	}

	public virtual bool isStopped
	{
		get
		{
			return _stop;
		}
		set
		{
			_stop = value;
		}
	}

	public virtual vIControlAI aiController { get; set; }

	public virtual int indexOffCurrentState
	{
		get
		{
			if (!currentState || !_fsmBehaviour)
			{
				return -1;
			}
			return _fsmBehaviour.states.IndexOf(currentState);
		}
	}

	public virtual string nameOffCurrentState
	{
		get
		{
			if (!currentState)
			{
				return string.Empty;
			}
			return currentState.Name;
		}
	}

	public virtual List<vFSMDebugObject> debugList { get; protected set; }

	public virtual vFSMState anyState
	{
		get
		{
			if (_fsmBehaviour.states.Count <= 1)
			{
				return null;
			}
			return _fsmBehaviour.states[1];
		}
	}

	public virtual vFSMState currentState
	{
		get
		{
			return _currentState;
		}
		protected set
		{
			_currentState = value;
		}
	}

	public virtual vFSMState lastState
	{
		get
		{
			return _lastState;
		}
		protected set
		{
			_lastState = value;
		}
	}

	public vMessageReceiver messageReceiver
	{
		get
		{
			if (_messageReceiver == null && !tryGetMessageReceiver)
			{
				_messageReceiver = GetComponent<vMessageReceiver>();
			}
			if (_messageReceiver == null && !tryGetMessageReceiver)
			{
				tryGetMessageReceiver = true;
			}
			return _messageReceiver;
		}
	}

	protected virtual void Start()
	{
		aiController = GetComponent<vIControlAI>();
	}

	protected virtual void Update()
	{
		if (aiController != null && !aiController.isDead && !isStopped)
		{
			UpdateStates();
		}
	}

	protected virtual void UpdateStates()
	{
		if ((bool)currentState)
		{
			if (!inChangeState)
			{
				currentState.UpdateState(this);
				UpdateAnyState();
			}
		}
		else
		{
			Entry();
		}
	}

	public virtual void ResetFSM()
	{
		if ((bool)currentState)
		{
			currentState.OnStateExit(this);
		}
		onResetFSM.Invoke();
		currentState = null;
	}

	protected virtual void Entry()
	{
		if ((bool)_fsmBehaviour)
		{
			if (_fsmBehaviour.states.Count > 1)
			{
				currentState = _fsmBehaviour.states[0];
				currentState.OnStateEnter(this);
			}
			else if (currentState != null)
			{
				currentState = null;
			}
		}
	}

	protected virtual void UpdateAnyState()
	{
		if ((bool)currentState && (bool)_fsmBehaviour && _fsmBehaviour.states.Count > 1)
		{
			_fsmBehaviour.states[1].UpdateState(this);
		}
	}

	public virtual void SendDebug(string message, UnityEngine.Object sender = null)
	{
		if (debugList == null)
		{
			debugList = new List<vFSMDebugObject>();
		}
		if (debugList.Exists((vFSMDebugObject d) => d.sender == sender))
		{
			debugList.Find((vFSMDebugObject d) => d.sender == sender).message = message;
		}
		else
		{
			debugList.Add(new vFSMDebugObject(message, sender));
		}
	}

	public virtual bool HasTimer(string key)
	{
		return _timers.ContainsKey(key);
	}

	public virtual void RemoveTimer(string key)
	{
		if (_timers.ContainsKey(key))
		{
			_timers.Remove(key);
		}
	}

	public virtual float GetTimer(string key)
	{
		if (!_timers.ContainsKey(key))
		{
			_timers.Add(key, 0f);
		}
		if (_timers.ContainsKey(key))
		{
			if (debugMode)
			{
				SendDebug("<color=yellow>Get Timer " + key + " = " + _timers[key].ToString("0.0") + " </color> ", base.gameObject);
			}
			return _timers[key];
		}
		return 0f;
	}

	public virtual void SetTimer(string key, float value)
	{
		if (!_timers.ContainsKey(key))
		{
			_timers.Add(key, value);
		}
		else if (_timers.ContainsKey(key))
		{
			_timers[key] = value;
		}
		if (debugMode)
		{
			SendDebug("<color=yellow>Set " + key + " Timer to " + value.ToString("0.0") + " </color> ", base.gameObject);
		}
	}

	public virtual void ChangeState(vFSMState state)
	{
		if (!state || !(state != currentState) || inChangeState)
		{
			return;
		}
		inChangeState = true;
		_lastState = currentState;
		currentState = null;
		if ((bool)_lastState)
		{
			if (debugMode)
			{
				SendDebug("<color=red>EXIT:" + _lastState.name + "</color>  <color=yellow> ENTER :" + state.Name + " </color> ", base.gameObject);
			}
			_lastState.OnStateExit(this);
			onStateExit.Invoke(_lastState);
		}
		currentState = state;
		state.OnStateEnter(this);
		inChangeState = false;
		onStateExit.Invoke(state);
	}

	public virtual void ChangeBehaviour(vFSMBehaviour behaviour)
	{
		if (_fsmBehaviour != behaviour)
		{
			inChangeState = true;
			_fsmBehaviour = behaviour;
			currentState = null;
			ResetFSM();
			if (debugMode)
			{
				SendDebug("CHANGE BEHAVIOUR TO " + behaviour.name);
			}
			inChangeState = false;
			onChangeBehaviour.Invoke(_fsmBehaviour);
		}
	}

	public virtual void StartFSM()
	{
		isStopped = false;
		onStartFSM.Invoke();
	}

	public virtual void StopFSM()
	{
		isStopped = true;
		onPauseFSM.Invoke();
	}

	[SpecialName]
	Transform vIFSMBehaviourController.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIFSMBehaviourController.get_gameObject()
	{
		return base.gameObject;
	}
}
