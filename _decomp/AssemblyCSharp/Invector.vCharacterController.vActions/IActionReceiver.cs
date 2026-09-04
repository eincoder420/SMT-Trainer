namespace Invector.vCharacterController.vActions;

public interface IActionReceiver : IActionController
{
	void OnReceiveAction(vTriggerGenericAction actionInfo);
}
