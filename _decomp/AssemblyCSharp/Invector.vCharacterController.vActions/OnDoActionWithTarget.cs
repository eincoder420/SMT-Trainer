using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions;

[Serializable]
public class OnDoActionWithTarget : UnityEvent<GameObject>
{
}
