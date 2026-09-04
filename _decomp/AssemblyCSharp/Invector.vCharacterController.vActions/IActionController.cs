using System;
using UnityEngine;

namespace Invector.vCharacterController.vActions;

public interface IActionController
{
	bool enabled { get; set; }

	GameObject gameObject { get; }

	Transform transform { get; }

	string name { get; }

	new Type GetType();
}
