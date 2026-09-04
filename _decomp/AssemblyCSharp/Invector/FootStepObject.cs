using UnityEngine;

namespace Invector;

public class FootStepObject
{
	public string name;

	public Transform sender;

	public Collider ground;

	public Terrain terrain;

	public vFootStepHandler stepHandle;

	public Renderer renderer;

	public bool spawnSoundEffect;

	public bool spawnStepMarkEffect;

	public bool spawnParticleEffect;

	public float volume;

	public bool isTerrain => terrain != null;

	public FootStepObject(Transform sender, Collider ground)
	{
		name = "";
		this.sender = sender;
		this.ground = ground;
		terrain = ground.GetComponent<Terrain>();
		stepHandle = ground.GetComponent<vFootStepHandler>();
		renderer = ground.GetComponent<Renderer>();
		spawnSoundEffect = true;
		spawnStepMarkEffect = true;
		spawnParticleEffect = true;
		volume = 1f;
		if (!(renderer != null) || !(renderer.material != null))
		{
			return;
		}
		int num = 0;
		name = string.Empty;
		if (stepHandle != null && stepHandle.material_ID > 0)
		{
			num = stepHandle.material_ID;
		}
		if ((bool)stepHandle)
		{
			switch (stepHandle.stepHandleType)
			{
			case vFootStepHandler.StepHandleType.materialName:
				name = renderer.materials[num].name;
				break;
			case vFootStepHandler.StepHandleType.textureName:
				name = renderer.materials[num].mainTexture.name;
				break;
			}
		}
		else
		{
			name = renderer.materials[num].name;
		}
	}
}
