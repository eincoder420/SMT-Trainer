using UnityEngine;

[ExecuteInEditMode]
public class HxVolumetricParticleSystem : MonoBehaviour
{
	public enum ParticleBlendMode
	{
		Max,
		Add,
		Min,
		Sub
	}

	[Range(0f, 4f)]
	public float DensityStrength = 1f;

	private HxOctreeNode<HxVolumetricParticleSystem>.NodeObject octreeNode;

	[HideInInspector]
	public Renderer particleRenderer;

	public ParticleBlendMode BlendMode = ParticleBlendMode.Add;

	private Vector3 minBounds;

	private Vector3 maxBounds;

	private Bounds LastBounds;

	private void OnEnable()
	{
		particleRenderer = GetComponent<Renderer>();
		LastBounds = particleRenderer.bounds;
		minBounds = LastBounds.min;
		maxBounds = LastBounds.max;
		if (octreeNode == null)
		{
			HxVolumetricCamera.AllParticleSystems.Add(this);
			octreeNode = HxVolumetricCamera.AddParticleOctree(this, minBounds, maxBounds);
		}
	}

	public void UpdatePosition()
	{
		_ = base.transform.hasChanged;
		LastBounds = particleRenderer.bounds;
		minBounds = LastBounds.min;
		maxBounds = LastBounds.max;
		HxVolumetricCamera.ParticleOctree.Move(octreeNode, minBounds, maxBounds);
		base.transform.hasChanged = false;
	}

	private void OnDisable()
	{
		if (octreeNode != null)
		{
			HxVolumetricCamera.AllParticleSystems.Remove(this);
			HxVolumetricCamera.RemoveParticletOctree(this);
			octreeNode = null;
		}
	}

	private void OnDestroy()
	{
		if (octreeNode != null)
		{
			HxVolumetricCamera.AllParticleSystems.Remove(this);
			HxVolumetricCamera.RemoveParticletOctree(this);
			octreeNode = null;
		}
	}
}
