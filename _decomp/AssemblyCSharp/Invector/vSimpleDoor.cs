using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[RequireComponent(typeof(BoxCollider))]
[vClassHeader("Simple Door", true, "icon_v2", false, "", openClose = false)]
public class vSimpleDoor : vMonoBehaviour
{
	public enum DoorState
	{
		Closed,
		Opened,
		Closing,
		Opening
	}

	[vReadOnly(true)]
	public DoorState state;

	public Transform pivot;

	public bool startOpened;

	public bool autoOpen = true;

	public bool autoClose = true;

	[vHideInInspector("autoClose", false)]
	[Tooltip("Close the door only if door is completely opened\n**The TimeToClose will be used yet")]
	public bool closeOnlyWhenOpened;

	[Tooltip("Target angle of Opened door")]
	public float angleOfOpen = 90f;

	[vHideInInspector("autoOpen", false)]
	[Tooltip("Min angle between character forward and door that  can auto open")]
	public float minAngleToOpen = 45f;

	[Tooltip("Door can open to left side and to right side, if false, door will open just in to right side")]
	public bool openBothSide = true;

	public float closeSpeed = 2f;

	public float openSpeed = 2f;

	[vHideInInspector("autoClose", false)]
	[Tooltip("Time to auto close door after Opened")]
	public float timeToClose = 1f;

	[Tooltip("Used when autoOpen or autoClose is checked")]
	public vTagMask tagsToOpen = new List<string> { "Player" };

	private Vector3 currentAngle;

	private float angle;

	private bool _invertOpenSide;

	private Collider colliderInTrigger;

	public UnityEvent onStartOpen;

	public UnityEvent onStartOpenRight;

	public UnityEvent onStartOpenLeft;

	public UnityEvent onStartClose;

	public UnityEvent onOpen;

	public UnityEvent onOpenRight;

	public UnityEvent onOpenLeft;

	public UnityEvent onClose;

	private float targetDoorAngle;

	private bool stopDoor;

	protected virtual bool invertOpenSide
	{
		get
		{
			if (_invertOpenSide)
			{
				return openBothSide;
			}
			return false;
		}
	}

	protected virtual void Start()
	{
		if (!pivot)
		{
			base.enabled = false;
		}
		if (startOpened)
		{
			state = DoorState.Closed;
			Open();
		}
		else
		{
			onClose.Invoke();
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if ((bool)pivot)
		{
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawLine(base.transform.position, pivot.position);
			Gizmos.DrawSphere(pivot.position, 0.1f);
		}
	}

	public virtual void SetAutoOpen(bool value)
	{
		autoOpen = value;
	}

	public virtual void SetAutoClose(bool value)
	{
		autoClose = value;
	}

	public virtual void Open(bool invert)
	{
		_invertOpenSide = invert;
		Open();
	}

	public virtual void Open()
	{
		if (state != DoorState.Opening && state != DoorState.Opening)
		{
			targetDoorAngle = (invertOpenSide ? (0f - angleOfOpen) : angleOfOpen);
			StartCoroutine(HandleDoor());
		}
	}

	public virtual void Close()
	{
		if (state != DoorState.Closing && state != 0)
		{
			targetDoorAngle = 0f;
			StartCoroutine(HandleDoor());
		}
	}

	public virtual void ToggleOpenClose()
	{
		if (state == DoorState.Closed && state != DoorState.Opening)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	protected virtual IEnumerator HandleDoor()
	{
		bool open = Mathf.Abs(targetDoorAngle).Equals(angleOfOpen);
		state = (open ? DoorState.Opening : DoorState.Closing);
		switch (state)
		{
		case DoorState.Opening:
			onStartOpen.Invoke();
			if (invertOpenSide)
			{
				onStartOpenLeft.Invoke();
			}
			else
			{
				onStartOpenRight.Invoke();
			}
			break;
		case DoorState.Closing:
			onStartClose.Invoke();
			break;
		}
		stopDoor = true;
		yield return new WaitForEndOfFrame();
		stopDoor = false;
		while (!stopDoor)
		{
			currentAngle.y = Mathf.MoveTowardsAngle(currentAngle.y, targetDoorAngle, open ? openSpeed : closeSpeed);
			if (Mathf.Abs(currentAngle.y - targetDoorAngle) < 0.01f)
			{
				currentAngle.y = targetDoorAngle;
				pivot.localEulerAngles = currentAngle;
				break;
			}
			pivot.localEulerAngles = currentAngle;
			yield return null;
		}
		if (stopDoor)
		{
			yield break;
		}
		state = (open ? DoorState.Opened : DoorState.Closed);
		if (open && autoClose && !colliderInTrigger)
		{
			CloseWithDelay();
		}
		switch (state)
		{
		case DoorState.Opened:
			onOpen.Invoke();
			if (invertOpenSide)
			{
				onOpenLeft.Invoke();
			}
			else
			{
				onOpenRight.Invoke();
			}
			break;
		case DoorState.Closed:
			onClose.Invoke();
			break;
		}
	}

	protected virtual void OnTriggerStay(Collider other)
	{
		if (!tagsToOpen.Contains(other.tag) || !autoOpen || (state != DoorState.Closing && state != 0))
		{
			return;
		}
		if (base.transform.InverseTransformPoint(other.transform.position).z > 0f)
		{
			_invertOpenSide = false;
		}
		else
		{
			_invertOpenSide = true;
		}
		angle = Mathf.Abs(Vector3.Angle(_invertOpenSide ? base.transform.forward : (-base.transform.forward), other.transform.forward));
		if (angle < minAngleToOpen)
		{
			if (!colliderInTrigger)
			{
				colliderInTrigger = other;
			}
			Open();
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (autoClose && tagsToOpen.Contains(other.tag) && ((colliderInTrigger != null && colliderInTrigger.gameObject.Equals(other.gameObject)) || colliderInTrigger == null))
		{
			colliderInTrigger = null;
			if (!closeOnlyWhenOpened || state == DoorState.Opened)
			{
				CloseWithDelay();
			}
		}
	}

	protected virtual void CloseWithDelay()
	{
		CancelInvoke("Close");
		Invoke("Close", timeToClose);
	}
}
