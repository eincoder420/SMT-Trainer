using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("Damage Modifier Controller", true, "icon_v2", false, "", openClose = false, useHelpBox = true, helpBoxText = "Needs a HealthController component")]
public class vDamageModifierController : vMonoBehaviour
{
	public enum GetHealthControllerMethod
	{
		GetComponent,
		GetComponentInParent,
		GetComponentInChildren
	}

	[vReadOnly(true)]
	public bool isInit;

	[SerializeField]
	protected GetHealthControllerMethod getHealthMethod;

	[Tooltip("Modifier List")]
	public List<vDamageModifier> modifiers;

	public UnityEvent onAllModifiersIsBroken;

	protected vIHealthController healthController;

	protected virtual void Awake()
	{
		Init();
	}

	protected void Init()
	{
		GetHealthController();
		if (healthController != null)
		{
			AddDamageEvent();
			InitModifiers();
			isInit = true;
		}
	}

	protected virtual void InitModifiers()
	{
		for (int i = 0; i < modifiers.Count; i++)
		{
			modifiers[i].ResetModifier();
			modifiers[i].onBroken.AddListener(delegate
			{
				CheckBrokedModifiers();
			});
		}
	}

	protected virtual void AddDamageEvent()
	{
		RemoveDamageEvent();
		healthController.onStartReceiveDamage.AddListener(ApplyModifiers);
	}

	protected virtual void RemoveDamageEvent()
	{
		healthController.onStartReceiveDamage.RemoveListener(ApplyModifiers);
	}

	protected virtual void GetHealthController()
	{
		switch (getHealthMethod)
		{
		case GetHealthControllerMethod.GetComponent:
			healthController = GetComponent<vIHealthController>();
			break;
		case GetHealthControllerMethod.GetComponentInChildren:
			healthController = GetComponentInChildren<vIHealthController>();
			break;
		case GetHealthControllerMethod.GetComponentInParent:
			healthController = GetComponentInParent<vIHealthController>();
			break;
		}
	}

	protected virtual void OnEnable()
	{
		if (isInit)
		{
			AddDamageEvent();
		}
	}

	protected virtual void OnDisable()
	{
		if (isInit)
		{
			RemoveDamageEvent();
		}
	}

	protected virtual void CheckBrokedModifiers()
	{
		if (!modifiers.Exists((vDamageModifier m) => !m.isBroken))
		{
			onAllModifiersIsBroken.Invoke();
		}
	}

	protected virtual void ApplyModifiers(vDamage damage)
	{
		for (int i = 0; i < modifiers.Count; i++)
		{
			modifiers[i].ApplyModifier(damage);
		}
	}

	public void ResetAllModifiers()
	{
		for (int i = 0; i < modifiers.Count; i++)
		{
			modifiers[i].ResetModifier();
		}
	}
}
