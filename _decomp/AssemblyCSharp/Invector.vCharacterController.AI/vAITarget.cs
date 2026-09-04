using System;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[Serializable]
public class vAITarget
{
	[SerializeField]
	protected Transform _transform;

	[SerializeField]
	[HideInInspector]
	protected Collider _collider;

	public vIHealthController healthController;

	public vIControlAICombat combateController;

	public vIMeleeFighter meleeFighter;

	public vICharacter character;

	public bool isFixedTarget = true;

	[HideInInspector]
	public bool isLost;

	[HideInInspector]
	public bool _hadHealthController;

	public Transform transform
	{
		get
		{
			return _transform;
		}
		protected set
		{
			_transform = value;
		}
	}

	public Collider collider
	{
		get
		{
			return _collider;
		}
		protected set
		{
			_collider = value;
		}
	}

	public bool hasCollider => collider != null;

	public bool hasHealthController
	{
		get
		{
			if (_hadHealthController && healthController == null)
			{
				transform = null;
			}
			return healthController != null;
		}
	}

	public bool isDead
	{
		get
		{
			bool result = true;
			if (hasHealthController)
			{
				result = healthController.isDead;
			}
			else if (_hadHealthController)
			{
				result = true;
			}
			else if (!transform.gameObject.activeInHierarchy)
			{
				result = true;
			}
			else if ((bool)_collider)
			{
				result = !_collider.enabled;
			}
			return result;
		}
	}

	public bool isArmed
	{
		get
		{
			if (!isFighter)
			{
				return false;
			}
			if (meleeFighter == null)
			{
				if (combateController == null)
				{
					return false;
				}
				return combateController.isArmed;
			}
			return meleeFighter.isArmed;
		}
	}

	public bool isBlocking
	{
		get
		{
			if (!isFighter)
			{
				return false;
			}
			if (meleeFighter == null)
			{
				if (combateController == null)
				{
					return false;
				}
				return combateController.isBlocking;
			}
			return meleeFighter.isBlocking;
		}
	}

	public bool isAttacking
	{
		get
		{
			if (!isFighter)
			{
				return false;
			}
			if (meleeFighter == null)
			{
				if (combateController == null)
				{
					return false;
				}
				return combateController.isAttacking;
			}
			return meleeFighter.isAttacking;
		}
	}

	public bool isFighter
	{
		get
		{
			if (meleeFighter == null)
			{
				return combateController != null;
			}
			return true;
		}
	}

	public bool isCharacter => character != null;

	public float currentHealth
	{
		get
		{
			if (hasHealthController)
			{
				return healthController.currentHealth;
			}
			return 0f;
		}
	}

	public static implicit operator Transform(vAITarget m)
	{
		try
		{
			return m.transform;
		}
		catch
		{
			return null;
		}
	}

	public void InitTarget(Transform target)
	{
		if ((bool)target)
		{
			transform = target;
			collider = transform.GetComponent<Collider>();
			healthController = transform.GetComponent<vIHealthController>();
			_hadHealthController = healthController != null;
			meleeFighter = transform.GetComponent<vIMeleeFighter>();
			character = transform.GetComponent<vICharacter>();
			combateController = transform.GetComponent<vIControlAICombat>();
		}
	}

	public void ClearTarget()
	{
		transform = null;
		collider = null;
		healthController = null;
		meleeFighter = null;
		character = null;
		combateController = null;
	}
}
