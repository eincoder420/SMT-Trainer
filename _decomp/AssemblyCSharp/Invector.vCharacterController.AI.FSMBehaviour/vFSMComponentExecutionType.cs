using System;

namespace Invector.vCharacterController.AI.FSMBehaviour;

[Flags]
public enum vFSMComponentExecutionType
{
	OnStateUpdate = 1,
	OnStateEnter = 2,
	OnStateExit = 4
}
