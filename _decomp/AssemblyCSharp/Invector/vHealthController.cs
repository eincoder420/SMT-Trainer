using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("HealthController", true, "icon_v2", false, "", iconName = "HealthControllerIcon")]
public class vHealthController : vMonoBehaviour, vIHealthController, vIDamageReceiver
{
	[Serializable]
	public class CheckHealthEvent
	{
		public enum HealthCompare
		{
			Equals,
			HigherThan,
			LessThan
		}

		public int healthToCheck;

		public bool disableEventOnCheck;

		public HealthCompare healthCompare;

		public UnityEvent OnCheckHealth;
	}

	[Serializable]
	public class ValueChangedEvent : UnityEvent<float>
	{
	}

	[vEditorToolbar("Health", false, "", false, false, order = 0)]
	[SerializeField]
	[vReadOnly(true)]
	protected bool _isDead;

	[vBarDisplay("maxHealth", false)]
	[SerializeField]
	protected float _currentHealth;

	public bool isImmortal;

	[vHelpBox("If you want to start with different value, uncheck this and make sure that the current health has a value greater zero", vHelpBoxAttribute.MessageType.None)]
	public bool fillHealthOnStart = true;

	public int maxHealth = 100;

	public float healthRecovery;

	public float healthRecoveryDelay;

	[HideInInspector]
	public float currentHealthRecoveryDelay;

	[vEditorToolbar("Events", false, "", false, false, order = 100)]
	public List<CheckHealthEvent> checkHealthEvents = new List<CheckHealthEvent>();

	[SerializeField]
	protected OnReceiveDamage _onStartReceiveDamage = new OnReceiveDamage();

	[SerializeField]
	protected OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();

	[SerializeField]
	protected OnDead _onDead = new OnDead();

	public ValueChangedEvent onChangeHealth;

	public UnityEvent onResetHealth;

	internal bool inHealthRecovery;

	public int MaxHealth
	{
		get
		{
			return maxHealth;
		}
		protected set
		{
			maxHealth = value;
		}
	}

	public float currentHealth
	{
		get
		{
			return _currentHealth;
		}
		protected set
		{
			if (_currentHealth != value)
			{
				_currentHealth = value;
				onChangeHealth.Invoke(_currentHealth);
			}
			if (!_isDead && _currentHealth <= 0f)
			{
				_isDead = true;
				onDead.Invoke(base.gameObject);
			}
			else if (isDead && _currentHealth > 0f)
			{
				_isDead = false;
			}
		}
	}

	public bool isDead
	{
		get
		{
			if (!_isDead && currentHealth <= 0f)
			{
				_isDead = true;
				onDead.Invoke(base.gameObject);
			}
			return _isDead;
		}
		set
		{
			_isDead = value;
		}
	}

	public OnReceiveDamage onStartReceiveDamage
	{
		get
		{
			return _onStartReceiveDamage;
		}
		protected set
		{
			_onStartReceiveDamage = value;
		}
	}

	public OnReceiveDamage onReceiveDamage
	{
		get
		{
			return _onReceiveDamage;
		}
		protected set
		{
			_onReceiveDamage = value;
		}
	}

	public OnDead onDead
	{
		get
		{
			return _onDead;
		}
		protected set
		{
			_onDead = value;
		}
	}

	protected virtual bool canRecoverHealth
	{
		get
		{
			if (currentHealth >= 0f && healthRecovery > 0f)
			{
				return currentHealth < (float)maxHealth;
			}
			return false;
		}
	}

	protected virtual void Start()
	{
		if (fillHealthOnStart)
		{
			currentHealth = maxHealth;
		}
		currentHealthRecoveryDelay = healthRecoveryDelay;
	}

	protected virtual IEnumerator RecoverHealth()
	{
		inHealthRecovery = true;
		while (canRecoverHealth && !isDead)
		{
			HealthRecovery();
			yield return null;
		}
		inHealthRecovery = false;
	}

	protected virtual void HealthRecovery()
	{
		if (!canRecoverHealth || isDead)
		{
			return;
		}
		if (currentHealthRecoveryDelay > 0f)
		{
			currentHealthRecoveryDelay -= Time.deltaTime;
			return;
		}
		if (currentHealth > (float)maxHealth)
		{
			currentHealth = maxHealth;
		}
		if (currentHealth < (float)maxHealth)
		{
			currentHealth += healthRecovery * Time.deltaTime;
		}
	}

	public virtual void AddHealth(int value)
	{
		currentHealth += value;
		currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
		if (!isDead && currentHealth <= 0f)
		{
			isDead = true;
			onDead.Invoke(base.gameObject);
		}
		HandleCheckHealthEvents();
	}

	public virtual void ChangeHealth(int value)
	{
		currentHealth = value;
		currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
		if (!isDead && currentHealth <= 0f)
		{
			isDead = true;
			onDead.Invoke(base.gameObject);
		}
		HandleCheckHealthEvents();
	}

	public virtual void ResetHealth(float health)
	{
		currentHealth = health;
		onResetHealth.Invoke();
		if (isDead)
		{
			isDead = false;
		}
	}

	public virtual void ResetHealth()
	{
		currentHealth = maxHealth;
		onResetHealth.Invoke();
		if (isDead)
		{
			isDead = false;
		}
	}

	public virtual void ChangeMaxHealth(int value)
	{
		maxHealth += value;
		if (maxHealth < 0)
		{
			maxHealth = 0;
		}
	}

	public virtual void SetHealthRecovery(float value)
	{
		healthRecovery = value;
		StartCoroutine(RecoverHealth());
	}

	public virtual void TakeDamage(vDamage damage)
	{
		if (damage != null)
		{
			onStartReceiveDamage.Invoke(damage);
			currentHealthRecoveryDelay = ((currentHealth <= 0f) ? 0f : healthRecoveryDelay);
			if (currentHealth > 0f && !isImmortal)
			{
				currentHealth -= damage.damageValue;
			}
			if (damage.damageValue > 0)
			{
				onReceiveDamage.Invoke(damage);
			}
			HandleCheckHealthEvents();
		}
	}

	protected virtual void HandleCheckHealthEvents()
	{
		List<CheckHealthEvent> list = checkHealthEvents.FindAll((CheckHealthEvent e) => (e.healthCompare == CheckHealthEvent.HealthCompare.Equals && currentHealth.Equals(e.healthToCheck)) || (e.healthCompare == CheckHealthEvent.HealthCompare.HigherThan && currentHealth > (float)e.healthToCheck) || (e.healthCompare == CheckHealthEvent.HealthCompare.LessThan && currentHealth < (float)e.healthToCheck));
		for (int i = 0; i < list.Count; i++)
		{
			list[i].OnCheckHealth.Invoke();
		}
		if (currentHealth < (float)maxHealth && base.gameObject.activeInHierarchy && !inHealthRecovery)
		{
			StartCoroutine(RecoverHealth());
		}
	}

	[SpecialName]
	Transform vIDamageReceiver.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIDamageReceiver.get_gameObject()
	{
		return base.gameObject;
	}
}
