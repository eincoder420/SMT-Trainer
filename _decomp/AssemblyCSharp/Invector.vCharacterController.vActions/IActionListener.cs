namespace Invector.vCharacterController.vActions;

public interface IActionListener : IActionEnterListener, IActionController, IActionExitListener, IActionStayListener
{
	bool actionEnter { get; set; }

	bool actionExit { get; set; }

	bool actionStay { get; set; }

	bool doingAction { get; set; }
}
