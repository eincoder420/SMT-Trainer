using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Invector.vCharacterController.vActions;

public abstract class vActionListener : vMonoBehaviour, IActionListener, IActionEnterListener, IActionController, IActionExitListener, IActionStayListener
{
	[vEditorToolbar("Events", false, "", false, false, order = 10)]
	public vOnActionHandle OnDoAction = new vOnActionHandle();

	public bool actionEnter { get; set; }

	public bool actionExit { get; set; }

	public bool actionStay { get; set; }

	public bool doingAction { get; set; }

	protected virtual void Awake()
	{
		SetUpListener();
	}

	protected virtual void SetUpListener()
	{
		actionEnter = true;
		actionExit = true;
		actionStay = true;
	}

	protected virtual void Start()
	{
		IActionReceiver[] components = GetComponents<IActionReceiver>();
		for (int i = 0; i < components.Length; i++)
		{
			OnDoAction.AddListener(components[i].OnReceiveAction);
		}
	}

	public virtual void OnActionEnter(Collider other)
	{
	}

	public virtual void OnActionStay(Collider other)
	{
	}

	public virtual void OnActionExit(Collider other)
	{
	}

	[SpecialName]
	bool IActionController.get_enabled()
	{
		return base.enabled;
	}

	[SpecialName]
	void IActionController.set_enabled(bool value)
	{
		base.enabled = value;
	}

	[SpecialName]
	GameObject IActionController.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IActionController.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	string IActionController.get_name()
	{
		return base.name;
	}

	Type IActionController.GetType()
	{
		return GetType();
	}
}
