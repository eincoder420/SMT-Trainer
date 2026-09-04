using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public interface vIFSMBehaviourController
{
	Transform transform { get; }

	GameObject gameObject { get; }

	bool isStopped { get; set; }

	bool debugMode { get; set; }

	List<vFSMDebugObject> debugList { get; }

	vIControlAI aiController { get; set; }

	vFSMBehaviour fsmBehaviour { get; set; }

	vFSMState anyState { get; }

	vFSMState lastState { get; }

	vFSMState currentState { get; }

	int indexOffCurrentState { get; }

	string nameOffCurrentState { get; }

	vMessageReceiver messageReceiver { get; }

	bool HasTimer(string key);

	void RemoveTimer(string key);

	float GetTimer(string key);

	void SetTimer(string key, float timer);

	void ChangeState(vFSMState state);

	void ChangeBehaviour(vFSMBehaviour behaviour);

	void StartFSM();

	void StopFSM();

	void SendDebug(string message, Object sender = null);
}
