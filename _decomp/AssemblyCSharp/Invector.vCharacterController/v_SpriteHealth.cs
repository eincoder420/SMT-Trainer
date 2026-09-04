using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vCharacterController;

[vClassHeader("SpriteHealth", "Assign your canvas object in the 'healthBar' field to hide and only display when receive damage - leave it empty if you want to display the healthbar all the time.", openClose = false)]
public class v_SpriteHealth : vMonoBehaviour
{
	[Tooltip("UI to show on receive damage")]
	[SerializeField]
	protected GameObject healthBar;

	public bool lookToCamera = true;

	[Header("UI properties")]
	[SerializeField]
	protected Slider _healthSlider;

	[SerializeField]
	protected Slider _damageDelay;

	[SerializeField]
	protected float _smoothDamageDelay;

	[SerializeField]
	protected Text _damageCounter;

	[SerializeField]
	protected float _damageCounterTimer = 1.5f;

	[SerializeField]
	protected bool _showDamageType = true;

	private vHealthController healthControl;

	private bool inDelay;

	private float damage;

	private float currentSmoothDamage;

	internal Transform cameraMain;

	private void Start()
	{
		cameraMain = (Camera.main ? Camera.main.transform : null);
		healthControl = base.transform.GetComponentInParent<vHealthController>();
		if (healthControl == null)
		{
			Debug.LogWarning("The character must have a ICharacter Interface");
			Object.Destroy(base.gameObject);
		}
		healthControl.onReceiveDamage.AddListener(Damage);
		_healthSlider.maxValue = healthControl.maxHealth;
		_healthSlider.value = _healthSlider.maxValue;
		_damageDelay.maxValue = healthControl.maxHealth;
		_damageDelay.value = _healthSlider.maxValue;
		_damageCounter.text = string.Empty;
		if ((bool)healthBar)
		{
			healthBar.SetActive(value: false);
		}
	}

	private void SpriteBehaviour()
	{
		if (lookToCamera && cameraMain != null)
		{
			base.transform.LookAt(cameraMain.position, Vector3.up);
		}
		if (healthControl == null || healthControl.currentHealth <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
		_healthSlider.value = healthControl.currentHealth;
	}

	private void Update()
	{
		if (!healthBar)
		{
			SpriteBehaviour();
		}
	}

	public void Damage(vDamage damage)
	{
		try
		{
			this.damage += damage.damageValue;
			_damageCounter.text = this.damage.ToString("00") + ((_showDamageType && !string.IsNullOrEmpty(damage.damageType)) ? (" : by " + damage.damageType) : "");
			_healthSlider.value -= damage.damageValue;
			if (!inDelay && (bool)healthControl && healthControl.gameObject.activeInHierarchy)
			{
				StartCoroutine(DamageDelay());
			}
		}
		catch
		{
			Object.Destroy(this);
		}
	}

	private IEnumerator DamageDelay()
	{
		inDelay = true;
		if ((bool)healthBar)
		{
			SpriteBehaviour();
		}
		if ((bool)healthBar)
		{
			healthBar.SetActive(value: true);
		}
		while (_damageDelay.value > _healthSlider.value)
		{
			if ((bool)healthBar)
			{
				SpriteBehaviour();
			}
			_damageDelay.value -= _smoothDamageDelay;
			yield return null;
		}
		inDelay = false;
		yield return new WaitForSeconds(_damageCounterTimer);
		damage = 0f;
		_damageCounter.text = string.Empty;
		if ((bool)healthBar)
		{
			healthBar.SetActive(value: false);
		}
	}
}
