using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine;

namespace Invector;

public static class vCharacterExtensions
{
	public static void LoadActionControllers(this vCharacter character, bool debug = false)
	{
		IActionController[] components = character.GetComponents<IActionController>();
		for (int i = 0; i < components.Length; i++)
		{
			if (!components[i].enabled)
			{
				continue;
			}
			if (components[i] is IActionListener)
			{
				IActionListener actionListener = components[i] as IActionListener;
				if (actionListener.actionEnter)
				{
					character.onActionEnter.RemoveListener(actionListener.OnActionEnter);
					character.onActionEnter.AddListener(actionListener.OnActionEnter);
					if (debug)
					{
						Debug.Log("Register Action Enter event to the " + actionListener.GetType().Name);
					}
				}
				if (actionListener.actionStay)
				{
					character.onActionStay.RemoveListener(actionListener.OnActionStay);
					character.onActionStay.AddListener(actionListener.OnActionStay);
					if (debug)
					{
						Debug.Log("Register Action Stay event to the " + actionListener.GetType().Name);
					}
				}
				if (actionListener.actionExit)
				{
					character.onActionExit.RemoveListener(actionListener.OnActionExit);
					character.onActionExit.AddListener(actionListener.OnActionExit);
					if (debug)
					{
						Debug.Log("Register action Exit event to the " + actionListener.GetType().Name);
					}
				}
				continue;
			}
			if (components[i] is IActionEnterListener)
			{
				character.onActionEnter.RemoveListener((components[i] as IActionEnterListener).OnActionEnter);
				character.onActionEnter.AddListener((components[i] as IActionEnterListener).OnActionEnter);
				if (debug)
				{
					Debug.Log("Register Action Enter event to the " + components[i].GetType().Name);
				}
			}
			if (components[i] is IActionStayListener)
			{
				character.onActionStay.RemoveListener((components[i] as IActionStayListener).OnActionStay);
				character.onActionStay.AddListener((components[i] as IActionStayListener).OnActionStay);
				if (debug)
				{
					Debug.Log("Register Action Stay event to the " + components[i].GetType().Name);
				}
			}
			if (components[i] is IActionExitListener)
			{
				character.onActionExit.RemoveListener((components[i] as IActionExitListener).OnActionExit);
				character.onActionExit.AddListener((components[i] as IActionExitListener).OnActionExit);
				if (debug)
				{
					Debug.Log("Register Action Exit event to the " + components[i].GetType().Name);
				}
			}
		}
	}
}
