using UnityEngine;

namespace Invector.vMelee;

[vClassHeader("Hit Effects", "Search for the 'AudioSource' prefab in the project or create your own custom AudioSource.")]
public class vHitEffects : vMonoBehaviour
{
	public GameObject audioSource;

	public AudioClip[] hitSounds;

	public AudioClip[] recoilSounds;

	public GameObject[] recoilParticles;

	public AudioClip[] defSounds;

	private void Start()
	{
		vMeleeWeapon component = GetComponent<vMeleeWeapon>();
		if ((bool)component)
		{
			component.onDamageHit.AddListener(PlayHitEffects);
			component.onRecoilHit.AddListener(PlayRecoilEffects);
			component.onDefense.AddListener(PlayDefenseEffects);
		}
	}

	public void PlayHitEffects(vHitInfo hitInfo)
	{
		if (audioSource != null && hitSounds.Length != 0)
		{
			AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
			Object.Instantiate(audioSource, base.transform.position, base.transform.rotation).GetComponent<AudioSource>().PlayOneShot(clip);
		}
	}

	public void PlayRecoilEffects(vHitInfo hitInfo)
	{
		if (audioSource != null && recoilSounds.Length != 0)
		{
			AudioClip clip = recoilSounds[Random.Range(0, recoilSounds.Length)];
			Object.Instantiate(audioSource, base.transform.position, base.transform.rotation).GetComponent<AudioSource>().PlayOneShot(clip);
		}
		if (recoilParticles.Length != 0)
		{
			GameObject gameObject = recoilParticles[Random.Range(0, recoilParticles.Length)];
			Quaternion rotation = Quaternion.LookRotation(new Vector3(base.transform.position.x, hitInfo.hitPoint.y, base.transform.position.z) - hitInfo.hitPoint);
			if (gameObject != null)
			{
				Object.Instantiate(gameObject, hitInfo.hitPoint, rotation);
			}
		}
	}

	public void PlayDefenseEffects()
	{
		if (audioSource != null && defSounds.Length != 0)
		{
			AudioClip clip = defSounds[Random.Range(0, defSounds.Length)];
			Object.Instantiate(audioSource, base.transform.position, base.transform.rotation).GetComponent<AudioSource>().PlayOneShot(clip);
		}
	}
}
