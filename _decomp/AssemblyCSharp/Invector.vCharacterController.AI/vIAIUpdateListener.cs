namespace Invector.vCharacterController.AI;

public interface vIAIUpdateListener : vIAIComponent
{
	void OnStart(vIControlAI controller);

	void OnUpdate(vIControlAI controller);

	void OnPause(vIControlAI controller);
}
