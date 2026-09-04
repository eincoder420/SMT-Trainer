using UnityEngine;

namespace Invector.vCharacterController;

public class vPunchingBag : MonoBehaviour
{
	public Rigidbody _rigidbody;

	public float forceMultipler = 0.5f;

	public SpringJoint joint;

	public vHealthController character;

	public bool removeComponentsAfterDie;

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
		character = GetComponent<vHealthController>();
		character.onReceiveDamage.AddListener(TakeDamage);
	}

	public void TakeDamage(vDamage damage)
	{
		Vector3 hitPosition = damage.hitPosition;
		Vector3 position = base.transform.position;
		position.y = hitPosition.y;
		Vector3 vector = position - hitPosition;
		if (character != null && joint != null && character.currentHealth < 0f)
		{
			joint.connectedBody = null;
			if (removeComponentsAfterDie)
			{
				MonoBehaviour[] componentsInChildren = character.gameObject.GetComponentsInChildren<MonoBehaviour>();
				foreach (MonoBehaviour monoBehaviour in componentsInChildren)
				{
					if (monoBehaviour != this)
					{
						Object.Destroy(monoBehaviour);
					}
				}
			}
		}
		if (_rigidbody != null)
		{
			_rigidbody.AddForce(vector * ((float)damage.damageValue * forceMultipler), ForceMode.Impulse);
		}
	}
}
