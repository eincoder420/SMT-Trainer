using Invector.vCharacterController;
using UnityEngine;

namespace Invector.vEventSystems;

public interface vIMeleeFighter : vIAttackReceiver, vIAttackListener
{
	bool isAttacking { get; }

	bool isArmed { get; }

	bool isBlocking { get; }

	Transform transform { get; }

	GameObject gameObject { get; }

	vICharacter character { get; }

	void BreakAttack(int breakAtkID);

	void OnRecoil(int recoilID);
}
