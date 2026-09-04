namespace Invector.vCharacterController;

public interface vIAnimatorMoveReceiver
{
	bool enabled { get; set; }

	void OnAnimatorMoveEvent();
}
