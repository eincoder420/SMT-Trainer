using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector;

[vClassHeader("HITDAMAGE PARTICLE", "Default hit Particle to instantiate every time you receive damage and Custom hit Particle to instantiate based on a custom DamageType that comes from the MeleeControl Behaviour (AnimatorController)")]
public class vHitDamageParticle : vMonoBehaviour
{
	public List<GameObject> defaultDamageEffects = new List<GameObject>();

	public List<vDamageEffect> customDamageEffects = new List<vDamageEffect>();

	private vFisherYatesRandom _random;

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		vHealthController component = GetComponent<vHealthController>();
		if (component != null)
		{
			component.onReceiveDamage.AddListener(OnReceiveDamage);
		}
	}

	public void OnReceiveDamage(vDamage damage)
	{
		Vector3 vector = damage.hitPosition - new Vector3(base.transform.position.x, damage.hitPosition.y, base.transform.position.z);
		Quaternion rotation = ((vector != Vector3.zero) ? Quaternion.LookRotation(vector) : base.transform.rotation);
		if (damage.damageValue > 0)
		{
			TriggerEffect(new vDamageEffectInfo(new Vector3(base.transform.position.x, damage.hitPosition.y, base.transform.position.z), rotation, damage.damageType, damage.receiver));
		}
	}

	private void TriggerEffect(vDamageEffectInfo damageEffectInfo)
	{
		if (_random == null)
		{
			_random = new vFisherYatesRandom();
		}
		vDamageEffect vDamageEffect2 = customDamageEffects.Find((vDamageEffect effect) => effect.damageType.Equals(damageEffectInfo.damageType));
		if (vDamageEffect2 != null)
		{
			vDamageEffect2.onTriggerEffect.Invoke();
			if (vDamageEffect2.customDamageEffect != null && vDamageEffect2.customDamageEffect.Count > 0)
			{
				GameObject gameObject = vDamageEffect2.customDamageEffect[_random.Next(vDamageEffect2.customDamageEffect.Count)];
				Object.Instantiate(gameObject, damageEffectInfo.position, vDamageEffect2.rotateToHitDirection ? damageEffectInfo.rotation : gameObject.transform.rotation, (vDamageEffect2.attachInReceiver && (bool)damageEffectInfo.receiver) ? damageEffectInfo.receiver : vObjectContainer.root);
			}
		}
		else if (defaultDamageEffects.Count > 0 && damageEffectInfo != null)
		{
			Object.Instantiate(defaultDamageEffects[_random.Next(defaultDamageEffects.Count)], damageEffectInfo.position, damageEffectInfo.rotation, vObjectContainer.root);
		}
	}

	private void Reset()
	{
		defaultDamageEffects = new List<GameObject>();
		Object @object = Resources.Load("defaultDamageEffect");
		if (@object != null)
		{
			defaultDamageEffects.Add(@object as GameObject);
		}
	}
}
