using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public sealed class vFSMBehaviour : ScriptableObject
{
	public struct TransitionPreview
	{
		public Rect transitionRect;

		public bool? sideRight;

		public Action<vFSMState> onValidate;

		public vFSMState state;
	}

	[HideInInspector]
	[SerializeField]
	public vFSMState selectedNode;

	[HideInInspector]
	[SerializeField]
	public bool wantConnection;

	[HideInInspector]
	[SerializeField]
	public TransitionPreview transitionPreview;

	[HideInInspector]
	[SerializeField]
	public vFSMState connectionNode;

	[HideInInspector]
	[SerializeField]
	public bool showProperties;

	[HideInInspector]
	[SerializeField]
	public List<vFSMState> states;

	[HideInInspector]
	[SerializeField]
	public Vector2 panOffset;

	[HideInInspector]
	[SerializeField]
	public bool overNode;

	public string graphName
	{
		get
		{
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	private void OnEnable()
	{
		if (states == null)
		{
			states = new List<vFSMState>();
		}
	}
}
