using Invector.vEventSystems;

namespace Invector.vCharacterController.AI;

public interface vIStateAttackListener
{
	void OnReceiveAttack(vIControlAICombat combatController, ref vDamage damage, vIMeleeFighter attacker, ref bool canBlock);
}
